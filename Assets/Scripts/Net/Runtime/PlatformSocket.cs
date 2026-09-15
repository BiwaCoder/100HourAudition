using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HundredHour.Net
{
    /// <summary>WebGLのブラウザは生ソケットにもカスタムヘッダーにも対応しない(WebSocketはURL/サブプロトコルのみ)ため、
    /// System.Net.WebSockets.ClientWebSocketを直接使うコードはWebGLビルドで動かない。この抽象化で
    /// Standalone/Editorはこれまで通りClientWebSocket、WebGLビルドはブラウザ標準WebSocket(jslib経由)を使う。
    /// テキストフレーム(JSON)のみ対応。既存コードが受け渡すデータは常にJSON文字列のため十分。</summary>
    public enum PlatformSocketState { Connecting, Open, Closing, Closed }

    public interface IPlatformSocket
    {
        event Action OnOpen;
        event Action<string> OnMessage;
        event Action<string> OnError;
        event Action<int> OnClose;
        PlatformSocketState State { get; }
        Task Connect();
        Task Close();
        Task SendText(string message);
    }

    public static class PlatformSocket
    {
        /// <summary>subprotocolsはWebGLのブラウザWebSocketが唯一ハンドシェイクに載せられる認証手段
        /// (カスタムヘッダー不可)。Standalone/Editor側もClientWebSocket.Options.AddSubProtocolで
        /// 同じ値を載せるため、呼び出し側は両プラットフォームで同じ書き方にできる。</summary>
        public static IPlatformSocket Create(string url, IList<string> subprotocols = null)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return new PlatformSocketWebGL(url, subprotocols);
#else
            return new PlatformSocketStandalone(url, subprotocols);
#endif
        }
    }
}
