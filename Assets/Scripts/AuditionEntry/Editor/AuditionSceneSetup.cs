using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;
using HundredHour.Environments;
using HundredHour.LiveInterview;
using HundredHour.PhotoIllustration;
namespace HundredHour.AuditionEntry.Editor
{
    public static class AuditionSceneSetup
    {
        [MenuItem("Tools/100Hour/Create Audition Entry Copies")]
        public static void Create()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play first");
            if(SceneManager.GetActiveScene().isDirty)throw new InvalidOperationException("Current scene has unsaved edits");
            const string folder="Assets/Scenes/Audition";
            if(!AssetDatabase.IsValidFolder(folder))AssetDatabase.CreateFolder("Assets/Scenes","Audition");
            string[] sources={"SeasideMansion","GPTLiveVoiceTest","PhotoIllustrationTest"};
            string[] names={"SeasideMansionAudition","AuditionVoice","AuditionPortrait"};
            foreach(var name in names.Concat(new[]{"AuditionTitle"}))if(AssetDatabase.LoadAssetAtPath<SceneAsset>(folder+"/"+name+".unity"))throw new InvalidOperationException("Refusing to overwrite "+name);
            for(int i=0;i<3;i++)
            {
                string path=folder+"/"+names[i]+".unity";
                if(!AssetDatabase.CopyAsset("Assets/Scenes/GameScene/"+sources[i]+".unity",path))throw new Exception("Copy failed");
                var scene=EditorSceneManager.OpenScene(path);
                var bridge=new GameObject("Audition participant bridge").AddComponent<AuditionSceneBridge>();
                bridge.game=UnityEngine.Object.FindFirstObjectByType<MansionSignalDirector>();
                bridge.interview=UnityEngine.Object.FindFirstObjectByType<LiveInterviewController>();
                bridge.photo=UnityEngine.Object.FindFirstObjectByType<PhotoIllustrationDemoView>();
                bridge.portal=Portal(false);
                foreach(var es in UnityEngine.Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None))
                {
                    foreach(var old in es.GetComponents<StandaloneInputModule>())UnityEngine.Object.DestroyImmediate(old);
                    if(!es.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>())es.gameObject.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>().AssignDefaultActions();
                }
                EditorSceneManager.SaveScene(scene);
            }
            var titleScene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            new GameObject("EventSystem",typeof(EventSystem)).AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>().AssignDefaultActions();
            var camera=new GameObject("Camera").AddComponent<Camera>();camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.01f,.02f,.03f);camera.tag="MainCamera";
            Portal(true);
            EditorSceneManager.SaveScene(titleScene,folder+"/AuditionTitle.unity");
            var paths=new[]{folder+"/AuditionTitle.unity"}.Concat(names.Select(n=>folder+"/"+n+".unity")).ToArray();
            EditorBuildSettings.scenes=paths.Select(p=>new EditorBuildSettingsScene(p,true)).Concat(EditorBuildSettings.scenes.Where(s=>!paths.Contains(s.path))).ToArray();
            AssetDatabase.SaveAssets();
        }
        public static void Polish()
        {
            if(EditorApplication.isPlaying)throw new Exception("Stop Play");
            var scene=EditorSceneManager.OpenScene(AuditionPortal.Folder+"AuditionVoice.unity");
            var p=UnityEngine.Object.FindFirstObjectByType<AuditionPortal>();
            var r=p.continueLabel.GetComponentInParent<UnityEngine.UI.Button>().GetComponent<RectTransform>();r.anchorMin=new Vector2(.77f,.012f);r.anchorMax=new Vector2(.98f,.077f);r.offsetMin=r.offsetMax=Vector2.zero;
            EditorSceneManager.SaveScene(scene);
            scene=EditorSceneManager.OpenScene(AuditionPortal.Folder+"AuditionPortrait.unity");
            var view=UnityEngine.Object.FindFirstObjectByType<PhotoIllustrationDemoView>();var so=new SerializedObject(view);
            var canvas=view.GetComponentInParent<Canvas>();if(!canvas)canvas=UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None).First(c=>c.GetComponent<AuditionPortal>()==null);
            var scaler=canvas.GetComponent<UnityEngine.UI.CanvasScaler>();scaler.uiScaleMode=UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1440,900);scaler.matchWidthOrHeight=.5f;
            var bg=AuditionPortal.Rect("Audition Hologram",canvas.transform,Vector2.zero,Vector2.one);bg.SetAsFirstSibling();bg.gameObject.AddComponent<UnityEngine.UI.RawImage>().texture=Texture2D.whiteTexture;bg.gameObject.AddComponent<AuditionBackdrop>().shader=Shader.Find("Kumono/HologramBackground");
            var font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/RealityShow/Fonts/ShowJapanese.asset");
            TMP_Text Label(string name,string text,Vector2 min,Vector2 max,int size){var t=AuditionPortal.Rect(name,canvas.transform,min,max).gameObject.AddComponent<TextMeshProUGUI>();t.font=font;t.fontSize=size;t.text=text;t.color=new Color(.93f,.95f,.94f);t.raycastTarget=false;return t;}
            var labels=view.gameObject.AddComponent<AuditionPhotoLabels>();labels.title=Label("Portrait title","",new Vector2(.08f,.82f),new Vector2(.75f,.91f),38);
            labels.source=Label("Source heading","",new Vector2(.10f,.75f),new Vector2(.46f,.80f),20);labels.result=Label("Result heading","",new Vector2(.55f,.75f),new Vector2(.91f,.80f),20);
            for(int i=0;i<2;i++)
            {
                var image=(UnityEngine.UI.RawImage)so.FindProperty(i==0?"sourceImage":"resultImage").objectReferenceValue;
                var rt=image.rectTransform;rt.anchorMin=new Vector2(i==0?.10f:.55f,.27f);rt.anchorMax=new Vector2(i==0?.46f:.91f,.74f);rt.offsetMin=rt.offsetMax=Vector2.zero;image.color=new Color(.07f,.12f,.17f,1);image.raycastTarget=false;
                var button=(UnityEngine.UI.Button)so.FindProperty(i==0?"pickButton":"convertButton").objectReferenceValue;
                rt=button.GetComponent<RectTransform>();rt.anchorMin=new Vector2(i==0?.10f:.55f,.16f);rt.anchorMax=new Vector2(i==0?.46f:.91f,.24f);rt.offsetMin=rt.offsetMax=Vector2.zero;button.GetComponent<UnityEngine.UI.Image>().color=new Color(.08f,.16f,.19f);
                var t=button.GetComponentInChildren<UnityEngine.UI.Text>();t.font=AssetDatabase.LoadAssetAtPath<Font>("Assets/UI/AuditionHeader/Fonts/NotoSansCJKjp-Regular.otf");t.fontSize=25;t.alignment=TextAnchor.MiddleCenter;t.color=Color.white;t.rectTransform.anchorMin=Vector2.zero;t.rectTransform.anchorMax=Vector2.one;t.rectTransform.offsetMin=t.rectTransform.offsetMax=Vector2.zero;
                if(i==0)labels.pick=t;else labels.convert=t;
            }
            labels.status=(UnityEngine.UI.Text)so.FindProperty("statusText").objectReferenceValue;labels.status.font=AssetDatabase.LoadAssetAtPath<Font>("Assets/UI/AuditionHeader/Fonts/NotoSansCJKjp-Regular.otf");labels.status.fontSize=20;labels.status.color=Color.white;labels.status.alignment=TextAnchor.MiddleLeft;labels.status.rectTransform.anchorMin=new Vector2(.1f,.07f);labels.status.rectTransform.anchorMax=new Vector2(.91f,.14f);labels.status.rectTransform.offsetMin=labels.status.rectTransform.offsetMax=Vector2.zero;
            EditorSceneManager.SaveScene(scene);
            scene=EditorSceneManager.OpenScene(AuditionPortal.Folder+"SeasideMansionAudition.unity");
            var texture=UnityEngine.Object.FindFirstObjectByType<MansionSignalDirector>().content.Person("himari").portrait.texture;
            scene=EditorSceneManager.OpenScene(AuditionPortal.Folder+"AuditionTitle.unity");p=UnityEngine.Object.FindFirstObjectByType<AuditionPortal>();p.defaultPortrait=texture;EditorSceneManager.SaveScene(scene);
        }

        static AuditionPortal Portal(bool hub)
        {
            var p=new GameObject(hub?"Audition title":"Audition navigation",typeof(RectTransform)).AddComponent<AuditionPortal>();p.hub=hub;
            p.titleFont=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Environments/SeasideMansion/Arrival/Fonts/BerkshireSwash.asset");
            p.bodyFont=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/RealityShow/Fonts/ShowJapanese.asset");
            p.backgroundShader=Shader.Find("Kumono/HologramBackground");p.Build();return p;
        }
    }
}
