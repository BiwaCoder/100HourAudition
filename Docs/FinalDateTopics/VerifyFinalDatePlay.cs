if(GameObject.Find("FinalDateVerification"))return "already started";new GameObject("FinalDateVerification");
var d=UnityEngine.Object.FindFirstObjectByType<HundredHour.Environments.MansionSignalDirector>();d.saveEnabled=false;d.useAI=false;
System.Collections.IEnumerator Verify(){
var notes=new List<string>();void Check(bool ok,string name){if(!ok)throw new Exception(name);notes.Add("PASS "+name);}
var old=HundredHour.Localization.GameLanguage.Current;
try{
var language=UnityEngine.Object.FindFirstObjectByType<HundredHour.Localization.LanguageStartMenu>();if(language)language.Confirm();yield return null;
d.arrival.cinema.Stop();d.arrival.enabled=false;d.BeginEncounter();d.Game.State.phase=HundredHour.RealityShow.ShowPhase.Awakening;d.Game.Advance();d.Game.ChooseArchetype(2);d.Game.ChooseRoute(0);
var s=d.Game.State;s.chapter=2;s.turn=3;d.Game.TelemetryEnabled=false;
s.conversations.Add(new HundredHour.RealityShow.ConversationMemory{id="memory-action-test",partner=s.target,topic="静かなカフェ",reply="静かなカフェで絵を描いていると落ち着く。",loop=s.loop,timeline=s.timeLeaps,chapter=s.chapter,turn=s.turn});
s.recallA="memory-action-test";
var journal=UnityEngine.Object.FindFirstObjectByType<HundredHour.Environments.MansionMemoryJournal>();
int focus=s.focus,ai=s.agi;d.SpeakFromMemory();
Check(s.focus==focus-1&&s.agi==ai,"human action spends one Focus and no AI");Check(s.actions.Any(a=>a.id=="recall_memory"&&a.line.Contains("カフェ")),"human action deepens selected topic");
d.SpeakFromMemory();Check(s.focus==focus-1,"repeat human action cannot charge twice");
foreach(var locale in new[]{HundredHour.Localization.GameLocale.Japanese,HundredHour.Localization.GameLocale.English}){
HundredHour.Localization.GameLanguage.Select(locale,false);d.Refresh();
Check(d.view.tools.Count(b=>b.gameObject.activeSelf)==1,"only advice shown: "+locale);
Check(d.view.toolLabels[0].text.Contains(locale==HundredHour.Localization.GameLocale.English?"Saori":"沙織"),"remaining button is Saori advice");
journal.Open();yield return null;yield return null;
var selection=GameObject.Find("Selection").GetComponent<TMPro.TMP_Text>();selection.ForceMeshUpdate();Check(selection.preferredHeight<=selection.rectTransform.rect.height,"explanation fits above buttons");
var button=GameObject.Find("RecallWithoutAI").GetComponent<UnityEngine.UI.Button>();
Check(button.GetComponentInChildren<TMPro.TMP_Text>().text.Contains(locale==HundredHour.Localization.GameLocale.English?"Deepen":"同じ話題"),"human action label localized");
ScreenCapture.CaptureScreenshot("Docs/FinalDateTopics/"+locale+".png");yield return null;journal.Close();
}
s.recallA="";ai=s.agi;d.GenerateFromMemories();Check(s.agi==ai&&!s.acquired,"AI requires selected memory");
s.recallA="memory-action-test";s.acquired=false;d.useAI=true;d.GenerateFromMemories();
Check(s.agi==ai-d.Game.SkillCost("acquire"),"AI action charges AI once");d.GenerateFromMemories();Check(s.agi==ai-d.Game.SkillCost("acquire"),"busy AI cannot double charge");
float deadline=Time.realtimeSinceStartup+50;while(d.Busy&&Time.realtimeSinceStartup<deadline)yield return null;
Check(!d.Busy,"AI request completes");
notes.Add("SOURCE "+d.LastDialogueSource);notes.Add("GENERATED "+s.actions.First(a=>a.id=="ai_topic").line);Check(s.actions.First(a=>a.id=="ai_topic").line.ToLowerInvariant().Contains("marry"),"AI proposes marriage in final conversation");
Check(s.actions.First(a=>a.id=="ai_topic").recallA=="memory-action-test","AI result retains selected memory association");
}finally{HundredHour.Localization.GameLanguage.Select(old,false);System.IO.File.WriteAllLines("Docs/FinalDateTopics/Verification.txt",notes);}
}
d.StartCoroutine(Verify());return "started";
