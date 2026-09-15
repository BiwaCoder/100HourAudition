using System.Collections.Generic;
using UnityEngine;
#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif
namespace HundredHour.RealtimeVoice
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class PcmStreamPlayer : MonoBehaviour
    {
        public const int SampleRate=24000;
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] static extern void HHVoiceInit();
        [DllImport("__Internal")] static extern int HHVoiceIsRunning();
        [DllImport("__Internal")] static extern void HHVoiceUnlock();
        public static bool AudioReady => HHVoiceIsRunning() != 0;
        // Call from a user gesture (a button press) right before a voice session starts.
        public static void Unlock() => HHVoiceUnlock();
        [DllImport("__Internal")] static extern int HHVoiceCreate();
        [DllImport("__Internal")] static extern void HHVoiceEnqueue(int id, byte[] pcm, int length);
        [DllImport("__Internal")] static extern int HHVoiceQueuedSamples(int id);
        [DllImport("__Internal")] static extern int HHVoiceReadSignal(int id, [Out] float[] samples);
        [DllImport("__Internal")] static extern void HHVoiceClear(int id);
        [DllImport("__Internal")] static extern void HHVoiceDestroy(int id);
        int webPlayer;
        readonly float[] webSignal = new float[1024];
        // Arm gesture listeners before the title screen, even if voice is in a later scene.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void InitializeWebAudio() => HHVoiceInit();
        public VoiceSignalBuffer OutputSignal { get; } = new VoiceSignalBuffer();
        public bool HasPendingAudio => QueuedSamples > 0;
        public int QueuedSamples => webPlayer == 0 ? 0 : HHVoiceQueuedSamples(webPlayer);
        void Awake() { webPlayer = HHVoiceCreate(); }
        public void EnqueuePcm16(byte[] pcm) { if (webPlayer != 0 && pcm != null) HHVoiceEnqueue(webPlayer, pcm, pcm.Length); }
        public void Clear() { if (webPlayer != 0) HHVoiceClear(webPlayer); OutputSignal.Clear(); }
        void Update()
        {
            if (webPlayer == 0) return;
            int rate = HHVoiceReadSignal(webPlayer, webSignal);
            OutputSignal.WriteOutput(webSignal, 1, rate);
        }
        void OnDisable() => Clear();
        void OnDestroy() { if (webPlayer != 0) HHVoiceDestroy(webPlayer); webPlayer = 0; }
#else
        public static bool AudioReady => true;
        public static void Unlock() {}
        readonly Queue<float> buffer=new Queue<float>();
        readonly object gate=new object();
        AudioClip silentClip;
        int outputRate;
        public VoiceSignalBuffer OutputSignal { get; } = new VoiceSignalBuffer();
        public bool HasPendingAudio {get{lock(gate)return buffer.Count>0;}}
        public int QueuedSamples {get{lock(gate)return buffer.Count;}}
        void Awake()
        {
            outputRate=AudioSettings.outputSampleRate;
            var source=GetComponent<AudioSource>();source.spatialBlend=0;
            silentClip=AudioClip.Create("RealtimeVoiceStream",outputRate,1,outputRate,false);source.clip=silentClip;source.loop=true;source.Play();
            AudioSettings.OnAudioConfigurationChanged+=ConfigurationChanged;
        }
        void ConfigurationChanged(bool changed){lock(gate){buffer.Clear();outputRate=AudioSettings.outputSampleRate;}}
        public void EnqueuePcm16(byte[] pcm)
        {
            int count=pcm.Length/2;if(count==0)return;
            // OnAudioFilterRead runs at the output device rate, NOT the clip's 24 kHz rate.
            lock(gate)
            {
                int length=Mathf.RoundToInt((float)count*outputRate/SampleRate);
                for(int i=0;i<length;i++)
                {
                    float p=(float)i*SampleRate/outputRate;int left=Mathf.Min((int)p,count-1),right=Mathf.Min(left+1,count-1);
                    short a=(short)(pcm[left*2]|pcm[left*2+1]<<8),b=(short)(pcm[right*2]|pcm[right*2+1]<<8);
                    buffer.Enqueue(Mathf.Lerp(a,b,p-left)/32768f);
                }
            }
        }
        public void Clear(){lock(gate)buffer.Clear();OutputSignal.Clear();}
        void OnAudioFilterRead(float[] data,int channels)
        {
            lock(gate)for(int i=0;i<data.Length;i+=channels){float value=buffer.Count>0?buffer.Dequeue():0;for(int c=0;c<channels;c++)data[i+c]=value;}
            OutputSignal.WriteOutput(data,channels,outputRate);
        }
        void OnDestroy(){AudioSettings.OnAudioConfigurationChanged-=ConfigurationChanged;Clear();if(silentClip)Destroy(silentClip);}
#endif
    }
}
