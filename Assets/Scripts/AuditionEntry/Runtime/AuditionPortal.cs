using System;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using HundredHour.Localization;
namespace HundredHour.AuditionEntry
{
    public sealed class AuditionPortal : MonoBehaviour
    {
        // Hooks for the optional voice tutorial (AuditionTitleNarrator). No effect if nothing listens.
        public event Action<int> MenuFocused;
        public event Action<string> LanguageFocused;
        public event Action GenderScreenShown;
        public event Action GenderSelected;
        public event Action<string> EnteringMain;
        public const string Folder="Assets/Scenes/Audition/";
        public static bool LanguageConfirmed,GenderConfirmed;
        GameObject genderPanel;
        TMP_Text genderTitle,femaleLabel,maleLabel,genderChange;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]static void ResetSession(){LanguageConfirmed=GenderConfirmed=false;}
        public TMP_FontAsset titleFont,bodyFont;
        public Shader backgroundShader;
        public bool hub;
        public GameObject languagePanel,menuPanel;
        public TMP_Text subtitle,profileLabel,notice,continueLabel;
        public TMP_Text[] menuLabels;
        public UnityEngine.UI.RawImage portrait;
        public Texture2D defaultPortrait;
        public UnityEngine.UI.Button ja,en;
        Texture2D preview;
        bool loading,returnPlaced;
        // Sub-scenes were baked with the button top-right; runtime placement keeps them consistent without re-saving scenes.
        public static readonly Vector2 ReturnMin=new Vector2(.77f,.012f),ReturnMax=new Vector2(.98f,.077f);
        public UnityEngine.UI.Button ReturnButton=>hub||!continueLabel?null:continueLabel.GetComponentInParent<UnityEngine.UI.Button>(true);
        public void PlaceReturn(Vector2 min,Vector2 max){var b=ReturnButton;if(!b)return;var r=b.GetComponent<RectTransform>();r.anchorMin=min;r.anchorMax=max;r.offsetMin=r.offsetMax=Vector2.zero;returnPlaced=true;}
        public void SetReturnVisible(bool visible){var b=ReturnButton;if(b&&b.gameObject.activeSelf!=visible)b.gameObject.SetActive(visible);}
        public static string L(string ja,string en)=>GameLanguage.Current==GameLocale.English?en:ja;
        void Start()
        {
            if(hub){languagePanel.SetActive(!LanguageConfirmed);menuPanel.SetActive(LanguageConfirmed&&GenderConfirmed);genderPanel.SetActive(LanguageConfirmed&&!GenderConfirmed);Refresh();}
            else{if(!returnPlaced)PlaceReturn(ReturnMin,ReturnMax);Refresh();}
        }
        void OnEnable(){GameLanguage.Changed+=Refresh;}
        void OnDisable(){GameLanguage.Changed-=Refresh;}
        void OnDestroy(){if(preview)Destroy(preview);}
        public void SetNotice(string message){if(notice)notice.text=message;}
        public void SelectLanguage(bool english){GameLanguage.Select(english?GameLocale.English:GameLocale.Japanese);Refresh();}
        public void Continue(){LanguageConfirmed=true;languagePanel.SetActive(false);menuPanel.SetActive(GenderConfirmed);genderPanel.SetActive(!GenderConfirmed);if(!GenderConfirmed)GenderScreenShown?.Invoke();Refresh();}
        public void ChooseGender(bool male)
        {
            AuditionProfile.SelectGender(male);GenderConfirmed=true;
            if(preview){Destroy(preview);preview=null;}portrait.texture=null;
            genderPanel.SetActive(false);menuPanel.SetActive(true);Refresh();GenderSelected?.Invoke();
        }
        void ShowGender(){menuPanel.SetActive(false);languagePanel.SetActive(false);genderPanel.SetActive(true);GenderScreenShown?.Invoke();}
        void BuildGenderSelection()
        {
            genderPanel=Rect("Player Gender",transform,new Vector2(.1f,.1f),new Vector2(.9f,.51f)).gameObject;
            genderTitle=Label("Choose gender",genderPanel.transform,new Vector2(0,.72f),new Vector2(1,1),"",28);
            femaleLabel=Button("Female",genderPanel.transform,new Vector2(0,.12f),new Vector2(.48f,.65f),"",()=>ChooseGender(false)).GetComponentInChildren<TMP_Text>();
            maleLabel=Button("Male",genderPanel.transform,new Vector2(.52f,.12f),new Vector2(1,.65f),"",()=>ChooseGender(true)).GetComponentInChildren<TMP_Text>();
            genderChange=Button("Gender change",transform,new Vector2(.65f,.035f),new Vector2(.9f,.082f),"",ShowGender).GetComponentInChildren<TMP_Text>();genderChange.fontSize=18;
            genderPanel.SetActive(false);
        }
        public void Open(string name)
        {
            if(loading)return;
            if(hub&&!GenderConfirmed){ShowGender();return;}
            string path=Folder+name+".unity";
            if(!Application.CanStreamedLevelBeLoaded(path)){SetNotice(L("シーンがビルドに登録されていません。","Scene is missing from the build."));return;}
            if(hub&&name=="SeasideMansionAudition")EnteringMain?.Invoke(name);
            loading=true;SceneManager.LoadSceneAsync(path);
        }
        public void Refresh()
        {
            if(!hub){if(continueLabel)continueLabel.text=L("← メニューへ","← Menu");return;}
            if(genderTitle){genderChange.transform.parent.gameObject.SetActive(LanguageConfirmed);genderTitle.text=L("プレイするキャラクターの性別を選択","Choose your character’s gender");
                femaleLabel.text=L("女性でプレイ\n男性1人を攻略・女性ライバル3人","Play as a woman\nOne male love interest · 3 female rivals");
                maleLabel.text=L("男性でプレイ\n女性1人を攻略・男性ライバル3人","Play as a man\nOne female love interest · 3 male rivals");
                genderChange.text=L("性別を変更：","Change gender: ")+(AuditionProfile.IsMale?L("男性","Man"):L("女性","Woman"));}
            RefreshLanguagePresentation();
            menuLabels[0].text=L("01   本編に参加する\n<size=18>海辺の邸宅で、100時間のオーディションへ</size>","01   Enter the audition\n<size=18>Your story begins at the seaside mansion</size>");
            menuLabels[1].text=L("02   声でキャラメイク\n<size=18>AI-0と話して、あなたの人物像をつくる</size>","02   Create with your voice\n<size=18>Talk with AI-0 to shape your character</size>");
            menuLabels[2].text=L("03   写真でキャラメイク\n<size=18>一枚の写真から、物語の中の姿へ</size>","03   Create with a photo\n<size=18>Turn a photograph into your story portrait</size>");
            if(AuditionProfile.IsMale&&!preview){var sprite=AuditionProfile.MalePortrait("yuma");portrait.texture=sprite?sprite.texture:null;portrait.color=sprite?Color.white:Color.clear;if(sprite)portrait.GetComponent<AspectRatioFitter>().aspectRatio=(float)sprite.texture.width/sprite.texture.height;}
            if(!AuditionProfile.IsMale&&defaultPortrait&&!preview){portrait.texture=defaultPortrait;portrait.color=Color.white;portrait.GetComponent<AspectRatioFitter>().aspectRatio=(float)defaultPortrait.width/defaultPortrait.height;}
            var name=AuditionProfile.Name;
            profileLabel.text=(name.Length>0?name:(AuditionProfile.IsMale?L("悠真","Yuma"):L("ひまり","Himari")))+"   /   YOUR PARTICIPANT\n<size=18>"+(name.Length>0?L("人物像 保存済み","Profile saved"):L("未作成なら、"+AuditionProfile.DefaultName+"で参加できます","You can enter with the default character"))+"  ·  "+(File.Exists(AuditionProfile.PortraitPath)?L("画像 保存済み","Portrait saved"):L("画像 未作成","No portrait yet"))+"</size>";
            if(File.Exists(AuditionProfile.PortraitPath)&&!preview){preview=new Texture2D(2,2);if(preview.LoadImage(File.ReadAllBytes(AuditionProfile.PortraitPath))){portrait.texture=preview;portrait.color=Color.white;portrait.GetComponent<AspectRatioFitter>().aspectRatio=(float)preview.width/preview.height;}}
        }
        // Persist the same first screen that Start displays, without loading a participant profile.
        public void PrepareInitialView()
        {
            if(!hub)return;
            languagePanel.SetActive(true);
            menuPanel.SetActive(false);
            RefreshLanguagePresentation();
        }
        void RefreshLanguagePresentation()
        {
            subtitle.text=L("あなたの声から、人物が生まれる。\nAIとつくった自分で、物語の中へ。","Your voice becomes a character.\nEnter the story as someone you create with AI.");
            continueLabel.text=L("この言語でつづける  →","Continue in English  →");
            ja.GetComponent<UnityEngine.UI.Image>().color=GameLanguage.Current==GameLocale.Japanese?new Color(.16f,.28f,.30f,.9f):new Color(.06f,.10f,.14f,.8f);
            en.GetComponent<UnityEngine.UI.Image>().color=GameLanguage.Current==GameLocale.English?new Color(.16f,.28f,.30f,.9f):new Color(.06f,.10f,.14f,.8f);
        }
        public void Build()
        {
            var canvas=gameObject.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=31000;
            var scaler=gameObject.AddComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1440,900);scaler.matchWidthOrHeight=.5f;
            gameObject.AddComponent<GraphicRaycaster>();
            if(!hub)
            {
                var b=Button("Return",transform,ReturnMin,ReturnMax,"← メニューへ",()=>Open("AuditionTitle"));continueLabel=b.GetComponentInChildren<TMP_Text>();
                notice=Label("Saved status",transform,new Vector2(.2f,.005f),new Vector2(.8f,.05f),"",18);return;
            }
            var bg=Rect("Hologram",transform,Vector2.zero,Vector2.one).gameObject.AddComponent<RawImage>();bg.texture=Texture2D.whiteTexture;bg.raycastTarget=false;
            // Asset reference keeps the shader in player builds. Material is instantiated on first runtime frame.
            var backdrop=bg.gameObject.AddComponent<AuditionBackdrop>();backdrop.shader=backgroundShader;
            var shade=Rect("Shade",transform,Vector2.zero,Vector2.one).gameObject.AddComponent<UnityEngine.UI.Image>();shade.color=new Color(.01f,.025f,.035f,.38f);shade.raycastTarget=false;
            Label("Edition",transform,new Vector2(.09f,.86f),new Vector2(.9f,.92f),"A N   A I – N A T I V E   R E A L I T Y   S H O W",17).color=new Color(.5f,.78f,.78f);
            var title=Label("Title",transform,new Vector2(.08f,.65f),new Vector2(.93f,.87f),"100 Hour Audition",85);title.font=titleFont;title.color=new Color(1,.91f,.74f);title.enableAutoSizing=true;title.fontSizeMin=36;title.fontSizeMax=85;
            subtitle=Label("Premise",transform,new Vector2(.10f,.55f),new Vector2(.9f,.66f),"",25);
            var line=Rect("Signal line",transform,new Vector2(.1f,.53f),new Vector2(.90f,.532f)).gameObject.AddComponent<UnityEngine.UI.Image>();line.color=new Color(.4f,.8f,.82f,.5f);line.raycastTarget=false;
            languagePanel=Rect("Language",transform,new Vector2(.1f,.1f),new Vector2(.9f,.51f)).gameObject;
            Label("Choose",languagePanel.transform,new Vector2(0,.73f),new Vector2(1,.97f),"言語を選択 / Choose your language",22);
            ja=Button("Japanese",languagePanel.transform,new Vector2(0,.40f),new Vector2(.48f,.69f),"日本語",()=>SelectLanguage(false));
            en=Button("English",languagePanel.transform,new Vector2(.52f,.40f),new Vector2(1,.69f),"English",()=>SelectLanguage(true));
            continueLabel=Button("Continue",languagePanel.transform,new Vector2(0,.02f),new Vector2(1,.29f),"",Continue).GetComponentInChildren<TMP_Text>();
            menuPanel=Rect("Menu",transform,new Vector2(.1f,.1f),new Vector2(.9f,.51f)).gameObject;
            menuLabels=new TMP_Text[3];string[] scenes={"SeasideMansionAudition","AuditionVoice","AuditionPortrait"};
            for(int i=0;i<3;i++){string target=scenes[i];menuLabels[i]=Button("Menu "+i,menuPanel.transform,new Vector2(0,.67f-i*.33f),new Vector2(.67f,.97f-i*.33f),"",()=>Open(target)).GetComponentInChildren<TMP_Text>();menuLabels[i].alignment=TextAlignmentOptions.MidlineLeft;menuLabels[i].margin=new Vector4(24,0,10,0);menuLabels[i].fontSize=27;}
            var photoFrame=Rect("Participant image",menuPanel.transform,new Vector2(.73f,.37f),new Vector2(1,.96f));portrait=Rect("Portrait",photoFrame,Vector2.zero,Vector2.one).gameObject.AddComponent<RawImage>();portrait.color=new Color(.3f,.55f,.6f,.14f);portrait.raycastTarget=false;var fit=portrait.gameObject.AddComponent<AspectRatioFitter>();fit.aspectMode=AspectRatioFitter.AspectMode.FitInParent;fit.aspectRatio=1;
            profileLabel=Label("Participant",menuPanel.transform,new Vector2(.73f,.02f),new Vector2(1,.33f),"",21);profileLabel.alignment=TextAlignmentOptions.TopLeft;
            Button("Language change",transform,new Vector2(.10f,.035f),new Vector2(.28f,.082f),"日本語 / English",()=>{menuPanel.SetActive(false);if(genderPanel)genderPanel.SetActive(false);languagePanel.SetActive(true);}).GetComponentInChildren<TMP_Text>().fontSize=17;
            notice=Label("Notice",transform,new Vector2(.3f,.025f),new Vector2(.9f,.09f),"",17);
            PrepareInitialView();
        }
        public static RectTransform Rect(string name,Transform parent,Vector2 min,Vector2 max)
        {var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=min;r.anchorMax=max;r.offsetMin=r.offsetMax=Vector2.zero;return r;}
        TMP_Text Label(string name,Transform parent,Vector2 min,Vector2 max,string value,float size)
        {var t=Rect(name,parent,min,max).gameObject.AddComponent<TextMeshProUGUI>();t.font=bodyFont;t.text=value;t.fontSize=size;t.color=new Color(.9f,.93f,.93f);t.alignment=TextAlignmentOptions.MidlineLeft;t.raycastTarget=false;return t;}
        UnityEngine.UI.Button Button(string name,Transform parent,Vector2 min,Vector2 max,string value,UnityEngine.Events.UnityAction action)
        {var r=Rect(name,parent,min,max);var image=r.gameObject.AddComponent<UnityEngine.UI.Image>();image.color=new Color(.06f,.10f,.14f,.85f);var b=r.gameObject.AddComponent<UnityEngine.UI.Button>();b.targetGraphic=image;var colors=b.colors;colors.highlightedColor=new Color(.68f,.95f,1);colors.pressedColor=new Color(.45f,.75f,.8f);b.colors=colors;
            var label=Label("Label",r,new Vector2(.035f,0),new Vector2(.97f,1),value,24);label.alignment=TextAlignmentOptions.Center;
#if UNITY_EDITOR
            // Runtime listeners are rebuilt by Wire after loading; no closures are serialized.
#endif
            b.onClick.AddListener(action);return b;}
        void Awake()
        {
            // Build is used by the editor; runtime callbacks must be wired after deserialization.
            if(hub)BuildGenderSelection();
            if(hub){AddLanguageHover(ja.gameObject,"ja");AddLanguageHover(en.gameObject,"en");ja.onClick.AddListener(()=>SelectLanguage(false));en.onClick.AddListener(()=>SelectLanguage(true));continueLabel.GetComponentInParent<UnityEngine.UI.Button>().onClick.AddListener(Continue);string[] scenes={"SeasideMansionAudition","AuditionVoice","AuditionPortrait"};for(int i=0;i<3;i++){string target=scenes[i];int index=i;var menuButton=menuLabels[i].GetComponentInParent<UnityEngine.UI.Button>(true);menuButton.onClick.AddListener(()=>Open(target));AddHoverRelay(menuButton.gameObject,index);}transform.Find("Language change").GetComponent<UnityEngine.UI.Button>().onClick.AddListener(()=>{menuPanel.SetActive(false);if(genderPanel)genderPanel.SetActive(false);languagePanel.SetActive(true);});gameObject.AddComponent<AuditionTitleNarrator>().Initialize(this);}
            else if(continueLabel)continueLabel.GetComponentInParent<UnityEngine.UI.Button>().onClick.AddListener(()=>Open("AuditionTitle"));
        }
        void AddLanguageHover(GameObject target,string language)
        {
            var trigger=target.GetComponent<EventTrigger>()??target.AddComponent<EventTrigger>();
            var enter=new EventTrigger.Entry{eventID=EventTriggerType.PointerEnter};
            enter.callback.AddListener(_=>LanguageFocused?.Invoke(language));trigger.triggers.Add(enter);
            var exit=new EventTrigger.Entry{eventID=EventTriggerType.PointerExit};
            exit.callback.AddListener(_=>LanguageFocused?.Invoke(null));trigger.triggers.Add(exit);
        }
        void AddHoverRelay(GameObject target,int index)
        {
            var trigger=target.AddComponent<EventTrigger>();
            var entry=new EventTrigger.Entry{eventID=EventTriggerType.PointerEnter};
            entry.callback.AddListener(_=>MenuFocused?.Invoke(index));
            trigger.triggers.Add(entry);
        }
    }
}
