using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
namespace HundredHour.Cutscenes
{
    public sealed class CutscenePlayer : MonoBehaviour
    {
        public CutsceneSequence sequence;
        [Tooltip("任意。指定時は映像ウィンドウ内にフェードと黒帯を表示する。") ]
        public RectTransform overlayParent;
        public event Action<ShotPose> ReturningToGameplay;
        public Camera outputCamera;
        public CutsceneActor[] actors;
        public Behaviour[] suspendDuringPlayback;
        public GameObject[] hideDuringPlayback;
        public Vector3 stageForward=Vector3.forward;
        public bool IsPlaying => phase!=Phase.Idle;
        public int CurrentShot => index;
        public bool IsShotActive => phase==Phase.Shot;
        public float ShotProgress => plan==null?0:Mathf.Clamp01(elapsed/plan.shots[index].duration);
        public float FadeAlpha => fade==null?0:fade.color.a;
        public string LastError {get;private set;}
        public event Action Finished;
        enum Phase {Idle,Enter,Shot,Blend,FadeOut,FadeIn,EndOut,EndIn}
        Phase phase;
        float elapsed;
        int index;
        CutsceneSequence plan;
        CinemachineCamera vcam;
        CinemachineBrain brain;
        CinemachineBlendDefinition savedBlend;
        GameObject overlay;
        UnityEngine.UI.Image fade;
        ShotPose previous;
        bool captured;
        readonly Dictionary<string,CutsceneActor> bindings=new Dictionary<string,CutsceneActor>();
        readonly Dictionary<Behaviour,bool> controls=new Dictionary<Behaviour,bool>();
        readonly Dictionary<GameObject,bool> visibility=new Dictionary<GameObject,bool>();
        readonly Dictionary<Rigidbody,bool> bodies=new Dictionary<Rigidbody,bool>();
        public Dictionary<string,CutsceneActor> GetActors()
        {
            var result=new Dictionary<string,CutsceneActor>();
            foreach(var a in actors??Array.Empty<CutsceneActor>())
            {if(a==null||string.IsNullOrWhiteSpace(a.actorId)||result.ContainsKey(a.actorId))throw new ArgumentException("人物IDが空・重複、または参照切れです。");result.Add(a.actorId,a);}
            return result;
        }
        public void PlayDefault()=>Play(sequence);
        public void Play(CutsceneSequence source)
        {
            if(!Application.isPlaying)throw new InvalidOperationException("Playモードで再生してください。");
            if(!isActiveAndEnabled)throw new InvalidOperationException("CutscenePlayer is disabled");
            if(source==null||outputCamera==null)throw new ArgumentException("SequenceとCameraが必要です。");
            var next=GetActors();CameraPlanValidation.Validate(source.shots,new List<string>(next.Keys));
            Stop();LastError=null;
            plan=Instantiate(source);bindings.Clear();foreach(var p in next)bindings.Add(p.Key,p.Value);
            try
            {
                brain=outputCamera.GetComponent<CinemachineBrain>();if(brain==null)throw new InvalidOperationException("CinemachineBrainが必要です。");
                savedBlend=brain.DefaultBlend;captured=true;brain.DefaultBlend=new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.Cut,0);
                foreach(var b in suspendDuringPlayback??Array.Empty<Behaviour>())if(b!=null&&b!=this&&!controls.ContainsKey(b)){controls.Add(b,b.enabled);b.enabled=false;}
                foreach(var g in hideDuringPlayback??Array.Empty<GameObject>())if(g!=null&&g!=gameObject&&!visibility.ContainsKey(g)){visibility.Add(g,g.activeSelf);g.SetActive(false);}
                foreach(var actor in bindings.Values){var b=actor.GetComponent<Rigidbody>();if(b!=null&&!bodies.ContainsKey(b)){bodies.Add(b,b.isKinematic);if(!b.isKinematic)b.linearVelocity=Vector3.zero;b.isKinematic=true;}}
                var go=new GameObject("CutsceneCamera");go.transform.SetParent(transform,false);vcam=go.AddComponent<CinemachineCamera>();vcam.Priority=10000;vcam.Lens.NearClipPlane=.1f;vcam.Lens.FarClipPlane=outputCamera.farClipPlane;
                CreateOverlay();index=0;Apply(Sample(0));SetAlpha(1);phase=Phase.Enter;elapsed=0;
            }
            catch{Stop();throw;}
        }
        void Update()
        {
            if(!IsPlaying)return;
            if(UnityEngine.InputSystem.Keyboard.current!=null&&UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame){Stop();return;}
            try{Tick(Time.deltaTime);}catch(Exception e){LastError=e.Message;Debug.LogException(e,this);Stop();}
        }
        ShotPose Sample(float t)=>CameraShotSolver.Evaluate(plan.shots[index],t,bindings,stageForward,outputCamera.aspect);
        void Apply(ShotPose p){vcam.transform.SetPositionAndRotation(p.position,p.rotation);vcam.Lens.FieldOfView=p.fov;}
        void Change(Phase p){phase=p;elapsed=0;}
        bool Progress(float duration,out float t){t=duration<=0?1:Mathf.Clamp01(elapsed/duration);return t>=1;}
        void Tick(float dt)
        {
            elapsed+=dt;var shot=plan.shots[index];float t;
            switch(phase)
            {
                case Phase.Enter: if(Progress(Mathf.Clamp(plan.fadeIn,0,3),out t))Change(Phase.Shot);SetAlpha(1-t);break;
                case Phase.Shot:
                    bool end=Progress(shot.duration,out t);Apply(Sample(t));if(!end)break;
                    if(index==plan.shots.Count-1){Change(Phase.EndOut);break;}
                    previous=new ShotPose{position=vcam.transform.position,rotation=vcam.transform.rotation,fov=vcam.Lens.FieldOfView};index++;
                    switch(plan.shots[index].transition){case ShotTransition.Cut:Apply(Sample(0));Change(Phase.Shot);break;case ShotTransition.Blend:Change(Phase.Blend);break;default:Change(Phase.FadeOut);break;}break;
                case Phase.Blend:
                    bool blended=Progress(shot.transitionSeconds,out t);var target=Sample(0);float smooth=Mathf.SmoothStep(0,1,t);
                    Apply(new ShotPose{position=Vector3.Lerp(previous.position,target.position,smooth),rotation=Quaternion.Slerp(previous.rotation,target.rotation,smooth),fov=CameraShotSolver.ZoomFov(previous.fov,target.fov,t)});if(blended)Change(Phase.Shot);break;
                case Phase.FadeOut:if(Progress(shot.transitionSeconds,out t)){Apply(Sample(0));Change(Phase.FadeIn);}SetAlpha(t);break;
                case Phase.FadeIn:if(Progress(shot.transitionSeconds,out t))Change(Phase.Shot);SetAlpha(1-t);break;
                case Phase.EndOut:if(Progress(Mathf.Clamp(plan.fadeOut,0,3),out t)){RestoreGameplay();Change(Phase.EndIn);}SetAlpha(t);break;
                case Phase.EndIn:if(Progress(Mathf.Clamp(plan.fadeOut,0,3),out t)){Stop();return;}SetAlpha(1-t);break;
            }
        }
        void CreateOverlay()
        {
            if(overlayParent!=null)
            {
                overlay=new GameObject("CutsceneOverlay",typeof(RectTransform));overlay.transform.SetParent(overlayParent,false);var rect=(RectTransform)overlay.transform;rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;
            }
            else {overlay=new GameObject("CutsceneOverlay",typeof(Canvas));overlay.transform.SetParent(transform,false);var canvas=overlay.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=32760;}
            UnityEngine.UI.Image Panel(string name,Vector2 min,Vector2 max){var g=new GameObject(name,typeof(RectTransform),typeof(UnityEngine.UI.Image));g.transform.SetParent(overlay.transform,false);var r=(RectTransform)g.transform;r.anchorMin=min;r.anchorMax=max;r.offsetMin=r.offsetMax=Vector2.zero;var image=g.GetComponent<UnityEngine.UI.Image>();image.color=Color.black;image.raycastTarget=false;return image;}
            Panel("TopBar",new Vector2(0,.92f),Vector2.one);Panel("BottomBar",Vector2.zero,new Vector2(1,.08f));fade=Panel("Fade",Vector2.zero,Vector2.one);
        }
        void SetAlpha(float a){if(fade!=null)fade.color=new Color(0,0,0,a);}
        void RestoreGameplay()
        {
            if(vcam!=null&&vcam.gameObject.activeSelf){ReturningToGameplay?.Invoke(new ShotPose{position=vcam.transform.position,rotation=vcam.transform.rotation,fov=vcam.Lens.FieldOfView});vcam.gameObject.SetActive(false);}
            if(captured&&brain!=null)brain.DefaultBlend=savedBlend;captured=false;
            foreach(var p in bodies)if(p.Key!=null)p.Key.isKinematic=p.Value;bodies.Clear();
            foreach(var p in controls)if(p.Key!=null)p.Key.enabled=p.Value;controls.Clear();
            foreach(var p in visibility)if(p.Key!=null)p.Key.SetActive(p.Value);visibility.Clear();
        }
        public void Stop()
        {
            bool wasPlaying=IsPlaying;phase=Phase.Idle;RestoreGameplay();
            if(overlay!=null){overlay.SetActive(false);Destroy(overlay);}if(vcam!=null)Destroy(vcam.gameObject);if(plan!=null)Destroy(plan);plan=null;vcam=null;fade=null;overlay=null;
            if(wasPlaying)Finished?.Invoke();
        }
        void OnDisable()=>Stop();
    }
}
