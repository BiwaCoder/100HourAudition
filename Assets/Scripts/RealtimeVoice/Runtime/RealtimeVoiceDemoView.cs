using System;
using TMPro;
using UnityEngine;
namespace HundredHour.RealtimeVoice
{
    // Visibility is derived from one state; status, interviewer and transcript never share a text field.
    public sealed class RealtimeVoiceDemoView : MonoBehaviour
    {
        public UnityEngine.UI.Button startButton,confirmButton,retryButton,cancelButton,copyButton;
        public TMP_Text titleText,stepText,statusText,captionText,userText,resultText,startLabel,confirmLabel,micLevelText;
        public GameObject welcomePanel,interviewerPanel,userPanel,microphonePanel,resultPanel;
        public UnityEngine.UI.Image micFill;
        public VoiceTerminalPresentation terminal;
        public bool localizeText;
        string L(string text)=>localizeText?HundredHour.Localization.GameLanguage.Text(text):text;
        string resultJson = "";
        float resultFontSize;
        public event Action StartRequested,ConfirmRequested,RetryRequested,CancelRequested;
        bool listening;
        Sprite meterSprite;
        string assistant="", expectedPrompt="";
        public void Awake()
        {
            resultFontSize=resultText.fontSize;
            if (micFill && !micFill.sprite)
            {
                var texture = Texture2D.whiteTexture;
                meterSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * .5f);
                micFill.sprite = meterSprite;
            }
            startButton.onClick.AddListener(()=>StartRequested?.Invoke());confirmButton.onClick.AddListener(()=>ConfirmRequested?.Invoke());
            retryButton.onClick.AddListener(()=>RetryRequested?.Invoke());cancelButton.onClick.AddListener(()=>CancelRequested?.Invoke());
            copyButton.onClick.AddListener(()=>{GUIUtility.systemCopyBuffer=resultJson;SetStatus("キャラクターJSONをコピーしました。");});
        }
        public void Present(VoiceInterviewFlow flow)
        {
            resultText.fontSize=resultFontSize;
            var s=flow.State;listening=flow.Listening;
            if(terminal)terminal.Present(flow);
            resultJson=flow.Result;
            bool ready=s==VoiceInterviewState.Ready,error=s==VoiceInterviewState.Error,done=s==VoiceInterviewState.Complete,generating=s==VoiceInterviewState.Generating;
            welcomePanel.SetActive(ready||error);interviewerPanel.SetActive(flow.Prompting||listening||flow.Reviewing);
            userPanel.SetActive(listening||flow.Reviewing);microphonePanel.SetActive(listening);resultPanel.SetActive(done);
            startButton.gameObject.SetActive(ready||error||done);startLabel.text=ready?"音声でキャラクターを作る":done?"もう一度作る":"最初からやり直す";
            startButton.interactable=true;confirmButton.gameObject.SetActive(flow.Reviewing);retryButton.gameObject.SetActive(flow.Reviewing);
            cancelButton.gameObject.SetActive(!ready&&!error&&!done);copyButton.gameObject.SetActive(done);
            string heading=ready?"あなたの声から、新しいキャラクターへ":error?"接続を確認してください":done?"キャラクターが完成しました":generating?"会話から、キャラクターを作っています":"AI-0とのキャラクターづくり";
            Reveal(titleText,heading);
            stepText.text=ready?"マイクチェック → 会話 → 完成":s==VoiceInterviewState.Connecting?"準備中":done?"完了":generating?"最後のステップ / JSON生成":error?"再接続できます":flow.QuestionNumber==0?"マイクチェック / 回答には含めません":$"質問 {flow.QuestionNumber} / 2";
            if(flow.Prompting){assistant="";expectedPrompt=flow.Prompt;Reveal(captionText,flow.Prompt);}
            else if(listening)Reveal(captionText,flow.CurrentQuestion);
            else if(flow.Reviewing)Reveal(captionText,flow.QuestionNumber==0?"声を文字にできました。表示内容が合っていれば、質問へ進みましょう。":flow.CurrentQuestion);
            if(listening){Reveal(userText,"ここに、あなたが話した内容が表示されます。");SetMicLevel(0);}
            if(flow.Reviewing)Reveal(userText,flow.Candidate);
            if(done){Reveal(resultText,flow.Result);var scroll=resultPanel.GetComponentInChildren<UnityEngine.UI.ScrollRect>();if(scroll){Canvas.ForceUpdateCanvases();scroll.verticalNormalizedPosition=1;}}
            else Reveal(resultText,"");
            confirmLabel.text=flow.QuestionNumber==0?"マイクOK・質問へ":flow.QuestionNumber==1?"この回答で、次の質問へ":"この会話から作成する";
            startLabel.text=L(startLabel.text);stepText.text=L(stepText.text);confirmLabel.text=L(confirmLabel.text);
            SetStatus(ready?"開始すると、相手から日本語で案内します。":error?flow.Error:done?"回答をもとにした設定です。JSONをコピーできます。":generating?"少しお待ちください。マイクは停止しています。":s==VoiceInterviewState.Connecting?"接続を準備しています……":flow.Prompting?"相手が話しています。案内が終わるまでお待ちください。":listening?"マイク受付中 / 日本語で話してください。":"聞き取った内容を確認してください。違っていたら言い直せます。");
        }
        public void ShowCharacterIntroduction(string name,string introduction,bool english)
        {
            Reveal(titleText,english?"Meet your new character":"新しいキャラクターから、ごあいさつ");
            resultText.fontSize=30;
            Reveal(resultText,(string.IsNullOrWhiteSpace(name)?"":name+"\n\n")+introduction);
            var scroll=resultPanel.GetComponentInChildren<UnityEngine.UI.ScrollRect>();
            if(scroll){Canvas.ForceUpdateCanvases();scroll.verticalNormalizedPosition=1;}
        }
        void Reveal(TMP_Text label,string text)
        {
            if(label!=resultText)text=L(text);
            var effect=label.GetComponent<TerminalTextReveal>();
            if(effect)effect.Show(text,terminal?terminal.RevealStage:0);else label.text=text;
        }
        public void SetStatus(string text)=>statusText.text=L(text);
        void OnDestroy(){if(meterSprite)Destroy(meterSprite);}
        public void AppendInterviewer(string delta){if(!interviewerPanel.activeSelf)return;assistant+=delta;if(!expectedPrompt.StartsWith(assistant,StringComparison.Ordinal))Reveal(captionText,assistant);}
        public void ShowPartialTranscript(string text){if(listening)Reveal(userText,text);}
        public void SetMicLevel(float value){if(!listening)return;micFill.fillAmount=Mathf.Clamp01(value);micLevelText.text=L(value>.035f?"声が入っています":"マイク受付中");}
    }
}
