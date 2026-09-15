using TMPro;
using HundredHour.Localization;
using UnityEngine;
namespace HundredHour.Environments
{
    public sealed class MansionArrivalView : MonoBehaviour
    {
        public CanvasGroup caption;
        public CanvasGroup openingTitle;
        public CanvasGroup startupBlack;
        string captionTitle,captionText,navigationSource;
        void Awake(){navigationSource=navigationHint.text;PrepareOpening();}
        void OnEnable(){GameLanguage.Changed+=RefreshLanguage;}
        void OnDisable(){GameLanguage.Changed-=RefreshLanguage;}
        void RefreshLanguage(){heading.text=GameLanguage.Text(captionTitle);narration.text=GameLanguage.Text(captionText);navigationHint.text=GameLanguage.Text(navigationSource);}
        public void PrepareOpening()
        {
            caption.gameObject.SetActive(false);
            navigationHint.gameObject.SetActive(false);
            if(openingTitle){openingTitle.gameObject.SetActive(true);openingTitle.alpha=0;}
            if(startupBlack){startupBlack.gameObject.SetActive(true);startupBlack.alpha=1;}
        }
        public void ReleaseBlack(){if(startupBlack)startupBlack.gameObject.SetActive(false);}
        public void SetTitleAlpha(float alpha){if(openingTitle)openingTitle.alpha=alpha;}

        public TMP_Text heading;
        public TMP_Text narration;
        public TMP_Text navigationHint;
        public void ShowCaption(string title,string text,float alpha=1)
        {
            caption.gameObject.SetActive(true);caption.alpha=alpha;
            captionTitle=title;captionText=text;RefreshLanguage();
            navigationHint.gameObject.SetActive(false);
        }
        public void SetCaptionAlpha(float alpha)=>caption.alpha=alpha;
        public void ShowNavigation(bool show)
        {
            caption.gameObject.SetActive(false);
            SetTitleAlpha(0);ReleaseBlack();
            RefreshLanguage();navigationHint.gameObject.SetActive(show);
        }
    }
}
