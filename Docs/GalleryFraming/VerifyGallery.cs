var d=UnityEngine.Object.FindFirstObjectByType<HundredHour.Environments.MansionSignalDirector>();
d.saveEnabled=false;d.useAI=false;
System.Collections.IEnumerator Verify(){
var old=HundredHour.Localization.GameLanguage.Current;
var language=UnityEngine.Object.FindFirstObjectByType<HundredHour.Localization.LanguageStartMenu>();if(language)language.Confirm();
yield return null;
d.arrival.cinema.Stop();d.arrival.enabled=false;d.BeginEncounter();
d.Game.State.phase=HundredHour.RealityShow.ShowPhase.Awakening;d.Game.Advance();d.Game.ChooseArchetype(2);d.Game.State.chapter=1;d.Game.State.contestants.First(c=>c.id=="shiori").eliminated=true;
d.Game.ChooseRoute(3);d.Refresh();
var stage=UnityEngine.Object.FindFirstObjectByType<HundredHour.Environments.MansionConversationStage>();
var notes=new System.Collections.Generic.List<string>();
foreach(var locale in new[]{HundredHour.Localization.GameLocale.Japanese,HundredHour.Localization.GameLocale.English}){
HundredHour.Localization.GameLanguage.Select(locale,false);d.Refresh();d.view.SetDialogue(locale.ToString(),"Gallery framing / 回廊の会話",new[]{"Next"});
foreach(var id in new string[]{null,"himari","yuto","ren","hikari"}){
stage.FocusActor(id);yield return new WaitForSecondsRealtime(2);
var cam=d.arrival.controls.outputCamera;
var actors=new[]{d.arrival.controls.player.transform,d.arrival.counterpart,GameObject.Find("Conversation Rival 0").transform,GameObject.Find("Conversation Rival 1").transform};
foreach(var actor in actors){
var p=cam.WorldToViewportPoint(actor.position);
if(p.z<=0||p.x<0||p.x>1||p.y<.26f||p.y>.84f)throw new Exception("Out of visible area: "+actor.name+" "+p);
var delta=actor.position-cam.transform.position;
var hits=Physics.RaycastAll(cam.transform.position,delta.normalized,delta.magnitude-.7f).Where(h=>h.collider.enabled&&!h.collider.isTrigger&&!actors.Any(a=>h.transform==a||h.transform.IsChildOf(a))).Select(h=>h.collider.name).ToArray();
if(hits.Length>0)throw new Exception("Obscured "+actor.name+": "+string.Join(",",hits));
}
notes.Add("PASS "+locale+" focus="+(id??"ensemble")+": all four cube centers visible and unoccluded");
}
foreach(var label in new[]{d.view.selfLabel,d.view.otherLabel})if(Mathf.Abs(label.fontSizeMax-21.6f)>.01f)throw new Exception("Label size");
ScreenCapture.CaptureScreenshot("Docs/GalleryFraming/"+locale+".png");yield return null;
}
HundredHour.Localization.GameLanguage.Select(old,false);
System.IO.File.WriteAllLines("Docs/GalleryFraming/Verification.txt",notes);
}
d.StartCoroutine(Verify());return "started";
