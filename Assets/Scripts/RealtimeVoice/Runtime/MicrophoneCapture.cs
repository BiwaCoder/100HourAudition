using System;
using UnityEngine;

namespace HundredHour.RealtimeVoice
{
    /// <summary>マイクからPCM16 mono 24kHzのチャンクを一定間隔で切り出して通知する。
    /// Realtime APIが要求するフォーマット(audio/pcm, 24000Hz, 16bit, mono)に合わせてリサンプリングする。
    /// WebGLビルドではUnityEngine.Microphoneが存在しないため、Assets/Plugins/WebGL/MicrophoneCapture.jslib
    /// 経由でgetUserMedia+Web Audioを使う(MicrophoneCaptureWebGL.cs)。どちらの経路でも公開イベント/APIは同じ。</summary>
    [AddComponentMenu("RealtimeVoice/Microphone Capture")]
    public sealed class MicrophoneCapture : MonoBehaviour
    {
        public const int TargetSampleRate = 24000;

        [SerializeField] string deviceName; // 空ならデフォルトデバイス(WebGLでは常にブラウザ既定のマイク)
        [SerializeField] float chunkIntervalSeconds = 0.1f;

        public event Action<byte[]> OnPcm16Chunk;
        public event Action<string> OnCaptureError;
        bool capturing;

        public bool IsCapturing => capturing;

        public void StartCapture()
        {
            if (capturing) return;
#if UNITY_WEBGL && !UNITY_EDITOR
            capturing = true;
            MicrophoneCaptureWebGL.Start(this);
#else
            StartCaptureNative();
#endif
        }

        public void StopCapture()
        {
            if (!capturing) return;
            capturing = false;
#if UNITY_WEBGL && !UNITY_EDITOR
            MicrophoneCaptureWebGL.Stop();
#else
            StopCaptureNative();
#endif
        }

        void OnDisable() => StopCapture();

#if UNITY_WEBGL && !UNITY_EDITOR
        int webglSampleRate;

        // Called by MicrophoneCaptureWebGL (same assembly) from the jslib callbacks.
        internal void WebGLCaptureStarted(int sampleRate) => webglSampleRate = sampleRate;

        internal void WebGLCaptureChunk(float[] samples)
        {
            byte[] pcm16 = ResampleAndConvert(samples, webglSampleRate, TargetSampleRate);
            OnPcm16Chunk?.Invoke(pcm16);
        }

        internal void WebGLCaptureError(string message)
        {
            capturing = false;
            OnCaptureError?.Invoke(message ?? "マイクを開始できませんでした。");
        }
#else
        string activeDevice;
        float startedAt;
        bool receivedSamples;

        AudioClip clip;
        int lastSamplePos;
        int nativeSampleRate;
        float timer;

        string Device => string.IsNullOrEmpty(deviceName) ? Microphone.devices[0] : deviceName;

        void StartCaptureNative()
        {
            if (Microphone.devices.Length == 0)
            {
                OnCaptureError?.Invoke("マイクが見つかりません。接続してからやり直してください。");
                return;
            }

            activeDevice=Device;
            if(Array.IndexOf(Microphone.devices,activeDevice)<0){OnCaptureError?.Invoke("選択したマイクが見つかりません。");return;}
            Microphone.GetDeviceCaps(activeDevice, out int minFreq, out int maxFreq);
            nativeSampleRate = maxFreq > 0 ? Mathf.Clamp(48000,Mathf.Max(1,minFreq),maxFreq) : 48000;
            try{clip = Microphone.Start(activeDevice, true, 10, nativeSampleRate);}catch(Exception){OnCaptureError?.Invoke("マイクを開始できません。OSのマイク許可を確認してください。");return;}
            if(!clip){OnCaptureError?.Invoke("マイクを開始できませんでした。");return;}
            nativeSampleRate=clip.frequency;startedAt=Time.unscaledTime;receivedSamples=false;
            lastSamplePos = 0;
            timer = 0f;
            capturing = true;
        }

        void StopCaptureNative()
        {
            Microphone.End(activeDevice);
            if(clip)Destroy(clip);clip = null;
        }

        void Update()
        {
            if (!capturing || clip == null) return;

            timer += Time.unscaledDeltaTime;
            if (timer < chunkIntervalSeconds) return;
            timer = 0f;

            if(Array.IndexOf(Microphone.devices,activeDevice)<0){StopCapture();OnCaptureError?.Invoke("マイクが外れました。接続してやり直してください。");return;}
            int pos = Microphone.GetPosition(activeDevice);
            if(pos<0||(!receivedSamples&&Time.unscaledTime-startedAt>5&&pos==0)){StopCapture();OnCaptureError?.Invoke("マイク入力を開始できませんでした。OSの設定を確認してください。");return;}
            int available = pos - lastSamplePos;
            if (available < 0) available += clip.samples;
            if (available <= 0) return;

            var interleaved=new float[available*clip.channels];
            clip.GetData(interleaved,lastSamplePos);
            var samples = new float[available];
            for(int i=0;i<available;i++){for(int ch=0;ch<clip.channels;ch++)samples[i]+=interleaved[i*clip.channels+ch];samples[i]/=clip.channels;}
            receivedSamples=true;
            lastSamplePos = (lastSamplePos + available) % clip.samples;

            byte[] pcm16 = ResampleAndConvert(samples, nativeSampleRate, TargetSampleRate);
            OnPcm16Chunk?.Invoke(pcm16);
        }
#endif

        static byte[] ResampleAndConvert(float[] samples, int fromRate, int toRate)
        {
            if (fromRate == toRate)
            {
                var same = new byte[samples.Length * 2];
                for (int i = 0; i < samples.Length; i++) WriteSample(same, i, samples[i]);
                return same;
            }

            float ratio = (float)fromRate / toRate;
            int outLength = Mathf.Max(0, Mathf.FloorToInt((samples.Length - 1) / ratio));
            var result = new byte[outLength * 2];
            for (int i = 0; i < outLength; i++)
            {
                float srcIndex = i * ratio;
                int idx = Mathf.FloorToInt(srcIndex);
                float frac = srcIndex - idx;
                float s0 = samples[Mathf.Clamp(idx, 0, samples.Length - 1)];
                float s1 = samples[Mathf.Clamp(idx + 1, 0, samples.Length - 1)];
                WriteSample(result, i, Mathf.Lerp(s0, s1, frac));
            }
            return result;
        }

        static void WriteSample(byte[] buffer, int index, float sample)
        {
            short pcm = (short)Mathf.Clamp(sample * 32767f, short.MinValue, short.MaxValue);
            buffer[index * 2] = (byte)(pcm & 0xFF);
            buffer[index * 2 + 1] = (byte)((pcm >> 8) & 0xFF);
        }
    }
}
