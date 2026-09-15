using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;
using HundredHour.Net;
using HundredHour.Localization;
using HundredHour.RealtimeVoice;

namespace HundredHour.AuditionEntry
{
    // Optional one-way voice tutorial for the AuditionTitle hub screen: gpt-live-1 narrates the
    // title/menu screen (no microphone, the player never needs to speak). Reacts to language
    // switches, the gender screen, hovering the 01/02/03 menu items, and entering the main story
    // (where the in-story guide, Kumono/Saori, takes over and this narrator disconnects).
    // Costs real API time whenever it runs, so it can be switched off entirely from the Unity
    // Editor via Tools/100 Hour/タイトル音声チュートリアル (see AuditionTitleNarratorToggle).
    public sealed class AuditionTitleNarrator : MonoBehaviour
    {
        const float MaxSeconds=180;
        AuditionPortal portal;
        PcmStreamPlayer player;
        string screen="title";
        string pendingCue,pendingValue,hoverLanguage;
        float hoverDue;
        IPlatformSocket socket;
        CancellationTokenSource lifetime;
        readonly ConcurrentQueue<Action> callbacks=new ConcurrentQueue<Action>();
        readonly SemaphoreSlim sending=new SemaphoreSlim(1,1);
        int generation;
        bool ready, waitingForAudio;
        public bool Ready=>ready;
        public string CurrentVoice {get;private set;}
        public int AudioChunks {get;private set;}
        public string LastError {get;private set;}
        float began;
        public static bool Enabled
        {
            get
            {
#if UNITY_EDITOR
                return UnityEditor.EditorPrefs.GetBool("100Hour.TitleNarrator.Enabled",true);
#else
                return true;
#endif
            }
        }
        public void Initialize(AuditionPortal p,string screenName="title")
        {
            portal=p;screen=screenName;
            if(!Enabled){Destroy(this);return;}
            player=gameObject.AddComponent<PcmStreamPlayer>();
            portal.MenuFocused+=OnMenuFocused;
            portal.LanguageFocused+=OnLanguageFocused;
            portal.GenderScreenShown+=OnGenderScreenShown;
            portal.GenderSelected+=OnGenderSelected;
            portal.EnteringMain+=OnEnteringMain;
            GameLanguage.Changed+=OnLanguageChanged;
            // Do not spend the tutorial while the browser is still blocking playback.
            waitingForAudio = !PcmStreamPlayer.AudioReady;
            if (!waitingForAudio) Connect();
        }
        void OnDestroy()
        {
            if(portal!=null){portal.MenuFocused-=OnMenuFocused;portal.LanguageFocused-=OnLanguageFocused;portal.GenderScreenShown-=OnGenderScreenShown;portal.GenderSelected-=OnGenderSelected;portal.EnteringMain-=OnEnteringMain;}
            GameLanguage.Changed-=OnLanguageChanged;
            Disconnect();
        }
        void Update()
        {
            if(waitingForAudio && PcmStreamPlayer.AudioReady){waitingForAudio=false;Connect();}
            if(lifetime!=null && !ready && Time.realtimeSinceStartup-began>20){ReportError("音声案内の準備がタイムアウトしました。ページを再読み込みしてください。");}
            while(callbacks.TryDequeue(out var callback))callback();
            if(hoverLanguage!=null&&Time.unscaledTime>=hoverDue)
            {
                if(portal&&portal.languagePanel&&portal.languagePanel.activeInHierarchy)SendCue("language_hover",hoverLanguage);
                hoverLanguage=null;
            }
            if(lifetime!=null&&Time.realtimeSinceStartup-began>MaxSeconds)Disconnect();
        }
        void Queue(int revision,Action action)=>callbacks.Enqueue(()=>{if(revision==generation&&isActiveAndEnabled)action();});
        void Connect(bool announce=false)
        {
            Disconnect();AudioChunks=0;LastError=null;began=Time.realtimeSinceStartup;lifetime=new CancellationTokenSource();
            string phase=screen=="portrait"?"portrait":AuditionPortal.GenderConfirmed?"menu":AuditionPortal.LanguageConfirmed?"gender":"language";
            string gender=screen=="portrait"||AuditionPortal.GenderConfirmed?AuditionProfile.Gender:"";
            _=Receive(generation,lifetime.Token,phase,gender,announce);
        }
        async Task Receive(int revision,CancellationToken token,string phase,string gender,bool announce)
        {
            try
            {
            string language=GameLanguage.Current==GameLocale.English?"en":"ja";
            string url=ApiEndpointConfig.Instance.Ws+"/api/live-title-tour?language="+Uri.EscapeDataString(language)+"&screen="+Uri.EscapeDataString(screen)+"&phase="+phase+"&gender="+gender+"&switched="+(announce?"1":"0")+ApiAuth.QuerySuffix();
            var connection=PlatformSocket.Create(url);socket=connection;
            connection.OnMessage+=text=>{
                try{var e=JObject.Parse(text);Queue(revision,()=>Handle(e));}
                catch(Exception){Queue(revision,()=>ReportError("音声メッセージを読み取れませんでした。"));}
            };
            connection.OnError+=msg=>Queue(revision,()=>ReportError("音声通信に失敗しました ("+msg+")。"));
            connection.OnClose+=code=>Queue(revision,()=>ReportError("音声接続が終了しました ("+code+")。ページを再読み込みしてください。"));
                var connectTask=connection.Connect();
                if(await Task.WhenAny(connectTask,Task.Delay(15000,token))!=connectTask||token.IsCancellationRequested)
                {
                    if(!token.IsCancellationRequested)Queue(revision,()=>ReportError("音声接続がタイムアウトしました。"));
                    return;
                }
                await connectTask;
            }
            catch(OperationCanceledException){if(!token.IsCancellationRequested)Queue(revision,()=>ReportError("音声接続がタイムアウトしました。"));}
            catch(Exception ex){Queue(revision,()=>ReportError("音声通信に失敗しました ("+ex.GetType().Name+")。"));}
        }
        void Handle(JObject e)
        {
            switch((string)e["type"])
            {
                case "live.ready":CurrentVoice=(string)e["voice"];ready=true;if(pendingCue!=null){SendCue(pendingCue,pendingValue);pendingCue=pendingValue=null;}break;
                case "live.finish":Disconnect();break;
                case "live.error":ReportError((string)e["message"]??"音声サーバーでエラーが発生しました。");break;
                case "live.audio":AudioChunks++;if(player)player.EnqueuePcm16(Convert.FromBase64String((string)e["delta"]??""));break;
            }
        }
        void ReportError(string message){LastError=message;Debug.LogWarning("Title voice: "+message);if(portal)portal.SetNotice(message);Disconnect();}
        public void PhotoCue(string kind){if(ready)SendCue(kind,null);else pendingCue=kind;}
        void SendCue(string kind,string value)
        {
            if(!ready){pendingCue=kind;pendingValue=value;return;}
            _=Send(new JObject{["type"]="cue",["kind"]=kind,["value"]=value??""});
        }
        async Task Send(JObject message)
        {
            var connection=socket;var life=lifetime;int revision=generation;
            if(connection==null||life==null||connection.State!=PlatformSocketState.Open)return;
            var token=life.Token;bool locked=false;
            try
            {
                await sending.WaitAsync(token);locked=true;if(revision!=generation)return;
                await connection.SendText(message.ToString(Newtonsoft.Json.Formatting.None));
            }
            catch(OperationCanceledException){if(!token.IsCancellationRequested)Queue(revision,()=>ReportError("音声接続がタイムアウトしました。"));}
            catch(Exception ex){Queue(revision,()=>ReportError("音声通信に失敗しました ("+ex.GetType().Name+")。"));}
            finally{if(locked)sending.Release();}
        }
        void OnMenuFocused(int index)=>SendCue("hover_0"+(index+1),null);
        void OnLanguageFocused(string language){hoverLanguage=language;hoverDue=Time.unscaledTime+.35f;}
        void OnGenderSelected(){hoverLanguage=null;pendingCue=pendingValue=null;Connect(true);}
        void OnGenderScreenShown(){hoverLanguage=null;SendCue("gender",GameLanguage.Current==GameLocale.English?"en":"ja");}
        // The scene unload that follows tears this object down (OnDestroy -> Disconnect); no
        // need to disconnect here, which would race the cue message before it is even sent.
        void OnEnteringMain(string name)=>SendCue("enter_main",null);
        void OnLanguageChanged()=>SendCue("language",GameLanguage.Current==GameLocale.English?"en":"ja");
        void Disconnect()
        {
            generation++;ready=false;
            if(player)player.Clear();
            lifetime?.Cancel();lifetime?.Dispose();lifetime=null;
            try{_=socket?.Close();}catch{}socket=null;
        }
    }
}
