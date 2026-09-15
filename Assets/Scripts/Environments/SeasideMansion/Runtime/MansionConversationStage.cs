using System.Linq;
using System.Collections;
using HundredHour.RealityShow;
using HundredHour.Cutscenes;
using UnityEngine;
using TMPro;

namespace HundredHour.Environments
{
    // Runtime staging: leaves the saved arrival scene and its camera rig intact.
    [DefaultExecutionOrder(200)]
    public sealed class MansionConversationStage : MonoBehaviour
    {
        MansionArrivalDirector arrival;MansionSignalView view;ShowContent content;
        Transform[] rivals=new Transform[2];RectTransform[] windows=new RectTransform[2];string[] rivalIds=new string[2];
        string focusActor;int focusCount;
        // Point the camera at one speaker (group talk); null returns to the ensemble framing.
        public void FocusActor(string actorId){focusActor=string.IsNullOrEmpty(actorId)?null:actorId;if(focusActor!=null)focusCount++;reactionUntil=0;if(current!=null&&staged)Frame(current,false);}
        Transform ActorOf(string id)
        {
            if(id=="himari")return arrival.controls.player.transform;
            if(id=="yuto")return arrival.counterpart;
            for(int i=0;i<2;i++)if(rivalIds[i]==id&&rivals[i]&&rivals[i].gameObject.activeSelf)return rivals[i];
            return null;
        }
        Camera camera;Vector3 center,forward,right;int chapter=-1,turn=-1;ShowPhase phase;
        Vector3 from,to;Quaternion fromRotation,toRotation;float blend=1,fromFov,toFov=52;
        public bool Travelling {get;private set;}
        UnityEngine.UI.Image travelFade;
        bool canvasWasEnabled;
        string location="";
        bool staged,showing;float reactionUntil;ShowState current;
        public void Initialize(MansionArrivalDirector a,MansionSignalView v,ShowContent c){arrival=a;view=v;content=c;camera=a.controls.outputCamera;}
        // Date spots (world space). Entrance hall is z 9..19 with sofas at (±7,15); the open galleries run
        // along x=±11.5 (rooms start at ±13); the fountain sits at the origin; the sea-view hall is around z -14.
        // "forward" is the direction the camera sits in, so each spot also gets its own angle.
        static bool IsGallery(string route)=>!string.IsNullOrEmpty(route)&&(route.Contains("回廊")||route.Contains("廊下")||route.Contains("gallery"));
        static (Vector3 center,Vector3 forward) ResolveSpot(string route,float y)
        {
            string r=route??"";
            if(r.Contains("ソファ")||r.Contains("sofa"))return (new Vector3(-3.5f,y,13f),new Vector3(.91f,0,.41f).normalized);
            // Stand in front of the gallery planting beds, with the camera in the open courtyard.
            if(IsGallery(r))return (new Vector3(-3.5f,y,6f),Vector3.right);
            if(r.Contains("庭")||r.Contains("garden")||r.Contains("星空")||r.Contains("stars"))return (new Vector3(0,y,4.6f),new Vector3(.55f,0,.835f).normalized);
            return (new Vector3(0,y,-17f),new Vector3(-.45f,0,.89f).normalized);
        }
        // Sunset drifts toward dusk as the audition goes on; a time-leap (chapter 0 again) brings it back.
        static void ApplySkyTime(int chapter)
        {
            var sky=FindFirstObjectByType<Funly.SkyStudio.TimeOfDayController>();
            if(sky)sky.skyTime=Mathf.Repeat(.70f+Mathf.Max(0,chapter)*.05f,1f);
        }
        public void Present(ShowState s)
        {
            current=s;bool active=s.deckMechanics&&s.loop>0&&s.phase!=ShowPhase.Awakening;
            showing=active;bool group=active&&s.chapter==1;
            if(!active){foreach(var r in rivals)if(r)r.gameObject.SetActive(false);foreach(var w in windows)if(w)w.gameObject.SetActive(false);return;}
            if(!staged)
            {
                center=arrival.counterpart.position;forward=new Vector3(4,0,6).normalized;right=Vector3.Cross(forward,Vector3.up);staged=true;
                for(int i=0;i<2;i++)
                {
                    var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name="Conversation Rival "+i;go.transform.localScale=arrival.counterpart.localScale;
                    var material=new Material(arrival.counterpart.GetComponentInChildren<Renderer>().sharedMaterial);material.color=i==0?new Color(.6f,.25f,.65f):new Color(.9f,.65f,.2f);go.GetComponent<Renderer>().material=material;
                    go.GetComponent<Collider>().enabled=false;rivals[i]=go.transform;
                    windows[i]=Instantiate(view.selfWindow,view.stage);windows[i].name="Rival Portrait "+i;
                    windows[i].GetComponent<CanvasGroup>().blocksRaycasts=false;
                }
                var body=arrival.controls.player.GetComponent<Rigidbody>();if(body)body.isKinematic=true;
            }
            var alive=s.contestants.Where(x=>x.id!="himari"&&!x.eliminated).Take(2).ToArray();
            for(int i=0;i<2;i++)
            {
                bool show=group&&i<alive.Length;rivals[i].gameObject.SetActive(show);windows[i].gameObject.SetActive(show);
                rivalIds[i]=show?alive[i].id:null;
                if(show){var person=content.Person(alive[i].id);rivals[i].GetComponent<Renderer>().material.color=person.accent;windows[i].GetComponentsInChildren<UnityEngine.UI.Image>(true).First(x=>x.transform!=windows[i]).sprite=person.portrait;windows[i].GetComponentInChildren<TMP_Text>().text=person.displayName;}
            }
            if(chapter!=s.chapter || (s.phase!=ShowPhase.Route && location!=s.route))
            {
                chapter=s.chapter;ApplySkyTime(s.chapter);
                if(s.phase!=ShowPhase.Route && !string.IsNullOrEmpty(s.route))
                {
                    location=s.route;
                    var spot=ResolveSpot(s.route,center.y);
                    center=spot.center;forward=spot.forward;right=Vector3.Cross(forward,Vector3.up);
                }
                arrival.controls.player.position=center-right*(group?3.3f:1.45f);
                arrival.counterpart.position=center+right*(group?1.1f:1.45f);
                rivals[0].position=center-right*1.1f;rivals[1].position=center+right*3.3f;
            }
            if(turn!=s.turn||phase!=s.phase){turn=s.turn;phase=s.phase;Frame(s,false);}
        }
        public void React(ShowState s){current=s;reactionUntil=Time.time+3.5f;Frame(s,true);}
        void Frame(ShowState s,bool reaction)
        {
            if(!staged)return;
            bool group=s.chapter==1;from=camera.transform.position;fromRotation=camera.transform.rotation;fromFov=camera.fieldOfView;
            var speaker=focusActor!=null?ActorOf(focusActor):null;
            if(speaker&&!(group&&IsGallery(location)))
            {
                // Close-up on whoever is talking, alternating the side a little so cuts don't all look alike.
                float side=(focusCount%2==0?1:-1)*.9f;
                to=speaker.position+forward*4.2f+right*side+Vector3.up*1.25f;
                toRotation=Quaternion.LookRotation((speaker.position+Vector3.up*.2f)-to);toFov=40;blend=0;return;
            }
            float distance=group?9.5f:s.chapter==2?6.8f:7.5f;
            // Keep the cast inside the open area above the cards, left of the support panel.
            var focus=center+right*1.25f-Vector3.up*1.45f;
            if(reaction&&!group){distance-=.7f;focus+=right*.25f;}
            if(reaction&&group){distance-=.5f;focus+=right*.3f;}
            to=center+forward*distance+Vector3.up*1.5f;
            toRotation=Quaternion.LookRotation(focus-to);toFov=group?53:s.chapter==2?48:51;blend=0;
        }
        void LateUpdate()
        {
            if(Travelling||!staged||!showing||!arrival||arrival.enabled)return;
            if(reactionUntil>0&&Time.time>=reactionUntil){reactionUntil=0;Frame(current,false);}
            {blend=Mathf.Min(1,blend+Time.deltaTime*.85f);float t=Mathf.SmoothStep(0,1,blend);camera.transform.SetPositionAndRotation(Vector3.Lerp(from,to,t),Quaternion.Slerp(fromRotation,toRotation,t));camera.fieldOfView=CameraShotSolver.ZoomFov(fromFov,toFov,blend);}
            for(int i=0;i<2;i++)if(windows[i]&&windows[i].gameObject.activeSelf)view.PlaceActorWindow(windows[i],rivals[i]);
        }
        // The fade conceals the long crossing; the visible approach stays on the open central aisle.
        public IEnumerator TravelToRoute(string route,System.Action arrive)
        {
            Travelling=true;reactionUntil=0;
            canvasWasEnabled=view.canvas.enabled;
            var overlay=new GameObject("Route Travel Fade",typeof(Canvas),typeof(UnityEngine.UI.GraphicRaycaster));
            var canvas=overlay.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=1000;
            var panel=new GameObject("Black",typeof(RectTransform),typeof(UnityEngine.UI.Image));
            panel.transform.SetParent(overlay.transform,false);
            var rect=(RectTransform)panel.transform;rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;
            travelFade=panel.GetComponent<UnityEngine.UI.Image>();travelFade.color=Color.clear;
            try
            {
                yield return FadeTravel(0,1,.4f);
                view.canvas.enabled=false;
                var spot=ResolveSpot(route,1.65f);
                var end=spot.center+spot.forward*1.2f;
                var start=end+spot.forward*5.2f;
                var rotation=Quaternion.LookRotation(-spot.forward);
                camera.transform.SetPositionAndRotation(start,rotation);camera.fieldOfView=60;
                yield return FadeTravel(1,0,.45f);
                const float duration=3.4f;
                for(float elapsed=0;elapsed<duration;elapsed+=Time.deltaTime)
                {
                    float t=Mathf.Clamp01(elapsed/duration);
                    float envelope=Mathf.Sin(t*Mathf.PI);
                    var bob=new Vector3(Mathf.Sin(elapsed*5.5f)*.025f,Mathf.Sin(elapsed*11f)*.035f,0)*envelope;
                    camera.transform.SetPositionAndRotation(Vector3.Lerp(start,end,Mathf.SmoothStep(0,1,t))+bob,rotation*Quaternion.Euler(0,0,Mathf.Sin(elapsed*5.5f)*.25f*envelope));
                    yield return null;
                }
                yield return FadeTravel(0,1,.3f);
                // Present moves the cast to the selected location, also used when resuming a save.
                location="";arrive();Present(current);Frame(current,false);
                camera.transform.SetPositionAndRotation(to,toRotation);camera.fieldOfView=toFov;blend=1;
                view.canvas.enabled=canvasWasEnabled;
                yield return FadeTravel(1,0,.5f);
            }
            finally
            {
                view.canvas.enabled=canvasWasEnabled;
                Travelling=false;
                if(overlay)Destroy(overlay);
                travelFade=null;
            }
        }
        IEnumerator FadeTravel(float a,float b,float seconds)
        {
            for(float elapsed=0;elapsed<seconds;elapsed+=Time.deltaTime)
            {
                travelFade.color=new Color(0,0,0,Mathf.Lerp(a,b,Mathf.SmoothStep(0,1,elapsed/seconds)));
                yield return null;
            }
            travelFade.color=new Color(0,0,0,b);
        }
        void OnDestroy(){for(int i=0;i<2;i++){if(rivals[i]){Destroy(rivals[i].GetComponent<Renderer>().sharedMaterial);Destroy(rivals[i].gameObject);}if(windows[i])Destroy(windows[i].gameObject);}}
    }
}
