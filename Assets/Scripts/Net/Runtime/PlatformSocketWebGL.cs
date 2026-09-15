#if UNITY_WEBGL && !UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AOT;

namespace HundredHour.Net
{
    /// <summary>WebGLビルド向け。Assets/Plugins/WebGL/PlatformSocket.jslibを介してブラウザ標準の
    /// WebSocketを操作する。テキストフレーム専用(既存コードは常にJSONを送受信するため十分)。</summary>
    sealed class PlatformSocketWebGL : IPlatformSocket
    {
        [DllImport("__Internal")] static extern int PlatformSocketAllocate(string url);
        [DllImport("__Internal")] static extern void PlatformSocketAddSubProtocol(int id, string subprotocol);
        [DllImport("__Internal")] static extern int PlatformSocketConnect(int id);
        [DllImport("__Internal")] static extern int PlatformSocketSendText(int id, string message);
        [DllImport("__Internal")] static extern int PlatformSocketClose(int id, int code);
        [DllImport("__Internal")] static extern int PlatformSocketGetState(int id);
        [DllImport("__Internal")] static extern void PlatformSocketFree(int id);
        [DllImport("__Internal")] static extern void PlatformSocketSetCallbacks(OnOpenDelegate onOpen, OnMessageDelegate onMessage, OnErrorDelegate onError, OnCloseDelegate onClose);

        delegate void OnOpenDelegate(int id);
        delegate void OnMessageDelegate(int id, IntPtr messagePtr);
        delegate void OnErrorDelegate(int id, IntPtr errorPtr);
        delegate void OnCloseDelegate(int id, int code);

        static readonly Dictionary<int, PlatformSocketWebGL> instances = new Dictionary<int, PlatformSocketWebGL>();
        static bool callbacksRegistered;

        readonly int id;

        public event Action OnOpen;
        public event Action<string> OnMessage;
        public event Action<string> OnError;
        public event Action<int> OnClose;

        public PlatformSocketWebGL(string url, IList<string> subprotocols)
        {
            if (!callbacksRegistered)
            {
                PlatformSocketSetCallbacks(HandleOpen, HandleMessage, HandleError, HandleClose);
                callbacksRegistered = true;
            }
            id = PlatformSocketAllocate(url);
            instances[id] = this;
            if (subprotocols != null)
                foreach (var protocol in subprotocols) PlatformSocketAddSubProtocol(id, protocol);
        }

        public PlatformSocketState State => PlatformSocketGetState(id) switch
        {
            0 => PlatformSocketState.Connecting,
            1 => PlatformSocketState.Open,
            2 => PlatformSocketState.Closing,
            _ => PlatformSocketState.Closed,
        };

        public Task Connect() { PlatformSocketConnect(id); return Task.CompletedTask; }
        public Task SendText(string message) { PlatformSocketSendText(id, message); return Task.CompletedTask; }
        public Task Close()
        {
            PlatformSocketClose(id, 1000);
            instances.Remove(id);
            PlatformSocketFree(id);
            return Task.CompletedTask;
        }

        [MonoPInvokeCallback(typeof(OnOpenDelegate))]
        static void HandleOpen(int id) { if (instances.TryGetValue(id, out var s)) s.OnOpen?.Invoke(); }

        [MonoPInvokeCallback(typeof(OnMessageDelegate))]
        static void HandleMessage(int id, IntPtr messagePtr) { if (instances.TryGetValue(id, out var s)) s.OnMessage?.Invoke(Marshal.PtrToStringUTF8(messagePtr)); }

        [MonoPInvokeCallback(typeof(OnErrorDelegate))]
        static void HandleError(int id, IntPtr errorPtr) { if (instances.TryGetValue(id, out var s)) s.OnError?.Invoke(Marshal.PtrToStringUTF8(errorPtr)); }

        [MonoPInvokeCallback(typeof(OnCloseDelegate))]
        static void HandleClose(int id, int code) { if (instances.TryGetValue(id, out var s)) { s.OnClose?.Invoke(code); instances.Remove(id); } }
    }
}
#endif
