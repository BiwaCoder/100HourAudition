// Execute through Unity Pipeline eval in a fresh Play session. Saves only Temp/MansionSignalCheck.json.
var d=UnityEngine.Object.FindFirstObjectByType<HundredHour.Environments.MansionSignalDirector>();
if(d==null||!Application.isPlaying)throw new Exception("Start SeasideMansion Play first.");
d.saveEnabled=true;d.testSavePath=System.IO.Path.GetFullPath("Temp/MansionSignalCheck.json");
Application.runInBackground=true;
System.Collections.IEnumerator VerifySignal(){
 var evidence=new System.Collections.Generic.List<string>();
 void Check(bool valid,string label){if(!valid)throw new Exception("Mansion signal: "+label);evidence.Add("PASS: "+label);}
 float deadline=Time.realtimeSinceStartup+60;
 while(d.arrival.Phase!=HundredHour.Environments.MansionArrivalDirector.ArrivalPhase.Walking && !d.Started){if(Time.realtimeSinceStartup>deadline)throw new Exception("Intro timeout");yield return null;}
 d.arrival.controls.player.position=d.arrival.counterpart.position+Vector3.forward*2.5f;
 while(!d.Started){if(Time.realtimeSinceStartup>deadline)throw new Exception("Encounter timeout");yield return null;}
 yield return new WaitForSecondsRealtime(2);
 Check(!d.arrival.controls.mover.enabled&&!d.arrival.controls.joystickCanvas.activeSelf,"arrival hands off and locks movement");
 Check(d.view.selfPortrait.sprite!=null&&d.view.otherPortrait.sprite==null,"self visible; counterpart unrevealed");
 ScreenCapture.CaptureScreenshot("Docs/SeasideSignal/01-first-contact.png");
 d.view.choices.SelectAndConfirm(2);yield return new WaitForSecondsRealtime(.6f);
 Check(d.Game.State.phase==HundredHour.RealityShow.ShowPhase.FirstReview,"existing ChoiceMenu confirmation reaches rules");
 d.Choose(0);yield return new WaitForSecondsRealtime(.15f);d.Choose(0);yield return new WaitForSecondsRealtime(.15f);
 Check(d.Game.State.phase==HundredHour.RealityShow.ShowPhase.FirstLoss&&d.Game.State.Player.eliminated,"first elimination");
 ScreenCapture.CaptureScreenshot("Docs/SeasideSignal/02-eliminated.png");yield return null;
 d.Choose(0);yield return new WaitForSecondsRealtime(2);
 ScreenCapture.CaptureScreenshot("Docs/SeasideSignal/03-reconnection.png");yield return null;
 d.Choose(0);yield return new WaitForSecondsRealtime(2);
 Check(d.view.otherPortrait.sprite==d.saoriSilhouette,"silhouette precedes identity");
 ScreenCapture.CaptureScreenshot("Docs/SeasideSignal/04-silhouette.png");yield return null;
 d.Choose(0);yield return new WaitForSecondsRealtime(.5f);
 Check(d.view.EyeOpening==0,"AI initially keeps eyes closed");
 ScreenCapture.CaptureScreenshot("Docs/SeasideSignal/05a-prayer-closed.png");
 yield return new WaitForSecondsRealtime(1.3f);
 Check(d.view.EyeOpening>0&&d.view.EyeOpening<1,"eye animation has intermediate frame");
 ScreenCapture.CaptureScreenshot("Docs/SeasideSignal/05b-prayer-opening.png");
 yield return new WaitForSecondsRealtime(1.3f);
 Check(d.view.EyeOpening==1,"eyes complete opening");
 Check(d.view.otherPortrait.sprite==d.saoriArt,"AI declaration resolves portrait");
 ScreenCapture.CaptureScreenshot("Docs/SeasideSignal/05-saori-reveal.png");yield return null;
 while(d.Game.State.phase==HundredHour.RealityShow.ShowPhase.Awakening){d.Choose(0);yield return new WaitForSecondsRealtime(.15f);}
 Check(d.Tutorial==1&&d.Game.State.phase==HundredHour.RealityShow.ShowPhase.Route,"tutorial enters simulated route");
 d.Choose(0);yield return new WaitForSecondsRealtime(2);
 int turn=d.Game.State.turn;int trust=d.Game.State.Player.trust;uint random=d.Game.State.randomState;
 Check(!d.view.choices.IsOpen&&!d.view.tools[1].interactable,"guided first observation blocks premature speech only");
 ScreenCapture.CaptureScreenshot("Docs/SeasideSignal/06-observation-tutorial.png");yield return null;
 d.view.tools[0].onClick.Invoke();yield return new WaitForSecondsRealtime(.2f);
 Check(d.Tutorial==2&&d.Game.State.forecast>=0&&d.Game.State.turn==turn&&d.Game.State.Player.trust==trust&&d.Game.State.randomState==random,"forecast previews without resolving talk or consuming RNG");
 Check(d.Game.State.agi==8,"one-time training energy");
 ScreenCapture.CaptureScreenshot("Docs/SeasideSignal/07-observed-future.png");yield return null;
 d.view.tools[1].onClick.Invoke();yield return new WaitForSecondsRealtime(.15f);
 Check(d.Game.State.perspective&&d.Game.State.agi==7,"optional perspective");
 d.view.tools[2].onClick.Invoke();yield return new WaitForSecondsRealtime(.15f);d.view.choices.SelectAndConfirm(1);yield return new WaitForSecondsRealtime(.6f);
 Check(d.Game.State.weather=="rain"&&d.Game.State.agi==5,"optional weather choice and cost");
 d.view.tools[3].onClick.Invoke();yield return new WaitForSecondsRealtime(.15f);
 Check(d.Game.State.phase==HundredHour.RealityShow.ShowPhase.Reward,"optional card draft");
 ScreenCapture.CaptureScreenshot("Docs/SeasideSignal/08-new-card.png");yield return null;
 int deck=d.Game.State.deck.Count;d.Choose(0);yield return new WaitForSecondsRealtime(.15f);
 Check(d.Game.State.deck.Count==deck+1&&d.Game.State.phase==HundredHour.RealityShow.ShowPhase.Conversation,"card choice returns to conversation");
 d.Choose(1);yield return new WaitForSecondsRealtime(.15f);
 Check(d.Tutorial==3&&d.Game.State.turn==1&&d.Game.State.weather=="clear","speech completes tutorial; weather expires");
 d.UseTool(6);yield return new WaitForSecondsRealtime(.15f);d.UseTool(2);yield return new WaitForSecondsRealtime(.15f);
 Check(d.Game.State.actions.Any(a=>a.id=="crafted"),"memory composes a playable line");
 d.Choose(d.Game.State.actions.FindIndex(a=>a.id=="crafted"));yield return new WaitForSecondsRealtime(.15f);
 Check(d.Game.State.memoryCards.Count>0,"memory card discovered from spoken memory");
 d.UseTool(6);yield return new WaitForSecondsRealtime(.15f);d.UseTool(6);yield return new WaitForSecondsRealtime(.15f);
 int memories=d.Game.State.memories.Count;int keptDeck=d.Game.State.deck.Count;int energy=d.Game.State.agi;
 d.view.tools[4].onClick.Invoke();yield return new WaitForSecondsRealtime(.15f);
 Check(d.Game.State.phase==HundredHour.RealityShow.ShowPhase.Route&&d.Game.State.timeLeaps==1&&d.Game.State.memories.Count==memories&&d.Game.State.deck.Count==keptDeck&&d.Game.State.agi==energy-3,"leap restores route and retains memories/cards with cost");
 d.Choose(1);yield return new WaitForSecondsRealtime(.15f);
 string saved=JsonUtility.ToJson(d.Game.State);d.Game.State.stress=99;d.Resume();yield return new WaitForSecondsRealtime(.15f);
 Check(JsonUtility.ToJson(d.Game.State)==saved&&d.Tutorial==3,"save/resume preserves exact rules and tutorial state");
 int steps=0;
 while(d.Game.State.phase!=HundredHour.RealityShow.ShowPhase.Victory&&d.Game.State.phase!=HundredHour.RealityShow.ShowPhase.Eliminated&&steps++<60){
  var s=d.Game.State;
  if(s.phase==HundredHour.RealityShow.ShowPhase.Conversation){
   for(int n=0;n<7;n++){int card=s.hand.FindIndex(id=>d.Game.Card(id).cost<=s.focus&&id!="confess"&&id!="blackout");if(card<0)break;d.PlayCard(card);yield return null;}
   int best=Enumerable.Range(0,s.actions.Count).OrderByDescending(i=>d.Game.Power(s.actions[i])).First();d.Choose(best);
  }else d.Choose(0);
  yield return new WaitForSecondsRealtime(.15f);
 }
 Check(steps<60,"integrated game reaches ending without deadlock");
 ScreenCapture.CaptureScreenshot("Docs/SeasideSignal/09-ending.png");
 System.IO.File.WriteAllLines("Docs/SeasideSignal/PlayVerification.txt",evidence);
 Debug.Log("MANSION CHECK COMPLETE: "+evidence.Count+" assertions. Ending="+d.Game.State.phase);
}
d.StartCoroutine(VerifySignal());return "Play verification running; read Docs/SeasideSignal/PlayVerification.txt when complete.";
