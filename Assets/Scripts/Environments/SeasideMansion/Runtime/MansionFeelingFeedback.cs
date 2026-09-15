using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using HundredHour.Localization;

namespace HundredHour.Environments
{
    // Hearts fly to 好感度; topic words fly into ふたりの記憶. Runtime overlay, no prefab.
    public sealed class MansionFeelingFeedback : MonoBehaviour
    {
        MansionSignalView view;
        RectTransform memoryTarget, layer;
        TMP_FontAsset font;
        Material fontMaterial;
        Sprite heartSprite;
        readonly List<Floater> floaters=new List<Floater>();
        float statusPunch, memoryPunch;
        int shownTrust=-1;
        sealed class Floater
        {
            public RectTransform rt;public Vector2 from,to,drift;public float t,life;public bool heart;
        }
        public void Initialize(MansionSignalView v,RectTransform memoryButton)
        {
            view=v;memoryTarget=memoryButton;font=v.body.font;fontMaterial=v.body.fontSharedMaterial;
            var go=new GameObject("FeelingFeedback",typeof(RectTransform));go.transform.SetParent(v.stage,false);
            layer=(RectTransform)go.transform;layer.anchorMin=Vector2.zero;layer.anchorMax=Vector2.one;layer.offsetMin=layer.offsetMax=Vector2.zero;
            var g=go.AddComponent<CanvasGroup>();g.blocksRaycasts=false;g.interactable=false;
            heartSprite=HeartSprite();
        }
        static Sprite HeartSprite()
        {
            const int s=32;var tex=new Texture2D(s,s,TextureFormat.RGBA32,false){filterMode=FilterMode.Bilinear,name="HeartMark"};
            for(int y=0;y<s;y++)for(int x=0;x<s;x++)
            {
                float u=(x+.5f)/s*2-1,v=(y+.5f)/s*2-1;v=v*.95f+.15f;
                float a=u*u+v*v-1;float h=a*a*a-u*u*v*v*v;
                tex.SetPixel(x,y,h<0?new Color(1f,.28f,.48f,1):Color.clear);
            }
            tex.Apply(false,false);
            return Sprite.Create(tex,new Rect(0,0,s,s),new Vector2(.5f,.5f),s);
        }
        public void Play(int trustFrom,int trustTo,int memoriesFrom,int memoriesTo,string word,bool good)
        {
            if(!layer)return;
            layer.SetAsLastSibling();
            int gain=trustTo-trustFrom;
            if(gain>0||good)
            {
                int n=good?Mathf.Clamp(3+gain/3,3,7):Mathf.Clamp(gain,1,3);
                var start=ScreenPoint(view.dialoguePanel.transform);
                var dest=ScreenPoint(view.status.rectTransform)+new Vector2(120,8);
                for(int i=0;i<n;i++)SpawnHeart(start+new Vector2(Random.Range(-80f,80f),Random.Range(-20f,40f)),dest,i*.06f);
                SpawnLabel(dest+new Vector2(Random.Range(-30f,40f),24),dest+new Vector2(0,70),(GameLanguage.Current==GameLocale.English?$"+{Mathf.Max(1,gain)} Affection":$"+{Mathf.Max(1,gain)} 好感度"),new Color(1f,.45f,.62f),32,.9f);
                statusPunch=1;
            }
            if(memoriesTo>memoriesFrom&&memoryTarget)
            {
                var start=ScreenPoint(view.dialoguePanel.transform)+new Vector2(0,40);
                var dest=ScreenPoint(memoryTarget);
                string chip=string.IsNullOrEmpty(word)?(GameLanguage.Current==GameLocale.English?"Memory":"記憶"):word;
                SpawnLabel(start,dest,"「"+chip+"」",new Color(.95f,.92f,1),26,1.15f);
                SpawnLabel(start+new Vector2(40,-16),dest+new Vector2(-12,10),chip,new Color(1f,.78f,.88f),22,1.25f);
                memoryPunch=1;
            }
            shownTrust=trustTo;
        }
        Vector2 ScreenPoint(Transform t)
        {
            var r=(RectTransform)t;
            var world=r.TransformPoint(r.rect.center);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(layer,RectTransformUtility.WorldToScreenPoint(null,world),null,out var local);
            return local;
        }
        void SpawnHeart(Vector2 from,Vector2 to,float delay)
        {
            var go=new GameObject("Heart",typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));
            go.transform.SetParent(layer,false);
            var rt=(RectTransform)go.transform;rt.sizeDelta=new Vector2(36,36);
            var img=go.GetComponent<Image>();img.sprite=heartSprite;img.color=new Color(1f,.32f,.5f,1);img.raycastTarget=false;
            float tilt=Random.Range(-40f,40f);
            floaters.Add(new Floater{rt=rt,from=from,to=to,drift=new Vector2(tilt,Random.Range(20f,70f)),t=-delay,life=1.05f,heart=true});
            rt.anchoredPosition=from;rt.localScale=Vector3.zero;
        }
        void SpawnLabel(Vector2 from,Vector2 to,string text,Color color,float size,float life)
        {
            var go=new GameObject("Chip",typeof(RectTransform),typeof(CanvasRenderer),typeof(TextMeshProUGUI));
            go.transform.SetParent(layer,false);
            var rt=(RectTransform)go.transform;rt.sizeDelta=new Vector2(280,40);
            var tmp=go.GetComponent<TMP_Text>();tmp.font=font;tmp.fontSharedMaterial=fontMaterial;tmp.fontSize=size;tmp.color=color;tmp.text=text;tmp.alignment=TextAlignmentOptions.Center;tmp.raycastTarget=false;tmp.richText=false;
            floaters.Add(new Floater{rt=rt,from=from,to=to,drift=new Vector2(Random.Range(-24f,24f),Random.Range(8f,28f)),t=0,life=life,heart=false});
            rt.anchoredPosition=from;rt.localScale=Vector3.one*.4f;
        }
        void Update()
        {
            float dt=Time.unscaledDeltaTime;
            for(int i=floaters.Count-1;i>=0;i--)
            {
                var f=floaters[i];f.t+=dt;float u=Mathf.Clamp01(f.t/f.life);
                if(f.t<0){f.rt.localScale=Vector3.zero;continue;}
                float rise=1-Mathf.Pow(1-u,2.2f);
                var p=Vector2.Lerp(f.from,f.to,rise)+f.drift*Mathf.Sin(u*Mathf.PI);
                f.rt.anchoredPosition=p;
                float scale=f.heart?Mathf.Lerp(0.35f,1.15f,Mathf.Sin(Mathf.Clamp01(u*1.4f)*Mathf.PI)):Mathf.Lerp(.55f,1f,Mathf.Sin(u*Mathf.PI));
                f.rt.localScale=Vector3.one*scale;
                var cg=f.rt.GetComponent<Graphic>();if(cg){var c=cg.color;c.a=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.55f,1f,u));cg.color=c;}
                if(u>=1){Destroy(f.rt.gameObject);floaters.RemoveAt(i);}
            }
            statusPunch=Mathf.MoveTowards(statusPunch,0,dt*2.4f);
            memoryPunch=Mathf.MoveTowards(memoryPunch,0,dt*2.2f);
            if(view&&view.status)view.status.rectTransform.localScale=Vector3.one*(1+statusPunch*.12f);
            if(memoryTarget)memoryTarget.localScale=Vector3.one*(1+memoryPunch*.16f);
        }
    }
}
