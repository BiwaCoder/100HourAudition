using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using HundredHour.UI.Audition;
namespace HundredHour.UI.SceneWindow.Editor
{
    public static class SceneWindowBuilder
    {
        const string Root="Assets/UI/SceneWindow";
        static TMP_FontAsset jp,serif,sans;
        static Color Ink=new Color(.08f,.23f,.34f),Muted=new Color(.24f,.4f,.52f),Coral=new Color(.98f,.39f,.36f);
        [MenuItem("Tools/Reality Show/Create Scene Window Demo")]
        public static void Build()
        {
            if(EditorApplication.isPlaying || SceneManager.GetActiveScene().isDirty) throw new InvalidOperationException("Stop Play Mode and save current scene first.");
            if(System.IO.File.Exists(Root+"/SceneWindow.prefab")) throw new InvalidOperationException("SceneWindow already exists; edit the prefab directly.");
            jp=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/UI/AuditionHeader/Fonts/AuditionJapanese.asset");
            serif=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/UI/AuditionHeader/Fonts/AuditionSerif.asset");
            sans=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var root=Surface("SceneWindow",null,new Color(1,1,1,.60f),new Color(.75f,.93f,1,.47f),new Color(1,1,1,.94f),36).rectTransform;
            root.sizeDelta=new Vector2(640,440);
            root.gameObject.AddComponent<CanvasGroup>();
            var layout=root.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();layout.preferredWidth=640;layout.preferredHeight=440;layout.minWidth=400;layout.minHeight=320;
            var viewport=Surface("Viewport",root,Color.white,Color.white,Color.clear,26).rectTransform;
            viewport.anchorMin=Vector2.zero;viewport.anchorMax=Vector2.one;viewport.offsetMin=new Vector2(20,78);viewport.offsetMax=new Vector2(-20,-20);
            viewport.gameObject.AddComponent<UnityEngine.UI.Mask>().showMaskGraphic=false;
            var placeholder=Surface("Placeholder",viewport,new Color(.95f,.76f,.72f),new Color(.43f,.53f,.72f),Color.clear,0);Stretch(placeholder.rectTransform);
            var glow=Surface("Glow",placeholder.transform,new Color(1,.9f,.82f,.32f),new Color(1,.75f,.83f,.1f),Color.clear,180);Place(glow.rectTransform,170,130,440,440);
            var head=Surface("PortraitHead",placeholder.transform,new Color(1,1,1,.28f),new Color(1,1,1,.16f),Color.clear,60);Place(head.rectTransform,0,42,96,96);
            var body=Surface("PortraitBody",placeholder.transform,new Color(1,1,1,.22f),new Color(1,1,1,.03f),Color.clear,100);Place(body.rectTransform,0,-109,216,196);
            var image=Rect("PreviewImage",viewport);Stretch(image);var raw=image.gameObject.AddComponent<UnityEngine.UI.RawImage>();raw.raycastTarget=false;image.gameObject.SetActive(false);
            var socket=Rect("ContentRoot",viewport);Stretch(socket);
            var shade=Surface("CaptionShade",viewport,new Color(.03f,.13f,.23f,0),new Color(.03f,.13f,.23f,.90f),Color.clear,0).rectTransform;
            shade.anchorMin=Vector2.zero;shade.anchorMax=new Vector2(1,0);shade.pivot=Vector2.zero;shade.offsetMin=Vector2.zero;shade.offsetMax=new Vector2(0,155);
            var rec=Surface("RecordingBadge",viewport,Coral,Coral,Color.clear,18);Top(rec.rectTransform,18,18,100,34);
            var recText=Text("Label",rec.transform,"● REC",sans,17,Color.white);Stretch(recText.rectTransform);recText.alignment=TextAlignmentOptions.Center;recText.fontStyle=FontStyles.Bold;
            var meta=Text("SpeakerAndTime",viewport,"RUNA  ·  JUST NOW",sans,15,new Color(.87f,.95f,1));Bottom(meta.rectTransform,18,82,-18,24);meta.characterSpacing=3;
            var quote=Text("Dialogue",viewport,"I think I am falling for him.\nIt scares me more than the tide.",jp,19,Color.white);Bottom(quote.rectTransform,18,18,-154,62);quote.textWrappingMode=TextWrappingModes.Normal;quote.alignment=TextAlignmentOptions.MidlineLeft;quote.enableAutoSizing=true;quote.fontSizeMin=14;quote.fontSizeMax=19;
            Button("OpenButton",viewport,"OPEN",72,44,72);
            Button("FavoriteButton",viewport,"LIKE",58,44,10);
            var footer=Rect("Footer",root);footer.anchorMin=Vector2.zero;footer.anchorMax=new Vector2(1,0);footer.pivot=Vector2.zero;footer.offsetMin=new Vector2(28,18);footer.offsetMax=new Vector2(-28, 60);
            var episode=Text("ClipTitle",footer,"Episode 7 · Clip 3",sans,20,Ink);Stretch(episode.rectTransform);episode.rectTransform.anchorMax=new Vector2(.53f,1);episode.fontStyle=FontStyles.Bold;
            var metrics=Text("StarsAndTime",footer,"12.4K STARS · 3m ago",sans,17,new Color(.73f,.22f,.21f));Stretch(metrics.rectTransform);metrics.rectTransform.anchorMin=new Vector2(.53f,0);metrics.alignment=TextAlignmentOptions.Right;
            var prefab=PrefabUtility.SaveAsPrefabAsset(root.gameObject,Root+"/SceneWindow.prefab");UnityEngine.Object.DestroyImmediate(root.gameObject);
            var camera=new GameObject("Main Camera",typeof(Camera));camera.tag="MainCamera";camera.transform.position=new Vector3(0,0,-10);camera.GetComponent<Camera>().clearFlags=CameraClearFlags.SolidColor;
            var canvas=new GameObject("Scene Window Canvas",typeof(Canvas),typeof(UnityEngine.UI.CanvasScaler),typeof(UnityEngine.UI.GraphicRaycaster));canvas.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
            var scaler=canvas.GetComponent<UnityEngine.UI.CanvasScaler>();scaler.uiScaleMode=UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1440,900);scaler.matchWidthOrHeight=.5f;
            var bg=Surface("Backdrop",canvas.transform,new Color(.77f,.90f,.92f),new Color(.31f,.65f,.77f),Color.clear,0);Stretch(bg.rectTransform);
            var light=Surface("Sunlight",canvas.transform,new Color(1,.94f,.80f,.55f),new Color(1,1,1,0),Color.clear,600);Place(light.rectTransform,400,350,1100,1100);
            var note=Text("Eyebrow",canvas.transform,"100HOUR AUDITION  /  PRIVATE MOMENTS",sans,14,Muted);Top(note.rectTransform,64,146,1200,28);note.characterSpacing=3;
            var title=Text("Title",canvas.transform,"Confession",serif,48,Ink);Top(title.rectTransform,64,190,270,68);
            var accent=Text("Accent",canvas.transform,"Cam",serif,48,Coral);Top(accent.rectTransform,340,190,170,68);accent.fontStyle=FontStyles.Italic;
            var sub=Text("Subtitle",canvas.transform,"告白室 ・ 新着",jp,20,Muted);Top(sub.rectTransform,510,204,400,50);
            for(int i=0;i<2;i++)
            {
                var go=(GameObject)PrefabUtility.InstantiatePrefab(prefab,canvas.transform);Place((RectTransform)go.transform,i==0?-336:336,-75,640,440);
                if(i==1)
                {
                    go.transform.Find("Viewport/SpeakerAndTime").GetComponent<TMP_Text>().text="MEI  ·  12 MIN AGO";
                    go.transform.Find("Viewport/Dialogue").GetComponent<TMP_Text>().text="She said yes.\nI have not breathed since.";
                    go.transform.Find("Footer/ClipTitle").GetComponent<TMP_Text>().text="Episode 7 · Clip 2";
                    go.transform.Find("Footer/StarsAndTime").GetComponent<TMP_Text>().text="8.9K STARS · 12m ago";
                    go.transform.Find("Viewport/Placeholder").GetComponent<GlassSurfaceGraphic>().SetStyle(new Color(1,.68f,.75f),new Color(.50f,.44f,.65f),Color.clear,0);
                    foreach(var c in go.GetComponentsInChildren<Component>(true))PrefabUtility.RecordPrefabInstancePropertyModifications(c);
                }
            }
            var es=new GameObject("EventSystem");es.SetActive(false);es.AddComponent<UnityEngine.EventSystems.EventSystem>();var module=es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            module.actionsAsset=AssetDatabase.LoadAssetAtPath<UnityEngine.InputSystem.InputActionAsset>("Assets/InputSystem_Actions.inputactions");var refs=AssetDatabase.LoadAllAssetsAtPath("Assets/InputSystem_Actions.inputactions").OfType<UnityEngine.InputSystem.InputActionReference>().ToArray();
            module.point=refs.First(r=>r.name=="UI/Point");module.leftClick=refs.First(r=>r.name=="UI/Click");module.move=refs.First(r=>r.name=="UI/Navigate");module.submit=refs.First(r=>r.name=="UI/Submit");module.cancel=refs.First(r=>r.name=="UI/Cancel");es.SetActive(true);
            AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(scene,"Assets/Scenes/Library/SceneWindowDemo.unity");Selection.activeObject=prefab;
        }
        static void Bottom(RectTransform r,float left,float bottom,float right,float height){r.anchorMin=Vector2.zero;r.anchorMax=new Vector2(1,0);r.pivot=Vector2.zero;r.offsetMin=new Vector2(left,bottom);r.offsetMax=new Vector2(right,bottom+height);}
        static void Button(string name,Transform parent,string label,float w,float h,float right)
        {
            var bg=Surface(name,parent,new Color(1,1,1,.25f),new Color(1,1,1,.13f),new Color(1,1,1,.7f),22);var r=bg.rectTransform;r.anchorMin=r.anchorMax=r.pivot=new Vector2(1,0);r.anchoredPosition=new Vector2(-right,20);r.sizeDelta=new Vector2(w,h);bg.raycastTarget=true;
            var b=bg.gameObject.AddComponent<UnityEngine.UI.Button>();b.targetGraphic=bg;var colors=b.colors;colors.highlightedColor=new Color(1,.8f,.75f);colors.pressedColor=new Color(.9f,.6f,.57f);b.colors=colors;
            var t=Text("Label",r,label,sans,13,Color.white);Stretch(t.rectTransform);t.alignment=TextAlignmentOptions.Center;
        }
        static RectTransform Rect(string name,Transform parent){var r=(RectTransform)new GameObject(name,typeof(RectTransform)).transform;if(parent!=null)r.SetParent(parent,false);return r;}
        static UnityEngine.UI.LayoutElement Element(RectTransform r,float height){var e=r.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();e.minHeight=e.preferredHeight=height;return e;}
        static GlassSurfaceGraphic Surface(string name,Transform parent,Color top,Color bottom,Color edge,float radius){var g=Rect(name,parent).gameObject.AddComponent<GlassSurfaceGraphic>();g.raycastTarget=false;g.SetStyle(top,bottom,edge,radius,1);return g;}
        static TMP_Text Text(string name,Transform parent,string value,TMP_FontAsset font,float size,Color color){var t=Rect(name,parent).gameObject.AddComponent<TextMeshProUGUI>();t.font=font;t.text=value;t.fontSize=size;t.color=color;t.raycastTarget=false;t.richText=false;t.alignment=TextAlignmentOptions.MidlineLeft;t.textWrappingMode=TextWrappingModes.NoWrap;return t;}
        static void Top(RectTransform r,float x,float y,float w,float h){r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);}
        static void Right(RectTransform r,float x,float y,float w,float h){r.anchorMin=r.anchorMax=r.pivot=Vector2.one;r.anchoredPosition=new Vector2(-x,-y);r.sizeDelta=new Vector2(w,h);}
        static void Place(RectTransform r,float x,float y,float w,float h){r.anchorMin=r.anchorMax=r.pivot=new Vector2(.5f,.5f);r.anchoredPosition=new Vector2(x,y);r.sizeDelta=new Vector2(w,h);}
        static void Stretch(RectTransform r){r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;}
    }
}
