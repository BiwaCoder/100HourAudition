var d=UnityEngine.Object.FindFirstObjectByType<HundredHour.Environments.MansionSignalDirector>();
var journal=d.GetComponent<HundredHour.Environments.MansionMemoryJournal>();
System.Collections.IEnumerator Runs(){
 var report=new System.Collections.Generic.List<string>();
 journal.Close();d.saveEnabled=false;d.Game.TelemetryEnabled=false;
 for(int run=0;run<2;run++)
 {
  d.useAI=false;d.Game.Begin(120+run,HundredHour.RealityShow.ShowDifficulty.Easy);d.Game.StartLoop();d.Refresh();yield return new WaitForSecondsRealtime(.3f);
  for(int step=0;step<32;step++)
  {
   var s=d.Game.State;
   if(s.phase==HundredHour.RealityShow.ShowPhase.Route||s.phase==HundredHour.RealityShow.ShowPhase.Ceremony){d.Choose(s.phase==HundredHour.RealityShow.ShowPhase.Route?run:0);yield return new WaitForSecondsRealtime(.3f);continue;}
   if(s.phase!=HundredHour.RealityShow.ShowPhase.Conversation)break;
   var memories=d.Game.SharedMemories();
   if(memories.Count>0)
   {
    d.Game.SelectRecall(memories.Last().id);if(memories.Count>1)d.Game.SelectRecall(memories[memories.Count-2].id,true);
    if(s.turn==2&&s.agi>=1)
    {
     d.useAI=true;d.UseTool(0);yield return new WaitForSecondsRealtime(.3f);d.UseTool((run+s.chapter)%3);
     float deadline=Time.realtimeSinceStartup+75;while(d.Busy&&Time.realtimeSinceStartup<deadline)yield return new WaitForSecondsRealtime(.2f);
     if(d.Busy)throw new Exception("Full Play topic timeout");
     report.Add($"run {run+1} chapter {s.chapter+1}: {d.LastDialogueSource} / {s.actions[0].line}");
    }
    else{d.useAI=false;d.SpeakFromMemory();}
   }
   else{d.useAI=false;d.Choose(0);}
   yield return new WaitForSecondsRealtime(.3f);
   int chosen=Enumerable.Range(0,s.actions.Count).OrderByDescending(i=>d.Game.Power(s.actions[i])).First();
   if(s.chapter==1&&s.turn==2)UnityEngine.ScreenCapture.CaptureScreenshot("Docs/ConversationMemory/04-group-topics.png");
   d.view.choices.SelectAndConfirm(chosen);yield return new WaitForSecondsRealtime(.4f);
   float replyDeadline=Time.realtimeSinceStartup+45;while(d.Busy&&Time.realtimeSinceStartup<replyDeadline)yield return new WaitForSecondsRealtime(.2f);
   if(d.Busy)throw new Exception("Full Play reply timeout");
   yield return new WaitForSecondsRealtime(.2f);
   System.IO.File.WriteAllLines("Docs/ConversationMemory/full-play-runs.txt",report);
  }
  report.Add($"run {run+1} ended {d.Game.State.phase} / memories {d.Game.SharedMemories().Count} / understanding {d.Game.State.understanding} / trust {d.Game.State.Player.trust}");
  System.IO.File.WriteAllLines("Docs/ConversationMemory/full-play-runs.txt",report);
 }
 UnityEngine.ScreenCapture.CaptureScreenshot("Docs/ConversationMemory/05-result.png");
}
d.StartCoroutine(Runs());return "Two complete Play runs started, test saves disabled";
