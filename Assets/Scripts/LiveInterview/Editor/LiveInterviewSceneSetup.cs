using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using HundredHour.RealtimeVoice;
namespace HundredHour.LiveInterview.Editor
{
    public static class LiveInterviewSceneSetup
    {
        public const string ScenePath="Assets/Scenes/GameScene/GPTLiveVoiceTest.unity";
        public static string AddTelemetry()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play first.");
            var scene=SceneManager.GetActiveScene();
            if(scene.path!=ScenePath)throw new InvalidOperationException("Open GPTLiveVoiceTest first.");
            var c=UnityEngine.Object.FindFirstObjectByType<LiveInterviewController>();
            var source=c.view.stepText;
            source.rectTransform.anchorMax=new Vector2(.57f,.97f);
            TMPro.TMP_Text Label(string name,float min,float max)
            {
                var existing=source.transform.parent.Find(name);
                var text=existing?existing.GetComponent<TMPro.TextMeshProUGUI>():new GameObject(name,typeof(RectTransform),typeof(TMPro.TextMeshProUGUI)).GetComponent<TMPro.TextMeshProUGUI>();
                text.transform.SetParent(source.transform.parent,false);
                text.font=source.font;text.fontSize=22;text.enableAutoSizing=true;text.fontSizeMin=14;text.fontSizeMax=22;
                text.alignment=TMPro.TextAlignmentOptions.Right;text.raycastTarget=false;
                var rect=text.rectTransform;rect.anchorMin=new Vector2(min,.94f);rect.anchorMax=new Vector2(max,.97f);rect.offsetMin=rect.offsetMax=Vector2.zero;
                return text;
            }
            c.connectionText=Label("LiveConnection",.58f,.79f);c.connectionText.text="VOICE / OFFLINE";c.connectionText.color=new Color(.72f,.76f,.82f);
            c.timerText=Label("LiveTimer",.80f,.92f);c.timerText.text="残り 02:00";c.timerText.color=Color.white;
            // Cancel is visible during conversation; keep it away from the persistent timer.
            // The result copy button uses this footer slot only after Cancel is hidden.
            var cancel=(RectTransform)c.view.cancelButton.transform;
            cancel.anchorMin=new Vector2(.73f,.05f);cancel.anchorMax=new Vector2(.92f,.12f);
            cancel.offsetMin=cancel.offsetMax=Vector2.zero;
            EditorUtility.SetDirty(c);EditorSceneManager.MarkSceneDirty(scene);
            return "Connection and timer added";
        }
        public static string UpgradeTwoMinutePresentation()
        {
            if(EditorApplication.isPlaying || SceneManager.GetActiveScene().path!=ScenePath)
                throw new InvalidOperationException("Open GPTLiveVoiceTest outside Play mode.");
            var c=UnityEngine.Object.FindFirstObjectByType<LiveInterviewController>();
            var background=c.GetComponent<HologramBackground>();
            if(!background)background=c.gameObject.AddComponent<HologramBackground>();
            background.terminal=c.view.terminal;
            background.shader=AssetDatabase.LoadAssetAtPath<Shader>("Assets/Scripts/LiveInterview/Runtime/AstraHologram.shader");
            if(!background.shader)throw new InvalidOperationException("Hologram shader missing.");
            c.view.terminal.holographicBackground=true;
            c.view.terminal.noise.color=Color.white;
            c.view.welcomePanel.GetComponentInChildren<TMPro.TMP_Text>(true).text="> AI-0 / LIVE\n\nAI-0との会話から、新しいキャラクターへ。\n好きなことや、大切にしていることを聞かせてください。\n会話は最長2分。最後にAI-0がご案内します。";
            c.view.SetStatus("会話は最長2分。会話の途中でも話しかけられます。");
            AddTelemetry();
            EditorUtility.SetDirty(background);EditorUtility.SetDirty(c.view.terminal);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            return "Two-minute hologram presentation configured";
        }
        public static string AddLanguageSelection()
        {
            if(EditorApplication.isPlaying || SceneManager.GetActiveScene().path!=ScenePath)
                throw new InvalidOperationException("Open GPTLiveVoiceTest outside Play mode.");
            var c=UnityEngine.Object.FindFirstObjectByType<LiveInterviewController>();
            c.view.localizeText=true;c.view.terminal.localizeText=true;
            UnityEngine.UI.Button Button(string name,string label,float y)
            {
                var parent=c.view.startButton.transform.parent;
                var existing=parent.Find(name);
                var button=existing?existing.GetComponent<UnityEngine.UI.Button>():UnityEngine.Object.Instantiate(c.view.startButton,parent);
                button.name=name;button.onClick=new UnityEngine.UI.Button.ButtonClickedEvent();
                button.gameObject.SetActive(true);
                var rect=(RectTransform)button.transform;rect.anchorMin=new Vector2(.08f,y);rect.anchorMax=new Vector2(.21f,y+.04f);rect.offsetMin=rect.offsetMax=Vector2.zero;
                var text=button.GetComponentInChildren<TMPro.TMP_Text>(true);
                var reveal=text.GetComponent<TerminalTextReveal>();if(reveal)UnityEngine.Object.DestroyImmediate(reveal);
                text.text=label;text.fontSize=22;
                return button;
            }
            c.japaneseButton=Button("LiveJapanese","日本語",.077f);
            c.englishButton=Button("LiveEnglish","English",.027f);
            void Bind(TMPro.TMP_Text text,string key)
            {
                var binding=text.GetComponent<HundredHour.Localization.LocalizedText>();
                if(!binding)binding=text.gameObject.AddComponent<HundredHour.Localization.LocalizedText>();
                binding.key=key;binding.japaneseFallback=text.text;EditorUtility.SetDirty(binding);
            }
            Bind(c.view.welcomePanel.GetComponentInChildren<TMPro.TMP_Text>(true),"live.welcome");
            foreach(var button in new[]{c.view.cancelButton,c.view.copyButton,c.view.retryButton})
                foreach(var text in button.GetComponentsInChildren<TMPro.TMP_Text>(true))Bind(text,text.text);
            foreach(var panel in new[]{c.view.interviewerPanel,c.view.userPanel})
                foreach(var text in panel.GetComponentsInChildren<TMPro.TMP_Text>(true))
                    if(text.name=="Interviewer label" || text.name=="Your label")Bind(text,text.text);
            EditorUtility.SetDirty(c);EditorUtility.SetDirty(c.view);EditorUtility.SetDirty(c.view.terminal);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            return "Japanese / English selection added";
        }
        public static string Create()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play first.");
            var current=SceneManager.GetActiveScene();if(current.isDirty)throw new InvalidOperationException("Save current scene changes first.");
            if(AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath))throw new InvalidOperationException("GPTLiveVoiceTest already exists.");
            if(!AssetDatabase.CopyAsset("Assets/Scenes/GameScene/RealtimeVoiceTest.unity",ScenePath))throw new InvalidOperationException("Scene copy failed");
            var scene=EditorSceneManager.OpenScene(ScenePath);
            var view=UnityEngine.Object.FindFirstObjectByType<RealtimeVoiceDemoView>();
            var go=view.gameObject;
            var oldController=go.GetComponent<RealtimeVoiceDemoController>();if(oldController)UnityEngine.Object.DestroyImmediate(oldController);
            view.terminal.session=null;
            var oldSession=go.GetComponent<RealtimeVoiceSession>();if(oldSession)UnityEngine.Object.DestroyImmediate(oldSession);
            var session=go.AddComponent<LiveVoiceSession>();session.microphone=UnityEngine.Object.FindFirstObjectByType<MicrophoneCapture>();session.player=UnityEngine.Object.FindFirstObjectByType<PcmStreamPlayer>();
            var controller=go.AddComponent<LiveInterviewController>();controller.session=session;controller.view=view;controller.builder=go.GetComponent<BuildCharacterApiClient>();
            view.welcomePanel.GetComponentInChildren<TMPro.TMP_Text>(true).text="> AI-0 / LIVE\n\nAI-0との会話から、新しいキャラクターへ。\n好きなことや、大切にしていることを聞かせてください。\n自然に話しかけて大丈夫です。目安90秒、上限120秒。";
            view.titleText.text="AI-0と、新しい物語をはじめる";view.stepText.text="GPT-LIVE 1 / 自然な音声会話";view.startLabel.text="AI-0と話す";
            view.statusText.text="開始するとAI-0から話しかけます。";
            view.microphonePanel.transform.Find("Meter track").gameObject.SetActive(false);view.micFill.gameObject.SetActive(false);
            view.captionText.enableAutoSizing=true;view.captionText.fontSizeMin=20;view.captionText.fontSizeMax=29;
            view.captionText.overflowMode=TMPro.TextOverflowModes.Ellipsis;
            UpgradeTwoMinutePresentation();
            AddLanguageSelection();
            EditorUtility.SetDirty(session);EditorUtility.SetDirty(controller);EditorUtility.SetDirty(view);EditorUtility.SetDirty(view.terminal);
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);return ScenePath;
        }
    }
}
