using System;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace HundredHour.RealtimeVoice.Editor
{
    // Incremental styling: preserves all existing controls, events and the JSON ScrollRect.
    public static class RealtimeVoiceTerminalSetup
    {
        static readonly Color White = new Color(.91f, .93f, .96f);
        [MenuItem("Tools/100Hour/Style Realtime Voice Terminal")]
        public static void Apply()
        {
            var scene = SceneManager.GetActiveScene();
            if (Application.isPlaying || scene.path != "Assets/Scenes/GameScene/RealtimeVoiceTest.unity") throw new InvalidOperationException("Open RealtimeVoiceTest outside Play.");
            var view = UnityEngine.Object.FindFirstObjectByType<RealtimeVoiceDemoView>();
            var root = view.startButton.transform.parent as RectTransform;
            if (!root || root.name != "Voice UX") throw new InvalidOperationException("Existing Voice UX required.");
            Directory.CreateDirectory("Backups/RealtimeVoice");
            if (!File.Exists("Backups/RealtimeVoice/BeforeTerminal.unity")) EditorSceneManager.SaveScene(scene,"Backups/RealtimeVoice/BeforeTerminal.unity",true);
            var presentation = view.GetComponent<VoiceTerminalPresentation>() ?? view.gameObject.AddComponent<VoiceTerminalPresentation>();
            view.terminal = presentation;
            presentation.session = view.GetComponent<RealtimeVoiceSession>();
            presentation.player = UnityEngine.Object.FindFirstObjectByType<PcmStreamPlayer>();
            foreach (var label in root.GetComponentsInChildren<TMP_Text>(true)) label.color = White;
            foreach (var panel in new[] {view.welcomePanel, view.interviewerPanel, view.userPanel, view.resultPanel})
            {
                panel.GetComponent<UnityEngine.UI.Image>().color = new Color(0,0,0,.90f);
                var outline = panel.GetComponent<UnityEngine.UI.Outline>() ?? panel.AddComponent<UnityEngine.UI.Outline>();
                outline.effectColor = new Color(.48f,.53f,.6f,.24f); outline.effectDistance = new Vector2(1,-1);
            }
            var backdrop = root.Find("Backdrop").GetComponent<UnityEngine.UI.Image>(); backdrop.color = Color.black;
            // Full canvas backdrop also covers wide/tall letterbox areas.
            var canvas = root.GetComponentInParent<Canvas>();
            var outside = Child(canvas.transform,"Terminal full background");Box(outside,0,0,1,1);outside.SetAsFirstSibling();
            (outside.GetComponent<UnityEngine.UI.Image>() ?? outside.gameObject.AddComponent<UnityEngine.UI.Image>()).color=Color.black;
            outside.GetComponent<UnityEngine.UI.Image>().raycastTarget=false;
            var grain=Child(root,"Digital grain");Box(grain,0,0,1,1);grain.SetSiblingIndex(backdrop.transform.GetSiblingIndex()+1);
            presentation.noise = grain.GetComponent<UnityEngine.UI.RawImage>() ?? grain.gameObject.AddComponent<UnityEngine.UI.RawImage>();
            presentation.noise.color = Color.white;presentation.noise.raycastTarget=false;
            // Thin scanlines are stable; only the very low-contrast grain animates.
            var scans=Child(root,"Scanlines");Box(scans,0,0,1,1);scans.SetSiblingIndex(grain.GetSiblingIndex()+1);
            for(int i=0;i<54;i++) { var line=Child(scans,"Line "+i);Box(line,0,i/54f,1,i/54f+.0006f);var im=line.GetComponent<UnityEngine.UI.Image>()??line.gameObject.AddComponent<UnityEngine.UI.Image>();im.color=new Color(0,0,0,.16f);im.raycastTarget=false; }
            Box((RectTransform)root.Find("Top line"),.08f,.917f,.92f,.918f);root.Find("Top line").GetComponent<UnityEngine.UI.Image>().color=new Color(.8f,.85f,.9f,.45f);
            Box(view.stepText.rectTransform,.08f,.936f,.72f,.97f);view.stepText.fontSize=22;
            Box(view.titleText.rectTransform,.08f,.818f,.92f,.887f);view.titleText.fontSize=40;
            var spectrum=Child(root,"Voice spectrum");Box(spectrum,.09f,.66f,.91f,.81f);
            if(!spectrum.GetComponent<CanvasRenderer>())spectrum.gameObject.AddComponent<CanvasRenderer>();
            presentation.spectrum=spectrum.GetComponent<TerminalSpectrumGraphic>()??spectrum.gameObject.AddComponent<TerminalSpectrumGraphic>();presentation.spectrum.raycastTarget=false;
            presentation.signalLabel=Label(root,"Signal label",view.stepText,.08f,.632f,.92f,.665f,19);presentation.signalLabel.text="SIGNAL / 待機中";presentation.signalLabel.color=VoiceTerminalPresentation.Blue;
            Box((RectTransform)view.welcomePanel.transform,.08f,.255f,.92f,.60f);
            var intro=view.welcomePanel.GetComponentInChildren<TMP_Text>(true);intro.fontSize=29;intro.text="> 音声インターフェースを起動\n\nAI-0が、あなたに話しかけます。\nまずはマイクチェック。その後、二つの質問へ。\nあなたの声から、新しいキャラクターを作ります。";
            Box(intro.rectTransform,.035f,.10f,.965f,.90f);
            Box((RectTransform)view.interviewerPanel.transform,.08f,.425f,.92f,.60f);
            Box((RectTransform)view.userPanel.transform,.08f,.235f,.92f,.40f);
            view.captionText.fontSize=29;view.userText.fontSizeMax=29;
            view.interviewerPanel.transform.Find("Interviewer label").GetComponent<TMP_Text>().text="SYSTEM / AI-0";
            view.userPanel.transform.Find("Your label").GetComponent<TMP_Text>().text="YOU / あなたの声";
            Box((RectTransform)view.microphonePanel.transform,.08f,.198f,.92f,.226f);
            view.micFill.color=VoiceTerminalPresentation.Red;
            view.microphonePanel.transform.Find("Meter track").GetComponent<UnityEngine.UI.Image>().color=new Color(.15f,.09f,.1f);
            view.micLevelText.fontSize=19;view.micLevelText.color=White;
            Box(view.statusText.rectTransform,.08f,.136f,.92f,.188f);view.statusText.fontSize=22;
            Box((RectTransform)view.resultPanel.transform,.08f,.235f,.92f,.585f);
            Box((RectTransform)view.startButton.transform,.24f,.05f,.70f,.12f);
            Box((RectTransform)view.confirmButton.transform,.19f,.05f,.70f,.12f);
            Box((RectTransform)view.retryButton.transform,.73f,.05f,.92f,.12f);
            Box((RectTransform)view.copyButton.transform,.73f,.05f,.92f,.12f);
            Box((RectTransform)view.cancelButton.transform,.8f,.935f,.92f,.975f);
            foreach(var button in new[]{view.startButton,view.confirmButton,view.retryButton,view.copyButton,view.cancelButton})
            {
                var im=button.GetComponent<UnityEngine.UI.Image>();im.sprite=null;im.color=new Color(.035f,.04f,.05f);im.raycastTarget=true;
                var border=button.GetComponent<UnityEngine.UI.Outline>()??button.gameObject.AddComponent<UnityEngine.UI.Outline>();border.effectColor=White;border.effectDistance=Vector2.one;
                var c=button.colors;c.normalColor=Color.white;c.highlightedColor=new Color(1.8f,1.8f,2);c.selectedColor=c.highlightedColor;c.pressedColor=new Color(.5f,.6f,.8f);button.colors=c;
                foreach(var text in button.GetComponentsInChildren<TMP_Text>(true)){text.color=White;text.fontSize=24;}
            }
            view.startLabel.fontSize=28;
            presentation.formationText=Label(root,"Formation",view.stepText,.08f,.59f,.92f,.626f,22);presentation.formationText.color=White;
            foreach(var label in new[]{view.titleText,view.captionText,view.userText,view.resultText,presentation.formationText})
                if(!label.GetComponent<TerminalTextReveal>())label.gameObject.AddComponent<TerminalTextReveal>();
            // Dynamic text is also untrusted text, never rich-text markup.
            foreach(var label in new[]{view.titleText,view.captionText,view.userText,view.resultText})label.richText=false;
            view.Present(new VoiceInterviewFlow());EditorUtility.SetDirty(view);EditorUtility.SetDirty(presentation);
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
        }
        static RectTransform Child(Transform parent,string name){var t=parent.Find(name);if(t)return (RectTransform)t;var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);return r;}
        static void Box(RectTransform r,float x0,float y0,float x1,float y1){r.anchorMin=new Vector2(x0,y0);r.anchorMax=new Vector2(x1,y1);r.offsetMin=r.offsetMax=Vector2.zero;}
        static TMP_Text Label(Transform parent,string name,TMP_Text source,float x0,float y0,float x1,float y1,int size){var r=Child(parent,name);Box(r,x0,y0,x1,y1);var t=r.GetComponent<TextMeshProUGUI>()??r.gameObject.AddComponent<TextMeshProUGUI>();t.font=source.font;t.fontSize=size;t.color=White;t.raycastTarget=false;return t;}
    }
}
