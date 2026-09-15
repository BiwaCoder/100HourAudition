#if UNITY_WEBGL && !UNITY_EDITOR
using System;
using System.Runtime.InteropServices;
using AOT;

namespace HundredHour.RealtimeVoice
{
    /// <summary>WebGLビルド向けマイク入力ブリッジ。Assets/Plugins/WebGL/MicrophoneCapture.jslibを介して
    /// getUserMedia+Web Audioを操作する。ブラウザは同時に1本のキャプチャしか想定しない(このアプリも
    /// 実際に同時に複数のMicrophoneCaptureを使わない)ため、シングルトンで束ねる。</summary>
    static class MicrophoneCaptureWebGL
    {
        [DllImport("__Internal")] static extern void MicCaptureSetCallbacks(OnStartedDelegate onStarted, OnChunkDelegate onChunk, OnErrorDelegate onError);
        [DllImport("__Internal")] static extern void MicCaptureStart();
        [DllImport("__Internal")] static extern void MicCaptureStop();
        [DllImport("__Internal")] static extern int MicCaptureIsCapturing();

        delegate void OnStartedDelegate(int sampleRate);
        delegate void OnChunkDelegate(IntPtr floatsPtr, int sampleCount);
        delegate void OnErrorDelegate(IntPtr messagePtr);

        static bool callbacksRegistered;
        static MicrophoneCapture active;

        public static bool IsCapturing => MicCaptureIsCapturing() != 0;

        public static void Start(MicrophoneCapture owner)
        {
            if (!callbacksRegistered)
            {
                MicCaptureSetCallbacks(HandleStarted, HandleChunk, HandleError);
                callbacksRegistered = true;
            }
            active = owner;
            MicCaptureStart();
        }

        public static void Stop()
        {
            MicCaptureStop();
            active = null;
        }

        [MonoPInvokeCallback(typeof(OnStartedDelegate))]
        static void HandleStarted(int sampleRate) => active?.WebGLCaptureStarted(sampleRate);

        [MonoPInvokeCallback(typeof(OnChunkDelegate))]
        static void HandleChunk(IntPtr floatsPtr, int sampleCount)
        {
            if (active == null) return;
            var samples = new float[sampleCount];
            Marshal.Copy(floatsPtr, samples, 0, sampleCount);
            active.WebGLCaptureChunk(samples);
        }

        [MonoPInvokeCallback(typeof(OnErrorDelegate))]
        static void HandleError(IntPtr messagePtr) => active?.WebGLCaptureError(Marshal.PtrToStringUTF8(messagePtr));
    }
}
#endif
