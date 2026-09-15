using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Unity.Cinemachine;
using HundredHour.Cutscenes;
namespace HundredHour.RealityShow.Cinema
{
    // Presentation observes rules; it never advances a scenario or applies rewards.
    public sealed class ShowFilmDirector : MonoBehaviour
    {
        public ShowController controller;
        public ShowScenario scenario;
        public CutscenePlayer player;
        public CinemachineCamera holdCamera;
        public CutsceneActor[] cast;
        public RawImage screen;
        public GameObject hostBackdrop,agiBackdrop;
        public Button skipButton;
        public TMPro.TMP_Text caption;
        GameObject displayOutput;
        RenderTexture texture;RenderTexture previous;CutsceneSequence runtimeSequence;string lastKey;bool[] visibility;
        void OnEnable()
        {
            controller.Refreshed+=Refresh;player.Finished+=Finished;player.ReturningToGameplay+=Hold;skipButton.onClick.AddListener(Skip);
            visibility=cast.Select(a=>a.gameObject.activeSelf).ToArray();lastKey=null;previous=player.outputCamera.targetTexture;texture=new RenderTexture(1600,640,24){name="Audition live camera"};texture.Create();player.outputCamera.targetTexture=texture;screen.texture=texture;EnsureDisplayOutput();Refresh();
        }
        // Overlay UI still needs a camera targeting the physical display in Game View.
        // This camera only clears the background; the stage is rendered once into the film texture.
        void EnsureDisplayOutput()
        {
            int display=player.outputCamera.targetDisplay;
            bool hasOutput=Camera.allCameras.Any(c=>c!=player.outputCamera&&c.isActiveAndEnabled&&c.targetTexture==null&&c.targetDisplay==display);
            if(hasOutput)return;
            displayOutput=new GameObject("Audition Display Output");displayOutput.transform.SetParent(transform,false);
            var camera=displayOutput.AddComponent<Camera>();camera.targetDisplay=display;camera.cullingMask=0;
            camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.02f,.06f,.09f,1);
            camera.depth=-100;camera.allowHDR=false;camera.allowMSAA=false;camera.useOcclusionCulling=false;
        }
        void Start()=>Refresh();
        void Hold(ShotPose pose){if(holdCamera==null)return;holdCamera.transform.SetPositionAndRotation(pose.position,pose.rotation);var lens=holdCamera.Lens;lens.FieldOfView=pose.fov;holdCamera.Lens=lens;}
        void Refresh()
        {
            if(controller==null||controller.Game==null)return;var s=controller.Game.State;
            string key=$"{ShowScenario.Key(s)}:{s.loop}:{s.chapter}:{s.turn}:{s.timeLeaps}";
            if(key==lastKey)return;lastKey=key;
            var beat=scenario.Beat(s);if(beat==null||beat.camera==null){Skip();return;}
            if(player.IsPlaying)player.Stop();
            if(runtimeSequence!=null)Destroy(runtimeSequence);
            runtimeSequence=Instantiate(beat.camera);
            string target=s.phase==ShowPhase.Conversation?s.target:"yuto";
            foreach(var shot in runtimeSequence.shots){if(shot.actorA=="yuto")shot.actorA=target;if(shot.actorB=="yuto")shot.actorB=target;}
            var ids=beat.actors.Select(id=>id=="yuto"?target:id).Distinct().Where(id=>s.phase!=ShowPhase.Route||!s.contestants.Any(a=>a.id==id&&a.eliminated)).ToArray();player.actors=cast.Where(a=>ids.Contains(a.actorId)).ToArray();
            foreach(var actor in cast)actor.gameObject.SetActive(ids.Contains(actor.actorId));
            if(hostBackdrop!=null)hostBackdrop.SetActive(ids.Contains("ren")||ids.Contains("misa"));
            if(agiBackdrop!=null)agiBackdrop.SetActive(ids.Contains("aria"));
            controller.PresentationLocked=true;skipButton.gameObject.SetActive(true);caption.text=beat.camera.title;
            try{player.Play(runtimeSequence);}catch(System.Exception e){Debug.LogError("Film sequence failed: "+e.Message,this);caption.text="演出を読み込めません。本文から続けられます。";}
            if(!player.IsPlaying){controller.PresentationLocked=false;skipButton.gameObject.SetActive(false);}
            controller.Refresh();
        }
        public void Skip(){if(player.IsPlaying)player.Stop();else Finished();}
        void Finished(){if(skipButton!=null)skipButton.gameObject.SetActive(false);if(controller!=null){controller.PresentationLocked=false;if(controller.isActiveAndEnabled)controller.Refresh();}}
        void OnDisable()
        {
            if(controller!=null){controller.Refreshed-=Refresh;controller.PresentationLocked=false;}
            if(skipButton!=null){skipButton.onClick.RemoveListener(Skip);skipButton.gameObject.SetActive(false);}
            if(player!=null){player.Finished-=Finished;player.ReturningToGameplay-=Hold;player.Stop();}
            if(player!=null&&player.outputCamera!=null)player.outputCamera.targetTexture=previous;if(screen!=null)screen.texture=null;
            if(visibility!=null)for(int i=0;i<cast.Length;i++)if(cast[i]!=null)cast[i].gameObject.SetActive(visibility[i]);
            if(displayOutput!=null){displayOutput.SetActive(false);Destroy(displayOutput);displayOutput=null;}
            if(texture!=null){texture.Release();Destroy(texture);texture=null;}if(runtimeSequence!=null)Destroy(runtimeSequence);
            if(controller!=null&&controller.isActiveAndEnabled)controller.Refresh();
        }
    }
}
