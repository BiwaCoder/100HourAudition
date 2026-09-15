var d=UnityEngine.Object.FindFirstObjectByType<HundredHour.Environments.MansionSignalDirector>();
if(!d||!Application.isPlaying)throw new Exception("Play the mansion first");
d.saveEnabled=false;d.useAI=false;
System.Collections.IEnumerator Verify(){
 var notes=new System.Collections.Generic.List<string>();
 void Check(bool ok,string label){if(!ok)throw new Exception(label);notes.Add("PASS "+label);}
 var language=UnityEngine.Object.FindFirstObjectByType<HundredHour.Localization.LanguageStartMenu>();if(language)language.Confirm();
 yield return null;yield return null;yield return null;
 d.arrival.cinema.Stop();d.arrival.enabled=false;d.arrival.controls.player.position=d.arrival.counterpart.position+Vector3.forward*4;
 d.arrival.controls.SelectView(3);yield return new WaitForSecondsRealtime(2);
 var a=d.view.selfWindow;var b=d.view.otherWindow;
 var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
 var camera=(Camera)typeof(HundredHour.Environments.MansionSignalView).GetField("worldCamera",flags).GetValue(d.view);
 var layout=typeof(HundredHour.Environments.MansionSignalView).GetMethod("LayoutPortraits",flags);
 var rotation=camera.transform.rotation;var positionA=a.anchoredPosition;var positionB=b.anchoredPosition;
 try
 {
  for(int i=-10;i<=10;i++)
  {
   camera.transform.rotation=rotation*Quaternion.Euler(0,i*.2f,0);layout.Invoke(d.view,null);
   Check(a.anchoredPosition==positionA&&b.anchoredPosition==positionB,"small camera angle change keeps portrait positions "+i);
  }
 }
 finally{camera.transform.rotation=rotation;}
 Check(!new Rect(a.anchoredPosition,a.sizeDelta).Overlaps(new Rect(b.anchoredPosition,b.sizeDelta)),"aligned actors have separate portrait windows");
 ScreenCapture.CaptureScreenshot("Docs/ConversationDeck/PortraitScroll/01-walking.png");yield return null;
 d.BeginEncounter();yield return null;d.Game.State.phase=HundredHour.RealityShow.ShowPhase.Awakening;d.Game.Advance();d.Game.ChooseArchetype(2);d.Game.State.chapter=1;d.Game.State.contestants.First(c=>c.id=="shiori").eliminated=true;d.Game.ChooseRoute(0);d.Refresh();yield return new WaitForSecondsRealtime(3);
 var windows=new[]{d.view.selfWindow,d.view.otherWindow,d.view.stage.Find("Rival Portrait 0") as RectTransform,d.view.stage.Find("Rival Portrait 1") as RectTransform};
 for(int i=0;i<windows.Length;i++)for(int j=i+1;j<windows.Length;j++)Check(!new Rect(windows[i].anchoredPosition,windows[i].sizeDelta).Overlaps(new Rect(windows[j].anchoredPosition,windows[j].sizeDelta)),"group portrait pair "+i+"/"+j+" does not overlap");
 ScreenCapture.CaptureScreenshot("Docs/ConversationDeck/PortraitScroll/02-group.png");yield return null;
 string longMessage=string.Join("\n",Enumerable.Range(1,14).Select(i=>i+"．ゲームや物語を作るのが好きなんですね。私も作品に込めた気持ちや、うまくいかなかった経験を聞いてみたいです。"))+"\n最後まで読んでくれて、ありがとう。";
 d.view.SetDialogue("松村悠斗",longMessage,new[]{"その理由を聞かせて。","私も創作の話をしたい。","次は一緒に考えよう。"});yield return null;yield return null;
 var message=d.view.dialoguePanel.GetComponentInChildren<HundredHour.Environments.MansionMessageScroll>();var scroll=message.GetComponent<UnityEngine.UI.ScrollRect>();
 Check(message.Overflowing&&scroll.verticalScrollbar.gameObject.activeSelf,"long text enables subtle scrollbar");
 Check(scroll.content.rect.height>scroll.viewport.rect.height,"scroll content measures actual wrapped text height");
 ScreenCapture.CaptureScreenshot("Docs/ConversationDeck/PortraitScroll/03-long-text.png");yield return null;
 scroll.OnScroll(new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current){scrollDelta=new Vector2(0,-3)});yield return null;
 Check(scroll.content.anchoredPosition.y>0,"mouse wheel scrolls long text");
 scroll.verticalNormalizedPosition=0;yield return null;yield return null;
 Check(scroll.content.anchoredPosition.y>0,"scroll reaches later lines");
 ScreenCapture.CaptureScreenshot("Docs/ConversationDeck/PortraitScroll/04-text-bottom.png");yield return null;
 d.view.SetDialogue("松村悠斗","続きを聞かせて。",new[]{"うん。"});yield return null;yield return null;
 Check(!message.Overflowing&&!scroll.verticalScrollbar.gameObject.activeSelf,"short text hides scrollbar and resets scroll");
 Check(Mathf.Abs(scroll.content.anchoredPosition.y)<1,"new short message resets to top");
 System.IO.File.WriteAllLines("Docs/ConversationDeck/PortraitScroll/Verification.txt",notes);
}
d.StartCoroutine(Verify());return "Portrait and scroll check running";
