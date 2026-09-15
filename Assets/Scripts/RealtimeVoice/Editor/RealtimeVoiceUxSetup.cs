using System;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace HundredHour.RealtimeVoice.Editor
{
    public static class RealtimeVoiceUxSetup
    {
        static TMP_FontAsset font;
        static readonly Color White=new Color(.94f,.97f,1),Mint=new Color(.5f,.89f,.82f),PanelColor=new Color(.055f,.12f,.17f);
        [MenuItem("Tools/100Hour/Polish Realtime Voice UX")]
        public static void Apply()
        {
            var scene=SceneManager.GetActiveScene();
            if(Application.isPlaying||scene.path!="Assets/Scenes/GameScene/RealtimeVoiceTest.unity")throw new InvalidOperationException("Open RealtimeVoiceTest outside Play.");
            var view=UnityEngine.Object.FindFirstObjectByType<RealtimeVoiceDemoView>();
            var canvas=UnityEngine.Object.FindFirstObjectByType<Canvas>();
            if(!view||!canvas)throw new InvalidOperationException("Expected the existing scene's view and Canvas.");
            if(canvas.transform.Find("Voice UX"))throw new InvalidOperationException("UX already installed; adjust its existing objects.");
            Directory.CreateDirectory("Backups/RealtimeVoice");EditorSceneManager.SaveScene(scene);File.Copy(scene.path,"Backups/RealtimeVoice/BeforeUx.unity",true);
            font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/RealityShow/Fonts/ShowJapanese.asset");
            var start=canvas.GetComponentInChildren<UnityEngine.UI.Button>(true);if(!start)throw new InvalidOperationException("Existing start button missing.");
            foreach(var text in canvas.GetComponentsInChildren<UnityEngine.UI.Text>(true))text.gameObject.SetActive(false);
            var scaler=canvas.GetComponent<UnityEngine.UI.CanvasScaler>();scaler.uiScaleMode=UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);scaler.screenMatchMode=UnityEngine.UI.CanvasScaler.ScreenMatchMode.Expand;
            canvas.renderMode=RenderMode.ScreenSpaceOverlay;
            var root=Rect("Voice UX",canvas.transform,0,0,1,1);root.anchorMin=root.anchorMax=new Vector2(.5f,.5f);root.sizeDelta=new Vector2(1920,1080);root.anchoredPosition=Vector2.zero;
            Panel("Backdrop",root,0,0,1,1,new Color(.025f,.055f,.085f));
            Panel("Top line",root,.1f,.916f,.9f,.92f,Mint);
            view.stepText=Label("Progress",root,.1f,.845f,.9f,.895f,25,Mint);
            view.titleText=Label("Heading",root,.1f,.765f,.9f,.845f,43,White);
            view.statusText=Label("Status",root,.1f,.16f,.9f,.215f,23,White);view.statusText.alignment=TextAlignmentOptions.Center;
            view.welcomePanel=Panel("Welcome",root,.1f,.30f,.9f,.725f,PanelColor).gameObject;
            var intro=Label("Welcome copy",view.welcomePanel.transform,.06f,.16f,.94f,.84f,34,White);intro.text="AI-0が、あなたに話しかけます。\n\nまずはマイクチェック。\nその後、二つの質問に日本語で答えてください。\nあなたの声から、新しいキャラクターを作ります。";
            view.interviewerPanel=Panel("Interviewer",root,.1f,.49f,.9f,.735f,PanelColor).gameObject;
            Label("Interviewer label",view.interviewerPanel.transform,.035f,.76f,.965f,.94f,23,Mint).text="AI-0 / 相手の案内";
            view.captionText=Label("Interviewer text",view.interviewerPanel.transform,.035f,.1f,.965f,.73f,34,White);
            view.userPanel=Panel("Your voice",root,.1f,.26f,.9f,.465f,new Color(.075f,.19f,.22f)).gameObject;
            Label("Your label",view.userPanel.transform,.035f,.73f,.965f,.94f,23,Mint).text="あなたの声 / 文字起こし";
            view.userText=Label("Transcript",view.userPanel.transform,.035f,.08f,.965f,.72f,31,White);view.userText.richText=false;view.userText.enableAutoSizing=true;view.userText.fontSizeMin=21;view.userText.fontSizeMax=31;
            view.microphonePanel=Rect("Microphone",root,.1f,.217f,.9f,.253f).gameObject;
            view.micLevelText=Label("Mic status",view.microphonePanel.transform,0,0,.35f,1,22,Mint);
            Panel("Meter track",view.microphonePanel.transform,.36f,.3f,1,.7f,new Color(.09f,.2f,.24f));
            view.micFill=Panel("Meter fill",view.microphonePanel.transform,.36f,.3f,1,.7f,Mint);view.micFill.type=UnityEngine.UI.Image.Type.Filled;view.micFill.fillMethod=UnityEngine.UI.Image.FillMethod.Horizontal;
            // Filled Image requires a sprite. Reuse the existing UI button's built-in sprite.
            view.micFill.sprite=start.GetComponent<UnityEngine.UI.Image>().sprite;
            view.startButton=start;start.gameObject.SetActive(true);start.transform.SetParent(root,false);Box((RectTransform)start.transform,.25f,.06f,.75f,.15f);Style(start,true);
            view.startLabel=Label("Start label",start.transform,.03f,.1f,.97f,.9f,32,new Color(.025f,.085f,.10f));view.startLabel.alignment=TextAlignmentOptions.Center;
            view.confirmButton=Button("Confirm",root,.25f,.06f,.71f,.15f,"",true,out view.confirmLabel);
            view.retryButton=Button("Retry voice",root,.73f,.06f,.9f,.15f,"言い直す",false,out _);
            view.cancelButton=Button("Cancel",root,.79f,.936f,.9f,.979f,"中止する",false,out _);
            view.copyButton=Button("Copy JSON",root,.75f,.06f,.9f,.15f,"JSONをコピー",false,out _);
            var result=Panel("Result",root,.1f,.25f,.9f,.735f,PanelColor);view.resultPanel=result.gameObject;
            var scrollGo=Rect("JSON scroll",result.transform,.025f,.025f,.975f,.975f);var scroll=scrollGo.gameObject.AddComponent<UnityEngine.UI.ScrollRect>();scroll.horizontal=false;scroll.vertical=true;scroll.movementType=UnityEngine.UI.ScrollRect.MovementType.Clamped;
            var viewport=Rect("Viewport",scrollGo,0,0,1,1);viewport.gameObject.AddComponent<UnityEngine.UI.RectMask2D>();var hit=viewport.gameObject.AddComponent<UnityEngine.UI.Image>();hit.color=Color.clear;hit.raycastTarget=true;
            view.resultText=Label("JSON",viewport,0,1,1,1,23,White);view.resultText.richText=false;view.resultText.rectTransform.pivot=new Vector2(.5f,1);view.resultText.rectTransform.sizeDelta=new Vector2(0,0);
            var fit=view.resultText.gameObject.AddComponent<UnityEngine.UI.ContentSizeFitter>();fit.verticalFit=UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;fit.horizontalFit=UnityEngine.UI.ContentSizeFitter.FitMode.Unconstrained;
            scroll.viewport=viewport;scroll.content=view.resultText.rectTransform;
            view.Present(new VoiceInterviewFlow());
            if(!canvas.GetComponent<UnityEngine.UI.GraphicRaycaster>())canvas.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            if(UnityEngine.Object.FindObjectsByType<UnityEngine.EventSystems.EventSystem>(FindObjectsSortMode.None).Length!=1)throw new InvalidOperationException("Expected exactly one EventSystem.");
            EditorUtility.SetDirty(view);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);Selection.activeGameObject=view.gameObject;
        }
        static RectTransform Rect(string name,Transform parent,float x0,float y0,float x1,float y1){var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);Box(r,x0,y0,x1,y1);return r;}
        static void Box(RectTransform r,float x0,float y0,float x1,float y1){r.anchorMin=new Vector2(x0,y0);r.anchorMax=new Vector2(x1,y1);r.offsetMin=r.offsetMax=Vector2.zero;}
        static UnityEngine.UI.Image Panel(string name,Transform parent,float x0,float y0,float x1,float y1,Color color){var image=Rect(name,parent,x0,y0,x1,y1).gameObject.AddComponent<UnityEngine.UI.Image>();image.color=color;image.raycastTarget=false;return image;}
        static TMP_Text Label(string name,Transform parent,float x0,float y0,float x1,float y1,float size,Color color){var t=Rect(name,parent,x0,y0,x1,y1).gameObject.AddComponent<TextMeshProUGUI>();t.font=font;t.fontSize=size;t.color=color;t.raycastTarget=false;t.textWrappingMode=TextWrappingModes.Normal;return t;}
        static void Style(UnityEngine.UI.Button b,bool primary){var image=b.GetComponent<UnityEngine.UI.Image>();image.color=primary?new Color(.52f,.92f,.79f):new Color(.12f,.25f,.3f);image.raycastTarget=true;b.targetGraphic=image;var c=b.colors;c.normalColor=Color.white;c.highlightedColor=new Color(.82f,1,1);c.pressedColor=new Color(.6f,.8f,.8f);c.disabledColor=new Color(.4f,.4f,.4f);b.colors=c;}
        static UnityEngine.UI.Button Button(string name,Transform parent,float x0,float y0,float x1,float y1,string text,bool primary,out TMP_Text label){var r=Panel(name,parent,x0,y0,x1,y1,White);var b=r.gameObject.AddComponent<UnityEngine.UI.Button>();Style(b,primary);label=Label("Label",r.transform,.025f,.1f,.975f,.9f,25,primary?new Color(.02f,.09f,.1f):White);label.text=text;label.alignment=TextAlignmentOptions.Center;return b;}
    }
}
