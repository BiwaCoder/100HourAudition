var report=new System.Collections.Generic.List<string>();
void Check(bool ok,string text){if(!ok)throw new Exception(text);report.Add("PASS "+text);}
foreach(bool hikari in new[]{true,false}){
 var c=ScriptableObject.CreateInstance<HundredHour.RealityShow.ShowContent>();HundredHour.RealityShow.ShowContent.Populate(c);HundredHour.RealityShow.RomanticLeadProfiles.Apply(c,hikari);
 var g=new HundredHour.RealityShow.ShowGame(c,null,true);g.State.communicationMechanics=true;g.Begin(72,HundredHour.RealityShow.ShowDifficulty.Easy);g.StartLoop();g.ChooseRoute(0);
 Check(g.MascotAdvice()&&g.State.learnedTechniques.Count==1&&g.State.notice.Contains("覚えました"),"first advice teaches "+hikari);
 g.MascotAdvice();Check(g.State.learnedTechniques.Count==1,"repeat advice does not unlock all");
 var a=g.State.actions.First(x=>x.id.StartsWith("tech_"));Check(g.TechniqueBonus(a)==6,"new technique bonus");
 var reply=HundredHour.RealityShow.ShowDialogue.Local(g,a);Check(reply.topic==(hikari?"髪型":"服のこだわり"),"character-specific response");
 int index=g.State.actions.IndexOf(a);g.ResolveTalk(index,reply);Check(g.TechniqueBonus(a)==0,"bonus consumed only by speaking");
 g.State.conversations.Add(new HundredHour.RealityShow.ConversationMemory{id="second",partner="yuto",loop=g.State.loop,topic="好きなこと",reply="話した"});
 g.SenseFeelings();Check(g.State.learnedTechniques.Count==2&&g.State.techniqueNotice.Contains(hikari?"ネイル":"写真とカメラ"),"sense teaches next technique");
 Check(g.State.actions.Count(x=>x.id.StartsWith("tech_"))==2,"both choices available");
 g.State.agi=12;Check(g.TimeLeap(),"time leap");g.ChooseRoute(0);
 Check(g.State.learnedTechniques.Count==2&&g.State.actions.Count(x=>x.id.StartsWith("tech_"))==2&&g.TechniqueBonus(a)==0,"leap retains knowledge without duplicate first-use reward");
 var saved=JsonUtility.FromJson<HundredHour.RealityShow.ShowState>(JsonUtility.ToJson(g.State));
 var loaded=new HundredHour.RealityShow.ShowGame(c,saved,true);Check(loaded.State.learnedTechniques.Count==2&&loaded.State.usedTechniques.Count==1,"save/load keeps techniques");
 for(int i=0;i<4;i++)g.State.conversations.Add(new HundredHour.RealityShow.ConversationMemory{id="more"+i,partner="yuto",loop=g.State.loop});
 g.MascotAdvice();g.MascotAdvice();Check(g.State.learnedTechniques.Count==4,"all four unlock with conversation progress");
 Check(g.State.actions.Any(x=>x.line.Contains(hikari?"研究":"車も使いますか")),"final character topic");
 UnityEngine.Object.DestroyImmediate(c);
}
System.IO.File.WriteAllLines("/tmp/advice-results.txt",report);return report;
