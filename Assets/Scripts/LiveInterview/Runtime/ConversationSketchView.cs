using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;

namespace HundredHour.LiveInterview
{
    /// <summary>Live-only, runtime-built cards. No inferred trait is confirmed by a tap.</summary>
    public sealed class ConversationSketchView : MonoBehaviour
    {
        public event Action<string> Selected;
        RectTransform panel;
        TMP_Text heading, hint;
        readonly List<UnityEngine.UI.Button> buttons=new List<UnityEngine.UI.Button>();
        readonly List<TMP_Text> labels=new List<TMP_Text>();
        readonly List<string> ids=new List<string>();
        readonly List<string> titles=new List<string>();
        readonly List<string> captions=new List<string>();
        readonly Dictionary<RectTransform,Vector2> originalMax=new Dictionary<RectTransform,Vector2>();
        bool english, accepting, running;
        string pending, selectedTitle;
        float unlockAt;
        TMP_FontAsset font;

        public void Initialize(Transform root,TMP_FontAsset sharedFont)
        {
            if(panel)return;
            font=sharedFont;
            panel=Rect("Conversation Sketch",root,new Vector2(.64f,.24f),new Vector2(.92f,.81f));
            var bg=panel.gameObject.AddComponent<UnityEngine.UI.Image>();bg.color=new Color(.035f,.06f,.095f,.25f);bg.raycastTarget=false;
            heading=Text("Heading",panel,28,new Vector2(.07f,.88f),new Vector2(.93f,.97f));
            hint=Text("Hint",panel,20,new Vector2(.07f,.02f),new Vector2(.93f,.13f));hint.color=new Color(.70f,.85f,.89f);
            for(int i=0;i<4;i++)
            {
                float top=.85f-i*.177f;
                var rect=Rect("Topic "+i,panel,new Vector2(.06f,top-.155f),new Vector2(.94f,top));
                var image=rect.gameObject.AddComponent<UnityEngine.UI.Image>();image.color=Color.clear;
                var button=rect.gameObject.AddComponent<UnityEngine.UI.Button>();button.targetGraphic=image;
                var colors=button.colors;colors.highlightedColor=new Color(.65f,.93f,1);colors.pressedColor=new Color(.35f,.8f,.85f);colors.disabledColor=new Color(.65f,.65f,.65f);button.colors=colors;
                var label=Text("Label",rect,23,new Vector2(.07f,.08f),new Vector2(.93f,.94f));
                int index=i;button.onClick.AddListener(()=>Choose(index));
                button.targetGraphic=label;label.raycastTarget=true;
                button.transition=UnityEngine.UI.Selectable.Transition.None;
                buttons.Add(button);labels.Add(label);ids.Add("");titles.Add("");captions.Add("");
            }
            foreach(var name in new[]{"Interviewer","Your voice","Microphone","Voice spectrum","Signal label","Formation"})
            {
                var r=root.Find(name) as RectTransform;if(r)originalMax[r]=r.anchorMax;
            }
            SetActive(false);
        }
        static RectTransform Rect(string name,Transform parent,Vector2 min,Vector2 max)
        {
            var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);
            r.anchorMin=min;r.anchorMax=max;r.offsetMin=r.offsetMax=Vector2.zero;return r;
        }
        TMP_Text Text(string name,Transform parent,float size,Vector2 min,Vector2 max)
        {
            var t=Rect(name,parent,min,max).gameObject.AddComponent<TextMeshProUGUI>();t.font=font;t.fontSize=size;
            t.color=new Color(.9f,.96f,1);t.raycastTarget=false;t.richText=false;t.textWrappingMode=TextWrappingModes.Normal;
            t.overflowMode=TextOverflowModes.Ellipsis;return t;
        }
        public void Begin(bool en)
        {
            SetActive(false);
            english=en;pending=null;selectedTitle=null;accepting=true;running=true;unlockAt=0;
            heading.text=en?"AI-0’s impression of you":"AI-0から見たあなた";
            hint.text=en?"Tap a topic to explore it further.":"タップした内容の話題を掘り下げます";
            for(int i=0;i<buttons.Count;i++){buttons[i].gameObject.SetActive(false);ids[i]="";}
        }
        public void Render(JArray cards)
        {
            if(!panel||!running)return;
            SetVisibility(cards!=null&&cards.Count>0);
            if(cards==null||cards.Count==0)pending=null;
            for(int i=0;i<buttons.Count;i++)
            {
                bool show=cards!=null&&i<cards.Count;
                buttons[i].gameObject.SetActive(show);ids[i]=show?(string)cards[i]["id"]:"";
                if(show)
                {
                    titles[i]=(string)cards[i]["title"];
                    labels[i].color=new Color(.9f,.96f,1);
                    captions[i]=(string)cards[i]["title"]+((string)cards[i]["kind"]=="portrait"?(english?"?":"かも"):"");
                    labels[i].fontSize=28;labels[i].enableAutoSizing=true;labels[i].fontSizeMin=20;labels[i].fontSizeMax=28;
                    labels[i].textWrappingMode=TextWrappingModes.NoWrap;
                    labels[i].fontStyle=FontStyles.Underline;
                }
            }
            RefreshButtons();
        }
        void Choose(int index)
        {
            if(!accepting||pending!=null||Time.unscaledTime<unlockAt||string.IsNullOrEmpty(ids[index]))return;
            selectedTitle=titles[index];
            pending=ids[index];unlockAt=Time.unscaledTime+8;
            labels[index].color=new Color(.45f,1f,.87f);
            hint.text=english?"Connecting to “"+selectedTitle+"”…":"「"+selectedTitle+"」の話へつなげています…";
            RefreshButtons();Selected?.Invoke(pending);
        }
        public void Acknowledge(string title)
        {
            selectedTitle=title;
            pending=null;unlockAt=Time.unscaledTime+3.2f;
            hint.text=english?"Exploring “"+title+"”":"「"+title+"」の話題を掘り下げます";RefreshButtons();
        }
        public void Reject()
        {
            selectedTitle=null;pending=null;unlockAt=Time.unscaledTime+1;
            hint.text=english?"Choose a current thought again.":"今のカードから、もう一度選んでね。";RefreshButtons();
        }
        public void StopChoosing(){running=false;accepting=false;pending=null;RefreshButtons();}
        void Update()
        {
            if(pending!=null&&Time.unscaledTime>=unlockAt)Reject();
            if(panel&&panel.gameObject.activeSelf)RefreshButtons();
        }
        void RefreshButtons()
        {
            for(int i=0;i<buttons.Count;i++)
            {
                bool selected=!string.IsNullOrEmpty(selectedTitle)&&titles[i]==selectedTitle;
                buttons[i].interactable=accepting&&pending==null&&Time.unscaledTime>=unlockAt;
                labels[i].color=selected?new Color(.45f,1f,.87f):new Color(.9f,.96f,1);
                labels[i].text=(selected?"> ":"")+captions[i];
                labels[i].fontStyle=selected?FontStyles.Underline|FontStyles.Bold:FontStyles.Underline;
            }
        }
        void SetVisibility(bool value)
        {
            if(panel)panel.gameObject.SetActive(value);
            foreach(var pair in originalMax)if(pair.Key)pair.Key.anchorMax=value?new Vector2(.60f,pair.Value.y):pair.Value;
        }
        public void SetActive(bool value)
        {
            SetVisibility(value);
            if(!value){running=false;accepting=false;pending=null;}
        }
        void OnDestroy(){SetActive(false);if(panel)Destroy(panel.gameObject);}
    }
}
