if(!UnityEngine.Application.isPlaying)throw new System.Exception("Open TimelineDemo in Play Mode first");
var feed=UnityEngine.Object.FindFirstObjectByType<HundredHour.UI.Timeline.TimelineFeedController>();
var view=feed.GetComponent<HundredHour.UI.Timeline.TimelineFeedView>();
var field=typeof(HundredHour.UI.Timeline.TimelineFeedController).GetField("entries",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
var original=((System.Collections.Generic.List<HundredHour.UI.Timeline.TimelineEntry>)field.GetValue(feed)).Select(e=>e.Copy()).ToArray();
System.Collections.IEnumerator Check()
{
    feed.SetPageSize(5);yield return new UnityEngine.WaitForSecondsRealtime(.05f);
    bool first=feed.VisibleCount==5&&view.DisplayedCount==5;
    feed.NextPage();feed.NextPage();
    bool locked=feed.IsTransitioning&&!view.NextButton.interactable;
    float min=1,max=0;bool intermediate=false;
    float deadline=UnityEngine.Time.realtimeSinceStartup+3;
    while(feed.IsTransitioning&&UnityEngine.Time.realtimeSinceStartup<deadline)
    {float a=view.Opacity;min=UnityEngine.Mathf.Min(min,a);max=UnityEngine.Mathf.Max(max,a);if(a>.02f&&a<.98f)intermediate=true;yield return new UnityEngine.WaitForSecondsRealtime(.02f);}
    bool second=feed.PageIndex==1&&feed.VisibleCount==5&&view.DisplayedCount==5;
    feed.NextPage();yield return new UnityEngine.WaitForSecondsRealtime(.6f);
    feed.NextPage();bool final=feed.PageIndex==2&&feed.VisibleCount==3&&view.DisplayedCount==3&&!view.NextButton.interactable;
    feed.ResetToFirstPage();feed.NextPage();feed.enabled=false;yield return new UnityEngine.WaitForSecondsRealtime(.5f);
    bool cancelled=!feed.IsTransitioning&&view.Opacity==1;
    feed.enabled=true;yield return new UnityEngine.WaitForSecondsRealtime(.05f);
    bool restored=feed.PageIndex==0&&feed.VisibleCount==5;
    feed.SetEntries(System.Array.Empty<HundredHour.UI.Timeline.TimelineEntry>());
    bool empty=view.DisplayedCount==0&&!view.NextButton.interactable;
    feed.SetEntries(original.Take(2).ToArray());bool shortPage=view.DisplayedCount==2&&!view.NextButton.interactable;
    feed.SetEntries(original);feed.SetPageSize(3);bool sizeChanged=view.DisplayedCount==3;
    feed.SetPageSize(5);yield return new UnityEngine.WaitForSecondsRealtime(.1f);
    bool noOverflow=feed.GetComponentsInChildren<TMPro.TMP_Text>().Where(t=>t.name=="Dialogue").All(t=>!t.isTextOverflowing);
    var result=new{pass=first&&locked&&intermediate&&second&&final&&cancelled&&restored&&empty&&shortPage&&sizeChanged&&noOverflow,first,locked,intermediate,min,max,second,final,cancelled,restored,empty,shortPage,sizeChanged,noOverflow};
    System.IO.File.WriteAllText("Temp/TimelineRuntimeCheck.json",Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));
}
feed.StartCoroutine(Check());return "Runtime pagination/fade checks started; results in Temp/TimelineRuntimeCheck.json";
