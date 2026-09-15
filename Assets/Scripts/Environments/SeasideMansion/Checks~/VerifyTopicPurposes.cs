var d=UnityEngine.Object.FindFirstObjectByType<HundredHour.Environments.MansionSignalDirector>();
if(d.Game.SharedMemories().Count<2){d.Game.ResolveTalk(0,HundredHour.RealityShow.ShowDialogue.Local(d.Game,d.Game.State.actions[0]));d.Refresh();}
var journal=d.GetComponent<HundredHour.Environments.MansionMemoryJournal>();
System.Collections.IEnumerator Purposes(){
 var report=new System.Collections.Generic.List<string>();
 journal.Open();yield return new WaitForSecondsRealtime(.3f);
 foreach(string name in new[]{"MemoryToggle","SelectMemoryA","CloseMemory"})
 {
  var button=d.view.stage.GetComponentsInChildren<UnityEngine.UI.Button>().First(b=>b.name==name);
  var rt=(RectTransform)button.transform;var point=RectTransformUtility.WorldToScreenPoint(null,rt.TransformPoint(rt.rect.center));
  var pointer=new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current){position=point};
  var hits=new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();UnityEngine.EventSystems.EventSystem.current.RaycastAll(pointer,hits);
  if(hits.Count==0||!hits[0].gameObject.transform.IsChildOf(button.transform))throw new Exception("Raycast blocked: "+name);
  report.Add("PASS raycast "+name);
 }
 journal.Close();
 for(int goal=0;goal<3;goal++)
 {
  d.useAI=true;d.saveEnabled=false;d.Game.State.agi=8;d.Game.State.acquired=false;
  var memories=d.Game.SharedMemories();d.Game.SelectRecall(memories[0].id);d.Game.SelectRecall(memories[1].id,true);
  d.UseTool(0);yield return new WaitForSecondsRealtime(.3f);d.UseTool(goal);
  float deadline=Time.realtimeSinceStartup+75;while(d.Busy&&Time.realtimeSinceStartup<deadline)yield return new WaitForSecondsRealtime(.3f);
  if(d.Busy)throw new Exception("Purpose request timeout");
  if(!d.Game.TopicMatchesGoal(d.Game.State.actions[0].line))throw new Exception("Purpose lost in generated line");
  report.Add(HundredHour.RealityShow.ShowGame.TopicGoals[goal]+" / "+d.LastDialogueSource+" / "+d.Game.State.actions[0].line);
  System.IO.File.WriteAllLines("Docs/ConversationMemory/purposes-and-raycast.txt",report);
  yield return new WaitForSecondsRealtime(.3f);
 }
}
d.StartCoroutine(Purposes());return "Checking three AI purposes on identical memories, no speech and no save";
