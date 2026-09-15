using System;
using System.Linq;
using System.IO;
using UnityEngine;

namespace HundredHour.RealityShow
{
    [Serializable] public sealed class ConversationBalance
    {
        public int version,preferencePercent,maxWarmth,maxWisdom,maxInitiative,maxPower;
        public int forecastCost,perspectiveCost,weatherCost,acquireCost,leapCost,interruptPenalty,rivalBase,rivalVariance,jealousyThreshold;
        public string preferredArchetype;
        public ShowCard[] cards;
        public static ConversationBalance Load()
        {
            var data=Resources.Load<TextAsset>("ConversationBalance");
            if(!data)throw new InvalidOperationException("ConversationBalance.json is missing");
            return JsonUtility.FromJson<ConversationBalance>(data.text);
        }
    }
    public sealed partial class ShowGame
    {
        ConversationBalance balance;
        public bool TelemetryEnabled {get;set;}=true;
        public void RecordDeckEvent(string id){if(State.deckMechanics)WriteDeckTelemetry(new TalkAction{id=id},0);}
        public ConversationBalance Balance=>balance??(balance=ConversationBalance.Load());
        public static readonly string[] Archetypes={"positive","strategic","artistic"};
        public static string ArchetypeName(string id)=>English
            ?(id=="positive"?"Positive Girl":id=="strategic"?"Strategic Girl":id=="artistic"?"Artist Girl":"Unselected")
            :(id=="positive"?"ポジティブガール":id=="strategic"?"戦略的ガール":id=="artistic"?"芸術家ガール":"未選択");
        public int SkillCost(string id)=>id=="forecast"?(State.deckMechanics?Balance.forecastCost:2):id=="perspective"?(State.deckMechanics?Balance.perspectiveCost:1):id=="weather"?(State.deckMechanics?Balance.weatherCost:2):id=="acquire"?(State.deckMechanics?Balance.acquireCost:1):(State.deckMechanics?Balance.leapCost:3);
        public string ChapterTitle=>English?(State.chapter==0?"Self-Introduction  1 vs 1":State.chapter==1?"Group Talk  1 vs 3":"One-on-One  1 vs 1"):(State.chapter==0?"自己紹介  1 vs 1":State.chapter==1?"グループ会話  1 vs 3":"ツーショット  1 vs 1");
        public string ChapterHint=>English?(State.chapter==0?"First impressions: close the distance with a self-introduction and honest reactions":State.chapter==1?(GroupTalk?$"Theme \"{GroupTheme(GroupThemeIndex)}\" / the room feels {GroupMoodLabel}":"Listen and share: carry the earlier conversation into the next topic"):(FinalTalk?$"Theme \"{FinalTheme(FinalThemeIndex)}\": say it in words, with \"like\"":"Affection +1 per 12 Understanding: talk about your future together")):(State.chapter==0?"第一印象：自己紹介と素直な反応で距離を縮める":State.chapter==1?(GroupTalk?$"お題「{GroupTheme(GroupThemeIndex)}」／場の空気：{GroupMoodLabel}":"聞く・伝える：以前の会話を次の話題へつなぐ"):(FinalTalk?$"お題「{FinalTheme(FinalThemeIndex)}」：『好き』を言葉にして、未来を描く":"相互理解12ごとに好感度+1：ふたりの未来を話す"));
        public string[] DeckRoutes()=>English?(State.chapter==0?new[]{"By the window / a creative story","In the garden / a story about nature"}:State.chapter==1?new[]{"Group talk by the window / creative","Group talk in the garden / nature","On the sofa by the entrance / easygoing","Down the right-hand gallery / quiet talk"}:new[]{"One-on-one under the stars / future","One-on-one in the rain / memory","On the sofa by the entrance / honest words","One-on-one in the gallery / a promise"}):(State.chapter==0?new[]{"窓辺で自己紹介 / 創作","庭で自己紹介 / 自然"}:State.chapter==1?new[]{"窓辺のグループ会話 / 創作","庭のグループ会話 / 自然","入口横のソファで / 気楽な話","入口から右の回廊で / 静かな話"}:new[]{"星空のツーショット / 未来","雨音のツーショット / 記憶","入口横のソファで / 本音","回廊のツーショット / 約束"});
        public bool ChooseArchetype(int index)
        {
            var s=State;if(!s.deckMechanics||s.phase!=ShowPhase.Archetype||index<0||index>=Archetypes.Length)return false;
            s.archetype=Archetypes[index];s.deck=Balance.cards.Where(c=>c.archetype==s.archetype&&c.rarity==0).Select(c=>c.id).ToList();
            s.deck.AddRange(s.ownedRare.Distinct());s.draw=s.deck.ToList();Shuffle(s.draw);s.discard.Clear();s.hand.Clear();s.played.Clear();ResetDeckChapter();
            s.phase=ShowPhase.Route;s.notice=English?$"Starting with {ArchetypeName(s.archetype)}'s 8 cards plus any rares you've earned.":ArchetypeName(s.archetype)+"の8枚＋獲得済みレアで開始。";
            Log(Content.Person("himari").displayName,s.notice);return true;
        }
        void ResetDeckChapter()
        {
            var s=State;s.warmth=s.wisdom=s.initiative=s.shield=s.sabotage=s.vulnerability=s.lingering=s.lingeringTurns=0;
            s.traps.Clear();s.lastCard=s.lastCardArchetype=s.pendingTopic=s.pendingTopicSeed=s.topicTag="";s.interrupted=false;
            if(s.chapter==0){s.intimacy=s.jealousy=0;s.npcScene="";}
        }
        void BeginDeckTurn()
        {
            var s=State;s.lastCard=s.lastCardArchetype=s.topicTag="";s.sabotage=0;
            bool scheduledInterrupt=!s.communicationMechanics&&s.chapter==1&&s.turn%2==1;
            // ダーティーな割り込みを使うほど(rivalResentment)、後で仕返しされる確率が上乗せされる。100%にはしない。
            bool retaliation=s.chapter==1&&Next(100)<Math.Min(70,s.rivalResentment*12);
            s.interrupted=scheduledInterrupt||retaliation;
            // One-time opener, not a standing filler: once shared it's consumed, instead of sitting
            // in the choice list turn after turn with nothing new to say.
            if(s.chapter==0&&s.introduced&&!s.impressionShared)s.actions.Add(new TalkAction{id="impression",line=English?"I'm a little nervous, but I want to get to know you too.":"少し緊張するけど、あなたのことも知りたい。",tag="empathy",basePower=6});
            if(s.chapter==1&&s.communicationMechanics)
            {
                // Group talk: three lines written for this turn's theme; techniques/memories are appended after.
                s.actions.Clear();s.actions.AddRange(GroupChoices(GroupThemeIndex));
            }
            else if(s.chapter==1)
            {
                s.actions.Clear();
                s.actions.Add(new TalkAction{id="lead",line=English?"I want everyone to hear that too.":"その話、みんなにも聞いてみたい。",tag="creation",basePower=6});
                s.actions.Add(new TalkAction{id="yield",line=English?"Go ahead first. I'll talk after.":"先に聞かせて。そのあと私も話すね。",tag="empathy",basePower=5});
                s.actions.Add(new TalkAction{id="challenge",line=English?"Wait a second, do you still remember our promise?":"ちょっと待って、私との約束も覚えてる？",tag="creation",basePower=8});
                s.actions.Add(new TalkAction{id="curious",line=English?"Hey, be honest -- who here are you curious about?":"ねえ、正直なところ、この中で誰が気になってる？",tag="empathy",basePower=7});
                s.actions.Add(new TalkAction{id="reminisce",line=English?"Come to think of it, want to swap old stories with everyone?":"そういえば、みんなで昔の話でもしない？",tag="creation",basePower=6});
            }
            if(s.chapter==2&&s.communicationMechanics)
            {
                // Final: themed lines built from the earlier rounds (see ShowFinalTalk).
                s.actions.Clear();s.actions.AddRange(FinalChoices(FinalThemeIndex));
            }
            else if(s.chapter==2)
            {
                s.actions.Clear();var m=AvailableMemories().LastOrDefault();
                s.actions.Add(new TalkAction{id="recall",line=m==null?(English?"Being with you, I feel like I can be honest.":"ふたりでいると、素直になれる気がする。"):(English?$"I still remember what you said about {Clip(m.detail,18)}.":Clip(m.detail,18)+"の話、覚えているよ。"),tag="empathy",basePower=6,memoryA=m?.id});
                s.actions.Add(new TalkAction{id="promise",line=English?"Even after the show ends, I want us to keep writing our story together.":"番組のあとも、ふたりで続きを作りたい。",tag="creation",basePower=s.intimacy>=20?10:4});
                s.actions.Add(new TalkAction{id="honest",line=English?"Seeing you talk with someone else made me a little jealous.":"他の子と話すあなたを見て、少し嫉妬した。",tag="empathy",basePower=6});
                s.actions.Add(new TalkAction{id="type",line=English?"Hey, I want to know your type, or little things about people that catch your eye.":"ねえ、好きなタイプとか、気になる人のクセとか聞いてみたいな。",tag="empathy",basePower=6});
            }
            if(s.interrupted)s.notice=English
                ?(retaliation&&!scheduledInterrupt?"Payback for an interruption, or a rival forcing their way into the conversation. Prepare with initiative, defense, or a counter.":"A rival looks ready to interrupt. Prepare with initiative, defense, or a counter.")
                :(retaliation&&!scheduledInterrupt?"割り込みの仕返しか、ライバルが強引に会話を奪おうとしている。主導権・防御・カウンターで備えよう。":"ライバルが割り込みそう。主導権・防御・カウンターを準備しよう。");
        }
        /// <summary>割り込みアピール: 会話の主導権を強引に奪う。効果は高いが、同性のライバルの心証を損ね、
        /// 以後BeginDeckTurnの retaliation 確率が上がる(rivalResentmentに比例、100%にはならない)。</summary>
        public bool InterruptAppeal()
        {
            var s=State;if(s.phase!=ShowPhase.Conversation||s.chapter!=1||s.agi<2)return false;
            s.agi-=2;s.rivalResentment=Math.Min(10,s.rivalResentment+1);s.initiative=Math.Min(Balance.maxInitiative,s.initiative+4);
            s.actions.RemoveAll(a=>a.id=="interrupt_appeal");
            s.actions.Insert(0,new TalkAction{id="interrupt_appeal",line=English?"Sorry, but listen to me for a second!":"ごめん、今それより私の話を聞いて！",tag="public",basePower=10});
            DescribeSupport(English?"Saori: I've prepared a forceful line to cut in with. It's effective, but it may hurt how the other participants see you. (AGI -2)":"沙織：強引に割り込む一言をご用意しました。効果は高いですが、他の参加者の心証を損ねるかもしれません。（AGI −2）");
            Log(ShowSupportVoice.Name,English?"I've prepared a line to interrupt with. It's strong, but it comes with risk.":"割り込みの一言をご用意しました。強い分、リスクも伴います。");
            RecordDeckEvent("skill:interrupt");return true;
        }
        public bool CanPlayDeckCard(ShowCard c)=>c!=null&&State.focus>=c.cost&&State.agi>=c.agiCost&&State.wisdom>=c.wisdomCost&&string.IsNullOrEmpty(State.pendingTopic);
        void ApplyDeckCard(ShowCard c)
        {
            var s=State;s.agi-=c.agiCost;s.wisdom=Math.Clamp(s.wisdom-c.wisdomCost+c.wisdom,0,Balance.maxWisdom);
            s.flatBonus+=c.power;
            if(!string.IsNullOrEmpty(c.combo)&&(s.lastCard==c.combo||s.lastCardArchetype==c.combo))s.flatBonus+=c.comboPower;
            if(c.trigger=="close"&&s.intimacy>=20||c.trigger=="open"&&s.vulnerability>0)s.flatBonus+=c.triggerPower;
            s.warmth=Math.Clamp(s.warmth+c.warmth,0,Balance.maxWarmth);s.vulnerability+=c.vulnerability;
            s.initiative=Math.Clamp(s.initiative+c.initiative,0,Balance.maxInitiative);s.shield+=c.shield;s.sabotage+=c.sabotage;
            if(c.lingering>0){s.lingering=Math.Max(s.lingering,c.lingering);s.lingeringTurns=Math.Max(s.lingeringTurns,c.duration);}
            if(!string.IsNullOrEmpty(c.trap)){if(s.traps.Count>=3)s.traps.RemoveAt(0);s.traps.Add(new ShowTrap{trigger=c.trap,power=c.trapPower});}
            if(!string.IsNullOrEmpty(c.weather))s.weather=c.weather;
            if(!string.IsNullOrEmpty(c.topicTag))s.topicTag=c.topicTag;
            s.lastCard=c.id;s.lastCardArchetype=c.archetype;
            if(c.generate){s.pendingTopic=c.id;s.pendingTopicSeed=c.topicSeed;}
            Log("カード",c.title+"："+c.description);
        }
        bool TrapReady(ShowTrap t)=>t.trigger=="rain"&&State.weather=="rain"||t.trigger=="plant"&&(State.topicTag=="plant"||State.topic.Contains("花")||State.topic.Contains("植物"))||t.trigger=="place"&&(State.route.Contains("庭")||State.route.Contains("窓辺")||State.route.Contains("星空"))||t.trigger=="interrupt"&&State.interrupted;
        public int DeckPower(int basePower,TalkAction action)
        {
            var s=State;int p=basePower+s.warmth+(s.lingeringTurns>0?s.lingering:0)+s.traps.Where(TrapReady).Sum(t=>t.power);
            if(s.chapter==0&&action.id=="introduce")p+=4;
            if(s.chapter==1)
            {
                p+=s.initiative/2;
                bool counter=s.traps.Any(t=>t.trigger=="interrupt"&&TrapReady(t));
                if(s.interrupted&&s.shield==0&&s.initiative<5&&!counter&&action.id!="yield"&&(!s.communicationMechanics||MemoryBonus(action)==0))p-=Balance.interruptPenalty;
                if(action.id=="challenge"&&s.initiative<3)p-=4;
            }
            if(s.communicationMechanics&&s.chapter==2)p+=s.understanding/12;
            if(s.chapter==2)p+=s.intimacy/10+(!string.IsNullOrEmpty(action.memoryA)?3:0);
            if(s.archetype==Balance.preferredArchetype)p=p*Balance.preferencePercent/100;
            return Math.Clamp(p,0,Balance.maxPower);
        }
        void ResolveDeckTurn(TalkAction action,int power)
        {
            var s=State;s.lastPower=power;s.intimacy=Math.Min(100,s.intimacy+power/3+(action.id=="honest"?3:0));
            if(s.rivalResentment>0&&Next(100)<25)s.rivalResentment--;
            s.traps.RemoveAll(TrapReady);if(s.lingeringTurns>0)s.lingeringTurns--;
            if(s.interrupted&&s.shield>0)s.shield--;s.initiative=Math.Max(0,s.initiative-1+(action.id=="lead"?2:0));
            var scenes=new System.Collections.Generic.List<string>();
            foreach(var rival in s.contestants.Where(x=>x.id!="himari"&&!x.eliminated))
            {
                if(s.communicationMechanics&&s.chapter==1)continue; // credited by the group round (ShowGroupTalk)
                if(s.communicationMechanics)
                {
                    // Rivals earn exactly what the player would for the same technique (6 basic / 8 advanced,
                    // +1 understanding): no rival-only flat bonus, no hidden random.
                    scenes.Add(RecordRivalConversation(rival,out int conversationGain));
                    rival.trust+=conversationGain;rival.stars+=2000+Next(4000);
                    continue;
                }
                string arch=rival.id=="konoa"?"positive":rival.id=="hikari"?"strategic":"artistic";
                bool jealous=s.Player.trust-rival.trust>=Balance.jealousyThreshold;
                var npc=rival.conversation??new ShowState{deckMechanics=true,archetype=arch,agi=8};
                rival.conversation=npc;
                if(npc.chapter!=s.chapter){npc.warmth=npc.wisdom=npc.vulnerability=npc.lingeringTurns=0;npc.traps.Clear();}
                npc.chapter=s.chapter;npc.turn=s.turn;npc.focus=3;npc.flatBonus=0;npc.sabotage=0;npc.topic=s.topic;npc.topicTag="";npc.route=s.route;npc.weather=s.weather;npc.interrupted=s.chapter==1&&jealous;npc.pendingTopic="";npc.played.Clear();
                var actorGame=new ShowGame(Content,npc,true);actorGame.balance=Balance;actorGame.TelemetryEnabled=false;
                var pool=Balance.cards.Where(c=>c.archetype==arch&&c.rarity==0&&actorGame.CanPlayDeckCard(c)).ToArray();var card=pool[Next(pool.Length)];
                npc.hand.Clear();npc.hand.Add(card.id);actorGame.PlayCard(0);
                if(card.generate)actorGame.AcceptGeneratedTopic(card.topicSeed,card.topicSeed+"を、一緒に考えたい。");
                string actionId=jealous?"challenge":arch=="strategic"?"honest":"lead";
                var npcAction=new TalkAction{id=actionId,basePower=Balance.rivalBase+Next(Balance.rivalVariance)};
                int gain=Math.Max(0,actorGame.DeckPower(npcAction.basePower+npc.flatBonus,npcAction)-s.sabotage);
                npc.intimacy=Math.Min(100,npc.intimacy+gain/3);npc.traps.RemoveAll(actorGame.TrapReady);if(npc.lingeringTurns>0)npc.lingeringTurns--;
                if(jealous){s.jealousy=Math.Min(100,s.jealousy+2);if(npc.sabotage>0){s.Player.trust=Math.Max(0,s.Player.trust-2);s.stress=Math.Min(100,s.stress+2);}}
                rival.trust+=gain;rival.stars+=2000+Next(4000);
                string line=jealous?"さっきから、あの子ばかり見てない？ 私の話も聞いて。":arch=="positive"?"わあ、それ素敵！ 私にも教えて。":arch=="strategic"?"私も、うまくいかなくて悩んだことがあるの。":"庭の花を見るたび、今日を思い出しそう。";
                string scene=Content.Person(rival.id).displayName+"［"+card.title+"］ → "+(jealous?"割り込む":arch=="strategic"?"自己開示する":"共感を伝える")+"\n「"+line+"」\n"+Content.Person("yuto").displayName+"「"+(jealous?"君の気持ちも、ちゃんと聞きたい。":"その一面を知れてうれしい。")+"」 好感度 +"+gain+"（現在 "+rival.trust+"）";
                scenes.Add(scene);Log(Content.Person(rival.id).displayName,scene);
            }
            s.npcScene=string.Join("\n\n",scenes);
            WriteDeckTelemetry(action,power);
        }
        public string PerspectiveScene()=>Content.AdaptText("他の場所で起きた会話の観測記録\n"+(string.IsNullOrEmpty(State.npcScene)?Content.Person("konoa").displayName+"［スマイル］「会えてうれしい！」\n"+Content.Person("yuto").displayName+"「君と話すと元気が出るね。」\nまだ初回発言前。これは開始時の様子です。":State.npcScene)+"\n\n沙織："+Content.Person("yuto").displayName+"は「"+ArchetypeName(Balance.preferredArchetype)+"」に惹かれやすいようです（好感度×1.2）。");
        public string RelationshipFlavor
        {
            get
            {
                string name=Content.Person("himari").displayName;
                int st=State.relationshipStage;
                string thought=English
                    ?(State.chapter==1&&State.jealousy>0?"Someone may cut in. Listen, and look for the right moment to speak.":
                      st>=3?"We've gotten close. From here, I want to be honest about how I feel too.":
                      st==2?"I got a smile out of them. Next, I'll mix in a little about myself.":
                      st==1&&!string.IsNullOrEmpty(State.topic)?$"\"{State.topic}\" connected us. Let's dig into it a little more.":
                      "Still some distance. First, give my name and ask what they like.")
                    :(State.chapter==1&&State.jealousy>0?"話を遮られそう。相手の言葉を聞きながら、話すタイミングを探そう。":
                      st>=3?"距離はかなり縮まった。ここからは、自分の気持ちも素直に伝えたい。":
                      st==2?"笑顔を引き出せた。次は、こちらの話も少し混ぜてみよう。":
                      st==1&&!string.IsNullOrEmpty(State.topic)?$"「{State.topic}」で話がつながった。もう少し掘り下げてみよう。":
                      "まだ距離がある。まずは名前を伝えて、相手の好きなことを聞こう。");
                return name+"（心の声）："+thought;
            }
        }
        // 最初の自己紹介(FirstChoice)で選んだアプローチが、最終日の告白の書き出しにまで残る。
        public string DeckEpilogue()
        {
            var s=State;
            if(HundredHour.Localization.GameLanguage.Current==HundredHour.Localization.GameLocale.English)
            {
                string first=s.firstApproach=="intro"?"I remember how you introduced yourself so honestly that day. ":
                    s.firstApproach=="greet"?"Your cheerful greeting that day made me happier than you knew. ":
                    s.firstApproach=="compliment"?"I still remember how my heart skipped when you looked at me that day. ":
                    "I sometimes think back to the day we first met. ";
                string last=s.relationshipStage>=3?"I can still feel the warmth of your hand in mine.":
                    s.relationshipStage==2?"Your smile still comes back to me in quiet moments.":
                    s.Player.trust>=40?"I cherish these hundred hours that helped us open up to each other.":
                    "We were still a little awkward, but I'm glad we got to talk.";
                return "“"+first+last+"”\nA hundred hours of conversation, woven together. Next time, beyond the show.";
            }
            string opening=s.firstApproach=="intro"?"あの日、まっすぐ名乗ってくれたことを覚えてる。":
                s.firstApproach=="greet"?"あの日の元気な挨拶、実はすごく嬉しかったんだ。":
                s.firstApproach=="compliment"?"あの日、見つめられてドキッとしたの、今でも覚えてる。":
                "はじめて会った日のことを、時々思い出す。";
            string closing=s.relationshipStage>=3?"隣で話すのが当たり前になった、この百時間が愛おしい。":
                s.relationshipStage==2?"あの時の笑顔が、今もふとした瞬間に浮かぶ。":
                s.Player.trust>=40?"少しずつ本音で話せるようになった、この百時間が愛おしい。":
                "まだぎこちなかったけれど、君と話せてよかった。";
            return "「"+opening+closing+"」\n百時間で紡いだ、ふたりの会話。次は、番組の外で。";
        }
        int WeightedCardIndex(System.Collections.Generic.List<string> pool)
        {
            int n=Next(pool.Sum(id=>Math.Max(1,Card(id).weight)));
            for(int i=0;i<pool.Count;i++){n-=Math.Max(1,Card(pool[i]).weight);if(n<0)return i;}return pool.Count-1;
        }
        public void AcceptGeneratedTopic(string topic,string line)
        {
            var s=State;if(string.IsNullOrEmpty(s.pendingTopic))return;
            s.topic=Clip(topic,30);s.actions.RemoveAll(a=>a.id=="ai_topic");
            s.actions.Insert(0,new TalkAction{id="ai_topic",line=WithName(Clip(line,160).Replace("<","〈").Replace(">","〉")),tag=s.topicGoal==TopicGoals[0]?"empathy":"creation",basePower=6,goal=s.topicGoal,recallA=SharedMemory(s.recallA)?.id,recallB=SharedMemory(s.recallB)?.id});
            s.pendingTopic=s.pendingTopicSeed="";s.forecast=-1;
        }
        [Serializable] sealed class DeckEvent {public int version,seed,loop,chapter,turn,power,trust,agi,focus,wisdom,initiative,intimacy;public string archetype,action,weather,topic;public string[] cards;}
        void WriteDeckTelemetry(TalkAction action,int power)
        {
            if(!TelemetryEnabled)return;
            var s=State;
            try {var e=new DeckEvent{version=Balance.version,seed=s.seed,loop=s.loop,chapter=s.chapter,turn=s.turn,power=power,trust=s.Player.trust,agi=s.agi,focus=s.focus,wisdom=s.wisdom,initiative=s.initiative,intimacy=s.intimacy,archetype=s.archetype,action=action.id,weather=s.weather,topic=s.topic,cards=s.played.ToArray()};File.AppendAllText(Path.Combine(Application.persistentDataPath,"ConversationTelemetry-v2.jsonl"),JsonUtility.ToJson(e)+"\n");}
            catch(Exception e){Debug.LogWarning("Conversation telemetry: "+e.Message);}
        }
    }
}
