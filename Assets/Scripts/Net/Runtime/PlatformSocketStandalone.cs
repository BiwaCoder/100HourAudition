#if !UNITY_WEBGL || UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HundredHour.Net
{
    /// <summary>Standalone/Editor向け。既存コードのClientWebSocket直叩きと同じ挙動
    /// (バックグラウンドのReceiveAsyncループでOnMessageを発火、呼び出し側が独自のQueue()で
    /// メインスレッドへ戻す)を維持する。</summary>
    sealed class PlatformSocketStandalone : IPlatformSocket
    {
        readonly Uri uri;
        readonly IList<string> subprotocols;
        readonly ClientWebSocket socket = new ClientWebSocket();
        readonly CancellationTokenSource cancellation = new CancellationTokenSource();

        public event Action OnOpen;
        public event Action<string> OnMessage;
        public event Action<string> OnError;
        public event Action<int> OnClose;

        public PlatformSocketStandalone(string url, IList<string> subprotocols)
        {
            uri = new Uri(url);
            this.subprotocols = subprotocols;
        }

        public PlatformSocketState State => socket.State switch
        {
            WebSocketState.Connecting => PlatformSocketState.Connecting,
            WebSocketState.Open => PlatformSocketState.Open,
            WebSocketState.CloseSent or WebSocketState.CloseReceived => PlatformSocketState.Closing,
            _ => PlatformSocketState.Closed,
        };

        public async Task Connect()
        {
            if (subprotocols != null)
                foreach (var protocol in subprotocols) socket.Options.AddSubProtocol(protocol);
            try
            {
                await socket.ConnectAsync(uri, cancellation.Token);
                OnOpen?.Invoke();
                _ = Receive();
            }
            catch (Exception e)
            {
                OnError?.Invoke(e.Message);
                OnClose?.Invoke((int)WebSocketCloseStatus.EndpointUnavailable);
            }
        }

        async Task Receive()
        {
            var bytes = new byte[16384];
            try
            {
                while (socket.State == WebSocketState.Open && !cancellation.IsCancellationRequested)
                {
                    using var message = new System.IO.MemoryStream();
                    WebSocketReceiveResult result;
                    do
                    {
                        result = await socket.ReceiveAsync(new ArraySegment<byte>(bytes), cancellation.Token);
                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            OnClose?.Invoke((int)(result.CloseStatus ?? WebSocketCloseStatus.Empty));
                            return;
                        }
                        message.Write(bytes, 0, result.Count);
                    }
                    while (!result.EndOfMessage);
                    OnMessage?.Invoke(Encoding.UTF8.GetString(message.ToArray()));
                }
            }
            catch (Exception e)
            {
                if (!cancellation.IsCancellationRequested) OnError?.Invoke(e.Message);
                OnClose?.Invoke((int)WebSocketCloseStatus.Empty);
            }
        }

        public async Task SendText(string message)
        {
            if (socket.State != WebSocketState.Open) return;
            byte[] bytes = Encoding.UTF8.GetBytes(message);
            await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, cancellation.Token);
        }

        public async Task Close()
        {
            cancellation.Cancel();
            if (socket.State == WebSocketState.Open)
            {
                try { await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None); }
                catch (Exception) { /* peer may already be gone */ }
            }
            socket.Dispose();
        }
    }
}
#endif
