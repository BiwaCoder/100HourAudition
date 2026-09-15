System.Collections.IEnumerator Verify(){
var controller=UnityEngine.Object.FindFirstObjectByType<HundredHour.RealityShow.ShowController>();var view=controller.GetComponent<HundredHour.RealityShow.ShowView>();
var failures=new System.Collections.Generic.List<string>();var savePath=HundredHour.RealityShow.ShowSaveStore.Path;string backup=System.IO.File.Exists(savePath)?System.IO.File.ReadAllText(savePath):null;
void Require(bool value,string why){if(!value)failures.Add(why);}
System.Collections.IEnumerator Pick(int n){yield return null;view.choices.SelectAndConfirm(n);yield return new UnityEngine.WaitForSecondsRealtime(.7f);float deadline=UnityEngine.Time.realtimeSinceStartup+38;while(controller.Busy&&UnityEngine.Time.realtimeSinceStartup<deadline)yield return null;Require(!controller.Busy,"button action timeout");}
try{
yield return new UnityEngine.WaitForEndOfFrame();UnityEngine.ScreenCapture.CaptureScreenshot("Screenshots/RealityShowTitle.png");
view.startButton.onClick.Invoke();yield return null;Require(controller.Game.State.phase==HundredHour.RealityShow.ShowPhase.Opening,"start button");
for(int i=0;i<3;i++)yield return Pick(0);Require(controller.Game.State.phase==HundredHour.RealityShow.ShowPhase.FirstChoice,"opening sequence");
yield return Pick(1);yield return Pick(0);yield return Pick(0);Require(controller.Game.State.Player.eliminated&&view.castCards[0].GetComponent<HundredHour.UI.Participants.ParticipantCardView>().OutVisible,"first loss OUT");UnityEngine.ScreenCapture.CaptureScreenshot("Screenshots/RealityShowFirstLoss.png");
yield return Pick(0);yield return Pick(0);Require(controller.Game.State.loop==1,"awakening starts loop");yield return Pick(0);
Require(controller.Game.State.phase==HundredHour.RealityShow.ShowPhase.Conversation,"route to conversation");
view.handButtons[0].onClick.Invoke();Require(controller.Game.State.focus<3,"hand button spends focus");
yield return Pick(1);Require(controller.Game.State.introduced,"introduce by choice UI");
view.memoryButtons[0].onClick.Invoke();view.askButton.onClick.Invoke();Require(controller.Game.State.actions.Any(a=>a.id=="crafted"),"memory button synthesis");
controller.useAI=true;yield return Pick(controller.Game.State.actions.FindIndex(a=>a.id=="crafted"));controller.useAI=false;
Require(controller.LastDialogueSource=="PythonAPI","actual AI reply");System.IO.File.WriteAllText("Temp/RealityShowAIReply.txt",controller.LastRawReply+"\n\nAccepted reply: "+controller.Game.State.lastNpc+"\nsource="+controller.LastDialogueSource+"\nerror="+controller.LastError);
Require(controller.Game.State.memories.Any(m=>m.used>0)&&controller.Game.State.memoryCards.Count>0,"memory creates reward card");
int oldCount=controller.Game.State.memories.Count;int oldTurn=controller.Game.State.turn;view.homeButton.onClick.Invoke();view.continueButton.onClick.Invoke();yield return null;Require(controller.Game.State.memories.Count==oldCount&&controller.Game.State.turn==oldTurn,"continue button restores conversation");
view.acquireButton.onClick.Invoke();Require(controller.Game.State.phase==HundredHour.RealityShow.ShowPhase.Reward,"draft from ability");yield return Pick(0);Require(controller.Game.State.turn==oldTurn&&controller.Game.State.phase==HundredHour.RealityShow.ShowPhase.Conversation,"draft returns to same conversation");
yield return new UnityEngine.WaitForEndOfFrame();UnityEngine.ScreenCapture.CaptureScreenshot("Screenshots/RealityShowConversation.png");
view.feedTab.onClick.Invoke();yield return new UnityEngine.WaitForSecondsRealtime(.2f);int page=view.feed.PageIndex;view.feed.GetComponent<HundredHour.UI.Timeline.TimelineFeedView>().NextButton.onClick.Invoke();yield return new UnityEngine.WaitForSecondsRealtime(.6f);Require(view.feed.PageIndex==page+1,"feed pagination");UnityEngine.ScreenCapture.CaptureScreenshot("Screenshots/RealityShowFeed.png");yield return new UnityEngine.WaitForSecondsRealtime(.15f);view.memoryTab.onClick.Invoke();
int steps=0;bool ceremonySeen=false;
while(controller.Game.State.phase!=HundredHour.RealityShow.ShowPhase.Victory&&controller.Game.State.phase!=HundredHour.RealityShow.ShowPhase.Eliminated&&steps++<70){
var s=controller.Game.State;
if(s.phase==HundredHour.RealityShow.ShowPhase.Conversation){for(int i=0;i<7;i++){int hand=s.hand.FindIndex(id=>controller.Game.Card(id).cost<=s.focus&&id!="blackout"&&id!="confess");if(hand<0)break;view.handButtons[hand].onClick.Invoke();}int action=Enumerable.Range(0,s.actions.Count).OrderByDescending(n=>controller.Game.Power(s.actions[n])).First();yield return Pick(action);}
else{if(s.phase==HundredHour.RealityShow.ShowPhase.Ceremony&&!ceremonySeen){ceremonySeen=true;UnityEngine.ScreenCapture.CaptureScreenshot("Screenshots/RealityShowCeremony.png");}yield return Pick(0);}
}
Require(steps<70,"run terminates");Require(controller.Game.State.phase==HundredHour.RealityShow.ShowPhase.Victory,"UI run reaches victory");UnityEngine.ScreenCapture.CaptureScreenshot("Screenshots/RealityShowFinale.png");
Require(UnityEngine.Object.FindObjectsByType<UnityEngine.EventSystems.EventSystem>(UnityEngine.FindObjectsSortMode.None).Length==1,"one EventSystem");Require(controller.LastError==null,"controller errors");
}finally{
controller.useAI=false;
if(backup!=null)System.IO.File.WriteAllText(savePath,backup);else if(System.IO.File.Exists(savePath))System.IO.File.Delete(savePath);
System.IO.File.WriteAllText("Temp/RealityShowPlayResults.txt","PASS="+(failures.Count==0)+"\n"+string.Join("\n",failures));
}
}
UnityEngine.EventSystems.EventSystem.current.StartCoroutine(Verify());return "Testing full UI flow and one real AI conversation";
