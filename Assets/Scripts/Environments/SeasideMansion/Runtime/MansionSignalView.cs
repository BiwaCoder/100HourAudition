using System;
using System.Collections;
using System.Linq;
using HundredHour.Localization;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using HundredHour.UI.Choices;

namespace HundredHour.Environments
{
    // Presentation only. Portraits follow the actors; all choices go back to the director.
    [DefaultExecutionOrder(300)]
    public sealed class MansionSignalView : MonoBehaviour
    {
        public Canvas canvas;
        public RectTransform stage, selfWindow, otherWindow;
        public GameObject dialoguePanel, toolsPanel, handPanel;
        public TMP_Text header, speaker, body, status, otherLabel, selfLabel, toolsTitle;
        public UnityEngine.UI.Image selfPortrait, otherPortrait, shade;
        public UnityEngine.UI.RawImage interference;
        public Material eyeAnimationMaterial;
        Material eyeMaterial;
        bool eyeActive;
        float eyeStarted;
        public float EyeOpening {get;private set;}
        public ChoiceMenuController choices;
        public UnityEngine.UI.Button[] tools, cards;
        public TMP_Text[] toolLabels, cardLabels;
        public UnityEngine.UI.Button resumeButton;
        public event Action<int> Choice, Tool, Card;
        public event Action Resume;
        Texture2D noise;
        readonly Color32[] pixels=new Color32[96*64];
        readonly System.Random noiseRandom=new System.Random(7041);
        float nextNoise, signalTarget=1, signal=1, brightnessTarget=1, brightness=1;
        bool centered;
        Camera worldCamera;
        Transform selfActor,otherActor;
        void Awake()
        {
            noise=new Texture2D(96,64,TextureFormat.RGBA32,false){name="Recognition interference",filterMode=FilterMode.Point};
            interference.texture=noise;
            if(eyeAnimationMaterial)eyeMaterial=new Material(eyeAnimationMaterial);
            ApplyReadableLayout();
            messageScroll=MansionMessageScroll.Attach(body);
            BuildChoiceDialog();
            choices.Confirmed+=ForwardChoice;
            choices.SelectionChanged+=StyleChoices;
            for(int i=0;i<tools.Length;i++){int n=i;tools[i].onClick.AddListener(()=>Tool?.Invoke(n));}
            for(int i=0;i<cards.Length;i++){int n=i;cards[i].onClick.AddListener(()=>Card?.Invoke(n));}
            resumeButton.onClick.AddListener(()=>Resume?.Invoke());
        }
        void ForwardChoice(int i)
        {
            if(observation||i<0||i>=pendingOptions.Length)return;
            CloseChoiceDialog();pendingOptions=Array.Empty<string>();choiceToggle.gameObject.SetActive(false);singleChoice.gameObject.SetActive(false);
            Choice?.Invoke(i);
        }
        static void Bounds(RectTransform r,float x0,float y0,float x1,float y1){r.anchorMin=new Vector2(x0,y0);r.anchorMax=new Vector2(x1,y1);r.offsetMin=r.offsetMax=Vector2.zero;}
        void ApplyReadableLayout()
        {
            Bounds(stage,0,0,1,1);
            Bounds(header.rectTransform,.025f,.949f,.76f,.99f);header.fontSize=28;
            Bounds(status.rectTransform,.025f,.851f,.77f,.945f);status.fontSize=28;status.enableAutoSizing=false;
            var statusBg=new GameObject("Status Backplate",typeof(RectTransform),typeof(UnityEngine.UI.Image));statusBg.transform.SetParent(stage,false);Bounds((RectTransform)statusBg.transform,.015f,.842f,.775f,.995f);statusBg.GetComponent<UnityEngine.UI.Image>().color=new Color(.015f,.04f,.06f,.92f);statusBg.GetComponent<UnityEngine.UI.Image>().raycastTarget=false;statusBg.transform.SetSiblingIndex(1);
            statusBackground=statusBg;SetStatusVisible(false);
            Bounds((RectTransform)dialoguePanel.transform,.02f,.02f,.98f,.26f);
            Bounds((RectTransform)handPanel.transform,.02f,.52f,.77f,.685f);
            Bounds((RectTransform)toolsPanel.transform,.79f,.52f,.98f,.90f);
            Bounds(speaker.rectTransform,.02f,.78f,.67f,.96f);
            body.fontSize=26;body.enableAutoSizing=true;body.fontSizeMin=23;body.fontSizeMax=26;
            toolsTitle.fontSize=25;toolsTitle.fontSizeMin=23;toolsTitle.fontSizeMax=25;
            for(int i=0;i<cards.Length;i++)
            {
                if(i<3)Bounds((RectTransform)cards[i].transform,i/3f,0,(i+.96f)/3f,1);
                else cards[i].gameObject.SetActive(false);
                cardLabels[i].fontSize=26;cardLabels[i].enableAutoSizing=false;
                cards[i].GetComponent<UnityEngine.UI.Image>().color=new Color(.025f,.08f,.12f,.98f);
            }
            for(int i=0;i<toolLabels.Length;i++){toolLabels[i].fontSize=24;toolLabels[i].enableAutoSizing=false;}
            foreach(var label in new[]{selfLabel,otherLabel})
            {
                label.fontSize=21.6f;label.enableAutoSizing=true;label.fontSizeMin=14.4f;label.fontSizeMax=21.6f;
                label.textWrappingMode=TextWrappingModes.NoWrap;label.overflowMode=TextOverflowModes.Ellipsis;
            }
        }
        MansionMessageScroll messageScroll;
        GameObject statusBackground;
        public void SetStatusVisible(bool visible)
        {
            if(statusBackground)statusBackground.SetActive(visible);
            status.gameObject.SetActive(visible);
        }
        GameObject observation;
        public void ShowObservation(string text,string closeLabel="会話に戻る",Action onClose=null)
        {
            CloseChoiceDialog();
            if(observation)Destroy(observation);
            observation=new GameObject("Observed Conversation",typeof(RectTransform),typeof(UnityEngine.UI.Image));observation.transform.SetParent(stage,false);Bounds((RectTransform)observation.transform,0,0,1,1);
            var bg=observation.GetComponent<UnityEngine.UI.Image>();bg.color=new Color(.015f,.035f,.06f,1f);bg.raycastTarget=true;
            var label=Instantiate(body,observation.transform);Bounds(label.rectTransform,.08f,.20f,.92f,.9f);label.text=text;label.fontSize=27;label.fontSizeMin=25;label.fontSizeMax=27;MansionMessageScroll.Attach(label);
            var button=Instantiate(tools[0],observation.transform);Bounds((RectTransform)button.transform,.35f,.04f,.65f,.12f);button.onClick.RemoveAllListeners();button.onClick.AddListener(()=>{CloseObservation();onClose?.Invoke();});button.interactable=true;
            // The AI step has already hidden the source button when this modal opens.
            button.gameObject.SetActive(true);button.GetComponentInChildren<TMP_Text>(true).text=closeLabel;
        }
        void CloseObservation(){if(observation)Destroy(observation);observation=null;}
        GameObject staffRoll;Coroutine staffRollScroll;
        public void ShowStaffRoll(string text,Action onFinished)
        {
            CloseChoiceDialog();
            if(staffRoll)Destroy(staffRoll);
            staffRoll=new GameObject("Staff Roll",typeof(RectTransform),typeof(UnityEngine.UI.Image));staffRoll.transform.SetParent(stage,false);Bounds((RectTransform)staffRoll.transform,0,0,1,1);
            var bg=staffRoll.GetComponent<UnityEngine.UI.Image>();bg.color=Color.black;bg.raycastTarget=true;
            var label=Instantiate(body,staffRoll.transform);Bounds(label.rectTransform,.18f,.08f,.82f,.92f);label.text=text;label.color=Color.white;label.fontSize=28;label.fontSizeMin=28;label.fontSizeMax=28;label.alignment=TextAlignmentOptions.Top;
            var scroller=MansionMessageScroll.Attach(label);scroller.HideScrollbar();
            var button=Instantiate(tools[0],staffRoll.transform);Bounds((RectTransform)button.transform,.4f,.015f,.6f,.075f);button.onClick.RemoveAllListeners();
            button.GetComponentInChildren<TMP_Text>(true).text=GameLanguage.Current==GameLocale.English?"Skip":"スキップ";
            void Finish(){CloseStaffRoll();onFinished?.Invoke();}
            button.onClick.AddListener(Finish);button.interactable=true;button.gameObject.SetActive(true);
            staffRollScroll=StartCoroutine(AutoScrollStaffRoll(scroller,button.GetComponentInChildren<TMP_Text>(true)));
        }
        // The crawl never leaves on its own: when it reaches the end, the button becomes "back to title" and waits.
        IEnumerator AutoScrollStaffRoll(MansionMessageScroll scroller,TMP_Text buttonLabel)
        {
            yield return null;yield return null;// let MansionMessageScroll.Measure() run once first
            var scroll=scroller.GetComponent<UnityEngine.UI.ScrollRect>();
            scroll.verticalNormalizedPosition=1f;
            // Hold on the opening poem for three seconds before the crawl starts.
            for(float hold=0;hold<3f&&staffRoll;hold+=Time.unscaledDeltaTime){scroll.verticalNormalizedPosition=1f;yield return null;}
            const float duration=48f;// a slow, classic end-credits crawl
            float t=0;
            while(t<duration&&staffRoll)
            {
                t+=Time.unscaledDeltaTime;
                scroll.verticalNormalizedPosition=Mathf.Clamp01(1f-t/duration);
                yield return null;
            }
            if(staffRoll&&buttonLabel)buttonLabel.text=GameLanguage.Current==GameLocale.English?"Back to title":"タイトルへ戻る";
        }
        void CloseStaffRoll(){if(staffRollScroll!=null)StopCoroutine(staffRollScroll);staffRollScroll=null;if(staffRoll)Destroy(staffRoll);staffRoll=null;}
        public void Track(Camera camera,Transform self,Transform other){worldCamera=camera;selfActor=self;otherActor=other;}
        public void SetWindows(bool visible,Sprite self,Sprite other,bool central,float noiseAmount,float light)
        {
            selfWindow.gameObject.SetActive(visible&&!central);otherWindow.gameObject.SetActive(visible);
            if(central && otherPortrait.sprite!=other)signal=1;
            if(centered!=central)portraitOrder.Clear();
            selfPortrait.sprite=self;otherPortrait.sprite=other;
            selfPortrait.enabled=self!=null;otherPortrait.enabled=other!=null;
            centered=central;signalTarget=other?noiseAmount:1;brightnessTarget=light;
            selfPortrait.preserveAspect=true;otherPortrait.preserveAspect=true;
            if(central)
            {
                otherWindow.GetComponent<CanvasGroup>().alpha=1;
                otherWindow.anchorMin=new Vector2(.23f,.35f);otherWindow.anchorMax=new Vector2(.77f,.89f);
                otherWindow.offsetMin=otherWindow.offsetMax=Vector2.zero;
            }
            else {otherWindow.anchorMin=otherWindow.anchorMax=Vector2.zero;}
        }
        public void SetEyeAnimation(bool active)
        {
            if(active&&!eyeActive)eyeStarted=Time.unscaledTime;
            eyeActive=active;otherPortrait.material=active?eyeMaterial:null;
            if(!active)EyeOpening=0;
        }
        public void SetDialogue(string name,string text,IReadOnlyList<string> options)
        {
            var translated=options.Select(GameLanguage.Text).ToArray();
            bool changed=body.text!=GameLanguage.Text(text)||!pendingOptions.SequenceEqual(translated);
            Bounds(messageScroll.Root,.02f,translated.Length==1?.31f:.06f,.98f,.73f);
            dialoguePanel.SetActive(true);speaker.text=GameLanguage.Text(name);body.text=GameLanguage.Text(text);
            pendingOptions=translated;
            choiceToggle.gameObject.SetActive(pendingOptions.Length>1);
            singleChoice.gameObject.SetActive(pendingOptions.Length==1);
            if(pendingOptions.Length==1)singleChoice.GetComponentInChildren<TMP_Text>().text=pendingOptions[0]+"  →";
            if(changed)choiceOpenedFrame=Time.frameCount;
            choiceToggle.GetComponentInChildren<TMP_Text>().text=GameLanguage.Current==GameLocale.English?"Choices":"選択肢を開く";
            if(pendingOptions.Length<=1)CloseChoiceDialog();
            else if(changed){selectedChoice=0;OpenChoiceDialog();}
        }
        GameObject choiceDialog;
        UnityEngine.UI.Button choiceToggle, singleChoice;
        int choiceOpenedFrame;
        string[] pendingOptions=Array.Empty<string>();
        int selectedChoice;
        public bool ChoiceDialogOpen=>choiceDialog&&choiceDialog.activeSelf;
        UnityEngine.UI.Button DialogButton(string name,Transform parent,string text,float x0,float y0,float x1,float y1,UnityEngine.Events.UnityAction action)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(UnityEngine.UI.Image),typeof(UnityEngine.UI.Button));
            go.transform.SetParent(parent,false);Bounds((RectTransform)go.transform,x0,y0,x1,y1);
            go.GetComponent<UnityEngine.UI.Image>().color=new Color(.045f,.16f,.20f,.98f);
            var button=go.GetComponent<UnityEngine.UI.Button>();button.onClick.AddListener(action);
            var labelGo=new GameObject("Label",typeof(RectTransform),typeof(TextMeshProUGUI));labelGo.transform.SetParent(go.transform,false);
            var label=labelGo.GetComponent<TMP_Text>();Bounds(label.rectTransform,.03f,.05f,.97f,.95f);
            label.font=body.font;label.fontSize=22;label.text=text;label.color=Color.white;label.alignment=TextAlignmentOptions.Center;label.raycastTarget=false;
            return button;
        }
        void BuildChoiceDialog()
        {
            choiceToggle=DialogButton("Open choices",dialoguePanel.transform,"選択肢を開く",.69f,.78f,.845f,.96f,OpenChoiceDialog);
            choiceToggle.gameObject.SetActive(false);
            singleChoice=DialogButton("Single choice",dialoguePanel.transform,"",.02f,.04f,.98f,.27f,()=>
            {
                if(pendingOptions.Length==1&&Time.frameCount>choiceOpenedFrame)ForwardChoice(0);
            });
            singleChoice.gameObject.SetActive(false);
            var singleLabel=singleChoice.GetComponentInChildren<TMP_Text>();
            singleLabel.enableAutoSizing=true;singleLabel.fontSizeMin=18;singleLabel.fontSizeMax=24;
            choiceDialog=new GameObject("Choice Dialog",typeof(RectTransform));choiceDialog.transform.SetParent(stage,false);Bounds((RectTransform)choiceDialog.transform,0,0,1,1);
            var backdrop=DialogButton("Dismiss backdrop",choiceDialog.transform,"",0,0,1,1,CloseChoiceDialog);
            backdrop.GetComponent<UnityEngine.UI.Image>().color=new Color(0,.015f,.025f,.40f);
            var panel=new GameObject("Choice Window",typeof(RectTransform),typeof(UnityEngine.UI.Image));panel.transform.SetParent(choiceDialog.transform,false);Bounds((RectTransform)panel.transform,.20f,.32f,.80f,.77f);
            panel.GetComponent<UnityEngine.UI.Image>().color=new Color(.018f,.055f,.08f,.98f);
            var heading=new GameObject("Heading",typeof(RectTransform),typeof(TextMeshProUGUI));heading.transform.SetParent(panel.transform,false);
            var title=heading.GetComponent<TMP_Text>();Bounds(title.rectTransform,.045f,.81f,.68f,.96f);title.font=body.font;title.fontSize=27;title.color=new Color(.5f,.86f,.92f);title.text=GameLanguage.Current==GameLocale.English?"Choices":"選択肢";title.raycastTarget=false;choiceTitle=title;
            DialogButton("Close choices",panel.transform,GameLanguage.Current==GameLocale.English?"Close":"閉じる",.72f,.83f,.96f,.96f,CloseChoiceDialog);
            choices.transform.SetParent(panel.transform,false);Bounds((RectTransform)choices.transform,.045f,.07f,.955f,.78f);
            choiceDialog.SetActive(false);
        }
        public void OpenChoiceDialog()
        {
            if(pendingOptions.Length<=1||observation)return;
            choiceDialog.SetActive(true);choiceDialog.transform.SetAsLastSibling();
            Canvas.ForceUpdateCanvases();
            choices.Show(pendingOptions,Mathf.Clamp(selectedChoice,0,pendingOptions.Length-1));StyleChoices(choices.SelectedIndex);
        }
        public void CloseChoiceDialog()
        {
            if(!ChoiceDialogOpen)return;
            selectedChoice=Mathf.Max(0,choices.SelectedIndex);
            choiceDialog.SetActive(false);
        }
        void Update()
        {
            if(ChoiceDialogOpen&&UnityEngine.InputSystem.Keyboard.current?.escapeKey.wasPressedThisFrame==true)CloseChoiceDialog();
            // Closing the choice modal without picking anything leaves just this small toggle
            // button on screen with no other cue that it's the way back in; a gentle pulse draws
            // the eye to it instead of leaving the player unsure what to do next.
            if(choiceToggle&&choiceToggle.gameObject.activeInHierarchy&&!ChoiceDialogOpen)
            {
                float pulse=(Mathf.Sin(Time.unscaledTime*3.2f)+1)*.5f;
                choiceToggle.transform.localScale=Vector3.one*(1+pulse*.06f);
                var img=choiceToggle.GetComponent<UnityEngine.UI.Image>();
                if(img)img.color=Color.Lerp(new Color(.045f,.16f,.20f,.98f),new Color(.35f,.72f,.8f,.98f),pulse);
            }
            else if(choiceToggle)choiceToggle.transform.localScale=Vector3.one;
        }
        UnityEngine.UI.ScrollRect choiceScroll;
        void StyleChoices(int selected)
        {
            var labels=choices.GetComponentsInChildren<TMP_Text>();
            if(labels.Length==0)return;
            if(!choiceScroll)
            {
                choices.gameObject.AddComponent<UnityEngine.UI.RectMask2D>();
                choiceScroll=choices.gameObject.AddComponent<UnityEngine.UI.ScrollRect>();
                choiceScroll.viewport=(RectTransform)choices.transform;choiceScroll.horizontal=false;
                choiceScroll.movementType=UnityEngine.UI.ScrollRect.MovementType.Clamped;choiceScroll.scrollSensitivity=36;
            }
            var content=(RectTransform)labels[0].transform.parent.parent;
            bool fresh=choiceScroll.content!=content;choiceScroll.content=content;
            float offset=0;
            for(int i=0;i<labels.Length;i++)
            {
                labels[i].color=i==selected?Color.white:new Color(.77f,.84f,.88f);
                labels[i].enableAutoSizing=false;labels[i].fontSize=25;labels[i].textWrappingMode=TextWrappingModes.Normal;
                var row=(RectTransform)labels[i].transform.parent;
                float width=Mathf.Max(200,((RectTransform)choices.transform).rect.width-48);
                float height=Mathf.Max(55,labels[i].GetPreferredValues(labels[i].text,width,Mathf.Infinity).y+14);
                row.sizeDelta=new Vector2(0,height);row.anchoredPosition=new Vector2(0,-offset);offset+=height+6;
            }
            content.sizeDelta=new Vector2(0,offset);choiceScroll.vertical=offset>choiceScroll.viewport.rect.height;
            if(fresh)choiceScroll.verticalNormalizedPosition=1;
            if(selected>=0&&selected<labels.Length)
            {
                var row=(RectTransform)labels[selected].transform.parent;
                float top=-row.anchoredPosition.y,bottom=top+row.rect.height,current=content.anchoredPosition.y;
                if(top<current)content.anchoredPosition=new Vector2(0,top);
                else if(bottom>current+choiceScroll.viewport.rect.height)content.anchoredPosition=new Vector2(0,Mathf.Min(offset-choiceScroll.viewport.rect.height,bottom-choiceScroll.viewport.rect.height));
            }
        }
        public void SetTools(string title,string[] labels,bool[] available)
        {
            toolsPanel.SetActive(labels.Length>0);toolsTitle.text=GameLanguage.Text(title);
            for(int i=0;i<tools.Length;i++){tools[i].gameObject.SetActive(i<labels.Length&&!string.IsNullOrEmpty(labels[i]));if(i<labels.Length){toolLabels[i].text=GameLanguage.Text(labels[i]);tools[i].interactable=available[i];}}
        }
        public void SetCards(string[] labels,bool[] available)
        {
            handPanel.SetActive(labels.Length>0);
            for(int i=0;i<cards.Length;i++){cards[i].gameObject.SetActive(i<3&&i<labels.Length);if(i<3&&i<labels.Length){cardLabels[i].text=GameLanguage.Text(labels[i]);cards[i].interactable=available[i];}}
        }
        void LateUpdate()
        {
            if(eyeActive&&eyeMaterial){EyeOpening=Mathf.Clamp01((Time.unscaledTime-eyeStarted-.9f)/1.8f);eyeMaterial.SetFloat("_EyeOpen",EyeOpening);}
            LayoutPortraits();
            signal=Mathf.MoveTowards(signal,signalTarget,Time.unscaledDeltaTime*.8f);
            brightness=Mathf.MoveTowards(brightness,brightnessTarget,Time.unscaledDeltaTime*.65f);
            otherPortrait.color=new Color(brightness,brightness,brightness,1);
            interference.color=new Color(1,1,1,signal);
            if(signal<.005f||Time.unscaledTime<nextNoise)return;
            nextNoise=Time.unscaledTime+.09f;
            int band=noiseRandom.Next(64);
            for(int y=0;y<64;y++)for(int x=0;x<96;x++)
            {
                byte n=(byte)noiseRandom.Next(y==band?65:12,y==band?125:68);
                pixels[y*96+x]=new Color32((byte)(n*.55f),(byte)(n*.9f),n,255);
            }
            noise.SetPixels32(pixels);noise.Apply(false);
        }
        readonly Dictionary<RectTransform,Transform> extraActors=new Dictionary<RectTransform,Transform>();
        readonly List<RectTransform> portraitOrder=new List<RectTransform>();
        Vector2 portraitArea;
        bool portraitDialogue,portraitTools;
        float portraitObscuredSince=-1,portraitNextMove;
        static float Overlap(Rect a,Rect b)=>Mathf.Max(0,Mathf.Min(a.xMax,b.xMax)-Mathf.Max(a.xMin,b.xMin))*Mathf.Max(0,Mathf.Min(a.yMax,b.yMax)-Mathf.Max(a.yMin,b.yMin));
        public void PlaceActorWindow(RectTransform window,Transform actor){extraActors[window]=actor;}
        TMP_Text choiceTitle;
        public void SetChoiceTitle(string text){if(choiceTitle)choiceTitle.text=text;}
        sealed class PortraitPlacement
        {
            public RectTransform window;public Rect actor;public float x,y;public int order;
        }
        Vector2 Project(Vector3 position)
        {
            var screen=worldCamera.WorldToScreenPoint(position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(stage,screen,null,out var local);
            return local-stage.rect.min;
        }
        Rect ActorBounds(Transform actor)
        {
            var renderer=actor.GetComponentInChildren<Renderer>();
            var bounds=renderer?renderer.bounds:new UnityEngine.Bounds(actor.position,Vector3.one);
            Vector2 min=new Vector2(float.MaxValue,float.MaxValue),max=new Vector2(float.MinValue,float.MinValue);
            for(int i=0;i<8;i++)
            {
                var corner=bounds.center+Vector3.Scale(bounds.extents,new Vector3((i&1)==0?-1:1,(i&2)==0?-1:1,(i&4)==0?-1:1));
                var p=Project(corner);min=Vector2.Min(min,p);max=Vector2.Max(max,p);
            }
            return Rect.MinMaxRect(min.x,min.y,max.x,max.y);
        }
        void LayoutPortraits()
        {
            if(!worldCamera)return;
            var entries=new List<PortraitPlacement>();
            void Add(RectTransform window,Transform actor,int order)
            {
                if(!window||!actor||!window.gameObject.activeSelf)return;
                bool visible=worldCamera.WorldToViewportPoint(actor.position).z>0;
                window.GetComponent<CanvasGroup>().alpha=visible?1:0;if(!visible)return;
                entries.Add(new PortraitPlacement{window=window,actor=ActorBounds(actor),order=order});
            }
            Add(selfWindow,selfActor,0);if(!centered)Add(otherWindow,otherActor,1);
            int n=2;foreach(var entry in extraActors)Add(entry.Key,entry.Value,n++);
            if(entries.Count==0){portraitOrder.Clear();return;}
            bool reset=portraitArea!=stage.rect.size||portraitDialogue!=dialoguePanel.activeSelf||portraitTools!=toolsPanel.activeSelf||
                entries.Count!=portraitOrder.Count||entries.Any(e=>!portraitOrder.Contains(e.window));
            if(reset)
            {
                portraitOrder.Clear();
                portraitOrder.AddRange(entries.OrderBy(e=>e.actor.center.x).ThenBy(e=>e.order).Select(e=>e.window));
                portraitArea=stage.rect.size;portraitDialogue=dialoguePanel.activeSelf;portraitTools=toolsPanel.activeSelf;
            }
            // Keep the initial left/right assignment even when the camera crosses the actors.
            entries=entries.OrderBy(e=>portraitOrder.IndexOf(e.window)).ToList();
            float Obscuration(Rect box)=>entries.Max(e=>Overlap(box,e.actor)/Mathf.Max(1,Mathf.Min(box.width*box.height,e.actor.width*e.actor.height)));
            float previousObscuration=entries.Max(e=>Obscuration(new Rect(e.window.anchoredPosition,e.window.sizeDelta)));
            if(!reset)
            {
                if(previousObscuration<.4f){portraitObscuredSince=-1;return;}
                if(portraitObscuredSince<0)portraitObscuredSince=Time.unscaledTime;
                if(Time.unscaledTime-portraitObscuredSince<1.2f||Time.unscaledTime<portraitNextMove)return;
            }
            float left=20,right=stage.rect.width*(toolsPanel.activeSelf?.77f:.98f),gap=18;
            float width=Mathf.Min(250,(right-left-gap*(entries.Count-1))/entries.Count),height=width*1.23f;
            float bottom=stage.rect.height*(dialoguePanel.activeSelf?.295f:.18f),top=stage.rect.height*.835f;
            // First choose the side of each cube, then resolve all windows together.
            for(int i=0;i<entries.Count;i++)
            {
                var e=entries[i];bool onLeft=i<entries.Count/2;
                e.x=Mathf.Clamp(onLeft?e.actor.xMin-width-gap:e.actor.xMax+gap,left,right-width);
                e.y=Mathf.Clamp(e.actor.center.y,bottom,top-height);
                if(i>0)e.x=Mathf.Max(e.x,entries[i-1].x+width+gap);
            }
            for(int i=entries.Count-1;i>=0;i--)
            {
                var e=entries[i];e.x=Mathf.Min(e.x,i==entries.Count-1?right-width:entries[i+1].x-width-gap);
                // If space at the side is tight, prefer a vertical gap above/below the cube.
                var box=new Rect(e.x,e.y,width,height);
                if(entries.Any(other=>box.Overlaps(other.actor)))
                {
                    float bestOverlap=float.MaxValue,bestY=e.y;
                    for(float y=bottom;y<=top-height+.1f;y+=12)
                    {
                        var candidate=new Rect(e.x,y,width,height);float overlap=0;
                        foreach(var other in entries)overlap+=Mathf.Max(0,Mathf.Min(candidate.xMax,other.actor.xMax)-Mathf.Max(candidate.xMin,other.actor.xMin))*Mathf.Max(0,Mathf.Min(candidate.yMax,other.actor.yMax)-Mathf.Max(candidate.yMin,other.actor.yMin));
                        if(overlap<bestOverlap){bestOverlap=overlap;bestY=y;}
                    }
                    e.y=bestY;
                }
            }
            // Do not move for tiny gains or chase every frame of a camera cut.
            float nextObscuration=entries.Max(e=>Obscuration(new Rect(e.x,e.y,width,height)));
            if(!reset&&nextObscuration>previousObscuration-.2f)return;
            portraitObscuredSince=-1;portraitNextMove=Time.unscaledTime+4;
            foreach(var e in entries)
            {
                e.window.anchorMin=e.window.anchorMax=Vector2.zero;e.window.pivot=Vector2.zero;
                e.window.sizeDelta=new Vector2(width,height);e.window.anchoredPosition=new Vector2(e.x,e.y);
            }
        }
        void OnDestroy(){if(choices){choices.Confirmed-=ForwardChoice;choices.SelectionChanged-=StyleChoices;}if(noise)Destroy(noise);if(eyeMaterial)Destroy(eyeMaterial);}
    }
}
