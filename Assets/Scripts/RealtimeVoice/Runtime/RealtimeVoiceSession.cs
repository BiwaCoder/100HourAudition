using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HundredHour.Net;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace HundredHour.RealtimeVoice
{
    // Transport never chooses a question. Each assistant turn is explicitly requested by the flow.
    public sealed class RealtimeVoiceSession : MonoBehaviour
    {
        string tokenEndpointBaseUrl;
        [SerializeField] MicrophoneCapture microphoneCapture;
        [SerializeField] PcmStreamPlayer pcmPlayer;
        public event Action<string> OnStatusChanged,OnInterviewerTextDelta,OnUserTranscriptDelta,OnUserTranscriptCompleted,OnConnectionError,OnInputError;
        public event Action OnReady,OnAssistantTurnFinished,OnSpeechStarted,OnSpeechStopped;
        public event Action<float> OnMicLevel;
        readonly ConcurrentQueue<Action> callbacks=new ConcurrentQueue<Action>();
        readonly SemaphoreSlim sendLock=new SemaphoreSlim(1,1);
        readonly Dictionary<string,int> inputTurns=new Dictionary<string,int>();
        readonly HashSet<string> completedItems=new HashSet<string>();
        IPlatformSocket socket;
        CancellationTokenSource cancellation;
        int generation,inputEpoch;
        bool ready,listening,awaitingResponse,responseDone;
        float drainedFor,responseStarted,connectionStarted;
        string partial="";
        public VoiceSignalBuffer InputSignal { get; } = new VoiceSignalBuffer();
        public bool IsConnected=>socket!=null&&socket.State==PlatformSocketState.Open;
        public bool IsListening=>listening;
        void Awake(){tokenEndpointBaseUrl=ApiEndpointConfig.Instance.Http;microphoneCapture.OnPcm16Chunk+=MicChunk;microphoneCapture.OnCaptureError+=MicError;}
        void OnDisable()=>Disconnect();
        void OnDestroy(){microphoneCapture.OnPcm16Chunk-=MicChunk;microphoneCapture.OnCaptureError-=MicError;Disconnect();}
        void MicError(string message)=>OnConnectionError?.Invoke(message);
        void Update()
        {
            while(callbacks.TryDequeue(out var callback))callback();
            if(cancellation!=null&&!ready&&Time.unscaledTime-connectionStarted>35){OnConnectionError?.Invoke("接続準備がタイムアウトしました。もう一度お試しください。");return;}
            if(!awaitingResponse)return;
            if(Time.unscaledTime-responseStarted>75){awaitingResponse=false;OnConnectionError?.Invoke("相手の音声が届きませんでした。接続を確認してやり直してください。");return;}
            if(responseDone&&!pcmPlayer.HasPendingAudio)
            {
                drainedFor+=Time.unscaledDeltaTime;
                if(drainedFor>=.35f){awaitingResponse=false;drainedFor=0;OnAssistantTurnFinished?.Invoke();}
            }
            else drainedFor=0;
        }
        void Enqueue(int id,Action action)=>callbacks.Enqueue(()=>{if(id==generation&&isActiveAndEnabled)action();});
        public void Connect()
        {
            Disconnect();connectionStarted=Time.unscaledTime;cancellation=new CancellationTokenSource();StartCoroutine(ConnectRoutine(generation,cancellation));
        }
        IEnumerator ConnectRoutine(int id,CancellationTokenSource lifetime)
        {
            OnStatusChanged?.Invoke("マイクと接続を準備しています……");
#if !UNITY_WEBGL || UNITY_EDITOR
            // WebGL has no Microphone-backed permission query; getUserMedia's own promise
            // rejection (surfaced via MicrophoneCapture.OnCaptureError) is the permission gate there.
            if(!Application.HasUserAuthorization(UserAuthorization.Microphone))yield return Application.RequestUserAuthorization(UserAuthorization.Microphone);
            if(id!=generation)yield break;
            if(!Application.HasUserAuthorization(UserAuthorization.Microphone)){OnConnectionError?.Invoke("マイクの使用が許可されていません。OSの設定でUnityのマイクを許可してください。");yield break;}
#endif
            using var request=new UnityWebRequest(tokenEndpointBaseUrl+"/api/realtime-token",UnityWebRequest.kHttpVerbPOST);
            request.uploadHandler=new UploadHandlerRaw(Array.Empty<byte>());request.downloadHandler=new DownloadHandlerBuffer();request.SetRequestHeader("Content-Type","application/json");request.timeout=25;
            ApiAuth.ApplyHeaders(request);
            yield return request.SendWebRequest();if(id!=generation)yield break;
            if(request.result!=UnityWebRequest.Result.Success){OnConnectionError?.Invoke("会話に接続できませんでした。PythonAPIが起動しているか確認してください。");yield break;}
            JObject token;
            try{token=JObject.Parse(request.downloadHandler.text);}catch{OnConnectionError?.Invoke("接続情報を読み取れませんでした。");yield break;}
            if(token.Value<bool?>("success")!=true||string.IsNullOrEmpty(token.Value<string>("client_secret"))){OnConnectionError?.Invoke("音声サービスの接続準備に失敗しました。サーバー設定を確認してください。");yield break;}
            OnStatusChanged?.Invoke("AI-0に接続しています……");
            _=OpenSocket(token.Value<string>("client_secret"),token.Value<string>("model"),id,lifetime.Token);
        }
        async Task OpenSocket(string secret,string model,int id,CancellationToken token)
        {
            // Browsers cannot set an Authorization header on a WebSocket handshake, so the ephemeral
            // key travels as a subprotocol instead -- the scheme OpenAI documents for browser clients
            // (see developers.openai.com/api/docs/guides/voice-websockets), and works the same way
            // for the desktop ClientWebSocket path.
            var subprotocols=new List<string>{"realtime","openai-insecure-api-key."+secret};
            var connection=PlatformSocket.Create("wss://api.openai.com/v1/realtime?model="+Uri.EscapeDataString(model),subprotocols);
            socket=connection;
            connection.OnMessage+=json=>Enqueue(id,()=>HandleServerEvent(json));
            connection.OnClose+=_=>Enqueue(id,()=>OnConnectionError?.Invoke("音声接続が終了しました。もう一度お試しください。"));
            connection.OnError+=_=>Enqueue(id,()=>OnConnectionError?.Invoke("音声接続が切れました。もう一度お試しください。"));
            try
            {
                var connectTask=connection.Connect();
                if(await Task.WhenAny(connectTask,Task.Delay(TimeSpan.FromSeconds(20),token))!=connectTask||token.IsCancellationRequested)
                {
                    if(!token.IsCancellationRequested)Enqueue(id,()=>OnConnectionError?.Invoke("音声接続がタイムアウトしました。もう一度お試しください。"));
                    return;
                }
                await connectTask;
            }
            catch(OperationCanceledException){if(!token.IsCancellationRequested)Enqueue(id,()=>OnConnectionError?.Invoke("音声接続がタイムアウトしました。もう一度お試しください。"));}
            catch(Exception){Enqueue(id,()=>OnConnectionError?.Invoke("音声接続が切れました。もう一度お試しください。"));}
        }
        void HandleServerEvent(string json)
        {
            JObject e;try{e=JObject.Parse(json);}catch{return;}
            string item=e.Value<string>("item_id")??"";
            switch(e.Value<string>("type"))
            {
                case "session.created":
                    _=Send(new JObject{["type"]="session.update",["session"]=new JObject{["type"]="realtime",["audio"]=new JObject{["input"]=new JObject{["transcription"]=new JObject{["model"]="whisper-1",["language"]="ja"},["turn_detection"]=new JObject{["type"]="server_vad",["create_response"]=false,["interrupt_response"]=false,["silence_duration_ms"]=900,["prefix_padding_ms"]=300}}}}});break;
                case "session.updated":
                    if(ready)break;ready=true;microphoneCapture.StartCapture();if(!microphoneCapture.IsCapturing)break;OnReady?.Invoke();break;
                case "response.output_audio.delta":
                    if(awaitingResponse){try{pcmPlayer.EnqueuePcm16(Convert.FromBase64String(e.Value<string>("delta")??""));}catch(FormatException){OnConnectionError?.Invoke("音声データを再生できませんでした。");}}break;
                case "response.output_audio_transcript.delta":if(awaitingResponse)OnInterviewerTextDelta?.Invoke(e.Value<string>("delta")??"");break;
                case "response.done":
                    if(!awaitingResponse)break;
                    if(e["response"]?.Value<string>("status")!="completed"){awaitingResponse=false;OnConnectionError?.Invoke("相手の発話を完了できませんでした。もう一度お試しください。");}
                    else responseDone=true;break;
                case "input_audio_buffer.speech_started":
                    if(listening){inputTurns[item]=inputEpoch;partial="";OnSpeechStarted?.Invoke();}break;
                case "input_audio_buffer.speech_stopped":if(CurrentInput(item))OnSpeechStopped?.Invoke();break;
                case "conversation.item.input_audio_transcription.delta":if(CurrentInput(item)){partial+=e.Value<string>("delta")??"";OnUserTranscriptDelta?.Invoke(partial);}break;
                case "conversation.item.input_audio_transcription.completed":
                    if(CurrentInput(item)&&completedItems.Add(item))OnUserTranscriptCompleted?.Invoke(e.Value<string>("transcript")??"");break;
                case "conversation.item.input_audio_transcription.failed":if(CurrentInput(item))OnInputError?.Invoke("音声を文字にできませんでした。");break;
                case "error":
                    Debug.LogWarning("RealtimeVoice service error code="+e["error"]?.Value<string>("code"));
                    OnConnectionError?.Invoke("音声サービスでエラーが起きました。もう一度お試しください。");break;
            }
        }
        bool CurrentInput(string id)=>listening&&inputTurns.TryGetValue(id,out int epoch)&&epoch==inputEpoch;
        public void Speak(string line)
        {
            if(!ready||!IsConnected){OnConnectionError?.Invoke("音声接続の準備ができていません。");return;}
            SetListening(false);awaitingResponse=true;responseDone=false;drainedFor=0;responseStarted=Time.unscaledTime;
            _=Send(new JObject{["type"]="response.create",["response"]=new JObject{["output_modalities"]=new JArray("audio"),["instructions"]="生まれたばかりのAI案内役AI-0（エーアイゼロ）という架空の案内役として、必ず日本語で話してください。渋い成熟した男性の低く深い声。胸に響く落ち着いた声色で、抑揚は控えめ、語尾は静かに下げ、句読点に自然な間を置き、少しゆっくり明瞭に話してください。威厳と穏やかな親しみを両立し、芝居がかった叫び・ささやき・笑い声は加えません。次の台詞だけをそのまま読み上げて止めてください。追加の質問・英語・説明は禁止。台詞："+line}});
        }
        public void SetListening(bool enabled)
        {
            InputSignal.Clear();listening=false;int epoch=++inputEpoch;inputTurns.Clear();completedItems.Clear();partial="";
            if(enabled&&ready&&IsConnected)_=OpenInput(generation,epoch);
        }
        async Task OpenInput(int id,int epoch)
        {
            bool sent=await Send(new JObject{["type"]="input_audio_buffer.clear"});
            if(sent)Enqueue(id,()=>{if(inputEpoch==epoch)listening=true;});
        }
        void MicChunk(byte[] bytes)
        {
            OnMicLevel?.Invoke(Rms(bytes));if(!listening||!IsConnected)return;
            InputSignal.WritePcm16(bytes);
            _=Send(new JObject{["type"]="input_audio_buffer.append",["audio"]=Convert.ToBase64String(bytes)});
        }
        async Task<bool> Send(JObject message)
        {
            var connection=socket;var life=cancellation;int id=generation;
            if(connection==null||life==null||connection.State!=PlatformSocketState.Open)return false;
            CancellationToken token=life.Token;bool locked=false;
            try
            {
                await sendLock.WaitAsync(token);locked=true;if(id!=generation)return false;
                await connection.SendText(message.ToString(Newtonsoft.Json.Formatting.None));return true;
            }
            catch(OperationCanceledException){return false;}
            catch(Exception){Enqueue(id,()=>OnConnectionError?.Invoke("音声を送信できませんでした。接続を確認してください。"));return false;}
            finally{if(locked)sendLock.Release();}
        }
        static float Rms(byte[] bytes)
        {
            if(bytes.Length<2)return 0;double sum=0;for(int i=0;i+1<bytes.Length;i+=2){short s=(short)(bytes[i]|bytes[i+1]<<8);sum+=(double)s*s;}return Mathf.Clamp01((float)(Math.Sqrt(sum/(bytes.Length/2))/32768)*4);
        }
        public void Disconnect()
        {
            InputSignal.Clear();generation++;inputEpoch++;StopAllCoroutines();listening=ready=awaitingResponse=responseDone=false;inputTurns.Clear();completedItems.Clear();
            if(microphoneCapture)microphoneCapture.StopCapture();if(pcmPlayer)pcmPlayer.Clear();
            cancellation?.Cancel();cancellation?.Dispose();cancellation=null;
            try{_=socket?.Close();}catch{}socket=null;
        }
    }
}
