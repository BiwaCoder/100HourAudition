var path=AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("t:ShowContent")[0]);
var content=AssetDatabase.LoadAssetAtPath<HundredHour.RealityShow.ShowContent>(path);
var report=new List<string>();
void Check(bool ok,string label){if(!ok)throw new Exception(label);report.Add("PASS "+label);}
HundredHour.RealityShow.ShowGame New(int seed){var g=new HundredHour.RealityShow.ShowGame(content,null,true){TelemetryEnabled=false};g.State.communicationMechanics=true;g.Begin(seed,HundredHour.RealityShow.ShowDifficulty.Easy);g.StartLoop();g.ChooseRoute(0);return g;}
var g=New(10);var s=g.State;
Check(g.BeginTopic(0),"topic starts");int agi=s.agi;Check(!g.BeginTopic(1)&&s.agi==agi,"double request cannot double charge");
g.AcceptGeneratedTopic(s.pendingTopicSeed,g.LocalTopicLine());Check(g.SharedMemories().Count==0,"unsaid AI topic is not shared memory");
g.ResolveTalk(0,HundredHour.RealityShow.ShowDialogue.Local(g,s.actions[0]));Check(g.SharedMemories().Count==1,"spoken line and reply saved together");
Check(!string.IsNullOrEmpty(g.SharedMemories()[0].meaning)&&g.SharedMemories()[0].effect.Contains("相互理解"),"memory stores partner meaning and effect");
var first=g.SharedMemories()[0];g.SelectRecall(first.id);g.BeginTopic(0);g.AcceptGeneratedTopic(s.pendingTopicSeed,g.LocalTopicLine());
Check(g.MemoryBonus(s.actions[0])==4,"one shared memory opens new angle +4");
g.ResolveTalk(0,HundredHour.RealityShow.ShowDialogue.Local(g,s.actions[0]));Check(s.understanding==5,"new angle increases understanding");
g.SelectRecall(first.id);g.BeginTopic(0);g.AcceptGeneratedTopic(s.pendingTopicSeed,g.LocalTopicLine());Check(g.MemoryBonus(s.actions[0])==0,"repeated goal on same memory has no synergy bonus");
var second=g.SharedMemories()[1];s.recallB=second.id;var pair=new HundredHour.RealityShow.TalkAction{recallA=first.id,recallB=second.id,goal=HundredHour.RealityShow.ShowGame.TopicGoals[2]};
Check(g.MemoryBonus(pair)==7,"two memories create +7 bridge");
s.actions.Insert(0,pair);pair.line="ふたりで続きを考えよう";pair.basePower=5;pair.tag="creation";
g.ResolveTalk(0,HundredHour.RealityShow.ShowDialogue.Local(g,pair));
var reverse=new HundredHour.RealityShow.TalkAction{recallA=second.id,recallB=first.id,goal=HundredHour.RealityShow.ShowGame.TopicGoals[2]};Check(g.MemoryBonus(reverse)==0,"reversing pair cannot farm bonus");
var restored=new HundredHour.RealityShow.ShowGame(content,JsonUtility.FromJson<HundredHour.RealityShow.ShowState>(JsonUtility.ToJson(s)),true);Check(restored.SharedMemories().Count==3&&restored.State.understanding==11,"save roundtrip retains memories and understanding");
restored.StartLoop();Check(restored.SharedMemories().Count==0&&!restored.SelectRecall(first.id),"partner cannot recall erased timeline");
for(int mode=0;mode<3;mode++)
{
 int wins=0,chapters=0,understanding=0;
 for(int seed=1;seed<=30;seed++)
 {
  var run=New(seed);
  for(int step=0;step<30;step++)
  {
   var r=run.State;
   if(r.phase==HundredHour.RealityShow.ShowPhase.Ceremony){run.Advance();continue;}
   if(r.phase==HundredHour.RealityShow.ShowPhase.Route){run.ChooseRoute(seed%2);continue;}
   if(r.phase!=HundredHour.RealityShow.ShowPhase.Conversation)break;
   if(mode>0&&run.SharedMemories().Count>0)
   {
    var memories=run.SharedMemories();run.SelectRecall(memories.Last().id);
    if(mode==2&&memories.Count>1)run.SelectRecall(memories[memories.Count-2].id,true);
    if(mode==2&&run.BeginTopic(step%3))run.AcceptGeneratedTopic(r.pendingTopicSeed,run.LocalTopicLine());
    else run.ComposeRecall();
   }
   int choice=Enumerable.Range(0,r.actions.Count).OrderByDescending(i=>run.Power(r.actions[i])).First();
   if(mode==1&&r.actions.Any(a=>a.id=="recall_memory"))choice=r.actions.FindIndex(a=>a.id=="recall_memory");
   run.ResolveTalk(choice,HundredHour.RealityShow.ShowDialogue.Local(run,r.actions[choice]));
  }
  if(run.State.phase==HundredHour.RealityShow.ShowPhase.Victory)wins++;
  chapters+=run.State.chapter;understanding+=run.State.understanding;
 }
 report.Add($"30 seeds mode={mode} wins={wins} meanChapter={chapters/30f:F1} meanUnderstanding={understanding/30f:F1}");
}
System.IO.File.WriteAllLines("Docs/ConversationMemory/rules-verification.txt",report);
return string.Join("\n",report);