using System;
using System.Linq;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using HundredHour.UI.Audition;
using HundredHour.UI.Participants;
using HundredHour.UI.Choices;
using HundredHour.UI.Timeline;

namespace HundredHour.RealityShow.Editor
{
    public static class RealityShowBuilder
    {
        const string Root="Assets/RealityShow";
        public const string ScenePath="Assets/Scenes/GameScene/RealityShowGame.unity";
        static TMP_FontAsset jp,serif;
        static Color Ink=new Color(.07f,.17f,.25f),White=new Color(.94f,.97f,1),Coral=new Color(1,.54f,.45f);
        [MenuItem("Tools/100Hour/Create Reality Show Game")]
        public static void Build()
        {
            if(EditorApplication.isPlaying||SceneManager.GetActiveScene().isDirty)throw new InvalidOperationException("Save the active scene and stop Play first.");
            if(File.Exists(ScenePath))throw new InvalidOperationException("RealityShowGame already exists; open the scene instead.");
            Folder(Root);Folder(Root+"/Art");Folder(Root+"/Data");Folder(Root+"/UI");
            PrepareFonts();
            serif=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/UI/AuditionHeader/Fonts/AuditionSerif.asset");
            if(jp==null||serif==null)throw new InvalidOperationException("Audition fonts missing.");
            var content=ScriptableObject.CreateInstance<ShowContent>();ShowContent.Populate(content);
            foreach(var p in content.cast)p.portrait=Portrait(p.id);
            content.cast[1].accent=new Color(.70f,.60f,.95f);content.cast[2].accent=new Color(.95f,.65f,.36f);content.cast[3].accent=new Color(.40f,.8f,.76f);
            AssetDatabase.CreateAsset(content,Root+"/Data/ShowContent.asset");
            var studio=Portrait("studio");
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var camera=new GameObject("Main Camera",typeof(Camera),typeof(AudioListener));camera.tag="MainCamera";camera.GetComponent<Camera>().clearFlags=CameraClearFlags.SolidColor;camera.GetComponent<Camera>().backgroundColor=Ink;
            var canvas=new GameObject("Reality Show Canvas",typeof(Canvas),typeof(UnityEngine.UI.CanvasScaler),typeof(UnityEngine.UI.GraphicRaycaster));canvas.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
            var scaler=canvas.GetComponent<UnityEngine.UI.CanvasScaler>();scaler.uiScaleMode=UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);scaler.screenMatchMode=UnityEngine.UI.CanvasScaler.ScreenMatchMode.Expand;
            var root=Rect("RealityShow",canvas.transform);root.anchorMin=root.anchorMax=root.pivot=new Vector2(.5f,.5f);root.sizeDelta=new Vector2(1920,1080);
            var photo=Image("Studio",root,studio);Box(photo.rectTransform,0,0,1920,1080);photo.color=new Color(.4f,.55f,.60f,.6f);
            var shade=Panel("Atmosphere",root,0,0,1920,1080,new Color(.025f,.085f,.13f,.90f),new Color(.12f,.26f,.32f,.92f),0);
            var view=root.gameObject.AddComponent<ShowView>();
            BuildTitle(root,view,content);
            BuildGame(root,view,content);
            ConfigureFeed(view);
            ConfigureDialogue(view);
            var controller=root.gameObject.AddComponent<ShowController>();controller.content=content;controller.transitionGroup=view.gameScreen.AddComponent<CanvasGroup>();
            view.titleScreen.SetActive(true);view.gameScreen.SetActive(false);
            PrefabUtility.SaveAsPrefabAsset(root.gameObject,Root+"/UI/RealityShowUI.prefab");
            var es=new GameObject("EventSystem");es.SetActive(false);es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            var input=es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();input.AssignDefaultActions();es.SetActive(true);
            AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(scene,ScenePath);Selection.activeGameObject=root.gameObject;
        }
        static void BuildTitle(RectTransform root,ShowView v,ShowContent content)
        {
            var screen=Rect("TitleScreen",root);Box(screen,0,0,1920,1080);v.titleScreen=screen.gameObject;
            Panel("Glass",screen,58,66,1804,948,new Color(.78f,.91f,.95f,.94f),new Color(.61f,.83f,.86f,.89f),46);
            Label("Eyebrow",screen,"A REALITY SHOW ABOUT THE WORDS WE KEEP",108,118,1180,42,22,Ink);
            var title=Rect("AuditionTitle",screen);Box(title,100,180,900,280);
            var first=Label("First",title,"100Hour",0,0,900,150,104,Ink,serif);
            var second=Label("Second",title,"Audition",0,130,900,150,104,Coral,serif);
            title.gameObject.AddComponent<AuditionTitleView>().Configure(first,second);
            v.titleDescription=Label("Description",screen,"",108,482,870,220,29,Ink);
            var description=v.titleDescription.gameObject.AddComponent<AuditionDescriptionView>();description.Configure(v.titleDescription,Array.Empty<TMP_Text>());
            description.SetDescription("あなたは、覚悟と期待と共にこの舞台にやってきた。\n\n100時間の出会い。残るのは、たった一人。\n会話の記憶を次の言葉へ。手札とAGIの力を借りて、\n一度は失った未来を、もう一度つかみ取る。");
            v.easyButton=Button("Easy",screen,"● Easy",108,738,190,55);v.normalButton=Button("Normal",screen,"Normal",310,738,190,55);v.hardButton=Button("Hard",screen,"Hard",512,738,190,55);
            v.aiButton=Button("AI",screen,"AI会話 OFF",724,738,238,55);
            v.startButton=Button("Start",screen,"新しく始める   →",108,820,405,80,true);v.continueButton=Button("Continue",screen,"続きから",539,820,423,80);
            v.modeLabel=Label("Mode",screen,"ローカル会話 · Easy",110,923,870,34,20,Ink);
            Label("PortraitCaption",screen,"FOUR HEARTS.  ONE MORE CHANCE.",1060,130,720,40,20,Ink);
            for(int i=0;i<4;i++)
            {
                var card=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/UI/ParticipantCards/ParticipantCard.prefab"),screen);
                Box((RectTransform)card.transform,1060+(i%2)*350,200+(i/2)*350,324,322);LocalFonts(card);
                var p=content.cast[i];card.GetComponent<ParticipantCardController>().SetData(new ParticipantCardData{displayName=p.displayName,portrait=p.portrait,subtitle=p.role,number=i+1,status=i==0?"YOU":"ON AIR",stars=20000,support=.25f,accent=p.accent});
                Record(card);
            }
        }
        static void BuildGame(RectTransform root,ShowView v,ShowContent content)
        {
            var screen=Rect("GameScreen",root);Box(screen,0,0,1920,1080);v.gameScreen=screen.gameObject;
            v.header=Label("Header",screen,"100Hour Audition",38,22,1000,55,38,White,serif);v.header.richText=true;
            v.resources=Label("Resources",screen,"",40,78,1260,34,23,new Color(.68f,.86f,.9f));
            v.homeButton=Button("Home",screen,"保存してタイトル",1650,26,228,54);
            var timer=Rect("Clock",screen);Box(timer,1360,18,260,86);
            var h=Label("Hours",timer,"100",0,0,100,62,43,White,serif);var m=Label("Minutes",timer,"00",100,0,70,62,43,White,serif);var s=Label("Seconds",timer,"00",180,0,70,62,43,White,serif);
            Label("Units",timer,"HOURS      MIN        SEC",4,60,250,22,13,new Color(.65f,.83f,.86f));v.clock=timer.gameObject.AddComponent<AuditionClockView>();v.clock.Configure(h,m,s);
            v.castCards=new ParticipantCardController[4];
            for(int i=0;i<4;i++)
            {
                var card=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/UI/ParticipantCards/ParticipantCard.prefab"),screen);card.name="Cast"+i;
                Box((RectTransform)card.transform,40+i*314,132,298,270);LocalFonts(card);v.castCards[i]=card.GetComponent<ParticipantCardController>();
                var p=content.cast[i];v.castCards[i].SetData(new ParticipantCardData{displayName=p.displayName,portrait=p.portrait,subtitle=p.role,number=i+1,status=i==0?"YOU":"ON AIR",stars=20000,support=.25f,accent=p.accent});Record(card);
            }
            var talk=Panel("Conversation",screen,40,423,1240,377,new Color(.07f,.15f,.21f,.98f),new Color(.04f,.09f,.14f,.98f),28);
            v.chapterLabel=Label("Chapter",talk,"",28,17,950,30,18,new Color(.5f,.77f,.80f));
            v.speakerPortrait=Image("SpeakerPortrait",talk,content.Person("yuto").portrait);Box(v.speakerPortrait.rectTransform,28,58,78,96);v.speakerPortrait.preserveAspect=true;
            v.speaker=Label("Speaker",talk,"",127,50,1010,32,22,Coral);
            v.dialogue=Label("Dialogue",talk,"",127,89,1065,96,25,White);
            var options=Rect("Choices",talk);Box(options,24,193,1192,178);v.choices=ChoiceMenuFactory.Create(options,jp,true);
            var choiceStyle=new SerializedObject(v.choices.GetComponent<ChoiceMenuView>());choiceStyle.FindProperty("fontSize").floatValue=22;choiceStyle.FindProperty("rowHeight").floatValue=31;choiceStyle.FindProperty("spacing").floatValue=0;choiceStyle.ApplyModifiedPropertiesWithoutUndo();
            v.handLabel=Label("HandHeader",screen,"",44,814,1240,33,21,new Color(.72f,.86f,.89f));
            var hand=Rect("Hand",screen);Box(hand,40,858,1240,180);
            var layout=hand.gameObject.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();layout.spacing=10;layout.childControlWidth=layout.childControlHeight=true;layout.childForceExpandWidth=false;layout.childForceExpandHeight=true;
            v.handButtons=new UnityEngine.UI.Button[7];v.handTexts=new TMP_Text[7];
            for(int i=0;i<7;i++)
            {
                v.handButtons[i]=Button("Card"+i,hand,"",0,0,166,180);var element=v.handButtons[i].gameObject.AddComponent<UnityEngine.UI.LayoutElement>();element.flexibleWidth=1;
                v.handTexts[i]=v.handButtons[i].GetComponentInChildren<TMP_Text>();v.handTexts[i].alignment=TextAlignmentOptions.TopLeft;v.handTexts[i].fontSize=18;v.handTexts[i].richText=true;v.handTexts[i].margin=new Vector4(14,14,14,10);
            }
            var side=Panel("AriaPanel",screen,1310,132,570,906,new Color(.76f,.90f,.92f,.97f),new Color(.59f,.78f,.84f,.96f),30);
            Label("Aria",side,"沙織 / ご相談ノート",28,20,510,40,24,Ink);
            v.notice=Label("Notice",side,"会話から、未来を変える。",28,68,510,100,22,Ink);
            v.memoryTab=Button("MemoryTab",side,"記憶",24,182,164,46);v.feedTab=Button("FeedTab",side,"放送ログ",201,182,164,46);v.deckTab=Button("DeckTab",side,"デッキ",378,182,166,46);
            var memory=Rect("MemoryPanel",side);Box(memory,24,246,522,347);v.memoryPanel=memory.gameObject;
            v.memoryPageLabel=Label("Page",memory,"MEMORY",0,0,520,58,18,Ink);
            v.memoryButtons=new UnityEngine.UI.Button[4];v.memoryTexts=new TMP_Text[4];
            for(int i=0;i<4;i++){v.memoryButtons[i]=Button("Memory"+i,memory,"",0,66+i*53,520,48);v.memoryTexts[i]=v.memoryButtons[i].GetComponentInChildren<TMP_Text>();v.memoryTexts[i].alignment=TextAlignmentOptions.MidlineLeft;v.memoryTexts[i].margin=new Vector4(12,0,12,0);v.memoryTexts[i].fontSize=18;v.memoryTexts[i].richText=true;}
            v.memoryPrevious=Button("Previous",memory,"←",0,282,55,40);v.memoryNext=Button("Next",memory,"→",65,282,55,40);
            v.askButton=Button("Ask",memory,"尋ねる 1",136,282,119,40);v.shareButton=Button("Share",memory,"伝える 1",265,282,119,40);v.connectButton=Button("Connect",memory,"結ぶ 1",394,282,126,40);
            var feed=Rect("FeedPanel",side);Box(feed,24,245,522,368);v.feedPanel=feed.gameObject;
            var feedObj=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/UI/Timeline/TimelineFeed.prefab"),feed);
            Box((RectTransform)feedObj.transform,0,0,522,350);v.feed=feedObj.GetComponent<TimelineFeedController>();v.feed.SetPageSize(3);Record(feedObj);feed.gameObject.SetActive(false);
            var deck=Rect("DeckPanel",side);Box(deck,28,246,514,352);v.deckPanel=deck.gameObject;v.deckText=Label("DeckText",deck,"",0,0,514,352,22,Ink);deck.gameObject.SetActive(false);
            v.motivation=Label("Motivation",side,"",28,617,510,132,18,Ink);
            v.perspectiveButton=Button("Perspective",side,"他人の視点 / AGI 1",24,766,250,52);v.futureButton=Button("Future",side,"未来観測 / AGI 2",294,766,250,52);
            v.weatherButton=Button("Weather",side,"偶然の発動 / AGI 2",24,836,250,52);v.acquireButton=Button("Acquire",side,"カード取得 / AGI 1",294,836,250,52);
        }
        [MenuItem("Tools/100Hour/Refresh Reality Show Layout")]
        public static void RefreshLayout()
        {
            if(EditorApplication.isPlaying||SceneManager.GetActiveScene().path!=ScenePath)throw new InvalidOperationException("Open RealityShowGame outside Play.");
            PrepareFonts();
            var view=UnityEngine.Object.FindFirstObjectByType<ShowView>(FindObjectsInactive.Include);
            foreach(var text in view.GetComponentsInChildren<TMP_Text>(true))if(text.font.name.Contains("Japanese"))text.font=jp;
            view.choices.GetComponent<ChoiceMenuView>().Font=jp;
            var title=(RectTransform)view.titleScreen.transform.Find("AuditionTitle");Box(title,100,180,900,280);
            Box((RectTransform)title.Find("First"),0,0,900,150);Box((RectTransform)title.Find("Second"),0,130,900,150);
            ConfigureFeed(view);
            ConfigureDialogue(view);
            view.motivation.fontSize=18;
            foreach(var t in view.memoryTexts)t.fontSize=18;
            view.handButtons[0].transform.parent.GetComponent<UnityEngine.UI.HorizontalLayoutGroup>().childForceExpandWidth=false;
            Record(view.gameObject);
            PrefabUtility.SaveAsPrefabAsset(view.gameObject,Root+"/UI/RealityShowUI.prefab");EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        }
        static void ConfigureFeed(ShowView view)
        {
            var panel=(RectTransform)view.feedPanel.transform;Box(panel,24,245,522,637);
            var viewport=panel.Find("FeedViewport") as RectTransform;
            if(viewport==null)
            {
                viewport=Rect("FeedViewport",panel);Fill(viewport);
                var image=viewport.gameObject.AddComponent<UnityEngine.UI.Image>();image.color=Color.white;
                viewport.gameObject.AddComponent<UnityEngine.UI.Mask>().showMaskGraphic=false;
                var content=(RectTransform)view.feed.transform;content.SetParent(viewport,false);
                content.anchorMin=new Vector2(0,1);content.anchorMax=new Vector2(1,1);content.pivot=new Vector2(.5f,1);content.anchoredPosition=Vector2.zero;content.sizeDelta=new Vector2(0,350);
                var scroll=panel.gameObject.AddComponent<UnityEngine.UI.ScrollRect>();scroll.viewport=viewport;scroll.content=content;scroll.horizontal=false;scroll.vertical=true;scroll.movementType=UnityEngine.UI.ScrollRect.MovementType.Clamped;scroll.scrollSensitivity=25;
            }
            foreach(var t in view.feed.GetComponentsInChildren<TMP_Text>(true))if(t.font.name.Contains("Japanese"))t.font=jp;
            Record(view.feed.gameObject);
            var row=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/UI/ShowTimelineRow.prefab");
            if(row==null)
            {
                var source=PrefabUtility.LoadPrefabContents("Assets/UI/Timeline/TimelineRow.prefab");LocalFonts(source);
                row=PrefabUtility.SaveAsPrefabAsset(source,Root+"/UI/ShowTimelineRow.prefab");PrefabUtility.UnloadPrefabContents(source);
            }
            var config=new SerializedObject(view.feed.GetComponent<TimelineFeedView>());config.FindProperty("rowPrefab").objectReferenceValue=row.GetComponent<TimelineRowView>();config.ApplyModifiedPropertiesWithoutUndo();
            Box((RectTransform)view.deckPanel.transform,28,246,514,625);view.deckText.rectTransform.sizeDelta=new Vector2(514,625);view.feed.SetPageSize(2);
        }
        static void ConfigureDialogue(ShowView view)
        {
            var text=view.dialogue;var talk=text.transform.parent;
            if(talk.name!="DialogueViewport")
            {
                var scroll=Rect("DialogueScroll",talk);Box(scroll,127,89,1065,110);
                var viewport=Rect("DialogueViewport",scroll);Fill(viewport);
                viewport.gameObject.AddComponent<UnityEngine.UI.Image>();viewport.gameObject.AddComponent<UnityEngine.UI.Mask>().showMaskGraphic=false;
                text.transform.SetParent(viewport,false);text.rectTransform.anchorMin=new Vector2(0,1);text.rectTransform.anchorMax=new Vector2(1,1);text.rectTransform.pivot=new Vector2(.5f,1);text.rectTransform.anchoredPosition=Vector2.zero;text.rectTransform.sizeDelta=new Vector2(0,110);
                var fit=text.gameObject.AddComponent<UnityEngine.UI.ContentSizeFitter>();fit.verticalFit=UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
                var movement=scroll.gameObject.AddComponent<UnityEngine.UI.ScrollRect>();movement.content=text.rectTransform;movement.viewport=viewport;movement.horizontal=false;movement.movementType=UnityEngine.UI.ScrollRect.MovementType.Clamped;movement.scrollSensitivity=25;
                Label("ScrollHint",talk,"台詞をスクロール / 全文は放送ログへ",700,53,490,27,15,new Color(.56f,.72f,.78f));
            }
            text.fontSize=23;text.overflowMode=TextOverflowModes.Overflow;
            Box((RectTransform)view.choices.transform.parent,24,205,1192,168);
            var config=new SerializedObject(view.choices.GetComponent<ChoiceMenuView>());config.FindProperty("fontSize").floatValue=20;config.FindProperty("rowHeight").floatValue=28;config.ApplyModifiedPropertiesWithoutUndo();
        }
        static void PrepareFonts()
        {
            Folder(Root+"/Fonts");
            string primary=Root+"/Fonts/ShowJapanese.asset",fallbackPath=Root+"/Fonts/ShowJapaneseFallback.asset";
            if(!File.Exists(primary))AssetDatabase.CopyAsset("Assets/UI/AuditionHeader/Fonts/AuditionJapanese.asset",primary);
            if(!File.Exists(fallbackPath))
            {
                AssetDatabase.CopyAsset("Assets/UI/AuditionHeader/Fonts/AuditionJapaneseFallback.asset",fallbackPath);
                var dynamicFont=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fallbackPath);dynamicFont.isMultiAtlasTexturesEnabled=true;dynamicFont.ClearFontAssetData();
                var settings=new SerializedObject(dynamicFont);settings.FindProperty("m_AtlasWidth").intValue=1024;settings.FindProperty("m_AtlasHeight").intValue=1024;settings.FindProperty("m_ClearDynamicDataOnBuild").boolValue=true;settings.ApplyModifiedPropertiesWithoutUndo();dynamicFont.ClearFontAssetData(true);EditorUtility.SetDirty(dynamicFont);
            }
            jp=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(primary);jp.fallbackFontAssetTable=new System.Collections.Generic.List<TMP_FontAsset>{AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fallbackPath)};EditorUtility.SetDirty(jp);
        }
        static void Folder(string path){if(AssetDatabase.IsValidFolder(path))return;Folder(Path.GetDirectoryName(path).Replace('\\','/'));AssetDatabase.CreateFolder(Path.GetDirectoryName(path).Replace('\\','/'),Path.GetFileName(path));}
        static Sprite Portrait(string id)
        {
            string path=Root+"/Art/"+(id=="himari"?"himari_bob.png":id+".jpg");
            if(!File.Exists(path))File.Copy("/Users/matsumurakatsuhiro/Documents/AiProgramLib/100AI/GrokProto/static/img/"+id+".jpg",path);
            AssetDatabase.ImportAsset(path);var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.maxTextureSize=1024;importer.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
        static void LocalFonts(GameObject go){foreach(var t in go.GetComponentsInChildren<TMP_Text>(true))t.font=jp;}
        static void Record(GameObject go){foreach(var c in go.GetComponentsInChildren<Component>(true))PrefabUtility.RecordPrefabInstancePropertyModifications(c);}
        static RectTransform Rect(string name,Transform parent){var r=(RectTransform)new GameObject(name,typeof(RectTransform)).transform;r.SetParent(parent,false);return r;}
        static void Box(RectTransform r,float x,float y,float w,float h){r.anchorMin=r.anchorMax=new Vector2(0,1);r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);}
        static void Fill(RectTransform r){r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;}
        static RectTransform Panel(string name,Transform parent,float x,float y,float w,float h,Color top,Color bottom,float radius)
        {var r=Rect(name,parent);Box(r,x,y,w,h);var g=r.gameObject.AddComponent<GlassSurfaceGraphic>();g.SetStyle(top,bottom,new Color(1,1,1,.28f),radius);g.raycastTarget=false;return r;}
        static TMP_Text Label(string name,Transform parent,string value,float x,float y,float w,float h,float size,Color color,TMP_FontAsset font=null)
        {var r=Rect(name,parent);Box(r,x,y,w,h);var t=r.gameObject.AddComponent<TextMeshProUGUI>();t.font=font??jp;t.fontSize=size;t.color=color;t.text=value;t.raycastTarget=false;t.richText=false;t.textWrappingMode=TextWrappingModes.Normal;t.overflowMode=TextOverflowModes.Ellipsis;return t;}
        static UnityEngine.UI.Image Image(string name,Transform parent,Sprite sprite){var r=Rect(name,parent);var image=r.gameObject.AddComponent<UnityEngine.UI.Image>();image.sprite=sprite;image.raycastTarget=false;return image;}
        static UnityEngine.UI.Button Button(string name,Transform parent,string title,float x,float y,float w,float h,bool accent=false)
        {
            var r=Panel(name,parent,x,y,w,h,accent?new Color(.98f,.47f,.38f):new Color(.20f,.35f,.43f,.98f),accent?new Color(.8f,.30f,.25f):new Color(.11f,.24f,.31f,.98f),14);
            var graphic=r.GetComponent<GlassSurfaceGraphic>();graphic.raycastTarget=true;var b=r.gameObject.AddComponent<UnityEngine.UI.Button>();b.targetGraphic=graphic;
            var colors=b.colors;colors.highlightedColor=new Color(1,.83f,.73f);colors.pressedColor=new Color(.69f,.87f,.9f);colors.disabledColor=new Color(.53f,.56f,.58f,.7f);b.colors=colors;
            var t=Label("Label",r,title,0,0,w,h,22,White);Fill(t.rectTransform);t.alignment=TextAlignmentOptions.Center;return b;
        }
    }
}
