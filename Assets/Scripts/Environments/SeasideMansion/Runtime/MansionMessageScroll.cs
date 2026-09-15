using TMPro;
using UnityEngine;

namespace HundredHour.Environments
{
    // Measures rendered text at its actual width; short messages never show a scrollbar.
    [DefaultExecutionOrder(400)]
    public sealed class MansionMessageScroll : MonoBehaviour
    {
        public RectTransform Root=> (RectTransform)transform;
        public bool Overflowing {get;private set;}
        TMP_Text label;
        RectTransform viewport,content;
        UnityEngine.UI.ScrollRect scroll;
        UnityEngine.UI.Scrollbar scrollbar;
        UnityEngine.UI.LayoutElement textLayout;
        string previousText;bool hideScrollbar;
        // End credits scroll on their own; the track at the right edge only spoils the picture there.
        public void HideScrollbar(){hideScrollbar=true;if(scroll)scroll.verticalScrollbar=null;if(scrollbar){Destroy(scrollbar.gameObject);scrollbar=null;}if(viewport)viewport.offsetMax=Vector2.zero;}
        float previousWidth=-1,previousHeight=-1;
        static RectTransform Rect(string name,Transform parent)
        {
            var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);
            r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;return r;
        }
        public static MansionMessageScroll Attach(TMP_Text text)
        {
            var original=text.rectTransform;var root=Rect(text.name+" Scroll",original.parent);
            root.SetSiblingIndex(original.GetSiblingIndex());root.anchorMin=original.anchorMin;root.anchorMax=original.anchorMax;root.pivot=original.pivot;root.sizeDelta=original.sizeDelta;root.anchoredPosition=original.anchoredPosition;
            var result=root.gameObject.AddComponent<MansionMessageScroll>();result.Build(text);return result;
        }
        void Build(TMP_Text text)
        {
            label=text;label.enableAutoSizing=false;label.textWrappingMode=TextWrappingModes.Normal;label.overflowMode=TextOverflowModes.Overflow;
            viewport=Rect("Viewport",transform);viewport.offsetMax=new Vector2(-14,0);
            var image=viewport.gameObject.AddComponent<UnityEngine.UI.Image>();image.color=Color.white;
            viewport.gameObject.AddComponent<UnityEngine.UI.Mask>().showMaskGraphic=false;
            content=Rect("Content",viewport);content.anchorMin=new Vector2(0,1);content.anchorMax=Vector2.one;content.pivot=new Vector2(.5f,1);content.sizeDelta=Vector2.zero;
            var layout=content.gameObject.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();layout.childControlWidth=layout.childControlHeight=true;layout.childForceExpandWidth=true;layout.childForceExpandHeight=false;
            var fit=content.gameObject.AddComponent<UnityEngine.UI.ContentSizeFitter>();fit.verticalFit=UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
            label.transform.SetParent(content,false);textLayout=label.GetComponent<UnityEngine.UI.LayoutElement>()??label.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();
            scroll=gameObject.AddComponent<UnityEngine.UI.ScrollRect>();scroll.viewport=viewport;scroll.content=content;scroll.horizontal=false;scroll.movementType=UnityEngine.UI.ScrollRect.MovementType.Clamped;scroll.scrollSensitivity=30;
            var track=Rect("Scrollbar",transform);track.anchorMin=new Vector2(1,0);track.anchorMax=Vector2.one;track.pivot=new Vector2(1,.5f);track.sizeDelta=new Vector2(8,0);
            var trackImage=track.gameObject.AddComponent<UnityEngine.UI.Image>();trackImage.color=new Color(.6f,.78f,.82f,.10f);
            scrollbar=track.gameObject.AddComponent<UnityEngine.UI.Scrollbar>();scrollbar.direction=UnityEngine.UI.Scrollbar.Direction.BottomToTop;
            var handle=Rect("Handle",track);var handleImage=handle.gameObject.AddComponent<UnityEngine.UI.Image>();handleImage.color=new Color(.65f,.84f,.88f,.45f);scrollbar.handleRect=handle;scrollbar.targetGraphic=handleImage;
            scroll.verticalScrollbar=scrollbar;scroll.verticalScrollbarVisibility=UnityEngine.UI.ScrollRect.ScrollbarVisibility.Permanent;
            scrollbar.gameObject.SetActive(false);
        }
        void LateUpdate()=>Measure();
        public void Measure()
        {
            if(!label)return;
            float width=viewport.rect.width,height=viewport.rect.height;if(width<=0||height<=0)return;
            bool changed=previousText!=label.text;
            if(!changed&&Mathf.Abs(width-previousWidth)<.1f&&Mathf.Abs(height-previousHeight)<.1f)return;
            previousText=label.text;previousWidth=width;previousHeight=height;
            float required=Mathf.Ceil(label.GetPreferredValues(label.text,width,Mathf.Infinity).y)+4;
            textLayout.preferredHeight=required;textLayout.minHeight=required;
            Overflowing=required>height+.5f;scroll.vertical=Overflowing;if(scrollbar)scrollbar.gameObject.SetActive(Overflowing&&!hideScrollbar);
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(content);
            if(changed||!Overflowing){scroll.StopMovement();scroll.verticalNormalizedPosition=1;}
        }
    }
}
