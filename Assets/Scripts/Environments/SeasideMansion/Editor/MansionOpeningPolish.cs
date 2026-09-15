using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using HundredHour.Cutscenes;
namespace HundredHour.Environments.Editor
{
    public static class MansionOpeningPolish
    {
        const string Root="Assets/Environments/SeasideMansion/Arrival";
        [MenuItem("Tools/100Hour/Polish Mansion Opening")]
        public static void Apply()
        {
            var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if(EditorApplication.isPlaying || scene.path!="Assets/Scenes/GameScene/SeasideMansion.unity")throw new InvalidOperationException("Open SeasideMansion outside Play mode.");
            var d=UnityEngine.Object.FindFirstObjectByType<MansionArrivalDirector>();
            if(!d)throw new InvalidOperationException("Configure Mansion Arrival first.");
            var font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Root+"/Fonts/BerkshireSwash.asset");
            if(!font)
            {
                var source=AssetDatabase.LoadAssetAtPath<Font>(Root+"/Fonts/BerkshireSwash-Regular.ttf");
                font=TMP_FontAsset.CreateFontAsset(source);
                font.name="Berkshire Swash";
                font.TryAddCharacters("100 Hour Audition");
                AssetDatabase.CreateAsset(font,Root+"/Fonts/BerkshireSwash.asset");
                foreach(var atlas in font.atlasTextures)AssetDatabase.AddObjectToAsset(atlas,font);
                AssetDatabase.AddObjectToAsset(font.material,font);
            }
            var v=d.view;
            if(!v.openingTitle)
            {
                var rect=Rect("Opening title",v.transform,new Vector2(.08f,.3f),new Vector2(.92f,.7f));
                v.openingTitle=rect.gameObject.AddComponent<CanvasGroup>();v.openingTitle.blocksRaycasts=false;v.openingTitle.interactable=false;
                var t=Rect("100 Hour Audition",rect,new Vector2(0,.2f),new Vector2(1,.8f)).gameObject.AddComponent<TextMeshProUGUI>();
                t.font=font;t.text="100 Hour Audition";t.fontSize=96;t.alignment=TextAlignmentOptions.Center;t.color=new Color(1,.91f,.72f);t.raycastTarget=false;
                t.textWrappingMode=TextWrappingModes.NoWrap;t.enableAutoSizing=true;t.fontSizeMin=42;t.fontSizeMax=96;
                t.outlineWidth=.12f;t.outlineColor=new Color32(31,38,36,180);
                Rule(rect,new Vector2(.28f,.16f),new Vector2(.48f,.165f));
                Rule(rect,new Vector2(.52f,.16f),new Vector2(.72f,.165f));
                var diamond=Rect("Central ornament",rect,new Vector2(.5f,.1625f),new Vector2(.5f,.1625f));diamond.sizeDelta=new Vector2(7,7);diamond.localRotation=Quaternion.Euler(0,0,45);var image=diamond.gameObject.AddComponent<Image>();image.color=new Color(1,.86f,.6f,.9f);image.raycastTarget=false;
            }
            var titleRoot=(RectTransform)v.openingTitle.transform;
            titleRoot.anchorMin=Vector2.zero;titleRoot.anchorMax=Vector2.one;titleRoot.offsetMin=titleRoot.offsetMax=Vector2.zero;
            var shade=titleRoot.GetComponent<Image>()??titleRoot.gameObject.AddComponent<Image>();shade.color=new Color(.015f,.025f,.035f,.38f);shade.raycastTarget=false;
            var titleText=titleRoot.GetComponentInChildren<TextMeshProUGUI>();
            titleText.rectTransform.anchorMin=new Vector2(.08f,.4f);titleText.rectTransform.anchorMax=new Vector2(.92f,.6f);
            titleText.rectTransform.offsetMin=titleText.rectTransform.offsetMax=Vector2.zero;
            int ruleIndex=0;
            foreach(Transform child in titleRoot)
            {
                var r=(RectTransform)child;
                if(child.name=="Gold rule") {r.anchorMin=new Vector2(ruleIndex==0?.32f:.52f,.365f);r.anchorMax=new Vector2(ruleIndex++==0?.48f:.68f,.367f);}
                if(child.name=="Central ornament")r.anchorMin=r.anchorMax=new Vector2(.5f,.366f);
            }
            if(!v.startupBlack)
            {
                var black=Rect("Startup black",v.transform,Vector2.zero,Vector2.one);
                var image=black.gameObject.AddComponent<Image>();image.color=Color.black;image.raycastTarget=false;
                v.startupBlack=black.gameObject.AddComponent<CanvasGroup>();v.startupBlack.blocksRaycasts=false;v.startupBlack.interactable=false;
            }
            v.PrepareOpening();
            var intro=d.introduction;intro.fadeIn=1.25f;intro.fadeOut=.18f;
            intro.instruction="黒からタイトル → 上空から明確なズームと説明 → 中庭を右から横移動 → ひまりへ寄る → 背後から操作";
            intro.shots.Clear();
            intro.shots.Add(Shot("100 Hour Audition",ShotKind.Single,"mansion",2.8f,76,32,38,50,46,1.5f,ShotTransition.Cut));
            intro.shots.Add(Shot("海辺の邸宅へズーム",ShotKind.DollyIn,"mansion",3.8f,76,32,38,46,40,1.5f,ShotTransition.Cut));
            intro.shots.Add(Shot("中庭・右からの横移動",ShotKind.Single,"mansion",3.8f,8,1.6f,0,64,56,2.8f,ShotTransition.Cut));
            intro.shots.Add(Shot("ひまりへ寄る",ShotKind.DollyIn,"himari",2.8f,10,1.3f,0,56,42,.8f,ShotTransition.Cut));
            intro.shots.Add(Shot("背後から一歩目",ShotKind.Single,"himari",1.2f,2.6f,1.3f,0,63,62,.15f,ShotTransition.Cut));
            EditorUtility.SetDirty(intro);EditorUtility.SetDirty(v);EditorUtility.SetDirty(d);
            d.controls.SelectView(0);
            var pose=CameraShotSolver.Evaluate(intro.shots[0],0,d.cinema.GetActors(),Vector3.forward,16f/9);
            d.controls.outputCamera.transform.SetPositionAndRotation(pose.position,pose.rotation);d.controls.outputCamera.fieldOfView=pose.fov;
            PrefabUtility.SaveAsPrefabAsset(v.gameObject,Root+"/LocationCaption.prefab");
            AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
        }
        static CameraShot Shot(string label,ShotKind kind,string actor,float duration,float distance,float elevation,float yaw,float from,float to,float lateral,ShotTransition transition)=>new CameraShot{label=label,kind=kind,actorA=actor,duration=duration,distance=distance,elevation=elevation,yaw=yaw,startFov=from,endFov=to,lateralTravel=lateral,transition=transition,transitionSeconds=.25f};
        static RectTransform Rect(string name,Transform parent,Vector2 min,Vector2 max){var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=min;r.anchorMax=max;r.offsetMin=r.offsetMax=Vector2.zero;return r;}
        static void Rule(Transform parent,Vector2 min,Vector2 max){var i=Rect("Gold rule",parent,min,max).gameObject.AddComponent<Image>();i.color=new Color(1,.86f,.6f,.75f);i.raycastTarget=false;}
    }
}
