using UnityEngine;
using HundredHour.RealtimeVoice;
namespace HundredHour.LiveInterview
{
    // A single low contrast GPU pass. Unscaled time keeps the drift independent of gameplay.
    public sealed class HologramBackground : MonoBehaviour
    {
        public VoiceTerminalPresentation terminal;
        public Shader shader;
        Material material, previousMaterial;
        Texture previousTexture;
        Rect previousUV;
        void OnEnable()
        {
            if(!terminal || !terminal.noise || !shader)return;
            var target=terminal.noise;
            previousMaterial=target.material;previousTexture=target.texture;previousUV=target.uvRect;
            material=new Material(shader){name="Kumono hologram (runtime)"};
            target.material=material;target.texture=Texture2D.whiteTexture;target.uvRect=new Rect(0,0,1,1);
            terminal.holographicBackground=true;
        }
        void Update(){if(material)material.SetFloat("_FlowTime",Time.unscaledTime);}
        void OnDisable()
        {
            if(terminal && terminal.noise)
            {
                terminal.noise.material=previousMaterial;terminal.noise.texture=previousTexture;
                terminal.noise.uvRect=previousUV;terminal.holographicBackground=false;
            }
            if(material)Destroy(material);
        }
    }
}
