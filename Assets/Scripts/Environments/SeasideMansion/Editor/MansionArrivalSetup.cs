using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using TMPro;
using HundredHour.Cutscenes;
namespace HundredHour.Environments.Editor
{
    public static class MansionArrivalSetup
    {
        const string Root="Assets/Environments/SeasideMansion/Arrival";
        [MenuItem("Tools/100Hour/Add Mansion Arrival Story")]
        public static void Configure()
        {
            var scene=SceneManager.GetActiveScene();
            if(EditorApplication.isPlaying||scene.path!="Assets/Scenes/GameScene/SeasideMansion.unity")throw new InvalidOperationException("Open SeasideMansion outside Play mode.");
            if(UnityEngine.Object.FindFirstObjectByType<MansionArrivalDirector>())throw new InvalidOperationException("Arrival is already configured.");
            Directory.CreateDirectory(Root);AssetDatabase.Refresh();
            var controls=UnityEngine.Object.FindFirstObjectByType<MansionSceneTour>();
            controls.enabled=false;controls.player.transform.position=new Vector3(0,.71f,23);
            var lead=controls.player.GetComponent<CutsceneActor>()??controls.player.gameObject.AddComponent<CutsceneActor>();lead.actorId="himari";lead.displayName="ひまり";lead.aimHeight=.35f;lead.framingRadius=.6f;
            var counterpart=GameObject.CreatePrimitive(PrimitiveType.Cube);counterpart.name="Yuto - First Encounter";counterpart.transform.position=new Vector3(0,.71f,7.5f);counterpart.transform.localScale=new Vector3(.95f,1.3f,.95f);
            var material=new Material(Shader.Find("Standard")){color=new Color(.08f,.46f,.51f)};material.SetFloat("_Glossiness",.35f);AssetDatabase.CreateAsset(material,Root+"/Yuto.mat");counterpart.GetComponent<Renderer>().sharedMaterial=material;
            var target=counterpart.AddComponent<CutsceneActor>();target.actorId="yuto";target.displayName="松村 悠斗";target.aimHeight=.4f;target.framingRadius=.65f;
            var focus=new GameObject("Mansion establishing focus").AddComponent<CutsceneActor>();focus.actorId="mansion";focus.aimHeight=1;focus.framingRadius=20;
            var rig=new GameObject("Mansion Arrival Story");focus.transform.SetParent(rig.transform);
            var cinema=rig.AddComponent<CutscenePlayer>();cinema.outputCamera=controls.outputCamera;cinema.actors=new[]{lead,target,focus};cinema.stageForward=Vector3.forward;cinema.suspendDuringPlayback=new[]{controls.mover};cinema.hideDuringPlayback=new[]{controls.joystickCanvas};
            var intro=ScriptableObject.CreateInstance<CutsceneSequence>();intro.title="海辺の邸宅への到着";intro.instruction="上空の引き → 中庭の説明 → 主人公にゆっくりズーム → 背後から操作開始";intro.fadeIn=.7f;intro.fadeOut=.25f;
            intro.shots.Add(Shot("海辺の邸宅・上空の引き",ShotKind.Single,"mansion",7,64,32,38,48,46,ShotTransition.Cut));
            intro.shots.Add(Shot("噴水と回廊・場所の紹介",ShotKind.Single,"mansion",4.5f,8,1.6f,0,64,64,ShotTransition.Fade));
            intro.shots.Add(Shot("ひまりのもとへズーム",ShotKind.DollyIn,"himari",4.5f,10,1.3f,0,52,42,ShotTransition.Fade));
            intro.shots.Add(Shot("背後カット・一歩目",ShotKind.Single,"himari",1.8f,2.6f,1.3f,0,62,62,ShotTransition.Cut));
            AssetDatabase.CreateAsset(intro,Root+"/MansionIntroduction.asset");
            var encounter=ScriptableObject.CreateInstance<CutsceneSequence>();encounter.title="いま、新たな物語が始まる";encounter.fadeIn=.25f;encounter.fadeOut=.35f;
            var shot=Shot("背中越しの初めての出会い",ShotKind.OverShoulder,"himari",6,9,2.5f,0,58,52,ShotTransition.Fade);shot.actorB="yuto";encounter.shots.Add(shot);AssetDatabase.CreateAsset(encounter,Root+"/FirstEncounter.asset");
            var canvasGo=new GameObject("Mansion Story Canvas",typeof(Canvas),typeof(UnityEngine.UI.CanvasScaler),typeof(UnityEngine.UI.GraphicRaycaster));var canvas=canvasGo.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=50;
            var scaler=canvasGo.GetComponent<UnityEngine.UI.CanvasScaler>();scaler.uiScaleMode=UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1440,900);scaler.matchWidthOrHeight=.5f;
            var overlay=Rect("Cinema overlay",canvasGo.transform,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);cinema.overlayParent=overlay;
            var panel=Rect("Location caption",canvasGo.transform,new Vector2(.07f,.105f),new Vector2(.93f,.275f),Vector2.zero,Vector2.zero);
            var image=panel.gameObject.AddComponent<UnityEngine.UI.Image>();image.color=new Color(.025f,.07f,.095f,.83f);image.raycastTarget=false;
            var group=panel.gameObject.AddComponent<CanvasGroup>();group.blocksRaycasts=false;group.interactable=false;
            var font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/RealityShow/Fonts/ShowJapanese.asset");
            var title=Text("Place",panel,font,23,new Color(1,.74f,.48f),new Vector2(.025f,.66f),new Vector2(.975f,.95f));
            var body=Text("Narration",panel,font,27,Color.white,new Vector2(.025f,.08f),new Vector2(.975f,.65f));
            var hint=Text("Walk hint",canvasGo.transform,font,23,Color.white,new Vector2(.15f,.90f),new Vector2(.85f,.97f));hint.alignment=TextAlignmentOptions.Center;hint.text="ジョイスティックでまっすぐ進み、相手のもとへ。";hint.outlineWidth=.15f;hint.outlineColor=new Color32(10,25,35,255);hint.gameObject.SetActive(false);
            var view=canvasGo.AddComponent<MansionArrivalView>();view.caption=group;view.heading=title;view.narration=body;view.navigationHint=hint;
            var director=rig.AddComponent<MansionArrivalDirector>();director.controls=controls;director.cinema=cinema;director.view=view;director.introduction=intro;director.firstEncounter=encounter;director.counterpart=counterpart.transform;
            director.openingCaptions=new[]{Caption("SEASIDE MANSION / 海辺の邸宅","海に抱かれた、白亜の邸宅。\nここで、百時間の恋と駆け引きが幕を開ける。"),Caption("COURTYARD / 出会いの中庭","噴水の音が響く中庭と、夕陽を映す大きな窓。\nこの場所には、まだ誰も知らない物語が待っている。"),Caption("HIMARI / あなたの一歩","覚悟と期待を胸に、ひまりは舞台へ。\nあなたの一歩から、このオーディションは動き出す。"),Caption("FIRST STEP / あの人のもとへ","準備はいい？　レッドカーペットの先へ進もう。")};
            director.encounterCaption=Caption("FIRST ENCOUNTER / 松村 悠斗","いま、新たな物語が始まる。\nこの出会いが、百時間の運命を変えていく。");
            view.ShowCaption(director.openingCaptions[0].heading,director.openingCaptions[0].text);
            controls.SelectView(0);controls.enabled=false;
            // Save the same aerial composition shown at the opening frame.
            var pose=CameraShotSolver.Evaluate(intro.shots[0],0,cinema.GetActors(),Vector3.forward,16f/9);controls.outputCamera.transform.SetPositionAndRotation(pose.position,pose.rotation);controls.outputCamera.fieldOfView=pose.fov;
            PrefabUtility.SaveAsPrefabAsset(canvasGo,Root+"/LocationCaption.prefab");AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);MansionOpeningPolish.Apply();Selection.activeGameObject=rig;
        }
        static CameraShot Shot(string label,ShotKind kind,string actor,float duration,float distance,float elevation,float yaw,float from,float to,ShotTransition transition)=>new CameraShot{label=label,kind=kind,actorA=actor,duration=duration,distance=distance,elevation=elevation,yaw=yaw,startFov=from,endFov=to,transition=transition,transitionSeconds=.45f};
        static MansionArrivalDirector.Caption Caption(string heading,string text)=>new MansionArrivalDirector.Caption{heading=heading,text=text};
        static RectTransform Rect(string name,Transform parent,Vector2 min,Vector2 max,Vector2 offsetMin,Vector2 offsetMax){var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=min;r.anchorMax=max;r.offsetMin=offsetMin;r.offsetMax=offsetMax;return r;}
        static TMP_Text Text(string name,Transform parent,TMP_FontAsset font,float size,Color color,Vector2 min,Vector2 max){var t=Rect(name,parent,min,max,Vector2.zero,Vector2.zero).gameObject.AddComponent<TextMeshProUGUI>();t.font=font;t.fontSize=size;t.color=color;t.raycastTarget=false;t.textWrappingMode=TextWrappingModes.Normal;return t;}
    }
}
