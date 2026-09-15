System.Collections.IEnumerator Verify(){
var d=UnityEngine.Object.FindFirstObjectByType<HundredHour.RealityShow.Cinema.ShowFilmDirector>();var c=d.controller;var v=c.GetComponent<HundredHour.RealityShow.ShowView>();
var errors=new System.Collections.Generic.List<string>();
void Check(bool ok,string name){if(!ok)errors.Add(name);}
System.Collections.IEnumerator Pick(int i){d.Skip();yield return null;v.choices.SelectAndConfirm(i);yield return new UnityEngine.WaitForSecondsRealtime(.65f);float end=UnityEngine.Time.realtimeSinceStartup+5;while(c.Busy&&UnityEngine.Time.realtimeSinceStartup<end)yield return null;Check(!c.Busy,"choice completed");}
System.Collections.IEnumerator Capture(string name){yield return new UnityEngine.WaitForSecondsRealtime(.9f);UnityEngine.ScreenCapture.CaptureScreenshot("Screenshots/AuditionFilm"+name+".png");yield return new UnityEngine.WaitForSecondsRealtime(.3f);}
try{
d.Skip();c.Command("home");yield return null;yield return Capture("Title");v.startButton.onClick.Invoke();yield return null;
Check(c.PresentationLocked&&d.player.IsPlaying,"cinematic locks choices");int page=c.Game.State.openingPage;c.Choose(0);Check(c.Game.State.openingPage==page,"choices cannot skip scenario during movie");
for(int n=0;n<3;n++)yield return Pick(0);Check(c.Game.State.phase==HundredHour.RealityShow.ShowPhase.FirstChoice,"three intros");d.Skip();yield return Capture("Choice");
yield return Pick(2);Check(c.Game.State.phase==HundredHour.RealityShow.ShowPhase.FirstReview,"studio transition");yield return Capture("Hosts");yield return Pick(0);yield return Capture("Analyst");yield return Pick(0);d.Skip();
Check(c.Game.State.Player.eliminated,"first elimination");v.castTab.onClick.Invoke();yield return Capture("Loss");Check(v.castPanel.activeSelf&&v.castCards[0].GetComponent<HundredHour.UI.Participants.ParticipantCardView>().OutVisible,"OUT in cast tab");
v.memoryTab.onClick.Invoke();yield return Pick(0);yield return Capture("Awakening");yield return Pick(0);yield return Pick(0);
Check(c.Game.State.phase==HundredHour.RealityShow.ShowPhase.Conversation,"playable conversation");yield return Capture("Conversation");d.Skip();int energy=c.Game.State.agi;v.leapButton.onClick.Invoke();Check(c.Game.State.phase==HundredHour.RealityShow.ShowPhase.Route&&c.Game.State.agi==energy-3,"leap UI");yield return Capture("TimeLeap");yield return Pick(1);d.Skip();
var state=c.Game.State;int turn=state.turn;v.handButtons[0].onClick.Invoke();Check(!c.PresentationLocked&&!d.player.IsPlaying&&state.turn==turn,"card does not replay film or advance turn");
yield return Pick(1);d.Skip();Check(c.Game.State.turn==1&&c.Game.State.memories.Count>0,"conversation choice stores memory");v.feedTab.onClick.Invoke();yield return Capture("Feed");
Check(d.player.LastError==null&&d.screen.texture!=null,"render and camera valid");
v.memoryTab.onClick.Invoke();int steps=0;
while(c.Game.State.phase!=HundredHour.RealityShow.ShowPhase.Victory&&c.Game.State.phase!=HundredHour.RealityShow.ShowPhase.GameOver&&steps++<65){
d.Skip();var s=c.Game.State;
if(s.phase==HundredHour.RealityShow.ShowPhase.Conversation){
for(int n=0;n<7;n++){int card=s.hand.FindIndex(id=>c.Game.Card(id).cost<=s.focus&&id!="confess"&&id!="blackout");if(card<0)break;v.handButtons[card].onClick.Invoke();}
int best=System.Linq.Enumerable.First(System.Linq.Enumerable.OrderByDescending(System.Linq.Enumerable.Range(0,s.actions.Count),i=>c.Game.Power(s.actions[i])));yield return Pick(best);
}else if(s.phase==HundredHour.RealityShow.ShowPhase.Ceremony){v.castTab.onClick.Invoke();yield return Capture("Ceremony");v.memoryTab.onClick.Invoke();yield return Pick(0);
}else yield return Pick(0);
}
Check(c.Game.State.phase==HundredHour.RealityShow.ShowPhase.Victory,"full film run reaches victory");yield return Capture("Finale");d.Skip();
Check(d.player.LastError==null,"all runtime shots valid");
}finally{System.IO.File.WriteAllText("Temp/FilmPlayResults.txt",errors.Count==0?"PASS: UI start, 3 intros, locked choices, first selection, two studio hosts, OUT, awakening, conversation, active leap, card/movie isolation, memory, feed, live RenderTexture and full run to victory.":"FAIL: "+string.Join("; ",errors));}
}
UnityEngine.EventSystems.EventSystem.current.StartCoroutine(Verify());return "Film verification started";
