using System;
using System.Linq;
using HundredHour.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HundredHour.Environments
{
    // Read-only scrollable history of the conversation log. Opened from a small button in the
    // dialogue window's top-right corner; uses the existing canvas and font, no prefab needed.
    public sealed class MansionLogViewer : MonoBehaviour
    {
        static bool English=>GameLanguage.Current==GameLocale.English;
        static string L(string ja,string en)=>English?en:ja;
        MansionSignalDirector director;
        MansionSignalView view;
        GameObject panel;
        Button toggle, close;
        TMP_Text entry;
        public bool IsOpen=>panel&&panel.activeSelf;
        static void Rect(RectTransform r,float x,float y,float right,float top)
        {r.anchorMin=new Vector2(x,y);r.anchorMax=new Vector2(right,top);r.offsetMin=r.offsetMax=Vector2.zero;}
        TMP_Text Text(Transform parent,string name,float x,float y,float right,float top,int size)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(TextMeshProUGUI));go.transform.SetParent(parent,false);
            var t=go.GetComponent<TMP_Text>();t.font=view.body.font;t.fontSharedMaterial=view.body.fontSharedMaterial;t.fontSize=size;t.color=new Color(.91f,.95f,1);t.text="";t.richText=false;t.raycastTarget=false;
            Rect(t.rectTransform,x,y,right,top);return t;
        }
        Button Button(Transform parent,string name,string label,float x,float y,float right,float top,Action action)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Image),typeof(Button));go.transform.SetParent(parent,false);
            Rect((RectTransform)go.transform,x,y,right,top);go.GetComponent<Image>().color=new Color(.06f,.16f,.20f,1);
            var b=go.GetComponent<Button>();b.targetGraphic=go.GetComponent<Image>();
            var t=Text(go.transform,"Label",.03f,.05f,.97f,.95f,20);t.text=label;t.alignment=TextAlignmentOptions.Center;t.enableAutoSizing=true;t.fontSizeMin=13;t.fontSizeMax=20;
            b.onClick.AddListener(()=>action());return b;
        }
        public void Initialize(MansionSignalDirector d,MansionSignalView v)
        {
            director=d;view=v;
            toggle=Button((RectTransform)view.dialoguePanel.transform,"LogToggle",L("ログ","Log"),.86f,.78f,.98f,.96f,()=>{if(IsOpen)Close();else Open();});
            panel=new GameObject("ConversationLogWindow",typeof(RectTransform),typeof(Image));panel.transform.SetParent(view.stage,false);
            Rect((RectTransform)panel.transform,.06f,.10f,.94f,.92f);panel.GetComponent<Image>().color=new Color(.01f,.03f,.05f,.99f);
            Text(panel.transform,"Title",.035f,.90f,.7f,.975f,26).text=L("画面に表示した会話・地の文","Conversation and narration shown so far");
            close=Button(panel.transform,"CloseLog",L("閉じる","Close"),.80f,.90f,.965f,.975f,Close);
            entry=Text(panel.transform,"Entry",.04f,.05f,.96f,.86f,22);entry.alignment=TextAlignmentOptions.TopLeft;MansionMessageScroll.Attach(entry);
            panel.SetActive(false);toggle.gameObject.SetActive(false);
        }
        public void Open(){view.CloseChoiceDialog();panel.SetActive(true);panel.transform.SetAsLastSibling();Refresh();}
        public void Close(){panel.SetActive(false);if(director)director.Refresh();}
        public void Refresh()
        {
            if(toggle)toggle.gameObject.SetActive(director.Started);
            if(!IsOpen||director.Game==null)return;
            var log=director.Game.State.presentedLog;
            entry.text=log.Count==0?L("ここから画面に表示された会話を記録します。人物設定や内部の進行記録は表示しません。","Conversation shown on screen will be recorded here. Character settings and internal progress logs are not shown."):string.Join("\n\n",log.TakeLast(60).Select(b=>b.speaker+"「"+b.text+"」"));
        }
    }
}
