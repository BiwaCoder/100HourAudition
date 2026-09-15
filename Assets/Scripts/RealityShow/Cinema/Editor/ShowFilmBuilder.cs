using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Unity.Cinemachine;
using HundredHour.Cutscenes;
namespace HundredHour.RealityShow.Cinema.Editor
{
    public static class ShowFilmBuilder
    {
        const string Root="Assets/RealityShow/Film";
        public const string ScenePath="Assets/Scenes/GameScene/RealityShowFilm.unity";
        static TMP_FontAsset font;
        [MenuItem("Tools/100Hour/Create Audition Battle Film")]
        public static void Build()
        {
            if(EditorApplication.isPlaying||(SceneManager.GetActiveScene().isDirty&&SceneManager.GetActiveScene().path!=ScenePath))throw new InvalidOperationException("Save scene and stop Play first.");
            if(File.Exists(ScenePath)&&SceneManager.GetActiveScene().path!=ScenePath)throw new InvalidOperationException("Open the generated film scene before rebuilding it.");
            foreach(var dir in new[]{Root,Root+"/Shots",Root+"/Art",Root+"/Materials",Root+"/UI"})Directory.CreateDirectory(dir);
            AssetDatabase.Refresh();font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/RealityShow/Fonts/ShowJapanese.asset");
            var scenario=Scenario();
            var source=EditorSceneManager.OpenScene("Assets/Scenes/GameScene/NatureStage.unity");EditorSceneManager.SaveScene(source,ScenePath,true);
            var scene=EditorSceneManager.OpenScene(ScenePath);
            foreach(var c in UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include,FindObjectsSortMode.None))c.gameObject.SetActive(false);
            foreach(var c in UnityEngine.Object.FindObjectsByType<CinemachineCamera>(FindObjectsInactive.Include,FindObjectsSortMode.None))c.gameObject.SetActive(false);
            var oldPlayer=GameObject.Find("NaturePlayerCube");if(oldPlayer!=null)oldPlayer.SetActive(false);
            var camera=Camera.main;if(camera==null)camera=UnityEngine.Object.FindFirstObjectByType<Camera>();
            if(camera.GetComponent<CinemachineBrain>()==null)camera.gameObject.AddComponent<CinemachineBrain>();
            var stage=new GameObject("Audition Film Stage");var center=new Vector3(-11,5,48);
            var floor=GameObject.CreatePrimitive(PrimitiveType.Cylinder);floor.name="Audition podium";floor.transform.SetParent(stage.transform);floor.transform.position=center-Vector3.up*.25f;floor.transform.localScale=new Vector3(18,.25f,14);floor.GetComponent<Renderer>().sharedMaterial=Material("Podium",new Color(.11f,.23f,.28f));
            var content=AssetDatabase.LoadAssetAtPath<ShowContent>("Assets/RealityShow/Data/ShowContent.asset");
            string[] ids={"himari","konoa","hikari","shiori","yuto","ren","misa","aria"};
            Vector3[] places={new Vector3(-4,0,0),new Vector3(-1.4f,0,0),new Vector3(1.4f,0,0),new Vector3(4,0,0),new Vector3(0,0,4),new Vector3(17,0,0),new Vector3(20,0,0),new Vector3(-18,0,0)};
            var actors=new CutsceneActor[ids.Length];
            for(int i=0;i<ids.Length;i++)
            {
                string id=ids[i];string photo=i<5?"Assets/RealityShow/Art/"+id+".jpg":CopyPortrait(i==5?"announce":i==6?"analyst":"agi");
                var root=new GameObject(id);root.transform.SetParent(stage.transform);root.transform.position=center+places[i];
                var a=root.AddComponent<CutsceneActor>();a.actorId=id;a.displayName=i<5?content.Person(id).displayName:i==5?"実況 レン":i==6?"解説 ミサ":ShowSupportVoice.Label;a.aimHeight=1.7f;a.framingRadius=1.25f;actors[i]=a;
                var quad=GameObject.CreatePrimitive(PrimitiveType.Quad);quad.name="Portrait (replace with character model)";UnityEngine.Object.DestroyImmediate(quad.GetComponent<Collider>());quad.transform.SetParent(root.transform,false);quad.transform.localPosition=Vector3.up*1.8f;quad.transform.localScale=new Vector3(2.1f,3.3f,1);
                var mat=new Material(Shader.Find("Unlit/Texture"));mat.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(photo);AssetDatabase.CreateAsset(mat,Root+"/Materials/"+id+".mat");quad.GetComponent<Renderer>().sharedMaterial=mat;quad.AddComponent<ShowPortraitActor>().audienceCamera=camera;
                var plinth=GameObject.CreatePrimitive(PrimitiveType.Cylinder);plinth.name="Light pedestal";plinth.transform.SetParent(root.transform,false);plinth.transform.localScale=new Vector3(2.6f,.12f,2.6f);plinth.GetComponent<Renderer>().sharedMaterial=Material("Pedestal"+i,i==7?new Color(.2f,.8f,.9f):new Color(.9f,.55f,.45f));
            }
            var canvas=new GameObject("Audition Film Canvas",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));canvas.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;var scale=canvas.GetComponent<CanvasScaler>();scale.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scale.referenceResolution=new Vector2(1920,1080);scale.screenMatchMode=CanvasScaler.ScreenMatchMode.Expand;
            var ui=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/RealityShow/UI/RealityShowUI.prefab"),canvas.transform);PrefabUtility.UnpackPrefabInstance(ui,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);
            var v=ui.GetComponent<ShowView>();v.scenario=scenario;var c0=ui.GetComponent<ShowController>();
            var movie=Rect("Live Film Window",v.gameScreen.transform,40,132,1240,483);var back=movie.gameObject.AddComponent<Image>();back.color=new Color(.03f,.08f,.1f);back.raycastTarget=false;
            var raw=Rect("Live Camera",movie,4,4,1232,479).gameObject.AddComponent<RawImage>();raw.raycastTarget=false;
            var captionPanel=Rect("Subtitles",movie,0,318,1240,165);var shade=captionPanel.gameObject.AddComponent<Image>();shade.color=new Color(.02f,.06f,.09f,.88f);shade.raycastTarget=false;
            v.speaker.transform.SetParent(captionPanel,false);Box(v.speaker.rectTransform,24,9,1110,32);
            var scroll=v.dialogue.GetComponentInParent<ScrollRect>(true);scroll.transform.SetParent(captionPanel,false);Box((RectTransform)scroll.transform,24,47,1188,108);v.dialogue.fontSize=24;
            var talk=v.gameScreen.transform.Find("Conversation");Box((RectTransform)talk,40,628,1240,172);v.speakerPortrait.gameObject.SetActive(false);v.chapterLabel.gameObject.SetActive(false);
            var hint=talk.Find("ScrollHint");if(hint!=null)hint.gameObject.SetActive(false);Box((RectTransform)talk.Find("Choices"),24,6,1192,166);
            var side=v.memoryPanel.transform.parent;v.castPanel=Rect("CastPanel",side,24,246,522,625).gameObject;
            for(int i=0;i<v.castCards.Length;i++){v.castCards[i].transform.SetParent(v.castPanel.transform,false);Box((RectTransform)v.castCards[i].transform,i%2*266,i/2*302,254,288);}
            var tabs=new[]{v.memoryTab,v.feedTab,v.deckTab};for(int i=0;i<3;i++)Box((RectTransform)tabs[i].transform,24+i*133,182,123,46);
            v.castTab=CloneButton(v.memoryTab,"CastTab","出演者",side,423,182,123,46);
            Box((RectTransform)v.perspectiveButton.transform,24,735,250,45);Box((RectTransform)v.futureButton.transform,294,735,250,45);Box((RectTransform)v.weatherButton.transform,24,791,250,45);Box((RectTransform)v.acquireButton.transform,294,791,250,45);
            Box(v.motivation.rectTransform,28,602,510,124);v.motivation.fontSize=17;
            v.leapButton=CloneButton(v.weatherButton,"TimeLeap","タイムリープ / AGI 3",side,24,847,520,40);
            var skip=CloneButton(v.memoryTab,"SkipFilm","演出をスキップ →",movie,950,12,272,40);
            var bar=Rect("Film Header",movie,0,0,1240,48);bar.SetSiblingIndex(1);var barImage=bar.gameObject.AddComponent<Image>();barImage.color=new Color(.02f,.05f,.08f,.85f);barImage.raycastTarget=false;
            var cap=Text("ShotTitle",movie,"LIVE",18,12,880,40,20);cap.color=Color.white;
            var title=v.titleScreen.transform.Find("AuditionTitle/Second").GetComponent<TMP_Text>();title.text="Audition Battle";title.fontSize=82;
            var rig=new GameObject("Scenario Film Director");var hold=new GameObject("Film Hold Camera").AddComponent<CinemachineCamera>();hold.Priority=1000;hold.transform.position=center+new Vector3(0,4,15);hold.transform.LookAt(center+Vector3.up*1.7f);
            var player=rig.AddComponent<CutscenePlayer>();player.outputCamera=camera;player.overlayParent=raw.rectTransform;player.actors=actors;player.stageForward=Vector3.forward;
            var director=rig.AddComponent<ShowFilmDirector>();director.controller=c0;director.scenario=scenario;director.player=player;director.cast=actors;director.holdCamera=hold;director.screen=raw;director.skipButton=skip;director.caption=cap;
            director.hostBackdrop=Backdrop("Studio backdrop",stage.transform,center+new Vector3(18.5f,3,-3),new Color(.03f,.1f,.17f),"Assets/RealityShow/Art/studio.jpg");
            director.agiBackdrop=Backdrop("AGI chamber",stage.transform,center+new Vector3(-18,3,-3),new Color(.015f,.045f,.075f),null);
            // Keep subtitles and skip clickable above the player's window-local fades.
            var es=UnityEngine.Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();if(es==null){var e=new GameObject("EventSystem",typeof(UnityEngine.EventSystems.EventSystem));e.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>().AssignDefaultActions();}
            v.castPanel.SetActive(false);v.gameScreen.SetActive(false);v.titleScreen.SetActive(true);
            PrefabUtility.SaveAsPrefabAsset(ui,Root+"/UI/AuditionBattleUI.prefab");AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(scene);Selection.activeGameObject=rig;
        }
        static ShowScenario Scenario()
        {
            var s=ScriptableObject.CreateInstance<ShowScenario>();AssetDatabase.CreateAsset(s,Root+"/AuditionScenario.asset");
            Add(s,"opening0","100 Hour Audition Battle","海と森に囲まれた島の舞台。四人の参加者が百時間を共に過ごし、たった一人の心を射止める戦いが今始まる。","himari,konoa,hikari,shiori,yuto",ShotKind.GroupWide,"島の舞台を引きで紹介",ShotKind.DollyOut);
            Add(s,"opening1","ひまり / YOU","わたしはひまり。童話と小さな生き物が好きな二十二歳。覚悟と期待を胸に自分の言葉で誰かとつながりたい。","himari",ShotKind.DollyIn,"覚悟と期待を胸に、ゆっくり寄る");
            Add(s,"opening2","松村悠斗 / THE ONE","相手は松村悠斗。二十八歳のゲームクリエイター。穏やかな笑顔の奥で、共に物語を紡げる一人を探している。","yuto",ShotKind.Single,"悠斗の表情へゆっくりズームイン");
            Add(s,"firstchoice","ひまり","最初のライトが点く。覚悟と期待は、まだ声にならない。\n悠斗に、どんな自分を見せよう？","himari,yuto",ShotKind.TwoShot,"最初の視線、二人を横から");
            Add(s,"review0","実況 レン","さあ、100 Hour Audition Battle、開幕です！\n四人が一人の心を射止める百時間。しかし最初の関門は、第一印象。今ここで一人が脱落します。","ren,misa",ShotKind.TwoShot,"スタジオへフェード、実況と解説",ShotKind.Single);
            Add(s,"review1","解説 ミサ","悠斗さんに届いた印象と、視聴者の期待。その両方が試されます。\nただ、最初の一瞬だけでは見えない思いもある。舞台のひまりさんを見てみましょう。","misa,ren",ShotKind.Single,"解説ミサのコメント");
            Add(s,"firstloss","実況 レン → ひまり","「最初の脱落者は、ひまりさんです」\n笑っても、礼をしても、物語を語っても、結末は変わらなかった。\nまだ何も伝えられていない。百時間の時計が、突然、逆に動き始める。","himari",ShotKind.DollyOut,"声が遠ざかる、初回の敗退");
            Add(s,"awakening",ShowSupportVoice.Label,ShowSupportVoice.Awakening,"aria",ShotKind.DollyIn,"暗転の向こう、沙織のご挨拶");
            for(int i=0;i<3;i++)Add(s,"route"+i,ShowSupportVoice.Label,ShowSupportVoice.Route(i),"himari,konoa,hikari,shiori,yuto",ShotKind.GroupWide,"EP"+(i+1)+" 舞台に戻る");
            Add(s,"conversation","","","himari,yuto",ShotKind.OverShoulder,"肩越しで言葉を聞く",ShotKind.ReverseShoulder,ShotKind.TwoShot);
            Add(s,"ceremony","LIVE / 選考","","himari,konoa,hikari,shiori,yuto",ShotKind.RearWide,"背後から選考を見守る",ShotKind.GroupWide);
            Add(s,"reward",ShowSupportVoice.Label,ShowSupportVoice.Reward,"himari",ShotKind.Single,"会話の記憶が、次の手札になる");
            Add(s,"eliminated","ひまり","","himari",ShotKind.DollyOut,"次の周回を選ぶ前に");
            Add(s,"victory","松村悠斗","","himari,yuto",ShotKind.TwoShot,"百時間の答え",ShotKind.DollyIn);
            Add(s,"gameover","100 Hour Audition Battle","","himari",ShotKind.DollyOut,"次の物語へ");
            Add(s,"timeleap",ShowSupportVoice.Label,ShowSupportVoice.Rewind,"aria",ShotKind.DollyOut,"記憶を残し、時計を巻き戻す");
            ShowSupportUpdate.ApplyScenario(s);EditorUtility.SetDirty(s);return s;
        }
        static void Add(ShowScenario s,string id,string speaker,string text,string ids,ShotKind first,string direction,params ShotKind[] rest)
        {
            var actors=ids.Split(',');var seq=ScriptableObject.CreateInstance<CutsceneSequence>();seq.title=direction;seq.instruction=direction;seq.fadeIn=.35f;seq.fadeOut=.25f;
            foreach(var kind in new[]{first}.Concat(rest))seq.shots.Add(new CameraShot{label=kind.ToString(),kind=kind,actorA=actors[0],actorB=actors.Length>1?actors[1]:actors[0],duration=kind==ShotKind.Single?2.6f:3.2f,transition=ShotTransition.Fade,transitionSeconds=.3f,distance=7,elevation=.6f,startFov=40,endFov=kind==ShotKind.Single?34:40});
            AssetDatabase.CreateAsset(seq,Root+"/Shots/"+id+".asset");s.beats.Add(new ShowFilmBeat{id=id,speaker=speaker,text=text,actors=actors,camera=seq});
        }
        static GameObject Backdrop(string name,Transform parent,Vector3 position,Color tint,string texture)
        {
            var g=GameObject.CreatePrimitive(PrimitiveType.Quad);g.name=name;UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());g.transform.SetParent(parent);g.transform.position=position;g.transform.rotation=Quaternion.Euler(0,180,0);g.transform.localScale=new Vector3(24,14,1);
            var m=new Material(Shader.Find(texture==null?"Unlit/Color":"Unlit/Texture"));m.color=tint;if(texture!=null)m.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(texture);AssetDatabase.CreateAsset(m,Root+"/Materials/"+name+".mat");g.GetComponent<Renderer>().sharedMaterial=m;return g;
        }
        static string CopyPortrait(string id){string path=Root+"/Art/"+id+".jpg";File.Copy("/Users/matsumurakatsuhiro/Documents/AiProgramLib/100AI/GrokProto/static/img/"+id+".jpg",path,true);AssetDatabase.ImportAsset(path);return path;}
        static Material Material(string name,Color color){var m=new Material(Shader.Find("Standard")){color=color};AssetDatabase.CreateAsset(m,Root+"/Materials/"+name+".mat");return m;}
        static RectTransform Rect(string name,Transform parent,float x,float y,float w,float h){var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);Box(r,x,y,w,h);return r;}
        static void Box(RectTransform r,float x,float y,float w,float h){r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);}
        static TMP_Text Text(string name,Transform p,string value,float x,float y,float w,float h,int size){var t=Rect(name,p,x,y,w,h).gameObject.AddComponent<TextMeshProUGUI>();t.font=font;t.text=value;t.fontSize=size;t.raycastTarget=false;return t;}
        static Button CloneButton(Button src,string name,string value,Transform p,float x,float y,float w,float h){var b=UnityEngine.Object.Instantiate(src,p);b.name=name;b.onClick=new Button.ButtonClickedEvent();b.GetComponentInChildren<TMP_Text>().text=value;Box((RectTransform)b.transform,x,y,w,h);return b;}
    }
}
