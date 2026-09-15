var d=UnityEngine.Object.FindFirstObjectByType<HundredHour.Environments.MansionSignalDirector>();
if(!d||d.content.playerGender!="male")throw new Exception("Male game not active");
d.saveEnabled=false;d.useAI=false;
var notes=new System.Collections.Generic.List<string>();
void Check(bool ok,string label){if(!ok)throw new Exception(label);notes.Add("PASS "+label);}
Check(d.content.cast.Count==5,"player, one love interest and three rivals");
Check(d.content.Person("yuto").displayName=="朝倉美月","female romantic lead");
foreach(var id in new[]{"konoa","hikari","shiori"})Check(d.content.Person(id).portrait&&d.content.Person(id).personality.Contains("男性"),id+" male rival with portrait");
Check(d.content.Person("himari").portrait,"default male player has portrait");
Check(d.SavePath.Contains("game-male-"),"male save path isolated");
System.Collections.IEnumerator Verify(){
 var lang=UnityEngine.Object.FindFirstObjectByType<HundredHour.Localization.LanguageStartMenu>();if(lang)lang.Confirm();yield return null;yield return null;yield return null;
 d.arrival.cinema.Stop();d.arrival.enabled=false;d.BeginEncounter();yield return null;
 d.Game.State.phase=HundredHour.RealityShow.ShowPhase.Awakening;d.Game.Advance();d.Game.ChooseArchetype(0);d.Game.State.chapter=1;d.Game.State.contestants.First(c=>c.id=="shiori").eliminated=true;d.Game.ChooseRoute(0);d.Refresh();yield return new WaitForSecondsRealtime(3);
 var context=HundredHour.RealityShow.ShowDialogue.Context(d.Game,d.Game.State.actions[0]);
 Check(context.Contains("朝倉美月")&&context.Contains("male")&&!context.Contains("プレイヤーはひまり"),"AI context uses selected cast and gender");
 Check(!d.view.body.text.Contains("悠斗")&&!d.view.status.text.Contains("ガール"),"visible dialogue and archetype use male cast");
 var portraits=d.view.stage.GetComponentsInChildren<UnityEngine.UI.Image>().Count(i=>i.name.Contains("Portrait"));
 ScreenCapture.CaptureScreenshot("Docs/AuditionEntry/Gender/male-group.png");
 System.IO.File.WriteAllLines("Docs/AuditionEntry/Gender/Verification.txt",notes);
}
d.StartCoroutine(Verify());return "Checking male group scene";
