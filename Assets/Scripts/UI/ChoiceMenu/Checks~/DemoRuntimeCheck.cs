var demo = UnityEngine.Object.FindFirstObjectByType<HundredHour.UI.Choices.Demo.ChoiceMenuDemo>();
if (!UnityEngine.Application.isPlaying || demo == null) throw new System.Exception("Open ChoiceMenuDemo in Play Mode");
System.Collections.IEnumerator Check()
{
    demo.Reopen();
    yield return new UnityEngine.WaitForSecondsRealtime(.02f);
    var frame = demo.Menu.GetComponentInChildren<HundredHour.UI.Choices.ChoiceSelectionFrame>();
    var animated = (UnityEngine.RectTransform)frame.transform.GetChild(0);
    float minScale=2, maxScale=0, minAlpha=2, maxAlpha=0;
    for (int i=0;i<90;i++)
    {
        minScale=UnityEngine.Mathf.Min(minScale,animated.localScale.x);
        maxScale=UnityEngine.Mathf.Max(maxScale,animated.localScale.x);
        float a=animated.GetChild(4).GetComponent<UnityEngine.UI.Image>().color.a;
        minAlpha=UnityEngine.Mathf.Min(minAlpha,a); maxAlpha=UnityEngine.Mathf.Max(maxAlpha,a);
        yield return new UnityEngine.WaitForSecondsRealtime(.02f);
    }
    int before=demo.ConfirmationCount;
    demo.Menu.SelectAndConfirm(1);
    demo.Menu.SelectAndConfirm(2);
    int hidden=0,visible=0;
    for(int i=0;i<30;i++)
    {
        var selected=demo.Menu.GetComponentsInChildren<HundredHour.UI.Choices.ChoiceSelectionFrame>(true);
        if(selected.Length>1 && selected[1].transform.childCount>0)
        {
            if(selected[1].transform.GetChild(0).gameObject.activeSelf) visible++; else hidden++;
        }
        yield return new UnityEngine.WaitForSecondsRealtime(.02f);
    }
    bool result=demo.LastConfirmedIndex==1 && demo.ConfirmationCount==before+1 && !demo.Menu.IsOpen;
    demo.Reopen(); yield return new UnityEngine.WaitForSecondsRealtime(.02f);
    demo.Menu.enabled=false; yield return new UnityEngine.WaitForSecondsRealtime(.02f);
    bool cleanup=demo.Menu.transform.childCount==0;
    demo.Menu.enabled=true; demo.Reopen(); yield return new UnityEngine.WaitForSecondsRealtime(.02f);
    bool reopened=demo.Menu.IsOpen && demo.Menu.GetComponentsInChildren<TMPro.TextMeshProUGUI>().Length==3;
    var report=new { pass=result && cleanup && reopened && maxScale-minScale>.01f && maxAlpha-minAlpha>.1f && hidden>0 && visible>0, minScale,maxScale,minAlpha,maxAlpha,hidden,visible, singleConfirmation=result,cleanup,reopened };
    System.IO.File.WriteAllText("Temp/ChoiceMenuRuntimeCheck.json",Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented));
}
demo.StartCoroutine(Check());
return "Started 120-frame runtime verification";
