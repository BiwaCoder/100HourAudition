using System;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using HundredHour.RealityShow;
using HundredHour.UI.Choices;

namespace HundredHour.Environments.Editor
{
    public static class MansionSignalSetup
    {
        static TMP_FontAsset font;
        static readonly Color Ink=new Color(.02f,.055f,.085f,.94f),Cyan=new Color(.56f,.9f,1),White=new Color(.94f,.97f,1);
        [MenuItem("Tools/100Hour/Add Mansion SF Reality Show")]
        public static void Configure()
        {
            var scene=SceneManager.GetActiveScene();
            if(EditorApplication.isPlaying||scene.path!="Assets/Scenes/GameScene/SeasideMansion.unity")throw new InvalidOperationException("Open SeasideMansion outside Play.");
            if(UnityEngine.Object.FindFirstObjectByType<MansionSignalDirector>())throw new InvalidOperationException("Signal is already installed; edit its components in place.");
            var arrival=UnityEngine.Object.FindFirstObjectByType<MansionArrivalDirector>();if(!arrival)throw new InvalidOperationException("Arrival is required.");
            Directory.CreateDirectory("Backups/SeasideMansionSignal");EditorSceneManager.SaveScene(scene);File.Copy(scene.path,"Backups/SeasideMansionSignal/BeforeSignal.unity",true);
            const string art="Assets/Environments/SeasideMansion/Signal/SaoriPrayerClosed.png";
            AssetDatabase.ImportAsset(art);var importer=(TextureImporter)AssetImporter.GetAtPath(art);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.mipmapEnabled=false;importer.maxTextureSize=2048;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();
            font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/RealityShow/Fonts/ShowJapanese.asset");
            var go=new GameObject("Mansion SF Signal Canvas",typeof(Canvas),typeof(UnityEngine.UI.CanvasScaler),typeof(UnityEngine.UI.GraphicRaycaster));
            var c=go.GetComponent<Canvas>();c.renderMode=RenderMode.ScreenSpaceOverlay;c.sortingOrder=60;
            var scaler=go.GetComponent<UnityEngine.UI.CanvasScaler>();scaler.uiScaleMode=UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);scaler.screenMatchMode=UnityEngine.UI.CanvasScaler.ScreenMatchMode.Expand;
            var root=Rect("Signal Stage",go.transform,0,0,1,1);root.anchorMin=root.anchorMax=new Vector2(.5f,.5f);root.sizeDelta=new Vector2(1920,1080);root.anchoredPosition=Vector2.zero;
            var view=go.AddComponent<MansionSignalView>();view.canvas=c;view.stage=root;
            view.shade=Panel("Signal atmosphere",root,0,0,1,1,Color.clear);
            view.header=Label("Broadcast",root,.035f,.94f,.77f,.985f,26,Cyan);
            view.status=Label("Connection",root,.035f,.90f,.90f,.936f,20,White);
            view.selfWindow=Window("Self Portrait",root,out view.selfPortrait,out view.selfLabel);
            view.otherWindow=Window("Unknown Signal",root,out view.otherPortrait,out view.otherLabel);
            var noise=Rect("Interference",view.otherWindow,.025f,.15f,.975f,.975f);view.interference=noise.gameObject.AddComponent<UnityEngine.UI.RawImage>();view.interference.raycastTarget=false;
            view.otherLabel.transform.SetAsLastSibling();
            var dialogue=Panel("Dialogue",root,.025f,.025f,.975f,.30f,Ink);view.dialoguePanel=dialogue.gameObject;
            Panel("Dialogue accent",dialogue.transform,0,.98f,1,1,new Color(.4f,.8f,.88f,.85f));
            view.speaker=Label("Speaker",dialogue.transform,.02f,.79f,.43f,.94f,25,Cyan);
            view.body=Label("Body",dialogue.transform,.02f,.06f,.43f,.77f,25,White);view.body.enableAutoSizing=true;view.body.fontSizeMin=20;view.body.fontSizeMax=25;
            var options=Rect("Choices",dialogue.transform,.455f,.04f,.985f,.94f);view.choices=ChoiceMenuFactory.Create(options,font,true);
            var style=new SerializedObject(view.choices.GetComponent<ChoiceMenuView>());style.FindProperty("fontSize").floatValue=23;style.FindProperty("rowHeight").floatValue=41;style.FindProperty("spacing").floatValue=2;style.ApplyModifiedPropertiesWithoutUndo();
            var tools=Panel("AI and Memory",root,.79f,.325f,.975f,.87f,Ink);view.toolsPanel=tools.gameObject;
            view.toolsTitle=Label("Tools title",tools.transform,.05f,.75f,.95f,.96f,22,Cyan);view.toolsTitle.enableAutoSizing=true;view.toolsTitle.fontSizeMin=15;view.toolsTitle.fontSizeMax=22;
            view.tools=new UnityEngine.UI.Button[7];view.toolLabels=new TMP_Text[7];
            for(int i=0;i<7;i++)view.tools[i]=Button("Ability "+i,tools.transform,.04f,.66f-i*.099f,.96f,.747f-i*.099f,out view.toolLabels[i]);
            var hand=Rect("Hand",root,.025f,.315f,.77f,.438f);view.handPanel=hand.gameObject;
            view.cards=new UnityEngine.UI.Button[7];view.cardLabels=new TMP_Text[7];
            for(int i=0;i<7;i++)
            {
                view.cards[i]=Button("Card "+i,hand,(float)i/7,0,(i+.94f)/7,1,out view.cardLabels[i]);view.cardLabels[i].fontSize=18;view.cardLabels[i].enableAutoSizing=true;view.cardLabels[i].fontSizeMin=15;view.cardLabels[i].fontSizeMax=18;
            }
            view.resumeButton=Button("Resume saved signal",root,.79f,.93f,.975f,.983f,out var resumeText);resumeText.text="前回の続きから";
            var director=go.AddComponent<MansionSignalDirector>();director.arrival=arrival;director.view=view;director.content=AssetDatabase.LoadAssetAtPath<ShowContent>("Assets/RealityShow/Data/ShowContent.asset");director.saoriArt=AssetDatabase.LoadAssetAtPath<Sprite>(art);
            const string silhouette="Assets/Environments/SeasideMansion/Signal/SaoriPrayerSilhouette.png";
            AssetDatabase.ImportAsset(silhouette);var si=(TextureImporter)AssetImporter.GetAtPath(silhouette);si.textureType=TextureImporterType.Sprite;si.spriteImportMode=SpriteImportMode.Single;si.mipmapEnabled=false;si.maxTextureSize=2048;si.textureCompression=TextureImporterCompression.Uncompressed;si.SaveAndReimport();director.saoriSilhouette=AssetDatabase.LoadAssetAtPath<Sprite>(silhouette);
            ConfigureEyes(view);
            view.dialoguePanel.SetActive(false);view.toolsPanel.SetActive(false);view.handPanel.SetActive(false);view.selfWindow.gameObject.SetActive(false);view.otherWindow.gameObject.SetActive(false);view.resumeButton.gameObject.SetActive(false);
            // The old name caption must not reveal the counterpart before recognition resolves.
            arrival.encounterCaption.heading="FIRST CONTACT / 認識できない相手";
            arrival.encounterCaption.text="いま、新たな物語が始まる。\n緊張で、相手の表情がうまく読み取れない。";
            EditorUtility.SetDirty(arrival);AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);Selection.activeGameObject=go;
        }
        public static void ConfigureEyes(MansionSignalView view)
        {
            const string root="Assets/Environments/SeasideMansion/Signal/";
            foreach(string name in new[]{"SaoriPrayerClosed","SaoriPrayerHalf","SaoriPrayerOpen","SaoriPrayerSilhouette"})
            {
                string path=root+name+".png";AssetDatabase.ImportAsset(path);
                var i=(TextureImporter)AssetImporter.GetAtPath(path);i.textureType=TextureImporterType.Sprite;i.spriteImportMode=SpriteImportMode.Single;i.mipmapEnabled=false;i.maxTextureSize=2048;i.textureCompression=TextureImporterCompression.Uncompressed;
                var settings=new TextureImporterSettings();i.ReadTextureSettings(settings);settings.spriteMeshType=SpriteMeshType.FullRect;i.SetTextureSettings(settings);i.SaveAndReimport();
            }
            string matPath=root+"SaoriPrayerEyes.mat";
            var material=AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if(!material){material=new Material(Shader.Find("100Hour/UI/PrayerEyeReveal"));AssetDatabase.CreateAsset(material,matPath);}
            material.SetTexture("_HalfTex",AssetDatabase.LoadAssetAtPath<Texture2D>(root+"SaoriPrayerHalf.png"));
            material.SetTexture("_OpenTex",AssetDatabase.LoadAssetAtPath<Texture2D>(root+"SaoriPrayerOpen.png"));
            material.SetFloat("_EyeOpen",0);view.eyeAnimationMaterial=material;EditorUtility.SetDirty(view);EditorUtility.SetDirty(material);
        }
        static RectTransform Window(string name,Transform parent,out UnityEngine.UI.Image portrait,out TMP_Text label)
        {
            var bg=Panel(name,parent,0,0,0,0,Ink);var r=bg.rectTransform;r.pivot=Vector2.zero;r.sizeDelta=new Vector2(180,220);r.gameObject.AddComponent<CanvasGroup>().blocksRaycasts=false;
            portrait=Panel("Portrait",r,.025f,.15f,.975f,.975f,Color.white);portrait.preserveAspect=true;
            label=Label("Identification",r,.04f,.015f,.96f,.145f,17,Cyan);label.enableAutoSizing=true;label.fontSizeMin=11;label.fontSizeMax=17;
            return r;
        }
        static RectTransform Rect(string name,Transform parent,float x0,float y0,float x1,float y1)
        {var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=new Vector2(x0,y0);r.anchorMax=new Vector2(x1,y1);r.offsetMin=r.offsetMax=Vector2.zero;return r;}
        static UnityEngine.UI.Image Panel(string name,Transform parent,float x0,float y0,float x1,float y1,Color color)
        {var i=Rect(name,parent,x0,y0,x1,y1).gameObject.AddComponent<UnityEngine.UI.Image>();i.color=color;i.raycastTarget=false;return i;}
        static TMP_Text Label(string name,Transform parent,float x0,float y0,float x1,float y1,float size,Color color)
        {var t=Rect(name,parent,x0,y0,x1,y1).gameObject.AddComponent<TextMeshProUGUI>();t.font=font;t.fontSize=size;t.color=color;t.textWrappingMode=TextWrappingModes.Normal;t.raycastTarget=false;return t;}
        static UnityEngine.UI.Button Button(string name,Transform parent,float x0,float y0,float x1,float y1,out TMP_Text label)
        {
            var image=Panel(name,parent,x0,y0,x1,y1,new Color(.07f,.17f,.22f,.97f));image.raycastTarget=true;
            var b=image.gameObject.AddComponent<UnityEngine.UI.Button>();var colors=b.colors;colors.highlightedColor=new Color(.6f,.9f,1);colors.pressedColor=new Color(.38f,.7f,.8f);colors.disabledColor=new Color(.42f,.45f,.48f,.6f);b.colors=colors;
            label=Label("Label",b.transform,.055f,.04f,.95f,.96f,20,White);label.alignment=TextAlignmentOptions.MidlineLeft;return b;
        }
    }
}
