using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using HundredHour.UI.Audition;
namespace HundredHour.UI.Timeline.Editor
{
    public static class TimelineBuilder
    {
        const string Root="Assets/UI/Timeline";
        static TMP_FontAsset jp,serif,sans;
        static Color Ink=new Color(.08f,.23f,.34f),Muted=new Color(.24f,.4f,.52f),Coral=new Color(.96f,.42f,.39f);
        [MenuItem("Tools/Reality Show/Create Timeline Demo")]
        public static void Build()
        {
            if(EditorApplication.isPlaying||SceneManager.GetActiveScene().isDirty)throw new InvalidOperationException("Stop Play Mode and save current scene first.");
            if(System.IO.File.Exists(Root+"/TimelineFeed.prefab"))throw new InvalidOperationException("Timeline assets already exist.");
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            jp=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/UI/AuditionHeader/Fonts/AuditionJapanese.asset");
            serif=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/UI/AuditionHeader/Fonts/AuditionSerif.asset");
            sans=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
            var rowGo=Row();var rowPrefab=PrefabUtility.SaveAsPrefabAsset(rowGo,Root+"/TimelineRow.prefab").GetComponent<TimelineRowView>();UnityEngine.Object.DestroyImmediate(rowGo);
            var root=Rect("TimelineFeed",null);root.sizeDelta=new Vector2(1120,100);root.pivot=new Vector2(.5f,1);
            var glass=root.gameObject.AddComponent<GlassSurfaceGraphic>();glass.raycastTarget=false;glass.SetStyle(new Color(1,1,1,.65f),new Color(.8f,.94f,1,.43f),new Color(1,1,1,.9f),30,1.5f);
            var layout=root.gameObject.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();layout.padding=new RectOffset(20,20,20,20);layout.spacing=12;layout.childControlWidth=true;layout.childControlHeight=true;layout.childForceExpandHeight=false;
            var fitter=root.gameObject.AddComponent<UnityEngine.UI.ContentSizeFitter>();fitter.verticalFit=UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
            var header=Rect("Header",root);Element(header,40);
            var title=Text("FeedTitle",header,"Audition",serif,29,Ink);Top(title.rectTransform,8,0,144,40);
            var accent=Text("FeedAccent",header,"Feed",serif,29,Coral);Top(accent.rectTransform,150,0,150,40);accent.fontStyle=FontStyles.Italic;
            var live=Text("Live",header,"LIVE",sans,13,Muted);Right(live.rectTransform,10,0,110,40);live.alignment=TextAlignmentOptions.Right;live.characterSpacing=5;
            var body=Rect("Body",root);var bodyLayout=Element(body,52);var group=body.gameObject.AddComponent<CanvasGroup>();
            var content=Rect("GeneratedRows",body);Stretch(content);
            var rowsLayout=content.gameObject.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();rowsLayout.spacing=8;rowsLayout.childControlHeight=true;rowsLayout.childControlWidth=true;rowsLayout.childForceExpandHeight=false;
            var empty=Text("EmptyState",body,"まだ投稿はありません",jp,17,Muted);Stretch(empty.rectTransform);empty.alignment=TextAlignmentOptions.Center;
            var footer=Rect("Footer",root);Element(footer,40);
            var page=Text("Page",footer,"",jp,14,Muted);Top(page.rectTransform,8,0,350,40);
            var buttonBg=Surface("NextPage",footer,new Color(1,.81f,.77f,.62f),new Color(1,.89f,.85f,.38f),new Color(1,.62f,.57f,.8f),17);Right(buttonBg.rectTransform,0,0,254,40);buttonBg.raycastTarget=true;
            var next=buttonBg.gameObject.AddComponent<UnityEngine.UI.Button>();next.targetGraphic=buttonBg;
            var buttonLabel=Text("Label",buttonBg.transform,"次の5件を見る  →",jp,16,Ink);Stretch(buttonLabel.rectTransform);buttonLabel.alignment=TextAlignmentOptions.Center;
            var view=root.gameObject.AddComponent<TimelineFeedView>();view.Configure(content,group,rowPrefab,next,buttonLabel,page,empty,bodyLayout);
            var controller=root.gameObject.AddComponent<TimelineFeedController>();
            // 空のPrefab。内容は配置先でInspectorまたはSetEntriesから設定する。
            controller.SetEntries(Array.Empty<TimelineEntry>());
            var prefab=PrefabUtility.SaveAsPrefabAsset(root.gameObject,Root+"/TimelineFeed.prefab");UnityEngine.Object.DestroyImmediate(root.gameObject);
            var camera=new GameObject("Main Camera",typeof(Camera),typeof(AudioListener));camera.tag="MainCamera";camera.transform.position=new Vector3(0,0,-10);
            camera.GetComponent<Camera>().clearFlags=CameraClearFlags.SolidColor;camera.GetComponent<Camera>().backgroundColor=new Color(.51f,.77f,.86f);
            var canvasGo=new GameObject("Timeline Canvas",typeof(Canvas),typeof(UnityEngine.UI.CanvasScaler),typeof(UnityEngine.UI.GraphicRaycaster));canvasGo.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
            var scaler=canvasGo.GetComponent<UnityEngine.UI.CanvasScaler>();scaler.uiScaleMode=UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1440,900);scaler.matchWidthOrHeight=.5f;
            var bg=Surface("SeaGlassBackdrop",canvasGo.transform,new Color(.75f,.88f,.88f),new Color(.25f,.6f,.74f),Color.clear,0);Stretch(bg.rectTransform);
            var sun=Surface("Light",canvasGo.transform,new Color(1,.91f,.77f,.5f),new Color(1,1,1,.05f),Color.clear,500);Place(sun.rectTransform,480,180,950,950);
            var arc=Surface("Waterline",canvasGo.transform,Color.clear,Color.clear,new Color(1,1,1,.3f),500);Place(arc.rectTransform,-410,-410,1700,1050);
            var note=Text("DemoHint",canvasGo.transform,"100HOUR AUDITION / TIMELINE     ·     13 POSTS · 5 PER PAGE",sans,13,Ink);note.alignment=TextAlignmentOptions.Center;note.characterSpacing=3;
            note.rectTransform.anchorMin=note.rectTransform.anchorMax=note.rectTransform.pivot=new Vector2(.5f,1);note.rectTransform.anchoredPosition=new Vector2(0,-20);note.rectTransform.sizeDelta=new Vector2(1120,30);
            var instance=(GameObject)PrefabUtility.InstantiatePrefab(prefab,canvasGo.transform);var rt=(RectTransform)instance.transform;rt.anchorMin=rt.anchorMax=rt.pivot=new Vector2(.5f,1);rt.anchoredPosition=new Vector2(0,-62);rt.sizeDelta=new Vector2(1120,100);
            string[] names={"Runa","Mei","Aoi","Rei","Sora","Runa","AGI","Mei","Rei","Aoi","Sora","AGI","Runa"};
            string[] actions={"won the first challenge","broke the silence","watched the horizon","made a promise","left a message","noticed a change","observed a possibility","asked a question","changed his mind","shared a memory","faced the result","rewound the clock","took the next step"};
            string[] dialogue={"水の中では、誰かと比べなくてよかった。今はそれだけで十分。","言わないまま終わるのだけは、もう嫌なの。","ここに来た理由を、まだ誰にも話していない。","最後まで、自分の言葉で話すと決めた。","もし私がいなくなっても、この時間を覚えていて。","さっきと同じ会話なのに、最後のひと言だけが違う。","未来は一つではありません。観測する視点を選んでください。","本当にそう思っている？ それとも、そう言うしかなかった？","勝つための選択が、正しい選択とは限らないんだね。","あの日の偶然がなければ、私たちは出会っていなかった。","結果は変わらなくても、ここに来たことは後悔していない。","時間を巻き戻しました。次の一手は、あなたに委ねられています。","今度こそ、目をそらさずに選びたい。"};
            var avatars=new[]{"CoralPortrait","BluePortrait","LilacPortrait"}.Select(n=>AssetDatabase.LoadAllAssetsAtPath("Assets/UI/ParticipantCards/Art/"+n+".asset").OfType<Sprite>().First()).ToArray();
            var data=new TimelineEntry[13];for(int i=0;i<data.Length;i++)data[i]=new TimelineEntry{speaker=names[i],action=actions[i],dialogue=dialogue[i],timeLabel=i==0?"now":i+"m",avatar=i%4==1?null:avatars[i%3],likes=12000-i*710,stars=3400+i*210,views=890+i*101};
            var feed=instance.GetComponent<TimelineFeedController>();feed.SetEntries(data);PrefabUtility.RecordPrefabInstancePropertyModifications(feed);
            var es=new GameObject("EventSystem");es.SetActive(false);es.AddComponent<UnityEngine.EventSystems.EventSystem>();var module=es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            module.actionsAsset=AssetDatabase.LoadAssetAtPath<UnityEngine.InputSystem.InputActionAsset>("Assets/InputSystem_Actions.inputactions");var refs=AssetDatabase.LoadAllAssetsAtPath("Assets/InputSystem_Actions.inputactions").OfType<UnityEngine.InputSystem.InputActionReference>().ToArray();
            module.point=refs.First(r=>r.name=="UI/Point");module.leftClick=refs.First(r=>r.name=="UI/Click");module.move=refs.First(r=>r.name=="UI/Navigate");module.submit=refs.First(r=>r.name=="UI/Submit");module.cancel=refs.First(r=>r.name=="UI/Cancel");es.SetActive(true);
            AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(scene,"Assets/Scenes/Library/TimelineDemo.unity");Selection.activeGameObject=instance;
        }
        static GameObject Row()
        {
            var root=Rect("TimelineRow",null);root.sizeDelta=new Vector2(1080,104);var layout=Element(root,104);
            var card=root.gameObject.AddComponent<GlassSurfaceGraphic>();card.raycastTarget=false;card.SetStyle(new Color(1,1,1,.48f),new Color(.91f,.98f,1,.28f),new Color(1,1,1,.8f),22,1.2f);
            var circle=Surface("AvatarCircle",root,new Color(1,.77f,.80f,.92f),new Color(1,.85f,.87f,.92f),Color.white,50);Top(circle.rectTransform,17,15,58,58);circle.gameObject.AddComponent<UnityEngine.UI.Mask>().showMaskGraphic=true;
            var initial=Text("Initial",circle.transform,"R",serif,23,Ink);Stretch(initial.rectTransform);initial.alignment=TextAlignmentOptions.Center;
            var imageGo=Rect("AvatarImage",circle.transform);var image=imageGo.gameObject.AddComponent<UnityEngine.UI.Image>();image.raycastTarget=false;
            var fit=imageGo.gameObject.AddComponent<UnityEngine.UI.AspectRatioFitter>();fit.aspectMode=UnityEngine.UI.AspectRatioFitter.AspectMode.EnvelopeParent;
            var speaker=Text("Speaker",root,"",jp,17,Ink);Top(speaker.rectTransform,92,13,130,28);speaker.fontStyle=FontStyles.Bold;
            var action=Text("Action",root,"",sans,16,Muted);Top(action.rectTransform,230,13,600,23);action.overflowMode=TextOverflowModes.Truncate;
            var time=Text("Time",root,"",sans,14,Muted);Right(time.rectTransform,20,13,65,23);time.alignment=TextAlignmentOptions.Right;
            var quote=Text("Dialogue",root,"",jp,17,Ink);Top(quote.rectTransform,92,42,968,26);quote.alignment=TextAlignmentOptions.TopLeft;quote.textWrappingMode=TextWrappingModes.Normal;
            var reactions=Rect("Reactions",root);reactions.anchorMin=reactions.anchorMax=reactions.pivot=Vector2.zero;reactions.anchoredPosition=new Vector2(92,12);reactions.sizeDelta=new Vector2(420,22);
            var labels=new TMP_Text[3];for(int i=0;i<3;i++)
            {
                var pill=Surface("Metric"+i,reactions,i==0?new Color(1,.73f,.7f,.55f):new Color(1,1,1,.7f),i==0?new Color(1,.83f,.8f,.4f):new Color(1,1,1,.45f),i==0?new Color(1,.62f,.56f,.6f):Color.white,12);
                Top(pill.rectTransform,i*126,0,116,22);labels[i]=Text("Value",pill.transform,"",sans,11,i==0?new Color(.7f,.24f,.2f):Muted);Stretch(labels[i].rectTransform);labels[i].alignment=TextAlignmentOptions.Center;
            }
            root.gameObject.AddComponent<TimelineRowView>().Configure(image,fit,initial,speaker,action,quote,time,labels[0],labels[1],labels[2],reactions,layout);return root.gameObject;
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
