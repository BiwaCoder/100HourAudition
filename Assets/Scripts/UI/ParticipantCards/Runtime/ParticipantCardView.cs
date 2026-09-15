using System.Globalization;
using TMPro;
using UnityEngine;

namespace HundredHour.UI.Participants
{
    /// <summary>表示専用。写真の色変更はカード専用Materialで行い、元Spriteを変更しない。</summary>
    public sealed class ParticipantCardView : MonoBehaviour
    {
        [SerializeField] UnityEngine.UI.Image portrait;
        [SerializeField] UnityEngine.UI.AspectRatioFitter portraitFit;
        [SerializeField] Sprite fallbackPortrait;
        [SerializeField] Material portraitMaterial;
        [SerializeField] TMP_Text nameLabel, subtitleLabel, numberLabel, starsLabel, supportLabel, statusLabel;
        [SerializeField] RoundedCardGraphic statusPill, supportFill;
        [SerializeField] GameObject outStamp;
        Material ownedMaterial;
        public UnityEngine.UI.Image Portrait => portrait;
        public bool OutVisible => outStamp != null && outStamp.activeSelf;

        public void Configure(UnityEngine.UI.Image image, UnityEngine.UI.AspectRatioFitter fit, Sprite fallback, Material material,
            TMP_Text name, TMP_Text subtitle, TMP_Text number, TMP_Text stars, TMP_Text support, TMP_Text status,
            RoundedCardGraphic pill, RoundedCardGraphic fill, GameObject stamp)
        {
            portrait=image; portraitFit=fit; fallbackPortrait=fallback; portraitMaterial=material;
            nameLabel=name; subtitleLabel=subtitle; numberLabel=number; starsLabel=stars;
            supportLabel=support; statusLabel=status; statusPill=pill; supportFill=fill; outStamp=stamp;
        }
        public void Render(ParticipantCardData data)
        {
            if (data==null || portrait==null) return;
            portrait.sprite=data.portrait!=null ? data.portrait : fallbackPortrait;
            if (portrait.sprite!=null) portraitFit.aspectRatio=portrait.sprite.rect.width/portrait.sprite.rect.height;
            if (ownedMaterial==null && portraitMaterial!=null)
            {
                ownedMaterial=new Material(portraitMaterial) { name="Participant portrait (instance)", hideFlags=HideFlags.HideAndDontSave };
                portrait.material=ownedMaterial;
            }
            if (ownedMaterial!=null)
            {
                ownedMaterial.SetFloat("_Grayscale",data.eliminated ? 1 : 0);
                ownedMaterial.SetFloat("_Dim",data.eliminated ? .68f : 1);
                // Mask生成済みのStencil materialにも値を渡す。
                var rendered=portrait.materialForRendering;
                rendered.SetFloat("_Grayscale",data.eliminated ? 1 : 0);
                rendered.SetFloat("_Dim",data.eliminated ? .68f : 1);
                portrait.SetMaterialDirty();
            }
            nameLabel.text=data.displayName ?? "";
            subtitleLabel.text=data.subtitle ?? "";
            numberLabel.text="#"+Mathf.Max(0,data.number);
            starsLabel.text=FormatStars(data.stars)+" STARS";
            supportLabel.text=Mathf.RoundToInt(Mathf.Clamp01(data.support)*100)+"%";
            statusLabel.text=data.eliminated ? "ELIMINATED" : data.status ?? "";
            Color pill=data.eliminated ? new Color(.67f,.71f,.75f,.92f) : data.accent;
            statusPill.SetStyle(pill,pill,20);
            var fillColor=data.eliminated ? new Color(.68f,.72f,.78f) : data.accent;
            supportFill.SetStyle(fillColor,fillColor,3);
            var rt=supportFill.rectTransform;
            rt.anchorMax=new Vector2(Mathf.Clamp01(data.support),1);
            rt.offsetMin=rt.offsetMax=Vector2.zero;
            supportFill.gameObject.SetActive(data.support>0);
            outStamp.SetActive(data.eliminated);
        }
        public static string FormatStars(long value)
        {
            value=System.Math.Max(0,value);
            if(value>=1000000000) return (value/1000000000d).ToString("0.#",CultureInfo.InvariantCulture)+"B";
            if(value>=1000000) return (value/1000000d).ToString("0.#",CultureInfo.InvariantCulture)+"M";
            if(value>=1000) return (value/1000d).ToString("0.#",CultureInfo.InvariantCulture)+"K";
            return value.ToString(CultureInfo.InvariantCulture);
        }
        void OnDestroy()
        {
            if(ownedMaterial==null) return;
            if(portrait!=null) portrait.material=null;
            if(Application.isPlaying) Destroy(ownedMaterial); else DestroyImmediate(ownedMaterial);
        }
    }
}
