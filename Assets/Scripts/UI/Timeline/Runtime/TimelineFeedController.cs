using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
namespace HundredHour.UI.Timeline
{
    [ExecuteAlways,DisallowMultipleComponent,RequireComponent(typeof(TimelineFeedView))]
    public sealed class TimelineFeedController : MonoBehaviour
    {
        [SerializeField,Min(1)] int pageSize=5;
        [SerializeField,Min(0)] float fadeDuration=.4f;
        [SerializeField] List<TimelineEntry> entries=new List<TimelineEntry>();
        [SerializeField] UnityEvent<int> onPageChanged=new UnityEvent<int>();
        readonly TimelinePaginationModel page=new TimelinePaginationModel();
        TimelineFeedView view;
        Coroutine transition;
        bool dirty=true,busy;
        float lastWidth;
        public int PageSize=>pageSize;
        public int PageIndex=>page.PageIndex;
        public bool IsTransitioning=>busy;
        public int VisibleCount=>page.VisibleCount;
        void OnEnable()
        {
            view=GetComponent<TimelineFeedView>();
            if(view.NextButton!=null)view.NextButton.onClick.AddListener(NextPage);
            dirty=true;
        }
        void OnDisable()
        {
            CancelTransition();
            if(view!=null&&view.NextButton!=null){view.NextButton.onClick.RemoveListener(NextPage);view.SetBusy(true);}
        }
        void OnValidate()=>dirty=true;
        void Update()
        {
            if(view==null||view.NextButton==null)return;
            if(dirty){Rebuild();return;}
            if(!busy&&Mathf.Abs(lastWidth-view.ContentWidth)>1){Render();}
        }
        public void SetEntries(IReadOnlyList<TimelineEntry> data)
        {
            var copy=new List<TimelineEntry>();
            if(data!=null)for(int i=0;i<data.Count;i++)copy.Add(data[i]?.Copy()??new TimelineEntry());
            entries=copy;Rebuild();
        }
        public void SetPageSize(int count){pageSize=Mathf.Max(1,count);Rebuild();}
        public void ResetToFirstPage()=>Rebuild();
        void Rebuild()
        {
            CancelTransition();if(view==null)view=GetComponent<TimelineFeedView>();
            pageSize=Mathf.Max(1,pageSize);if(entries==null)entries=new List<TimelineEntry>();
            for(int i=0;i<entries.Count;i++)if(entries[i]==null)entries[i]=new TimelineEntry();
            page.Reset(entries.Count,pageSize);Render();dirty=false;
        }
        void Render(){view.Render(entries,page);lastWidth=view.ContentWidth;}
        public void NextPage()
        {
            if(!isActiveAndEnabled||busy||!page.HasNext)return;
            if(!Application.isPlaying||fadeDuration<=0){page.Next();Render();onPageChanged.Invoke(page.PageIndex);return;}
            busy=true;view.SetBusy(true);transition=StartCoroutine(ChangePage());
        }
        IEnumerator ChangePage()
        {
            yield return Fade(1,0);
            page.Next();Render();view.SetBusy(true);
            yield return Fade(0,1);
            busy=false;transition=null;view.SetBusy(false);onPageChanged.Invoke(page.PageIndex);
        }
        IEnumerator Fade(float from,float to)
        {
            float elapsed=0,half=Mathf.Max(.001f,fadeDuration*.5f);
            while(elapsed<half){view.Opacity=Mathf.Lerp(from,to,elapsed/half);elapsed+=Time.unscaledDeltaTime;yield return null;}
            view.Opacity=to;
        }
        void CancelTransition()
        {
            if(transition!=null)StopCoroutine(transition);transition=null;busy=false;
            if(view!=null&&view.NextButton!=null)view.Opacity=1;
        }
    }
}
