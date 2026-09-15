using System;
using System.Linq;
using System.IO;
using UnityEngine;
namespace HundredHour.RealityShow.Editor
{
    public static class ShowRulesChecks
    {
        static void Check(bool pass,string label){if(!pass)throw new Exception("Reality Show check failed: "+label);}
        static ShowGame Enter(ShowContent content,int seed=1,ShowDifficulty level=ShowDifficulty.Easy)
        {var g=new ShowGame(content);g.Begin(seed,level);for(int i=0;i<3;i++)g.Advance();g.FirstChoice(0);g.Advance();g.Advance();g.Advance();g.Advance();g.ChooseRoute(0);return g;}
        static void Talk(ShowGame g,int index)=>g.ResolveTalk(index,ShowDialogue.Local(g,g.State.actions[index]));
        public static string Run()
        {
            var content=UnityEditor.AssetDatabase.LoadAssetAtPath<ShowContent>("Assets/RealityShow/Data/ShowContent.asset");
            var negative=Enter(content,-123);Talk(negative,0);
            var g=Enter(content);Check(g.State.loop==1&&g.State.attune==1,"opening one-time loss and awakening");
            Check(g.State.actions.Count==2&&!g.State.crafted,"opening actions gated");
            Check(!g.Compose(g.State.memories[0].id,"ask"),"craft locked before talking");
            Talk(g,1);Check(g.State.introduced&&g.State.actions.Any(a=>a.id=="purpose")&&g.State.actions.Any(a=>a.id=="pickup"),"introduction opens purpose and follow-up");
            var m=g.AvailableMemories()[0];Check(g.Compose(m.id,"ask"),"memory verb synthesis");Check(!g.Compose(m.id,"ask"),"once per turn crafting");Talk(g,g.State.actions.FindIndex(a=>a.id=="crafted"));
            Check(m.used==1&&g.State.memoryCards.Count>0,"first recall tracked and memory card discovered");var another=g.AvailableMemories().First(x=>x.id!=m.id);Check(g.Compose(m.id,"connect",another.id),"two memories combine after craft");
            Talk(g,g.State.actions.FindIndex(a=>a.id=="crafted"));Check(g.State.combined&&m.used==2&&another.used==1,"combined memory resolves");
            var before=g.State.memories.Count;g.Remember(m.owner,m.category,m.detail,"test");Check(before==g.State.memories.Count,"memory deduplication");
            var invalid=new HundredHour.AIChat.ChatResponsePayload{success=true,response="{\"reply\":\"hello\",\"topic\":\"not in reply\"}"};Check(!ShowDialogue.TryParse(invalid,out _,out _),"reject ungrounded topic");
            var risk=Enter(content);risk.State.hand.Clear();risk.State.hand.Add("blackout");risk.PlayCard(0);Talk(risk,0);Check(risk.State.phase==ShowPhase.Eliminated&&risk.State.CanLoop,"failed risk eliminates but allows loop");
            var kept=risk.State.memories[0];int bank=risk.State.bank;Check(risk.Loop()&&risk.State.loop==2&&risk.State.memories.Contains(kept)&&kept.loop==1&&risk.State.bank<bank,"loop retains memory and spends bank once");
            var hard=Enter(content,1,ShowDifficulty.Hard);hard.State.loop=3;hard.State.phase=ShowPhase.Eliminated;Check(!hard.Loop(),"hard loop limit");
            var storm=Enter(content);storm.State.hand.Clear();storm.State.hand.Add("blackout");storm.PlayCard(0);Check(storm.SetWeather("blackout")&&!storm.RiskFails(),"weather condition fulfils risk");
            int energy=storm.State.agi;Check(storm.Forecast()&&storm.State.agi==energy-2,"forecast charges");Check(!storm.Forecast(),"no negative AGI");
            uint randomBefore=storm.State.randomState;var pow=storm.Power(storm.State.actions[0]);Check(randomBefore==storm.State.randomState&&pow==storm.Power(storm.State.actions[0]),"preview does not consume RNG");
            string path=System.IO.Path.GetFullPath("Temp/RealityShowTestSave.json");ShowSaveStore.Save(g.State,path);Check(ShowSaveStore.TryLoad(content,out var restored,out _,path),"save/load");
            Check(restored.randomState==g.State.randomState&&restored.memories.Count==g.State.memories.Count&&restored.hand.SequenceEqual(g.State.hand),"save preserves RNG deck memory");
            var twin=new ShowGame(content,restored);int action=g.State.actions.FindIndex(x=>x.id=="pickup");if(action<0)action=0;Talk(g,action);Talk(twin,action);Check(g.State.Player.Merit==twin.State.Player.Merit&&g.State.phase==twin.State.phase,"save resumes same outcome");
            int wins=0,losses=0;var outcomes=new System.Collections.Generic.HashSet<string>();
            for(int seed=1;seed<=30;seed++)
            {
                var run=Enter(content,seed);int steps=0;
                while(run.State.phase!=ShowPhase.Victory&&run.State.phase!=ShowPhase.Eliminated&&steps++<80)
                {
                    var s=run.State;
                    switch(s.phase)
                    {
                        case ShowPhase.Conversation:
                            for(int attempts=0;attempts<7;attempts++)
                            {int c=s.hand.FindIndex(id=>run.Card(id).cost<=s.focus&&id!="confess"&&id!="blackout");if(c<0)break;run.PlayCard(c);}
                            int best=Enumerable.Range(0,s.actions.Count).OrderByDescending(i=>run.Power(s.actions[i])).First();Talk(run,best);break;
                        case ShowPhase.Ceremony:run.Advance();break;
                        case ShowPhase.Reward:run.TakeReward(0);break;
                        case ShowPhase.Route:run.ChooseRoute(seed%run.Routes().Length);break;
                        default:throw new Exception("Unexpected phase "+s.phase);
                    }
                    Check(s.hand.Count<=7&&s.focus>=0&&s.agi>=0,"resource bounds");
                }
                Check(steps<80,"full run terminates");if(run.State.phase==ShowPhase.Victory)wins++;else losses++;
                outcomes.Add(run.State.Player.Merit+":"+run.State.lastEliminated+":"+run.State.topic);
            }
            Check(wins>0,"victory reachable");Check(outcomes.Count>5,"seed variation");
            string result=$"PASS: memory flow, synthesis, duplicate suppression, risky cards, weather, resource limits, loop caps, save/RNG continuity. 30 runs: {wins} wins, {losses} losses, {outcomes.Count} distinct outcomes.";
            File.WriteAllText("Temp/RealityShowRulesResults.txt",result);return result;
        }
    }
}
