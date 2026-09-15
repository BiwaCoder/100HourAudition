using UnityEngine;
using UnityEngine.Events;
namespace HundredHour.UI.Audition
{
    [ExecuteAlways,DisallowMultipleComponent,RequireComponent(typeof(AuditionClockView))]
    public sealed class AuditionClockController : MonoBehaviour
    {
        [SerializeField,Min(0)] double initialHours=100;
        [SerializeField] bool startAutomatically;
        [SerializeField] bool useUnscaledTime=true;
        [SerializeField] UnityEvent onCompleted=new UnityEvent();
        AuditionClockModel model;
        AuditionClockView view;
        bool dirty=true;
        public double RemainingSeconds=>model?.RemainingSeconds??initialHours*3600;
        public bool IsRunning=>model!=null&&model.Running;
        public UnityEvent OnCompleted=>onCompleted;
        void Awake(){view=GetComponent<AuditionClockView>();}
        void Start(){if(Application.isPlaying){EnsureModel();if(startAutomatically)model.Resume();}}
        void EnsureModel()
        {
            if(view==null)view=GetComponent<AuditionClockView>();
            if(model!=null)return;
            model=new AuditionClockModel(initialHours*3600);model.Completed+=()=>onCompleted.Invoke();
        }
        void Update()
        {
            EnsureModel();
            if(!Application.isPlaying&&dirty){model.SetRemaining(initialHours*3600);dirty=false;}
            if(Application.isPlaying)model.Tick(useUnscaledTime?Time.unscaledDeltaTime:Time.deltaTime);
            view.Render(model.DisplaySeconds);
        }
        void OnEnable()=>dirty=true;
        void OnValidate()=>dirty=true;
        public void Resume(){EnsureModel();model.Resume();}
        public void Pause(){EnsureModel();model.Pause();}
        public void ResetClock(){EnsureModel();model.Pause();model.SetRemaining(initialHours*3600);view.Render(model.DisplaySeconds);}
        public void SetRemainingSeconds(double seconds){EnsureModel();model.SetRemaining(seconds);view.Render(model.DisplaySeconds);}
        public void AddTime(double seconds){EnsureModel();model.AddTime(seconds);view.Render(model.DisplaySeconds);}
    }
}
