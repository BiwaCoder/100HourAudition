var report=new System.Collections.Generic.List<string>();void Check(bool ok,string name){if(!ok)throw new Exception(name);report.Add("PASS "+name);}
foreach(bool female in new[]{true,false}){
 var c=ScriptableObject.CreateInstance<HundredHour.RealityShow.ShowContent>();HundredHour.RealityShow.ShowContent.Populate(c);HundredHour.RealityShow.RomanticLeadProfiles.Apply(c,female);
 var g=new HundredHour.RealityShow.ShowGame(c,null,true);g.State.communicationMechanics=true;g.Begin(319,HundredHour.RealityShow.ShowDifficulty.Easy);g.StartLoop();g.ChooseRoute(0);
 var starts=g.State.contestants.ToDictionary(x=>x.id,x=>x.trust);
 for(int i=0;i<4;i++){g.State.actions.Clear();g.State.actions.Add(new HundredHour.RealityShow.TalkAction{id="listen",line="もう少し聞かせてください。",tag="public",basePower=0});g.ResolveTalk(0,HundredHour.RealityShow.ShowDialogue.Local(g,g.State.actions[0]));}
 Check(g.State.phase==HundredHour.RealityShow.ShowPhase.Eliminated,"defeat reached");
 Check(g.State.rivalConversations.Count==12,"all rival exchanges captured");
 foreach(var rival in g.State.contestants.Where(x=>x.id!="himari")){
  var records=g.State.rivalConversations.Where(x=>x.rival==rival.id).ToArray();
  Check(records.Sum(x=>x.trustGain)+starts[rival.id]==rival.trust&&records.Sum(x=>x.understandingGain)==rival.understanding,"recorded deltas match "+rival.id);
  Check(records.Select(x=>x.line).Distinct().Count()==4,"varied dialogue "+rival.id);
 }
 var review=g.DefeatReview();Check(review.Contains("相互理解")&&review.Contains("同点")&&review.Contains("あと")&&review.Contains("好感度"),"score breakdown and tie explanation");
 Check(review.Contains(g.State.rivalConversations[0].reply),"review uses recorded reply");
 g.LearnComebackTechnique();Check(g.State.learnedTechniques.Count==1,"advanced reward");
 g.LearnComebackTechnique();Check(g.State.learnedTechniques.Count==1,"one reward per loss");
 Check(g.Loop(),"restart");
 g.ChooseRoute(0);var a=g.State.actions.First(x=>x.id.EndsWith("_future"));Check(g.TechniqueBonus(a)==12,"strong option available next loop");
 g.ResolveTalk(g.State.actions.IndexOf(a),HundredHour.RealityShow.ShowDialogue.Local(g,a));Check(g.TechniqueBonus(a)==0,"no repeated first use reward");
 g.State.phase=HundredHour.RealityShow.ShowPhase.Eliminated;g.LearnComebackTechnique();Check(g.State.learnedTechniques.Count==2,"second defeat unlocks scenery");
 var saved=JsonUtility.FromJson<HundredHour.RealityShow.ShowState>(JsonUtility.ToJson(g.State));var loaded=new HundredHour.RealityShow.ShowGame(c,saved,true);
 Check(loaded.State.rivalConversations.Count==15&&loaded.State.comebackReviews.Count==2,"save load review and lessons");
 loaded.State.rivalConversations.Clear();Check(loaded.DefeatReview().Contains("未記録")&&loaded.DefeatReview().Contains("会話記録はありません"),"legacy save does not fabricate conversations");
 UnityEngine.Object.DestroyImmediate(c);
}
System.IO.File.WriteAllLines("/tmp/rival-review-results.txt",report);return report;
