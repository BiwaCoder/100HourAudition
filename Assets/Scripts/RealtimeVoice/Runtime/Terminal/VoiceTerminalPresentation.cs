using TMPro;
using UnityEngine;
namespace HundredHour.RealtimeVoice
{
    public sealed class VoiceTerminalPresentation : MonoBehaviour
    {
        public RealtimeVoiceSession session;
        public PcmStreamPlayer player;
        public TerminalSpectrumGraphic spectrum;
        public UnityEngine.UI.RawImage noise;
        public bool holographicBackground, localizeText;
        string L(string text)=>localizeText?HundredHour.Localization.GameLanguage.Text(text):text;
        public TMP_Text signalLabel, formationText;
        public static readonly Color Blue = new Color(.22f, .55f, 1), Red = new Color(1, .25f, .32f), Purple = new Color(.65f, .42f, 1);
        readonly float[] systemBands = new float[64], userBands = new float[64];
        Texture2D noiseTexture;
        Color32[] pixels;
        float nextNoise, nextSpectrum;
        uint random = 19373;
        bool receivedVoice, listening;
        VoiceInterviewState state;
        VoiceSignalBuffer liveInput;
        public bool ReceivedVoice => receivedVoice;
        public Color SystemColor => receivedVoice ? Purple : Blue;
        public int RevealStage { get; private set; }
        void OnEnable() { if (session) session.OnUserTranscriptCompleted += VoiceReceived; }
        void OnDisable() { if (session) session.OnUserTranscriptCompleted -= VoiceReceived; }
        void VoiceReceived(string text) { if (!string.IsNullOrWhiteSpace(text)) receivedVoice = true; }
        public void PresentLive(string stage, bool received, VoiceSignalBuffer input)
        {
            liveInput=input;listening=true;receivedVoice=received;state=VoiceInterviewState.MicListening;
            RevealStage=stage=="greeting"?0:1;
        }
        public void Present(VoiceInterviewFlow flow)
        {
            liveInput=null;
            state = flow.State; listening = flow.Listening;
            if (state == VoiceInterviewState.Ready || state == VoiceInterviewState.Connecting || state == VoiceInterviewState.Error) receivedVoice = false;
            if (flow.Reviewing) VoiceReceived(flow.Candidate);
            RevealStage = state == VoiceInterviewState.Generating || state == VoiceInterviewState.Complete ? 2 : flow.QuestionNumber > 0 ? 1 : 0;
            if (formationText)
            {
                bool forming = state == VoiceInterviewState.Generating, done = state == VoiceInterviewState.Complete;
                formationText.gameObject.SetActive(forming || done);
                formationText.GetComponent<TerminalTextReveal>().Show(L(forming ? "声の記憶が、ひとつの輪郭になる。" : "ここから、新しい物語がはじまる。"), 2);
            }
        }
        void Update()
        {
            if (!spectrum) return;
            if (Time.unscaledTime >= nextSpectrum)
            {
                nextSpectrum = Time.unscaledTime + 1f / 30;
                float systemLevel = player ? player.OutputSignal.ReadBands(systemBands) : 0;
                float userLevel = listening ? (liveInput != null ? liveInput.ReadBands(userBands) : session ? session.InputSignal.ReadBands(userBands) : 0) : 0;
                bool user = listening && userLevel > .004f;
                var target = user ? Red : SystemColor;
                spectrum.SignalColor = Color.Lerp(spectrum.SignalColor, target, .24f);
                for (int i = 0; i < spectrum.Bands.Length; i++)
                {
                    float value = user ? userBands[i] : systemLevel > .001f ? systemBands[i] : 0;
                    spectrum.Bands[i] = Mathf.Lerp(spectrum.Bands[i], value, value > spectrum.Bands[i] ? .7f : .22f);
                }
                bool forming = state == VoiceInterviewState.Generating;
                spectrum.Formation = forming ? .65f + .15f * Mathf.Sin(Time.unscaledTime * 1.8f) : state == VoiceInterviewState.Complete ? .8f : 0;
                spectrum.SetVerticesDirty();
                signalLabel.text = user ? "INPUT / あなたの声" : systemLevel > .001f ? "OUTPUT / AI-0" : forming ? "FORMING / 輪郭を編んでいます" : state == VoiceInterviewState.Complete ? "CREATED / 新しいキャラクター" : listening ? "INPUT / 声を待っています" : receivedVoice ? "LINK / 声の記憶を受信" : "SIGNAL / 待機中";
                signalLabel.text=L(signalLabel.text);
                signalLabel.color = user ? Red : SystemColor;
            }
            if (holographicBackground || !noise || Time.unscaledTime < nextNoise) return;
            nextNoise = Time.unscaledTime + 1f / 12;
            if (!noiseTexture)
            {
                noiseTexture = new Texture2D(240, 135, TextureFormat.RGBA32, false) { name = "Terminal grain (runtime)", filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Repeat };
                pixels = new Color32[240 * 135]; noise.texture = noiseTexture;
            }
            for (int i = 0; i < pixels.Length; i++)
            {
                random ^= random << 13; random ^= random >> 17; random ^= random << 5;
                byte value = (byte)(3 + (random & 15));
                pixels[i] = new Color32(value, value, value, 255);
            }
            noiseTexture.SetPixels32(pixels); noiseTexture.Apply(false, false);
            noise.uvRect = new Rect(Time.unscaledTime * .009f, 0, 2.5f, 2.5f);
        }
        void OnDestroy() { if (noiseTexture) Destroy(noiseTexture); }
    }
}
