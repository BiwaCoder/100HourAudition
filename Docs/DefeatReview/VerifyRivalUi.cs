var d=UnityEngine.Object.FindFirstObjectByType<HundredHour.Environments.MansionSignalDirector>();d.saveEnabled=false;d.useAI=false;
System.Collections.IEnumerator Verify(){
 var menu=UnityEngine.Object.FindFirstObjectByType<HundredHour.Localization.LanguageStartMenu>();if(menu)menu.Confirm();yield return null;yield return null;
 d.arrival.StopAllCoroutines();d.arrival.cinema.Stop();d.arrival.view.ReleaseBlack();d.BeginEncounter();
 var g=d.Game;g.StartLoop();g.ChooseRoute(0);
 for(int i=0;i<4;i++){g.State.actions.Clear();g.State.actions.Add(new HundredHour.RealityShow.TalkAction{id="listen",line="聞かせてください。",tag="public",basePower=0});g.ResolveTalk(0,HundredHour.RealityShow.ShowDialogue.Local(g,g.State.actions[0]));}
 d.Refresh();yield return null;d.Choose(0);yield return null;
 var panel=d.view.stage.Find("Observed Conversation");if(!panel)throw new Exception("review not opened");
 var text=panel.GetComponentsInChildren<TMPro.TMP_Text>().First(x=>x.text.Contains("選考スコア"));if(!text.text.Contains("会話4"))throw new Exception("recording missing");
 if(panel.GetComponentsInChildren<UnityEngine.UI.ScrollRect>().Length==0)throw new Exception("not scrollable");
 ScreenCapture.CaptureScreenshot("/tmp/rival-review-ui.png");yield return null;
 var close=panel.GetComponentInChildren<UnityEngine.UI.Button>();if(!close.gameObject.activeInHierarchy||!close.interactable)throw new Exception("close unusable");close.onClick.Invoke();yield return null;
 if(d.view.stage.Find("Observed Conversation"))throw new Exception("review did not close");
 d.useAI=true;d.Choose(1);float deadline=Time.realtimeSinceStartup+85;while(d.Busy&&Time.realtimeSinceStartup<deadline)yield return null;
 panel=d.view.stage.Find("Observed Conversation");if(d.Busy||!panel)throw new Exception("advice did not complete");
 text=panel.GetComponentsInChildren<TMPro.TMP_Text>().First(x=>x.text.Contains("応用テク"));ScreenCapture.CaptureScreenshot("/tmp/comeback-advice-ui.png");
 System.IO.File.WriteAllText("/tmp/rival-ui-result.txt","PASS review opens, scrolls, closes; AI advice finishes; lesson shown\n"+d.LastDialogueSource+"\n"+text.text);
 yield return null;panel.GetComponentInChildren<UnityEngine.UI.Button>().onClick.Invoke();yield return null;d.Choose(2);yield return null;
 if(g.State.phase!=HundredHour.RealityShow.ShowPhase.Route||g.State.learnedTechniques.Count==0)throw new Exception("retry did not retain lesson");
 System.IO.File.AppendAllText("/tmp/rival-ui-result.txt","\nPASS retry retains technique");
}
d.StartCoroutine(Verify());return "running";
