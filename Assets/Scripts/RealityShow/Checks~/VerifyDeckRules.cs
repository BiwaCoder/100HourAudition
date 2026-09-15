var content=UnityEditor.AssetDatabase.LoadAssetAtPath<HundredHour.RealityShow.ShowContent>("Assets/RealityShow/Data/ShowContent.asset");
var evidence=new System.Collections.Generic.List<string>();
void Check(bool ok,string label){if(!ok)throw new Exception(label);evidence.Add("PASS "+label);}
HundredHour.RealityShow.ShowGame Fresh(int seed,int archetype){var g=new HundredHour.RealityShow.ShowGame(content,null,true);g.TelemetryEnabled=false;g.Begin(seed,HundredHour.RealityShow.ShowDifficulty.Easy);g.State.phase=HundredHour.RealityShow.ShowPhase.Awakening;g.Advance();Check(g.State.phase==HundredHour.RealityShow.ShowPhase.Archetype,"archetype gate");g.ChooseArchetype(archetype);g.ChooseRoute(0);return g;}
var fixtures=new System.Collections.Generic.List<string>();
for(int a=0;a<3;a++){
 var g=Fresh(100+a,a);Check(g.State.hand.Count==3,"three-card hand");
 foreach(var card in g.Balance.cards){
  for(int chapter=0;chapter<3;chapter++){
   var s=g.State;s.chapter=chapter;s.phase=HundredHour.RealityShow.ShowPhase.Conversation;s.archetype=HundredHour.RealityShow.ShowGame.Archetypes[a];s.focus=3;s.agi=8;s.wisdom=3;s.warmth=1;s.vulnerability=1;s.initiative=2;s.shield=0;s.sabotage=0;s.lingering=0;s.lingeringTurns=0;s.intimacy=25;s.flatBonus=0;s.interrupted=true;s.weather="clear";s.topic="庭の植物";s.topicTag="";s.route="窓辺";s.lastCard="rumor";s.lastCardArchetype="positive";s.traps.Clear();s.pendingTopic="";s.hand.Clear();s.hand.Add(card.id);s.played.Clear();
   s.chapterCheckpoint="";s.log.Clear();s.memories.Clear();s.draw.Clear();s.deck.Clear();s.discard.Clear();
   string before=JsonUtility.ToJson(s);Check(g.PlayCard(0),"card playable "+card.id);var act=new HundredHour.RealityShow.TalkAction{id=chapter==0?"introduce":chapter==1?"challenge":"recall",basePower=6,memoryA=chapter==2?"test":null};
   int p=g.DeckPower(10+s.flatBonus,act);
   fixtures.Add("{\"card\":\""+card.id+"\",\"action\":\""+act.id+"\",\"memory\":"+(chapter==2?"true":"false")+",\"power\":"+p+",\"before\":"+before+",\"after\":"+JsonUtility.ToJson(s)+"}");
  }
 }
}
System.IO.File.WriteAllText("Docs/ConversationDeck/parity-fixtures.json","["+string.Join(",",fixtures)+"]");
var token=Fresh(42,1);token.State.hand=new System.Collections.Generic.List<string>{"checkmate"};token.State.wisdom=0;int focus=token.State.focus;Check(!token.PlayCard(0)&&token.State.focus==focus,"insufficient wisdom costs nothing");
for(int a=0;a<3;a++){
 for(int seed=1;seed<=30;seed++){
  var g=Fresh(seed,a);int turns=0;while(g.State.phase!=HundredHour.RealityShow.ShowPhase.Victory&&g.State.phase!=HundredHour.RealityShow.ShowPhase.Eliminated&&turns++<80){var s=g.State;
   if(s.phase==HundredHour.RealityShow.ShowPhase.Conversation){for(int i=0;i<3;i++){int k=s.hand.FindIndex(id=>g.CanPlayDeckCard(g.Card(id)));if(k<0)break;g.PlayCard(k);if(!string.IsNullOrEmpty(s.pendingTopic))g.AcceptGeneratedTopic(s.pendingTopicSeed,s.pendingTopicSeed+"について話そう。");}int best=Enumerable.Range(0,s.actions.Count).OrderByDescending(i=>g.Power(s.actions[i])).First();g.ResolveTalk(best,HundredHour.RealityShow.ShowDialogue.Local(g,s.actions[best]));}
   else if(s.phase==HundredHour.RealityShow.ShowPhase.Ceremony)g.Advance();
   else if(s.phase==HundredHour.RealityShow.ShowPhase.Reward){Check(s.offers.Count==3&&s.offers.All(id=>g.Card(id).rarity>0),"stage rare offers");g.TakeReward(0);}
   else if(s.phase==HundredHour.RealityShow.ShowPhase.Route)g.ChooseRoute(0);
   else throw new Exception("Unhandled phase "+s.phase);
  }
  Check(turns<80,"terminates "+a+" / "+seed);
  if(g.State.phase==HundredHour.RealityShow.ShowPhase.Victory)Check(g.State.ownedRare.Count>0&&!string.IsNullOrEmpty(g.State.epilogue),"final rare and epilogue");
 }
}
System.IO.File.WriteAllLines("Docs/ConversationDeck/RulesVerification.txt",evidence);return new { assertions=evidence.Count,fixtures=fixtures.Count };
