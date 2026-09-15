using System;
using System.IO;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using HundredHour.RealtimeVoice;
using HundredHour.Localization;
namespace HundredHour.LiveInterview
{
    public sealed class LiveInterviewController : MonoBehaviour
    {
        public event Action<string> CharacterCreated;
        public LiveVoiceSession session;
        public RealtimeVoiceDemoView view;
        public BuildCharacterApiClient builder;
        public TMP_Text connectionText, timerText;
        public UnityEngine.UI.Button japaneseButton, englishButton;
        ConversationSketchView sketch;
        bool generating;
        string language="ja";
        string L(string text)=>GameLanguage.Text(text);
        public bool CanChangeLanguage=>!active&&!generating;
        public void SelectJapanese(){if(CanChangeLanguage)GameLanguage.Select(GameLocale.Japanese);}
        public void SelectEnglish(){if(CanChangeLanguage)GameLanguage.Select(GameLocale.English);}
        void LanguageChanged(){if(CanChangeLanguage)Ready();}
        bool timerRunning, finishing;
        float elapsed;
        int maxSeconds=120;
        string assistant="",user="",stage="";
        int revision;
        long assistantEnd,userEnd;
        bool active,received;
        float started;
        VoiceInterviewFlow visualFlow;
        void OnEnable()
        {
            view.localizeText=true;view.terminal.localizeText=true;
            GameLanguage.Changed+=LanguageChanged;
            if(japaneseButton)japaneseButton.onClick.AddListener(SelectJapanese);
            if(englishButton)englishButton.onClick.AddListener(SelectEnglish);
            session.EventReceived+=Receive;session.Failed+=Fail;
            view.StartRequested+=StartConversation;view.CancelRequested+=Cancel;view.ConfirmRequested+=Finish;
            if(!sketch){sketch=GetComponent<ConversationSketchView>();if(!sketch)sketch=gameObject.AddComponent<ConversationSketchView>();sketch.Initialize(view.titleText.transform.parent,view.captionText.font);}
            sketch.Selected+=session.SelectTopic;
            Ready();
        }
        void OnDisable()
        {
            GameLanguage.Changed-=LanguageChanged;
            if(japaneseButton)japaneseButton.onClick.RemoveListener(SelectJapanese);
            if(englishButton)englishButton.onClick.RemoveListener(SelectEnglish);
            session.EventReceived-=Receive;session.Failed-=Fail;
            view.StartRequested-=StartConversation;view.CancelRequested-=Cancel;view.ConfirmRequested-=Finish;
            if(sketch){sketch.Selected-=session.SelectTopic;sketch.SetActive(false);}
            revision++;builder.Cancel();session.Disconnect();active=false;
        }
        void Text(TMP_Text label,string value,int effect=0)
        {
            if(label==view.titleText)value=L(value);
            var reveal=label.GetComponent<TerminalTextReveal>();if(reveal)reveal.Show(value,effect);else label.text=value;
        }
        void Ready()
        {
            if(sketch)sketch.SetActive(false);
            generating=false;language=GameLanguage.Current==GameLocale.English?"en":"ja";
            timerRunning=false;finishing=false;elapsed=0;maxSeconds=120;
            active=false;received=false;view.confirmButton.interactable=true;assistant=user="";assistantEnd=userEnd=0;
            visualFlow=new VoiceInterviewFlow();view.Present(visualFlow);
            RefreshTelemetry();
            Text(view.titleText,"AI-0と、新しい物語をはじめる");
            view.stepText.text=L("GPT-LIVE 1 / 自然な音声会話");
            view.startLabel.text=L("AI-0と話す");
            view.SetStatus("目安90秒 / 上限120秒。会話の途中でも話しかけられます。");
        }
        public void StartConversation()
        {
            revision++;builder.Cancel();Ready();session.LanguageCode=language;active=true;RefreshTelemetry();
            visualFlow.Begin();view.Present(visualFlow);view.SetStatus("AI-0に接続しています……");session.Connect();
        }
        void Receive(JObject e)
        {
            switch((string)e["type"])
            {
                case "live.insights": if((string)e["language"]==language)sketch.Render(e["cards"] as JArray);break;
                case "live.topic_selected": sketch.Acknowledge((string)e["title"]);break;
                case "live.topic_rejected": sketch.Reject();break;
                case "live.ready":
                    sketch.Begin(language=="en");
                    maxSeconds=Mathf.Max(1,(int?)e["max_seconds"]??120);
                    started=Time.realtimeSinceStartup;timerRunning=true;RefreshTelemetry();
                    visualFlow.Connected();visualFlow.PromptFinished();view.Present(visualFlow);
                    Text(view.titleText,"AI-0との出会い");Text(view.captionText,"");Text(view.userText,"");
                    view.retryButton.gameObject.SetActive(false);break;
                case "live.stage":
                    stage=(string)e["stage"];if(stage=="closing"){finishing=true;sketch.StopChoosing();}view.stepText.text=(string)e["label"];
                    view.confirmButton.gameObject.SetActive(stage=="values");view.confirmLabel.text=L("今の内容で作成する");
                    view.terminal.PresentLive(stage,received,session.InputSignal);
                    break;
                case "live.text":
                    bool input=(string)e["role"]=="user";
                    string delta=(string)e["delta"]??"";
                    long begin=(long?)e["start_ms"]??0,end=(long?)e["end_ms"]??0;
                    // Group for display only. The server retains raw timestamped fragments.
                    if(input){if(begin-userEnd>1800)user="";userEnd=end;user+=delta;received=true;Text(view.userText,user,1);}
                    else {if(begin-assistantEnd>1800)assistant="";assistantEnd=end;assistant+=delta;Text(view.captionText,assistant,stage=="greeting"?0:1);}
                    view.terminal.PresentLive(stage,received,session.InputSignal);
                    break;
                case "live.finish": Generate((string)e["input"]);break;
            }
        }
        void Update()
        {
            RefreshTelemetry();
            if(!active||!session.Ready)return;
            view.SetStatus(finishing?"会話をまとめています……":"自由に話しかけてください。");
            view.SetMicLevel(0); // The spectrum shows actual PCM; keep a simple textual mic indicator.
            view.micLevelText.text=L("会話中 / マイクON");
            view.micFill.gameObject.SetActive(false);
        }
        void RefreshTelemetry()
        {
            if(japaneseButton)japaneseButton.interactable=CanChangeLanguage&&GameLanguage.Current!=GameLocale.Japanese;
            if(englishButton)englishButton.interactable=CanChangeLanguage&&GameLanguage.Current!=GameLocale.English;
            bool online=session && session.Ready && session.IsConnected;
            if(timerRunning)elapsed=Mathf.Clamp(Time.realtimeSinceStartup-started,0,maxSeconds);
            if(connectionText)
            {
                connectionText.text=online?"VOICE / ONLINE":active&&!session.Ready?"VOICE / CONNECTING":"VOICE / OFFLINE";
                connectionText.color=online?new Color(.35f,.85f,.65f):active&&!session.Ready?new Color(1f,.76f,.35f):new Color(.72f,.76f,.82f);
            }
            if(timerText)
            {
                int remaining=Mathf.CeilToInt(Mathf.Max(0,maxSeconds-elapsed));
                timerText.text=GameLanguage.Format("live.remaining",$"{remaining/60:00}:{remaining%60:00}");
                timerText.color=remaining<=30?new Color(1f,.65f,.4f):Color.white;
            }
        }
        void StopTimer(){RefreshTelemetry();timerRunning=false;}
        void Finish(){finishing=true;sketch.StopChoosing();view.confirmButton.interactable=false;session.Finish();view.SetStatus("会話をまとめています……");}
        void Generate(string transcript)
        {
            sketch.SetActive(false);
            StopTimer();active=false;generating=true;session.Disconnect();RefreshTelemetry();int request=++revision;
            // Reuse the existing result UI and JSON copy behavior; no synthetic answers reach the API.
            var f=new VoiceInterviewFlow();f.Begin();f.Connected();f.PromptFinished();f.ReceiveTranscript("UI");f.Confirm();f.PromptFinished();f.ReceiveTranscript("UI");f.Confirm();f.PromptFinished();f.ReceiveTranscript("UI");f.Confirm();
            visualFlow=f;view.Present(f);
            builder.BuildCharacter(transcript,"",result=>{
                if(!isActiveAndEnabled||request!=revision)return;
                if(result==null||!result.success){Fail(result?.error??"生成に失敗しました。");return;}
                string json=result.character_json;
                try
                {
                    var profile=JObject.Parse(json);
                    string dir=Path.Combine(Application.persistentDataPath,"Characters");Directory.CreateDirectory(dir);
                    string path=Path.Combine(dir,"astra-live-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff")+".json");File.WriteAllText(path,json);
                    generating=false;f.Complete(json);view.Present(f);view.stepText.text=L("GPT-LIVE 1 / 完了");
                    string introduction=(string)profile["introduction"];
                    if(!string.IsNullOrWhiteSpace(introduction))
                        view.ShowCharacterIntroduction((string)profile["characters"]?[0]?["name"],introduction,language=="en");
                    view.SetStatus("キャラクターを作成し、JSONファイルに保存しました。");
                    Debug.Log("GPT-Live character saved: "+path);
                    CharacterCreated?.Invoke(json);
                }
                catch(Exception){Fail("JSONファイルを保存できませんでした。");}
            },language,"interview");
        }
        public void Cancel(){revision++;builder.Cancel();session.Disconnect();view.confirmButton.interactable=true;Ready();}
        void Fail(string message)
        {
            sketch.SetActive(false);
            StopTimer();revision++;active=false;generating=false;builder.Cancel();session.Disconnect();visualFlow=new VoiceInterviewFlow();visualFlow.Fail(L(message));view.Present(visualFlow);
            view.confirmButton.interactable=true;view.stepText.text=L("GPT-LIVE 1 / 接続を確認");RefreshTelemetry();
        }
    }
}
