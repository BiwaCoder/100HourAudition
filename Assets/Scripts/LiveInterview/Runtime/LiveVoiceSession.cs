using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;
using HundredHour.Net;
using HundredHour.RealtimeVoice;
namespace HundredHour.LiveInterview
{
    public sealed class LiveVoiceSession : MonoBehaviour
    {
        string relayUrl;
        public MicrophoneCapture microphone;
        public PcmStreamPlayer player;
        public VoiceSignalBuffer InputSignal {get;}=new VoiceSignalBuffer();
        public event Action<JObject> EventReceived;
        public event Action<string> Failed;
        public string LanguageCode {get;set;}="ja";
        // Lets other relays (e.g. the finale conversation) reuse this session instead of
        // duplicating the connect/mic/playback plumbing. Defaults reproduce the original
        // character-creation-interview behavior exactly.
        public string EndpointPath="/api/live-interview";
        public string ExtraQuery="";
        public string GenderOverride="";
        public bool IsConnected=>socket!=null&&socket.State==PlatformSocketState.Open;
        public bool Ready {get;private set;}
        readonly ConcurrentQueue<Action> callbacks=new ConcurrentQueue<Action>();
        readonly SemaphoreSlim sending=new SemaphoreSlim(1,1);
        IPlatformSocket socket;
        CancellationTokenSource lifetime;
        int generation,pending;
        float began;
        bool finishing;
        // relayUrl is built in Connect(), and the mic events are (un)wired in Connect()/Disconnect()
        // rather than OnEnable()/OnDisable(): AddComponent() enables the object (running OnEnable)
        // immediately, before a caller gets a chance to assign microphone/player/EndpointPath.
        void OnDisable()=>Disconnect();
        void OnApplicationQuit()=>Disconnect();
        void Update()
        {
            while(callbacks.TryDequeue(out var callback))callback();
            if(lifetime!=null&&Time.realtimeSinceStartup-began>(Ready?127:25))Error("接続の制限時間に達したため停止しました。");
        }
        void Queue(int revision,Action action)=>callbacks.Enqueue(()=>{if(revision==generation&&isActiveAndEnabled)action();});
        public void Connect()
        {
            Disconnect();
            microphone.OnPcm16Chunk+=Mic;microphone.OnCaptureError+=Error;
            relayUrl=ApiEndpointConfig.Instance.Ws+EndpointPath;began=Time.realtimeSinceStartup;lifetime=new CancellationTokenSource();
            StartCoroutine(Authorize(generation,lifetime.Token));
        }
#if !UNITY_WEBGL || UNITY_EDITOR
        IEnumerator Authorize(int revision,CancellationToken token)
        {
            if(!Application.HasUserAuthorization(UserAuthorization.Microphone))yield return Application.RequestUserAuthorization(UserAuthorization.Microphone);
            if(revision!=generation)yield break;
            if(!Application.HasUserAuthorization(UserAuthorization.Microphone)){Error("マイクの使用を許可してください。");yield break;}
            _=Receive(revision,token);
        }
#else
        // WebGL has no Microphone-backed permission query; getUserMedia's own promise rejection
        // (surfaced via MicrophoneCapture.OnCaptureError) is the permission gate there.
        IEnumerator Authorize(int revision,CancellationToken token)
        {
            _=Receive(revision,token);
            yield break;
        }
#endif
        async Task Receive(int revision,CancellationToken token)
        {
            bool terminal=false;
            string gender=string.IsNullOrEmpty(GenderOverride)?HundredHour.AuditionEntry.AuditionProfile.Gender:GenderOverride;
            string url=relayUrl+(relayUrl.Contains("?")?"&":"?")+"language="+Uri.EscapeDataString(LanguageCode)+"&gender="+Uri.EscapeDataString(gender)+ExtraQuery+ApiAuth.QuerySuffix();
            var connection=PlatformSocket.Create(url);socket=connection;
            connection.OnMessage+=text=>{
                try
                {
                    var e=JObject.Parse(text);
                    if((string)e["type"]=="live.finish"||(string)e["type"]=="live.error")terminal=true;
                    Queue(revision,()=>Handle(e));
                }
                catch(Exception){Queue(revision,()=>Error("音声メッセージを読み取れませんでした。"));}
            };
            connection.OnClose+=_=>{if(!terminal)Queue(revision,()=>Error("音声接続が終了しました。もう一度お試しください。"));};
            connection.OnError+=msg=>Queue(revision,()=>Error("GPT-Liveとの接続が切れました。PythonAPIを確認してください。"));
            try
            {
                var connectTask=connection.Connect();
                if(await Task.WhenAny(connectTask,Task.Delay(20000,token))!=connectTask||token.IsCancellationRequested)
                {
                    if(!token.IsCancellationRequested)Queue(revision,()=>Error("接続がタイムアウトしました。"));
                    return;
                }
                await connectTask;
            }
            catch(OperationCanceledException){if(!token.IsCancellationRequested)Queue(revision,()=>Error("接続がタイムアウトしました。"));}
            catch(Exception){Queue(revision,()=>Error("GPT-Liveとの接続が切れました。PythonAPIを確認してください。"));}
        }
        void Handle(JObject e)
        {
            switch((string)e["type"])
            {
                case "live.ready": began=Time.realtimeSinceStartup;Ready=true;microphone.StartCapture();if(!microphone.IsCapturing)return;break;
                case "live.audio":
                    if(!finishing)player.EnqueuePcm16(Convert.FromBase64String((string)e["delta"]??""));return;
                case "live.error": Error(HundredHour.Localization.GameLanguage.Text((string)e["message"])+" ("+(string)e["code"]+")");return;
                case "live.finish": microphone.StopCapture();InputSignal.Clear();player.Clear();break;
            }
            EventReceived?.Invoke(e);
        }
        void Mic(byte[] bytes)
        {
            if(!Ready||finishing||!IsConnected)return;
            InputSignal.WritePcm16(bytes);
            _=Send(new JObject{["type"]="audio",["audio"]=Convert.ToBase64String(bytes),["playback_pending"]=player.HasPendingAudio});
        }
        async Task Send(JObject message)
        {
            var connection=socket;var life=lifetime;int revision=generation;
            if(connection==null||life==null||connection.State!=PlatformSocketState.Open)return;
            var token=life.Token;bool locked=false;
            if(Interlocked.Increment(ref pending)>8){Interlocked.Decrement(ref pending);Queue(revision,()=>Error("音声送信が追いつかないため接続を停止しました。"));return;}
            try
            {
                await sending.WaitAsync(token);locked=true;if(revision!=generation)return;
                await connection.SendText(message.ToString(Newtonsoft.Json.Formatting.None));
            }
            catch(OperationCanceledException){}
            catch(Exception){Queue(revision,()=>Error("音声を送信できませんでした。"));}
            finally{Interlocked.Decrement(ref pending);if(locked)sending.Release();}
        }
        public void SelectTopic(string id){if(Ready&&!finishing&&IsConnected)_=Send(new JObject{["type"]="topic.select",["id"]=id});}
        public void Finish(){if(!Ready||finishing)return;_ = Send(new JObject{["type"]="finish"});}
        void Error(string message){Disconnect();Failed?.Invoke(HundredHour.Localization.GameLanguage.Text(message));}
        public void Disconnect()
        {
            generation++;Ready=false;finishing=false;StopAllCoroutines();
            if(microphone){microphone.StopCapture();microphone.OnPcm16Chunk-=Mic;microphone.OnCaptureError-=Error;}
            if(player)player.Clear();InputSignal.Clear();
            lifetime?.Cancel();lifetime?.Dispose();lifetime=null;
            try{_=socket?.Close();}catch{}socket=null;
            // Relay observes socket loss and finalizes upstream, even when Unity has stopped.
        }
    }
}
