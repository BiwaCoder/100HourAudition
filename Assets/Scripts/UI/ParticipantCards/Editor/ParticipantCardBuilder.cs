using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace HundredHour.UI.Participants.Editor
{
    /// <summary>Unity APIでPrefabと独立デモを生成。既存アセットの上書きはしない。</summary>
    public static class ParticipantCardBuilder
    {
        const string Root="Assets/UI/ParticipantCards";
        const string PrefabPath=Root+"/ParticipantCard.prefab";
        const string ScenePath="Assets/Scenes/Library/ParticipantCardsDemo.unity";
        static TMP_FontAsset font;
        static Color Navy=new Color(.07f,.12f,.21f);

        [MenuItem("Tools/Reality Show/Create Participant Card Demo")]
        public static void CreateDemo()
        {
            if(EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play Mode first.");
            if(SceneManager.GetActiveScene().isDirty) throw new InvalidOperationException("Save the current scene first.");
            if(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath)!=null || System.IO.File.Exists(ScenePath))
                throw new InvalidOperationException("Participant prefab/demo already exists. Open the existing assets.");
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
            if(font==null) throw new InvalidOperationException("TMP Essential Resources are required.");
            if(!AssetDatabase.IsValidFolder(Root+"/Art")) AssetDatabase.CreateFolder(Root,"Art");
            var portraits=new[] {
                Portrait("CoralPortrait",new Color(.78f,.56f,.53f),new Color(.24f,.13f,.19f),new Color(.91f,.65f,.53f),new Color(.9f,.78f,.67f)),
                Portrait("BluePortrait",new Color(.37f,.69f,.8f),new Color(.13f,.29f,.36f),new Color(.92f,.71f,.55f),new Color(.65f,.84f,.84f)),
                Portrait("LilacPortrait",new Color(.62f,.58f,.77f),new Color(.24f,.2f,.28f),new Color(.85f,.65f,.55f),new Color(.56f,.62f,.8f))
            };
            var material=new Material(Shader.Find("HundredHour/UI/PortraitGrayscale"));
            AssetDatabase.CreateAsset(material,Root+"/Art/PortraitGrayscale.mat");
            var prototype=BuildCard(portraits[0],material);
            prototype.GetComponent<ParticipantCardView>().Portrait.material=material;
            var prefab=PrefabUtility.SaveAsPrefabAsset(prototype,PrefabPath);
            UnityEngine.Object.DestroyImmediate(prototype);
            var camera=new GameObject("Main Camera",typeof(Camera),typeof(AudioListener));
            camera.tag="MainCamera"; camera.transform.position=new Vector3(0,0,-10);
            camera.GetComponent<Camera>().clearFlags=CameraClearFlags.SolidColor;
            camera.GetComponent<Camera>().backgroundColor=new Color(.045f,.075f,.12f);
            var canvasGo=new GameObject("Reality Show Canvas",typeof(Canvas),typeof(UnityEngine.UI.CanvasScaler),typeof(UnityEngine.UI.GraphicRaycaster));
            canvasGo.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
            var scaler=canvasGo.GetComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode=UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(1280,800); scaler.matchWidthOrHeight=.5f;
            var bg=Surface("Backdrop",canvasGo.transform,new Color(.08f,.16f,.23f),new Color(.035f,.055f,.10f),0);
            Stretch(bg.rectTransform);
            var eyebrow=Text("Eyebrow",canvasGo.transform,"AFTER THE SPOTLIGHT   /   SEASON 01",15,new Color(.50f,.77f,.81f));
            Place(eyebrow.rectTransform,0,344,1120,28);
            var title=Text("Title",canvasGo.transform,"Meet the cast.",40,Color.white); Place(title.rectTransform,0,298,1120,60);
            var feedback=Text("Feedback",canvasGo.transform,"Live standings  /  2 active · 1 eliminated",16,new Color(.61f,.69f,.77f));
            Place(feedback.rectTransform,0,250,1120,30);
            var holder=Rect("Participant Cards",canvasGo.transform); Place(holder,0,-30,1120,500);
            var grid=holder.gameObject.AddComponent<UnityEngine.UI.GridLayoutGroup>();
            grid.cellSize=new Vector2(352,500); grid.spacing=new Vector2(32,0);
            grid.constraint=UnityEngine.UI.GridLayoutGroup.Constraint.FixedColumnCount; grid.constraintCount=3;
            var cards=new ParticipantCardController[3];
            string[] names={"Runa Aizawa","Aoi Nakagawa","Sora Bennett"};
            string[] subtitles={"22  ·  Aquarium keeper  ·  Kyoto","24  ·  Surf instructor  ·  Fukuoka","26  ·  Bartender  ·  Tokyo"};
            for(int i=0;i<3;i++)
            {
                var go=(GameObject)PrefabUtility.InstantiatePrefab(prefab,holder);
                go.name=names[i]; cards[i]=go.GetComponent<ParticipantCardController>();
                cards[i].SetData(new ParticipantCardData { portrait=portraits[i],displayName=names[i],subtitle=subtitles[i],number=i==2 ? 7 : i+1,
                    stars=i==0 ? 342000 : i==1 ? 231000 : 48000, support=i==0 ? .28f : i==1 ? .19f : .04f,
                    status=i==0 ? "FRONTRUNNER" : "RISING",accent=i==1 ? new Color(.39f,.85f,.79f) : new Color(1,.54f,.58f),eliminated=i==2 });
                PrefabUtility.RecordPrefabInstancePropertyModifications(cards[i]);
            }
            var demo=canvasGo.AddComponent<Demo.ParticipantCardsDemo>(); demo.Configure(cards,feedback);
            Button(canvasGo.transform,"TOGGLE SORA: IN / OUT",-361,-339,demo.ToggleElimination);
            Button(canvasGo.transform,"+10K STARS TO RUNA",0,-339,demo.AddStars);
            Button(canvasGo.transform,"RESET CAST",361,-339,demo.ResetCards);
            var es=new GameObject("EventSystem"); es.SetActive(false);
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            var module=es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            module.actionsAsset=AssetDatabase.LoadAssetAtPath<UnityEngine.InputSystem.InputActionAsset>("Assets/InputSystem_Actions.inputactions");
            var refs=AssetDatabase.LoadAllAssetsAtPath("Assets/InputSystem_Actions.inputactions").OfType<UnityEngine.InputSystem.InputActionReference>().ToArray();
            module.point=refs.First(r=>r.name=="UI/Point"); module.leftClick=refs.First(r=>r.name=="UI/Click");
            module.move=refs.First(r=>r.name=="UI/Navigate"); module.submit=refs.First(r=>r.name=="UI/Submit"); module.cancel=refs.First(r=>r.name=="UI/Cancel");
            es.SetActive(true);
            AssetDatabase.SaveAssets(); EditorSceneManager.SaveScene(scene,ScenePath);
            Selection.activeGameObject=cards[0].gameObject;
        }

        static GameObject BuildCard(Sprite fallback,Material material)
        {
            var root=Rect("ParticipantCard",null); root.sizeDelta=new Vector2(352,500);
            var border=root.gameObject.AddComponent<RoundedCardGraphic>(); border.raycastTarget=false;
            border.SetStyle(new Color(.85f,.92f,1,.8f),new Color(.85f,.92f,1,.8f),25);
            var content=Surface("ClippedContent",root,Navy,Navy,24); Stretch(content.rectTransform,1.5f);
            content.gameObject.AddComponent<UnityEngine.UI.Mask>().showMaskGraphic=true;
            var imageGo=Rect("Portrait",content.transform).gameObject;
            var image=imageGo.AddComponent<UnityEngine.UI.Image>(); image.sprite=fallback; image.raycastTarget=false;
            var fit=imageGo.AddComponent<UnityEngine.UI.AspectRatioFitter>(); fit.aspectMode=UnityEngine.UI.AspectRatioFitter.AspectMode.EnvelopeParent;
            fit.aspectRatio=fallback.rect.width/fallback.rect.height;
            var shade=Surface("ReadabilityGradient",content.transform,new Color(.025f,.06f,.12f,0),new Color(.025f,.06f,.12f,.98f),0); Stretch(shade.rectTransform);
            var pill=Surface("StatusPill",content.transform,Color.white,Color.white,16);
            Corner(pill.rectTransform,new Vector2(0,1),new Vector2(18,-18),new Vector2(145,29));
            var status=Text("Status",pill.transform,"",12,Navy); Stretch(status.rectTransform,8,0); status.alignment=TextAlignmentOptions.Center; status.fontStyle=FontStyles.Bold;
            var badge=Surface("NumberBadge",content.transform,new Color(1,1,1,.85f),new Color(1,1,1,.85f),18);
            Corner(badge.rectTransform,Vector2.one,new Vector2(-18,-18),new Vector2(46,30));
            var number=Text("Number",badge.transform,"",15,Navy); Stretch(number.rectTransform); number.alignment=TextAlignmentOptions.Center; number.fontStyle=FontStyles.Bold;
            var name=Text("Name",content.transform,"",27,Color.white); Bottom(name.rectTransform,96,42,20); name.fontStyle=FontStyles.Bold;
            var subtitle=Text("Subtitle",content.transform,"",13,new Color(.88f,.92f,.96f)); Bottom(subtitle.rectTransform,72,28,20);
            var track=Surface("SupportTrack",content.transform,new Color(.8f,.87f,.94f,.3f),new Color(.8f,.87f,.94f,.3f),3);
            Bottom(track.rectTransform,49,5,20);
            var fill=Surface("SupportFill",track.transform,Color.white,Color.white,3); Stretch(fill.rectTransform);
            var stars=Text("Stars",content.transform,"",14,Color.white); Bottom(stars.rectTransform,17,24,20); stars.characterSpacing=4;
            var support=Text("SupportPercent",content.transform,"",14,Color.white); Bottom(support.rectTransform,17,24,20); support.alignment=TextAlignmentOptions.Right;
            var stampBorder=Surface("OutStamp",content.transform,Color.white,Color.white,10);
            Place(stampBorder.rectTransform,0,12,156,72); stampBorder.rectTransform.localRotation=Quaternion.Euler(0,0,12);
            var stamp=Surface("StampFill",stampBorder.transform,new Color(.63f,.2f,.3f,.96f),new Color(.43f,.12f,.2f,.96f),7); Stretch(stamp.rectTransform,3);
            var outLabel=Text("OUT",stamp.transform,"OUT",37,Color.white); Stretch(outLabel.rectTransform); outLabel.alignment=TextAlignmentOptions.Center; outLabel.fontStyle=FontStyles.Bold|FontStyles.Italic;
            var view=root.gameObject.AddComponent<ParticipantCardView>();
            view.Configure(image,fit,fallback,material,name,subtitle,number,stars,support,status,pill,fill,stampBorder.gameObject);
            var controller=root.gameObject.AddComponent<ParticipantCardController>(); controller.Refresh();
            return root.gameObject;
        }
        static RectTransform Rect(string name,Transform parent)
        {
            var rt=(RectTransform)new GameObject(name,typeof(RectTransform)).transform; if(parent!=null) rt.SetParent(parent,false); return rt;
        }
        static RoundedCardGraphic Surface(string name,Transform parent,Color top,Color bottom,float radius)
        {
            var g=Rect(name,parent).gameObject.AddComponent<RoundedCardGraphic>(); g.raycastTarget=false; g.SetStyle(top,bottom,radius); return g;
        }
        static TMP_Text Text(string name,Transform parent,string value,float size,Color color)
        {
            var t=Rect(name,parent).gameObject.AddComponent<TextMeshProUGUI>(); t.font=font; t.text=value; t.fontSize=size;
            t.color=color; t.raycastTarget=false; t.richText=false; t.alignment=TextAlignmentOptions.MidlineLeft;
            t.enableAutoSizing=true; t.fontSizeMin=size*.65f; t.fontSizeMax=size; t.textWrappingMode=TextWrappingModes.NoWrap;
            t.overflowMode=TextOverflowModes.Ellipsis; return t;
        }
        static void Stretch(RectTransform r,float margin=0) => Stretch(r,margin,margin);
        static void Stretch(RectTransform r,float x,float y) { r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=new Vector2(x,y);r.offsetMax=new Vector2(-x,-y); }
        static void Place(RectTransform r,float x,float y,float w,float h) { r.anchorMin=r.anchorMax=r.pivot=new Vector2(.5f,.5f);r.anchoredPosition=new Vector2(x,y);r.sizeDelta=new Vector2(w,h); }
        static void Corner(RectTransform r,Vector2 anchor,Vector2 pos,Vector2 size) { r.anchorMin=r.anchorMax=r.pivot=anchor;r.anchoredPosition=pos;r.sizeDelta=size; }
        static void Bottom(RectTransform r,float y,float h,float margin) { r.anchorMin=Vector2.zero;r.anchorMax=new Vector2(1,0);r.pivot=new Vector2(.5f,0);r.sizeDelta=new Vector2(-margin*2,h);r.anchoredPosition=new Vector2(0,y); }
        static void Button(Transform parent,string label,float x,float y,UnityAction action)
        {
            var bg=Surface(label,parent,new Color(.15f,.23f,.32f),new Color(.15f,.23f,.32f),10); Place(bg.rectTransform,x,y,322,44);bg.raycastTarget=true;
            var b=bg.gameObject.AddComponent<UnityEngine.UI.Button>();b.targetGraphic=bg;
            UnityEditor.Events.UnityEventTools.AddPersistentListener(b.onClick,action);
            var t=Text("Label",bg.transform,label,13,Color.white);Stretch(t.rectTransform,10,0);t.alignment=TextAlignmentOptions.Center;t.fontStyle=FontStyles.Bold;
        }
        static Sprite Portrait(string name,Color background,Color hair,Color skin,Color shirt)
        {
            const int w=384,h=576;
            var texture=new Texture2D(w,h,TextureFormat.RGBA32,false) { name=name,filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp };
            var pixels=new Color[w*h];
            bool Ellipse(float x,float y,float cx,float cy,float rx,float ry) => (x-cx)*(x-cx)/(rx*rx)+(y-cy)*(y-cy)/(ry*ry)<1;
            for(int py=0;py<h;py++) for(int px=0;px<w;px++)
            {
                float x=(px+.5f)/w*2-1,y=(py+.5f)/h;
                Color c=Color.Lerp(background*.6f,background,y); c.a=1;
                if(((px+py)/15)%2==0)c=Color.Lerp(c,Color.white,.05f);
                if(Ellipse(x,y,0,.55f,.49f,.32f))c=hair;
                if(Ellipse(x,y,0,.04f,.78f,.27f))c=shirt;
                if(Mathf.Abs(x)<.105f && y>.21f && y<.44f)c=skin*.9f;
                if(Ellipse(x,y,0,.54f,.31f,.21f))c=skin;
                if(Ellipse(x,y,-.24f,.72f,.26f,.16f) || Ellipse(x,y,.27f,.72f,.22f,.17f))c=hair;
                if(Ellipse(x,y,-.115f,.55f,.024f,.016f) || Ellipse(x,y,.115f,.55f,.024f,.016f))c=new Color(.1f,.16f,.22f);
                if(Ellipse(x,y,-.19f,.5f,.055f,.014f)||Ellipse(x,y,.19f,.5f,.055f,.014f))c=Color.Lerp(skin,new Color(.85f,.3f,.35f),.28f);
                if(y>.447f && y<.453f && Mathf.Abs(x)<.065f)c=new Color(.58f,.29f,.29f);
                c.a=1;pixels[py*w+px]=c;
            }
            texture.SetPixels(pixels);texture.Apply();
            AssetDatabase.CreateAsset(texture,Root+"/Art/"+name+".asset");
            var sprite=Sprite.Create(texture,new Rect(0,0,w,h),new Vector2(.5f,.5f),100);sprite.name=name;
            AssetDatabase.AddObjectToAsset(sprite,texture);return sprite;
        }
    }
}
