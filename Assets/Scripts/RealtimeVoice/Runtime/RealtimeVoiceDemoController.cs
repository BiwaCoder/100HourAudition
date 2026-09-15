using UnityEngine;
namespace HundredHour.RealtimeVoice
{
    [RequireComponent(typeof(RealtimeVoiceDemoView))]
    public sealed class RealtimeVoiceDemoController : MonoBehaviour
    {
        [SerializeField] RealtimeVoiceSession session;
        [SerializeField] BuildCharacterApiClient buildCharacterApiClient;
        public VoiceInterviewFlow Flow {get;}=new VoiceInterviewFlow();
        RealtimeVoiceDemoView view;
        int revision;
        float listeningSince;
        bool hintShown;
        void OnEnable()
        {
            view=GetComponent<RealtimeVoiceDemoView>();
            view.StartRequested+=StartInterview;view.ConfirmRequested+=Confirm;view.RetryRequested+=Retry;view.CancelRequested+=Cancel;
            session.OnReady+=Connected;session.OnAssistantTurnFinished+=PromptFinished;
            session.OnStatusChanged+=Status;session.OnInterviewerTextDelta+=view.AppendInterviewer;
            session.OnUserTranscriptDelta+=view.ShowPartialTranscript;session.OnUserTranscriptCompleted+=Transcript;
            session.OnConnectionError+=Fail;session.OnInputError+=InputError;
            session.OnSpeechStarted+=SpeechStarted;session.OnSpeechStopped+=SpeechStopped;session.OnMicLevel+=view.SetMicLevel;
            Render();
        }
        void OnDisable()
        {
            revision++;buildCharacterApiClient.Cancel();
            view.StartRequested-=StartInterview;view.ConfirmRequested-=Confirm;view.RetryRequested-=Retry;view.CancelRequested-=Cancel;
            session.OnReady-=Connected;session.OnAssistantTurnFinished-=PromptFinished;session.OnStatusChanged-=Status;
            session.OnInterviewerTextDelta-=view.AppendInterviewer;session.OnUserTranscriptDelta-=view.ShowPartialTranscript;
            session.OnUserTranscriptCompleted-=Transcript;session.OnConnectionError-=Fail;session.OnInputError-=InputError;
            session.OnSpeechStarted-=SpeechStarted;session.OnSpeechStopped-=SpeechStopped;session.OnMicLevel-=view.SetMicLevel;
            session.Disconnect();Flow.Reset();
        }
        void Update()
        {
            if(Flow.Listening&&!hintShown&&Time.unscaledTime-listeningSince>12){hintShown=true;view.SetStatus("声を待っています。マイクの接続・入力音量を確認して、もう一度話してください。");}
        }
        public void StartInterview(){revision++;buildCharacterApiClient.Cancel();Flow.Begin();Render();session.Connect();}
        void Connected(){if(Flow.Connected())Ask();}
        void Ask(){Render();session.Speak(Flow.Prompt);}
        void PromptFinished(){if(Flow.PromptFinished())Listen();}
        void Listen(){listeningSince=Time.unscaledTime;hintShown=false;Render();session.SetListening(true);}
        void Status(string text){if(Flow.State==VoiceInterviewState.Connecting)view.SetStatus(text);}
        void SpeechStarted(){if(Flow.Listening){hintShown=true;view.SetStatus("声を受け取っています。話し終えたら、そのまま少しお待ちください。");view.ShowPartialTranscript("聞き取り中……");}}
        void SpeechStopped(){if(Flow.Listening)view.SetStatus("音声を文字にしています……");}
        void InputError(string error){if(Flow.Listening){view.SetStatus(error+" もう一度話してください。");listeningSince=Time.unscaledTime;hintShown=false;}}
        void Transcript(string text)
        {
            if(!Flow.Listening)return;
            if(!Flow.ReceiveTranscript(text)){InputError("声を文字にできませんでした。");return;}
            session.SetListening(false);Render();
        }
        public void Confirm()
        {
            if(!Flow.Confirm())return;
            if(Flow.State!=VoiceInterviewState.Generating){Ask();return;}
            session.Disconnect();Render();int request=++revision;
            buildCharacterApiClient.BuildCharacter(Flow.BuildInput,"",response=>{
                if(!isActiveAndEnabled||request!=revision||Flow.State!=VoiceInterviewState.Generating)return;
                if(response==null||!response.success||!Flow.Complete(response.character_json)){Fail(response?.error??"生成結果を受け取れませんでした。");return;}Render();
            });
        }
        public void Retry(){if(Flow.Retry())Listen();}
        public void Cancel(){revision++;buildCharacterApiClient.Cancel();session.Disconnect();Flow.Reset();Render();}
        void Fail(string error){revision++;session.Disconnect();buildCharacterApiClient.Cancel();Flow.Fail(error);Render();}
        void Render()=>view.Present(Flow);
    }
}
