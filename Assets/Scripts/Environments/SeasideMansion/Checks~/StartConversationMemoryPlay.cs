var d=UnityEngine.Object.FindFirstObjectByType<HundredHour.Environments.MansionSignalDirector>();
d.saveEnabled=false;d.useAI=false;
var gate=UnityEngine.Object.FindFirstObjectByType<HundredHour.Localization.LanguageStartMenu>();if(gate)gate.Confirm();
System.Collections.IEnumerator Begin(){
 yield return null;d.arrival.StopAllCoroutines();d.arrival.enabled=false;d.BeginEncounter();yield return null;
 d.Game.StartLoop();d.Refresh();yield return null;d.Choose(0);yield return null;
 d.Choose(0);yield return null;d.Choose(0);yield return null;
 d.GetComponent<HundredHour.Environments.MansionMemoryJournal>().Open();
}
d.StartCoroutine(Begin());return "memory Play started; save disabled";
