using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.LowLevel;
namespace HundredHour.UI.Audition.Editor
{
    public static class AuditionHeaderBuilder
    {
        const string Root="Assets/UI/AuditionHeader";
        static TMP_FontAsset jp,serif,sans;
        static Color Ink=new Color(.08f,.23f,.34f),Muted=new Color(.24f,.40f,.51f),Coral=new Color(.98f,.44f,.39f);
        [MenuItem("Tools/Reality Show/Create Audition Header Demo")]
        public static void Build()
        {
            if(EditorApplication.isPlaying || SceneManager.GetActiveScene().isDirty)throw new InvalidOperationException("Stop Play Mode and save the current scene first.");
            if(System.IO.File.Exists(Root+"/AuditionHeader.prefab"))throw new InvalidOperationException("Already created. Open the existing prefabs/demo.");
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            sans=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
            string ascii=new string(Enumerable.Range(32,95).Select(i=>(char)i).ToArray());
            string japanese=AuditionDescriptionView.DefaultDescription+"タイムリープ他人の視点未来観測偶然の発動残された時間";
            jp=Font("NotoSansCJKjp-Regular.otf","AuditionJapanese",48,5,ascii+japanese,false);
            var fallback=Font("NotoSansCJKjp-Regular.otf","AuditionJapaneseFallback",48,5," ",true);
            jp.fallbackFontAssetTable.Add(fallback);EditorUtility.SetDirty(jp);
            serif=Font("NotoSerif-Regular.ttf","AuditionSerif",80,8,ascii,false);
            var title=Save(Title(),"AuditionTitle");
            var description=Save(Description(),"AuditionDescription");
            var clock=Save(Clock(),"AuditionClock");
            var header=Rect("AuditionHeader",null);header.sizeDelta=new Vector2(1120,650);
            var glass=header.gameObject.AddComponent<GlassSurfaceGraphic>();glass.raycastTarget=false;
            glass.SetStyle(new Color(1,1,1,.72f),new Color(.78f,.94f,1,.45f),new Color(1,1,1,.9f),36,1.5f);
            var badge=Surface("LiveBadge",header,new Color(1,.67f,.61f,.55f),new Color(1,.75f,.69f,.35f),new Color(1,.55f,.49f,.7f),19);
            Top(badge.rectTransform,44,32,162,34);
            var badgeText=Text("Label",badge.transform,"AGI / REALITY SHOW",sans,12,Ink);Stretch(badgeText.rectTransform);badgeText.alignment=TextAlignmentOptions.Center;badgeText.fontStyle=FontStyles.Bold;
            var mark=Text("Season",header,"100 HOURS · ONE AUDITION",sans,12,Muted);Top(mark.rectTransform,230,33,330,32);mark.characterSpacing=5;
            var titleInstance=(GameObject)PrefabUtility.InstantiatePrefab(title,header);Top((RectTransform)titleInstance.transform,44,91,680,210);
            var descriptionInstance=(GameObject)PrefabUtility.InstantiatePrefab(description,header);Top((RectTransform)descriptionInstance.transform,48,328,1024,256);
            var clockInstance=(GameObject)PrefabUtility.InstantiatePrefab(clock,header);Top((RectTransform)clockInstance.transform,752,123,324,180);
            var headerPrefab=Save(header.gameObject,"AuditionHeader");
            var camera=new GameObject("Main Camera",typeof(Camera),typeof(AudioListener));camera.tag="MainCamera";camera.transform.position=new Vector3(0,0,-10);
            camera.GetComponent<Camera>().clearFlags=CameraClearFlags.SolidColor;camera.GetComponent<Camera>().backgroundColor=new Color(.54f,.79f,.85f);
            var canvasGo=new GameObject("Audition Canvas",typeof(Canvas),typeof(UnityEngine.UI.CanvasScaler),typeof(UnityEngine.UI.GraphicRaycaster));
            canvasGo.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
            var scaler=canvasGo.GetComponent<UnityEngine.UI.CanvasScaler>();scaler.uiScaleMode=UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1280,800);scaler.matchWidthOrHeight=.5f;
            var background=Surface("SkyWater",canvasGo.transform,new Color(.72f,.86f,.88f),new Color(.25f,.62f,.75f),Color.clear,0,0);Stretch(background.rectTransform);
            var light=Surface("Sunlight",canvasGo.transform,new Color(1,.91f,.75f,.65f),new Color(1,.97f,.89f,.05f),Color.clear,350,0);Place(light.rectTransform,370,130,700,700);
            var tide=Surface("Tide",canvasGo.transform,new Color(.24f,.58f,.67f,.24f),new Color(.2f,.58f,.7f,.1f),new Color(1,1,1,.32f),440,2);Place(tide.rectTransform,-430,-390,1400,880);tide.transform.localRotation=Quaternion.Euler(0,0,-17);
            var arc=Surface("LightArc",canvasGo.transform,Color.clear,Color.clear,new Color(1,1,1,.35f),400,2);Place(arc.rectTransform,510,310,1000,800);
            var instance=(GameObject)PrefabUtility.InstantiatePrefab(headerPrefab,canvasGo.transform);Place((RectTransform)instance.transform,0,24,1120,650);
            var state=Text("ClockState",canvasGo.transform,"READY  /  100 HOURS",sans,12,Ink);Place(state.rectTransform,0,-321,1120,24);state.alignment=TextAlignmentOptions.Center;state.characterSpacing=4;
            var demo=canvasGo.AddComponent<Demo.AuditionHeaderDemo>();demo.Configure(instance.GetComponentInChildren<AuditionClockController>(),state);
            Button(canvasGo.transform,"START / PAUSE",-300,-354,demo.Toggle);
            Button(canvasGo.transform,"TIME LEAP  +60 SEC",0,-354,demo.Rewind);
            Button(canvasGo.transform,"RESET  100 HOURS",300,-354,demo.ResetClock);
            var es=new GameObject("EventSystem");es.SetActive(false);es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            var input=es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();input.actionsAsset=AssetDatabase.LoadAssetAtPath<UnityEngine.InputSystem.InputActionAsset>("Assets/InputSystem_Actions.inputactions");
            var refs=AssetDatabase.LoadAllAssetsAtPath("Assets/InputSystem_Actions.inputactions").OfType<UnityEngine.InputSystem.InputActionReference>().ToArray();
            input.point=refs.First(r=>r.name=="UI/Point");input.leftClick=refs.First(r=>r.name=="UI/Click");input.move=refs.First(r=>r.name=="UI/Navigate");input.submit=refs.First(r=>r.name=="UI/Submit");input.cancel=refs.First(r=>r.name=="UI/Cancel");es.SetActive(true);
            AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(scene,"Assets/Scenes/Library/AuditionHeaderDemo.unity");Selection.activeGameObject=instance;
        }
        static TMP_FontAsset Font(string source,string name,int size,int padding,string chars,bool dynamic)
        {
            var f=TMP_FontAsset.CreateFontAsset(AssetDatabase.LoadAssetAtPath<UnityEngine.Font>(Root+"/Fonts/"+source),size,padding,GlyphRenderMode.SDFAA,1024,1024,AtlasPopulationMode.Dynamic);
            f.name=name;f.isMultiAtlasTexturesEnabled=true;
            if(!f.TryAddCharacters(chars,out string missing))throw new InvalidOperationException("Missing characters: "+missing);
            f.atlasPopulationMode=dynamic?AtlasPopulationMode.Dynamic:AtlasPopulationMode.Static;
            var so=new SerializedObject(f);so.FindProperty("m_ClearDynamicDataOnBuild").boolValue=dynamic;so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(f,Root+"/Fonts/"+name+".asset");
            f.material.name=name+" Material";AssetDatabase.AddObjectToAsset(f.material,f);
            for(int i=0;i<f.atlasTextures.Length;i++){f.atlasTextures[i].name=name+" Atlas "+i;AssetDatabase.AddObjectToAsset(f.atlasTextures[i],f);}
            EditorUtility.SetDirty(f);return f;
        }
        static GameObject Title()
        {
            var root=Rect("AuditionTitle",null);root.sizeDelta=new Vector2(680,210);
            var first=Text("100Hour",root,"100Hour",serif,82,Ink);Top(first.rectTransform,0,0,680,116);
            var second=Text("Audition",root,"Audition",serif,82,Coral);Top(second.rectTransform,0,89,680,116);second.fontStyle=FontStyles.Italic;
            root.gameObject.AddComponent<AuditionTitleView>().Configure(first,second);return root.gameObject;
        }
        static GameObject Description()
        {
            var root=Rect("AuditionDescription",null);root.sizeDelta=new Vector2(1024,256);
            var body=Text("Story",root,"",jp,18,Muted);Top(body.rectTransform,0,0,1024,204);body.alignment=TextAlignmentOptions.TopLeft;body.textWrappingMode=TextWrappingModes.Normal;body.lineSpacing=4;
            var labels=new TMP_Text[4];
            for(int i=0;i<4;i++)
            {
                var pill=Surface("Ability"+(i+1),root,new Color(1,1,1,.58f),new Color(.88f,.97f,1,.30f),new Color(1,1,1,.88f),13);
                Top(pill.rectTransform,i*256,213,244,38);
                var num=Text("Number",pill.transform,"0"+(i+1),sans,12,Coral);Top(num.rectTransform,14,0,30,38);num.fontStyle=FontStyles.Bold;
                labels[i]=Text("Ability",pill.transform,"",jp,16,Ink);Top(labels[i].rectTransform,49,0,190,38);
            }
            root.gameObject.AddComponent<AuditionDescriptionView>().Configure(body,labels);return root.gameObject;
        }
        static GameObject Clock()
        {
            var root=Rect("AuditionClock",null);root.sizeDelta=new Vector2(324,180);
            var caption=Text("Caption",root,"TIME REMAINING",sans,12,Muted);Top(caption.rectTransform,0,0,324,26);caption.characterSpacing=5;
            string[] units={"HOURS","MIN","SEC"};var digits=new TMP_Text[3];
            float[] x={0,128,230},width={108,84,84};
            for(int i=0;i<3;i++)
            {
                var tile=Surface("TimeTile"+i,root,new Color(1,1,1,.66f),new Color(.87f,.96f,1,.33f),new Color(1,1,1,.95f),20);
                Top(tile.rectTransform,x[i],40,width[i],116);
                digits[i]=Text("Digits",tile.transform,i==0?"100":"00",serif,i==0?36:40,Ink);Top(digits[i].rectTransform,0,10,width[i],67);digits[i].alignment=TextAlignmentOptions.Center;
                var unit=Text("Unit",tile.transform,units[i],sans,11,Muted);Top(unit.rectTransform,0,81,width[i],24);unit.alignment=TextAlignmentOptions.Center;unit.characterSpacing=3;
                if(i<2){var colon=Text("Colon"+i,root,":",serif,29,Muted);Top(colon.rectTransform,x[i]+width[i]+3,62,16,64);colon.alignment=TextAlignmentOptions.Center;}
            }
            root.gameObject.AddComponent<AuditionClockView>().Configure(digits[0],digits[1],digits[2]);root.gameObject.AddComponent<AuditionClockController>().ResetClock();return root.gameObject;
        }
        static GameObject Save(GameObject root,string name){var result=PrefabUtility.SaveAsPrefabAsset(root,Root+"/"+name+".prefab");UnityEngine.Object.DestroyImmediate(root);return result;}
        static RectTransform Rect(string name,Transform parent){var r=(RectTransform)new GameObject(name,typeof(RectTransform)).transform;if(parent!=null)r.SetParent(parent,false);return r;}
        static GlassSurfaceGraphic Surface(string name,Transform parent,Color top,Color bottom,Color border,float radius,float width=1.5f){var g=Rect(name,parent).gameObject.AddComponent<GlassSurfaceGraphic>();g.raycastTarget=false;g.SetStyle(top,bottom,border,radius,width);return g;}
        static TMP_Text Text(string name,Transform parent,string value,TMP_FontAsset font,float size,Color color){var t=Rect(name,parent).gameObject.AddComponent<TextMeshProUGUI>();t.font=font;t.text=value;t.fontSize=size;t.color=color;t.raycastTarget=false;t.richText=false;t.alignment=TextAlignmentOptions.MidlineLeft;t.textWrappingMode=TextWrappingModes.NoWrap;return t;}
        static void Top(RectTransform r,float x,float y,float w,float h){r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);}
        static void Place(RectTransform r,float x,float y,float w,float h){r.anchorMin=r.anchorMax=r.pivot=new Vector2(.5f,.5f);r.anchoredPosition=new Vector2(x,y);r.sizeDelta=new Vector2(w,h);}
        static void Stretch(RectTransform r){r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;}
        static void Button(Transform parent,string label,float x,float y,UnityAction action)
        {
            var bg=Surface(label,parent,new Color(1,1,1,.55f),new Color(.78f,.94f,1,.25f),new Color(1,1,1,.8f),16);Place(bg.rectTransform,x,y,272,40);bg.raycastTarget=true;
            var b=bg.gameObject.AddComponent<UnityEngine.UI.Button>();b.targetGraphic=bg;UnityEditor.Events.UnityEventTools.AddPersistentListener(b.onClick,action);
            var t=Text("Label",bg.transform,label,sans,12,Ink);Stretch(t.rectTransform);t.alignment=TextAlignmentOptions.Center;t.fontStyle=FontStyles.Bold;
        }
    }
}
