System.Collections.IEnumerator Verify(){
var d=UnityEngine.Object.FindFirstObjectByType<HundredHour.RealityShow.Cinema.ShowFilmDirector>();var c=d.controller;var v=c.GetComponent<HundredHour.RealityShow.ShowView>();var errors=new System.Collections.Generic.List<string>();bool completed=false;
void Check(bool ok,string label){if(!ok)errors.Add(label);}
try{
c.Game.Begin(117,HundredHour.RealityShow.ShowDifficulty.Easy);for(int i=0;i<3;i++)c.Game.Advance();c.Game.FirstChoice(0);for(int i=0;i<4;i++)c.Game.Advance();c.Game.ChooseRoute(0);c.Refresh();yield return new UnityEngine.WaitForSecondsRealtime(.5f);
Check(d.player.IsPlaying&&d.screen.texture!=null&&c.PresentationLocked,"play and texture after enable");int turn=c.Game.State.turn;
d.enabled=false;yield return null;Check(!c.PresentationLocked&&!d.player.IsPlaying&&d.screen.texture==null&&!d.skipButton.gameObject.activeSelf,"detach frees render and choices");Check(c.Game.State.turn==turn,"detach keeps game state");
d.enabled=true;yield return null;Check(d.player.IsPlaying&&d.screen.texture!=null,"reattach recreates render");
float end=UnityEngine.Time.realtimeSinceStartup+22;while(d.player.IsPlaying&&UnityEngine.Time.realtimeSinceStartup<end)yield return null;
Check(!d.player.IsPlaying&&!c.PresentationLocked,"natural film completion unlocks");
UnityEngine.ScreenCapture.CaptureScreenshot("Screenshots/AuditionFilmPlayable.png");yield return new UnityEngine.WaitForSecondsRealtime(.3f);
var pointer=new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current);pointer.position=UnityEngine.RectTransformUtility.WorldToScreenPoint(null,v.leapButton.transform.position+new UnityEngine.Vector3(120,-20,0));var hits=new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();UnityEngine.EventSystems.EventSystem.current.RaycastAll(pointer,hits);Check(hits.Count>0,"UI accepts pointer raycasts");
c.Game.State.phase=HundredHour.RealityShow.ShowPhase.Route;c.Game.State.chapter=1;c.Game.State.contestants[1].eliminated=true;c.Refresh();yield return new UnityEngine.WaitForSecondsRealtime(.6f);Check(!System.Linq.Enumerable.First(d.cast,a=>a.actorId==c.Game.State.contestants[1].id).gameObject.activeSelf,"eliminated actor absent from next stage");Check(d.player.LastError==null,"filtered group framing valid");d.Skip();completed=true;
}finally{System.IO.File.WriteAllText("Temp/FilmLifecycleResults.txt",completed&&errors.Count==0?"PASS: detach/re-enable render lifecycle, unchanged game state, natural movie completion, pointer input, eliminated actor visibility, camera binding.":"FAIL: "+string.Join(";",errors));}
}
UnityEngine.EventSystems.EventSystem.current.StartCoroutine(Verify());return "Lifecycle verification started";
