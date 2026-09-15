using System;
using System.Collections.Generic;
using System.Linq;
using HundredHour.Localization;
using UnityEngine;

namespace HundredHour.RealityShow
{
    /// <summary>Rules, random state and memory progression. No UI, HTTP or frame timing.</summary>
    public sealed partial class ShowGame
    {
        public ShowState State {get; private set;}
        public ShowContent Content {get;}
        static bool English=>GameLanguage.Current==GameLocale.English;
        public ShowGame(ShowContent content,ShowState state=null,bool deckMechanics=false){Content=content??throw new ArgumentNullException(nameof(content));State=state??new ShowState();if(deckMechanics)State.deckMechanics=true;State.traps=State.traps??new List<ShowTrap>();State.ownedRare=State.ownedRare??new List<string>();State.conversations??=new List<ConversationMemory>();State.memorySynergies??=new List<string>();State.presentedLog??=new List<ShowBeat>();State.learnedTechniques??=new List<string>();State.usedTechniques??=new List<string>();State.rivalConversations??=new List<RivalConversationBeat>();State.comebackReviews??=new List<string>();}
        public ShowCard Card(string id)
        {
            var card=State.deckMechanics?Balance.cards.FirstOrDefault(c=>c.id==id)??Content.Card(id):Content.Card(id);if(card!=null)return card;
            var memory=State.memoryCards.Find(m=>m.id==id);if(memory==null)return null;
            return new ShowCard{id=id,title=memory.title,cost=1,description="この記憶を含む会話に信頼+10。同じ相手なら+3。別の相手では+1。"};
        }
        int Next(int max){uint x=State.randomState;if(x==0)x=2463534242;x^=x<<13;x^=x>>17;x^=x<<5;State.randomState=x;return (int)(x%(uint)max);}
        void Shuffle(List<string> cards){for(int i=cards.Count-1;i>0;i--){int j=Next(i+1);var t=cards[i];cards[i]=cards[j];cards[j]=t;}}
        public void Begin(int seed,ShowDifficulty difficulty)
        {
            State=new ShowState{deckMechanics=State.deckMechanics,communicationMechanics=State.communicationMechanics,seed=seed,randomState=(uint)seed,difficulty=difficulty,phase=ShowPhase.Opening};
            ResetCast();Log("番組","100時間。四人の参加者と、一人の恋愛対象。ここから、あなたの物語が始まる。");
        }
        void ResetCast()
        {
            State.contestants=Content.cast.Where(c=>c.id!="yuto").Select(c=>new ShowContestant{id=c.id,trust=c.id=="himari"?0:12+Next(8),stars=20000+Next(10000)}).ToList();
        }
        public void Advance()
        {
            var s=State;
            switch(s.phase)
            {
                case ShowPhase.Opening: if(++s.openingPage>=3)s.phase=ShowPhase.FirstChoice;break;
                case ShowPhase.FirstReview:
                    if(++s.openingPage>=2){s.Player.eliminated=true;s.phase=ShowPhase.FirstLoss;Log("実況レン","最初の脱落者は……ひまりさんです。");s.notice="第一印象では届かなかった。時計の針が逆に動き始める。";}
                    else Log("解説ミサ","第一印象は優劣のすべてではありません。それでも今夜は、最初の印象だけで一人が舞台を去ります。");
                    break;
                case ShowPhase.FirstLoss:s.phase=ShowPhase.Awakening;Log(ShowSupportVoice.Name,ShowSupportVoice.Greeting+" よろしければ、もう一度言葉を届けるお手伝いをさせてください。");break;
                case ShowPhase.Awakening: s.deck=new List<string>{"listen","listen","story","purity","rain","draw","rest","memory"};s.attune=1;StartLoop();break;
                case ShowPhase.Ceremony:
                    if(s.chapter>=2){s.phase=ShowPhase.Victory;if(s.deckMechanics)s.epilogue=DeckEpilogue();Bank();Log("悠斗","君の言葉が、次の物語になった。一緒に続きを作りたい。");}
                    else if(s.deckMechanics){s.chapter++;ResetDeckChapter();s.turn=0;s.agi=Math.Min(12,s.agi+3);s.stress=Math.Max(0,s.stress-12);s.phase=ShowPhase.Route;}
                    else Draft(false);
                    break;
            }
        }
        // 3つの固定アプローチ(名乗る/元気な挨拶/容姿を褒める)。タイムリープ後もState.firstApproachとして持ち越り、
        // BuildActions()の一言・名乗り忘れの救済に影響する。
        public static readonly string[] FirstChoiceApproaches={"intro","greet","compliment"};
        public string[] FirstChoiceLines()
        {
            string name=Content.customParticipant?Content.Person("himari").displayName:"ひまり";
            var topics=Content.Person("himari").topics;
            if(GameLanguage.Current==GameLocale.English)
            {
                string introEn=Content.customParticipant&&topics!=null&&topics.Length>0?$"I'm {name}. I like {topics[0]} -- nice to meet you.":$"I'm {name}. Nice to meet you.";
                return new[]{introEn,"My voice came out so quiet, all I managed was a small hello.","Our eyes met, and my heart skipped. I thought, they're beautiful."};
            }
            string introLine=Content.customParticipant&&topics!=null&&topics.Length>0?$"{name}です。{topics[0]}が好きなんです、よろしくお願いします。":$"{name}です。よろしくお願いします。";
            return new[]{introLine,"声が小さくて、こんにちは、としか言えなかった。","目が合って、胸が鳴った。きれいだなって、思ってしまった。"};
        }
        public void FirstChoice(int choice,string[] customLines=null)
        {
            if(State.phase!=ShowPhase.FirstChoice||choice<0||choice>=FirstChoiceApproaches.Length)return;
            string[] lines=customLines!=null&&customLines.Length==FirstChoiceApproaches.Length?customLines:FirstChoiceLines();
            State.firstApproach=FirstChoiceApproaches[choice];
            if(State.firstApproach=="greet")State.greeted=true;if(State.firstApproach=="intro")State.introduced=true;
            Log("ひまり",lines[choice]);Log("実況レン","ここでスタジオへ。最初の関門は第一印象です。四人から一人が、早くも脱落します。");
            State.phase=ShowPhase.FirstReview;State.openingPage=0;State.notice="実況の天川レンと、解説の黒瀬ミサが選考を見届ける。";
        }
        public void StartLoop()
        {
            var s=State;s.understanding=0;s.recallA=s.recallB=s.memoryFeedback="";s.loop++;s.supportNarration="";s.chapter=0;s.turn=0;s.turnsSpoken=0;s.stress=0;s.purity=0;s.thorns=0;s.banked=false;
            int carry=Math.Min(s.bank,8-(s.difficulty==ShowDifficulty.Hard?3:4));s.bank-=carry;s.agi=(s.difficulty==ShowDifficulty.Hard?5:8)+carry;
            s.introduced=s.greeted=s.crafted=s.combined=s.complimented=s.deepened=s.romanceTalked=s.firstPartCleared=s.nameKnown=s.impressionShared=false;s.complimentLanded=false;s.relationshipStage=0;s.stageMoment="";s.nameTip="";s.lastNpc="";s.followup="";s.lastEliminated="";
            ResetCast();s.draw=new List<string>(s.deck);Shuffle(s.draw);s.hand.Clear();s.discard.Clear();s.played.Clear();
            if(s.deckMechanics)ResetDeckChapter();
            // カードシステム撤去済み: アーキタイプ選択は経由せず、常にRouteから始める。
            s.phase=ShowPhase.Route;
            string critique=English
                ?(s.firstApproach=="intro"?"Your first introduction was polite, but may have sounded a bit stiff. This time, try relaxing a little and watching how they react as you speak.":
                  s.firstApproach=="greet"?"The cheerful greeting made a good impression, but you never gave your name. This time, slip your name in right after the hello.":
                  s.firstApproach=="compliment"?"Complimenting their looks was a bold opener, but maybe a step too far for a first meeting. This time, introduce yourself first and close the distance little by little.":
                  "Last time the first impression didn't land. This time, try: give your name, ask what they like, then share how you feel.")
                :(s.firstApproach=="intro"?"最初の名乗りは丁寧でしたが、少し硬く聞こえたかもしれません。今度はもう少し肩の力を抜いて、相手の反応を見ながら話してみましょう。":
                  s.firstApproach=="greet"?"元気な挨拶は好印象でしたが、名前を伝えそびれていました。今度は挨拶のすぐあとに、さらっと名乗ってみましょう。":
                  s.firstApproach=="compliment"?"見た目を褒める一言は攻めた選択でした。ただ初対面ではやや踏み込みすぎたかもしれません。今度はまず名乗ってから、少しずつ距離を縮めていきましょう。":
                  "前回の第一印象は、うまく届きませんでした。今度は、名乗る→相手の好きなものを聞く→気持ちを伝える、の順で進めてみましょう。");
            s.notice=critique+(English?"\nMemories and rapport carry across loops. Their memory has been rewound.":"\n記憶と同調は周を越えて残ります。相手の記憶は巻き戻っています。");
            Log(ShowSupportVoice.Name,English?$"Loop {s.loop}. I'm here as your advisor. {critique}":$"{s.loop}回目の舞台ですね。私はご相談係です。{critique}");
            s.notice+=English?"\nTry using a memory: pick one or two from the memory list on screen to turn what you learned last loop into a strong topic. They don't remember the previous loop.":"\n記憶を使ってみよう。画面上の記憶一覧から1〜2件選ぶと、前の周で知った好みを強い話題にできます。相手は前の周を覚えていません。";
        }
        public void DescribeSupport(string text){State.supportNarration=text;State.notice=text;Log("地の文 / 沙織のサポート",text);}
        public bool TimeLeap()
        {
            var current=State;
            if(current.phase!=ShowPhase.Conversation||current.agi<SkillCost("leap")||string.IsNullOrEmpty(current.chapterCheckpoint))return false;
            var restored=JsonUtility.FromJson<ShowState>(current.chapterCheckpoint);
            restored.chapterCheckpoint=current.chapterCheckpoint;restored.memories=current.memories;restored.memoryCards=current.memoryCards;
            restored.deck=current.deck;restored.attune=current.attune;restored.agi=current.agi-SkillCost("leap");restored.randomState=current.randomState;
            restored.rivalConversations=current.rivalConversations;restored.comebackReviews=current.comebackReviews;
            restored.learnedTechniques=current.learnedTechniques;restored.usedTechniques=current.usedTechniques;restored.techniqueNotice="";
            restored.timeLeaps=current.timeLeaps+1;restored.log=current.log;restored.presentedLog=current.presentedLog;restored.conversations=current.conversations;restored.memorySynergies=current.memorySynergies;restored.draw=new List<string>(restored.deck);restored.hand.Clear();restored.discard.Clear();restored.played.Clear();
            restored.ownedRare=current.ownedRare;restored.preferenceKnown=current.preferenceKnown;restored.rivalResentment=current.rivalResentment;
            restored.phase=ShowPhase.Route;restored.turn=0;State=restored;Shuffle(restored.draw);
            Log(ShowSupportVoice.Name,ShowSupportVoice.Rewind);
            DescribeSupport("沙織が会話の記録に栞を挟み、時間を章の入口まで戻した。覚えたことと取得カードを持って、もう一度選び直せる。（AGI −3）");
            RecordDeckEvent("skill:leap");State.notice="タイムリープ。記憶一覧から、前に知った好みを選んでみましょう。1件で話を深め、2件で新しい話題にできます。相手は以前の会話を覚えていないので、初めての質問として話しかけましょう。";return true;
        }
        public bool Loop(){if(!State.CanLoop)return false;StartLoop();return true;}
        public void Leave(){if(State.phase!=ShowPhase.Eliminated)return;State.phase=ShowPhase.GameOver;Log("ひまり","ここで降りる。でも、知ったことは消えない。");}
        public string[] Routes()=>State.deckMechanics?DeckRoutes():State.chapter==0?
            (English?new[]{"By the window / a creative story","In the garden / a story about nature"}:new[]{"窓辺で自己紹介 / 創作の話","庭で自己紹介 / 自然の話"}):
            State.chapter==1?(English?new[]{"Work together with Yuto / deepen trust","Walk the garden with a contestant / form an alliance","Confession room / reach the viewers"}:new[]{"悠斗と共同制作 / 信頼を深める","参加者と庭の散歩 / 同盟を結ぶ","告白室 / 視聴者へ届ける"}):
            English?new[]{"One-on-one under the stars / talk about the future","One-on-one in the rain / revisit a memory"}:new[]{"星空の一対一 / 未来を話す","雨音の一対一 / 記憶を重ねる"};
        public void ChooseRoute(int index)
        {
            var s=State;if(s.phase!=ShowPhase.Route||index<0||index>=Routes().Length)return;
            s.chapterCheckpoint="";s.chapterCheckpoint=JsonUtility.ToJson(s);
            s.route=Routes()[index];s.target="yuto";
            if(!s.deckMechanics&&s.chapter==1&&index==1){var alive=s.contestants.Where(x=>x.id!="himari"&&!x.eliminated).ToList();s.target=alive[Next(alive.Count)].id;}
            s.preferredTag=index==0?"creation":index==1?"empathy":"public";
            s.topicIndex=Next(Content.Person(s.target).topics.Length);s.topic=Content.Person(s.target).topics[s.topicIndex];
            s.followup="";s.turn=0;s.phase=ShowPhase.Conversation;s.lastNpc=OpeningLine();
            s.balanceCompliments=s.balanceDisclosures=0;s.lastBalanceKind="";s.lastActionUnderstood=false;
            // The seed topic is only a hint for the opening line; it becomes a memory once it is actually talked about.
            Log(Content.Person(s.target).displayName,s.lastNpc);
            s.lastGroupWinner="";s.groupOpener="";s.lastGroupReaction="";s.sweetStreak=0;s.groupMood=0;s.groupPlayerDelta=0;
            if(s.chapter==2&&s.communicationMechanics)EnsureFinalMemories();
            BeginTurn();
        }
        string OpeningLine()
        {
            if(RomanticLeadProfiles.IsLead(Content.Person(State.target)))return RomanticLeadProfiles.Opening(this);
            var s=State;string topic=s.topic;int st=RelationshipStage(s.Player.trust);
            if(English)
            {
                if(st<=0)return "...Hello. I'm still not sure what to call you.";
                if(st==1)return topic+" has been on my mind since earlier. Do you like it too?";
                if(st==2)return "I just laughed a little. Was that weird? Can we keep talking about "+topic+"?";
                return "Want to walk a bit? We can keep talking about "+topic+" while we do.";
            }
            if(st<=0)return "……こんにちは。まだ、なんて呼べばいいかも分からなくて。";
            if(st==1)return topic+"の話、さっきから頭を離れなくて。あなたも、好き？";
            if(st==2)return "今、ちょっと笑ってしまった。変、かな。"+topic+"の続き、してもいい？";
            return "少し、歩かない？　"+topic+"の話、しながらでいいから。";
        }
        void BeginTurn()
        {
            var s=State;s.techniqueNotice="";s.supportNarration="";s.focus=3;s.guard=s.flatBonus=s.starsBonus=s.stressBonus=0;s.perspective=s.acquired=s.composedThisTurn=false;s.forecast=-1;s.played.Clear();s.weather="clear";
            if(s.turnsSpoken>0)s.agi=Math.Min(12,s.agi+1);
            s.intentPressure=5+Next(5)+(s.difficulty==ShowDifficulty.Hard?2:0);if(!s.deckMechanics)Draw(4);BuildActions();if(s.deckMechanics)BeginDeckTurn();DropUsedActions();
            s.notice=GroupTalk?(English?$"Theme: \"{GroupTheme(GroupThemeIndex)}\". Answer in your own words.":$"お題「{GroupTheme(GroupThemeIndex)}」に、自分の言葉で答えよう。"):FinalTalk?(English?$"Theme: \"{FinalTheme(FinalThemeIndex)}\". Say it in words.":$"お題「{FinalTheme(FinalThemeIndex)}」。気持ちを、言葉にしよう。"):s.deckMechanics&&s.interrupted?(English?"They're distracted by another participant's conversation. Try listening for now, or bring up something you remember.":"相手が他の参加者の話に気を取られている。いったん聞き役になるか、覚えている話でつなごう。"):(English?$"\"{s.topic}\" from their words is on your mind.":$"相手の言葉から「{s.topic}」が気になっています。");
        }
        void Draw(int count)
        {
            for(int i=0;i<count&&State.hand.Count<(State.deckMechanics?3:7);i++)
            {
                if(State.draw.Count==0){State.draw.AddRange(State.discard);State.discard.Clear();Shuffle(State.draw);}
                if(State.draw.Count==0)break;
                State.hand.Add(State.draw[0]);State.draw.RemoveAt(0);
            }
        }
        static string UsedKey(TalkAction a)=>a.id+"|"+a.line;
        // Dynamic lines (pickups, answers, crafted topics) are rebuilt each turn; everything else is consumed once spoken.
        public void DropUsedActions()
        {
            var s=State;s.usedActions??=new List<string>();
            s.actions.RemoveAll(a=>a.id!="pickup"&&a.id!="answer"&&a.id!="crafted"&&s.usedActions.Contains(UsedKey(a)));
        }
        void BuildActions()
        {
            var s=State;s.actions.Clear();
            void Add(string id,string line,string tag,int power,string memory=null)=>s.actions.Add(new TalkAction{id=id,line=line,tag=tag,basePower=power,memoryA=memory});
            string L(string ja,string en)=>English?en:ja;
            string himariName=Content.customParticipant?Content.Person("himari").displayName:"ひまり";
            // 名乗りそびれた場合の救済: 章0で2〜3ターン目までに名前を伝えられていなければ、自動で名乗る。
            if(s.chapter==0&&s.turn>=2&&!s.introduced)
            {
                s.introduced=true;
                Log(himariName,L($"{himariName}です、伝えそびれていました。",$"I'm {himariName}. I forgot to say so earlier."));
            }
            int st=RelationshipStage(s.Player.trust);
            if(s.chapter==0&&s.turnsSpoken==0)
            {
                var person=Content.Person(s.target);
                string like=person.topics!=null&&person.topics.Length>0?person.topics[0]:L("その話","that");
                Add("ask_name",L(person.displayName+"さん、ですよね。"+Clip(like,12)+"がお好きなんですね。",$"You're {person.displayName}, right? You like {Clip(like,12)}."),"empathy",5);
            }
            if(!s.greeted&&!s.introduced)Add("greet",L("こんにちは。少し、お話ししてもいいですか？","Hello. Could we talk for a moment?"),"empathy",4);
            if(!s.introduced)Add("introduce",Content.customParticipant?L($"{himariName}です。{string.Join("、",Content.Person("himari").topics.Take(3))}が好きです。",$"I'm {himariName}. I like {string.Join(", ",Content.Person("himari").topics.Take(3))}."):L("ひまりです。小さな生き物と童話が好きです。","I'm Himari. I love little creatures and fairy tales."),"creation",5);
            if(s.chapter==0&&s.turnsSpoken==0)
            {
                if(s.firstApproach=="intro")Add("share_more",Content.customParticipant?L(Clip(Content.Person("himari").personality,40)+"……そんな人間です。",$"{Clip(Content.Person("himari").personality,60)}... that's the kind of person I am."):L("まだうまく話せないけど、好きなことから伝えたいです。","I'm not great with words yet, but I want to start by sharing what I like."),"creation",6);
                else if(s.firstApproach=="greet")Add("first_greet",L("声が出せて、よかった。今日は、よろしくお願いします。","I'm glad I could speak up. Nice to meet you today."),"empathy",6);
                else if(s.firstApproach=="compliment")Add("first_compliment",L("目が合って、きれいだなって思ってしまった。","Our eyes met, and I thought you looked beautiful."),"empathy",7);
            }
            if(s.turnsSpoken>0)
            {
                if(!string.IsNullOrEmpty(s.followup))Add("answer",WithName(s.followup),"creation",6);
                var current=s.memories.LastOrDefault(m=>m.owner==s.target&&m.detail==s.topic&&m.loop==s.loop);
                if(st<=0)Add("pickup",WithName(L($"{Clip(s.topic,16)}……もう少し、聞かせてもらえますか？",$"Could you tell me a little more about {Clip(s.topic,16)}?")),s.preferredTag,5,current?.id);
                else if(st==1)Add("pickup",WithName(L($"{Clip(s.topic,14)}、私も気になっていたんです。",$"I've been curious about {Clip(s.topic,14)} too.")),s.preferredTag,6,current?.id);
                else Add("pickup",WithName(L($"{Clip(s.topic,16)}の話、もっと知りたい。",$"I want to know more about {Clip(s.topic,16)}.")),s.preferredTag,5,current?.id);
                if(st>=1&&s.introduced)Add("share_hobby",WithName(Content.customParticipant?L($"私も{Clip(Content.Person("himari").topics.FirstOrDefault()??"同じもの",12)}が好きで、近い気がします。",$"I like {Clip(Content.Person("himari").topics.FirstOrDefault()??"that too",12)} too, so I feel close to you."):L("私も童話が好きで、近い気がします。","I love fairy tales too, so I feel close to you.")),"creation",6);
                if(st==1)Add("compliment",WithName(L("同じ話ができるの、うれしいです。","I'm happy we can talk about the same things.")),"empathy",6);
                if(st==2)
                {
                    Add("compliment",WithName(L("今の笑顔、ちょっと……ドキッとしました。","That smile just now... it made my heart skip a beat.")),"empathy",7);
                    if(s.introduced&&s.deepened&&!s.romanceTalked)Add("romance",WithName(L("こういう時間、少し特別に感じてる。","This time together feels a little special to me.")),"empathy",7);
                }
                if(st>=3)
                {
                    Add("hold_hands",WithName(L("少し、手を繋いでもいいですか。","Would it be okay if we held hands for a bit?")),"empathy",8);
                    if(s.introduced)Add("purpose",WithName(L("終わったあとも、一緒に歩いてみませんか？","Even after this is over, would you want to keep walking together?")),"creation",8);
                    if(!s.firstPartCleared)
                    {
                        Add("spark",WithName(L("今……ちょっとドキドキしてない？","Right now... isn't your heart racing a little too?")),"empathy",8);
                        if(s.agi>=2)Add("spark_ai",WithName(L("素直な気持ちを、伝えてみる。","I want to tell you how I honestly feel.")),"empathy",8);
                    }
                }
            }
            AppendLearnedTechniques();
            if(s.actions.Count==0)Add("listen",L("その続きを、聞かせてください。","Please, tell me more."),"empathy",4);
        }
        public static string Clip(string value,int count)=>string.IsNullOrEmpty(value)?"":value.Length<=count?value:value.Substring(0,count)+"…";
        public List<ShowMemory> AvailableMemories()=>State.memories.Where(m=>m.owner==State.target||m.owner=="himari").ToList();
        public bool Compose(string memoryId,string verb,string otherId=null)
        {
            var s=State;if(s.phase!=ShowPhase.Conversation||s.turnsSpoken<1||s.focus<1||s.composedThisTurn)return false;
            var m=AvailableMemories().Find(x=>x.id==memoryId);if(m==null)return false;
            var other=AvailableMemories().Find(x=>x.id==otherId);
            if(verb=="connect"&&(!s.crafted||other==null||other.id==m.id))return false;
            if(verb!="ask"&&verb!="share"&&verb!="connect")return false;
            s.focus--;s.composedThisTurn=true;s.crafted=true;
            string detail=Clip(m.detail,16),line=verb=="ask"?$"{detail}のことを、もう少し教えて。":verb=="share"?$"{detail}を聞いて、わたしの童話を思い出した。":$"{detail}と{Clip(other.detail,12)}、つながる気がする。";
            if(m.loop<s.loop)line=verb=="connect"?$"{detail}と{Clip(other.detail,12)}は、関係あるかな？":$"ふと思ったんだけど、{detail}は好き？";
            s.actions.RemoveAll(a=>a.id=="crafted");
            s.actions.Add(new TalkAction{id="crafted",line=WithName(line),tag=verb=="ask"?"empathy":"creation",basePower=verb=="connect"?8:5,memoryA=m.id,memoryB=verb=="connect"?other.id:null});
            s.forecast=-1;s.notice="記憶から新しい一言が浮かびました。下の会話選択肢から話せます。";return true;
        }
        public bool PlayCard(int index)
        {
            var s=State;if(s.phase!=ShowPhase.Conversation||index<0||index>=s.hand.Count)return false;
            var card=Card(s.hand[index]);if(card==null||card.cost>s.focus||s.deckMechanics&&!CanPlayDeckCard(card))return false;
            s.focus-=card.cost;s.hand.RemoveAt(index);s.played.Add(card.id);s.forecast=-1;
            switch(card.id){case "listen":s.guard+=6;s.flatBonus+=2;break;case "purity":s.purity++;break;case "echo":s.attune++;s.guard+=2;break;case "draw":Draw(2);s.flatBonus++;break;case "rest":s.stress=Math.Max(0,s.stress-10);break;case "spotlight":s.starsBonus+=12000;s.stressBonus+=5;break;case "blackout":s.thorns++;break;}
            if(s.deckMechanics){ApplyDeckCard(card);RecordDeckEvent("card:"+card.id);}
            s.notice=$"「{card.title}」を準備。このあと選ぶ言葉に作用します。";return true;
        }
        public int Power(TalkAction action)
        {
            var s=State;int p=action.basePower+s.flatBonus+(action.tag==s.preferredTag?3:0);
            // 相手のタイプに刺さる話し方には追加ボーナス: ひかり系＝褒め・甘え、悠斗系＝創作・本音。
            if(s.target=="hikari"&&action.tag=="empathy")p+=3;
            if(s.target=="yuto"&&action.tag=="creation")p+=3;
            if(action.tag=="creation")p+=s.attune;
            if(s.purity>=3&&action.tag!="public")p+=6;
            foreach(string id in s.played)switch(id){case "story":p+=action.tag=="creation"?7+s.attune:1;break;case "rain":p+=s.weather=="rain"?10:3;break;case "wind":p+=s.weather=="wind"?8+(s.perspective?5:0):2;break;case "memory":p+=!string.IsNullOrEmpty(action.memoryA)?7:1;break;case "confess":p+=18;break;case "blackout":p+=20;break;}
            foreach(var id in s.played)
            {
                var echo=s.memoryCards.Find(x=>x.id==id);
                if(echo!=null)p+=echo.memoryId==action.memoryA||echo.memoryId==action.memoryB?10:echo.owner==s.target?3:1;
            }
            foreach(string id in new[]{action.memoryA,action.memoryB}.Where(x=>!string.IsNullOrEmpty(x)).Distinct())
            {var m=s.memories.Find(x=>x.id==id);if(m!=null)p+=m.used==0?4:Math.Max(0,2-m.used);}
            p+=MemoryBonus(action);
            p+=NameBonus(action);p+=TechniqueBonus(action);
            return s.deckMechanics?DeckPower(p,action):p;
        }
        public string CallName()
        {
            var person=Content.Person(State.target);
            // A given name cannot be inferred by cutting the last two characters.
            return string.IsNullOrWhiteSpace(person.callName)?person.displayName:person.callName;
        }
        public bool LineUsesName(string line)=>State.nameKnown&&!string.IsNullOrEmpty(line)&&(line.Contains(CallName())||line.Contains(Content.Person(State.target).displayName));
        public int NameBonus(TalkAction a)
        {
            if(a==null||!LineUsesName(a.line))return 0;
            return a.id=="call_name"||a.id=="compliment"||a.id=="first_compliment"?3:2;
        }
        string WithName(string line)
        {
            if(!State.nameKnown||string.IsNullOrEmpty(line)||LineUsesName(line))return line;
            return CallName()+"さん。"+line;
        }
        public bool RiskFails()=>State.played.Contains("confess")&&State.Player.trust<35&&State.purity<2||State.played.Contains("blackout")&&State.weather!="blackout";
        public bool UsePerspective()
        {
            var s=State;if(s.phase!=ShowPhase.Conversation||s.agi<SkillCost("perspective")||s.perspective)return false;s.agi-=SkillCost("perspective");s.perspective=true;DescribeSupport("沙織が相手の立場を整理し、いま大切にしていそうなことをそっと教えてくれた。（AGI −1）");
            if(s.deckMechanics){s.preferenceKnown=true;s.warmth=Math.Min(Balance.maxWarmth,s.warmth+3);s.notice=PerspectiveScene();Log("沙織 / 観測",s.notice);RecordDeckEvent("skill:perspective");return true;}
            s.notice=$"沙織：{Content.Person(s.target).displayName}さんは、{(s.preferredTag=="creation"?"完成品より、作る途中の迷いを分かち合いたい":s.preferredTag=="empathy"?"評価を急がず、気持ちを聞いてほしい":"この時間を番組の外へ届けたい")}ようです。予習ですので、答えはご本人に。";Log(ShowSupportVoice.Name,s.notice.Replace("沙織：",""));return true;
        }
        public bool Forecast()
        {
            var s=State;if(s.phase!=ShowPhase.Conversation||s.agi<SkillCost("forecast")||s.actions.Count==0)return false;
            DescribeSupport("沙織が手札と記憶から会話の行方を予習した。候補に［予測］の印が付く。危険札の条件にも注意しよう。（AGI −2）");
            s.agi-=SkillCost("forecast");s.forecast=Enumerable.Range(0,s.actions.Count).OrderByDescending(i=>Power(s.actions[i])+(s.actions[i].tag=="public"?2:0)).First();
            s.notice=RiskFails()?"沙織：お待ちください！ 危険札の条件が未達です。このままでは脱落します。まず深呼吸を、ご一緒に……。":$"沙織：今の状況なら「{Clip(s.actions[s.forecast].line,18)}」はいかがでしょう。お返事の予約まではできなくて……。";Log(ShowSupportVoice.Name,s.notice.Replace("沙織：",""));RecordDeckEvent("skill:forecast");return true;
        }
        public bool SetWeather(string weather)
        {
            if(State.phase!=ShowPhase.Conversation||State.agi<SkillCost("weather")||!new[]{"clear","rain","wind","fog","blackout"}.Contains(weather))return false;
            DescribeSupport("沙織が話しやすいきっかけを用意した。場は「"+(weather=="rain"?"雨":weather=="wind"?"風":weather=="fog"?"霧":weather=="blackout"?"停電":"晴れ")+"」に変わり、次の発言までカードの条件に影響する。（AGI −2）");
            State.agi-=SkillCost("weather");State.weather=weather;State.forecast=-1;if(State.deckMechanics)State.flatBonus+=4;State.notice="沙織：次の発言まで、場の条件を変えました。きっかけは、さりげなく。……今の説明は少し目立ちましたね。";Log(ShowSupportVoice.Name,State.notice.Replace("沙織：",""));RecordDeckEvent("skill:weather");return true;
        }
        public bool Acquire()
        {
            var s=State;if(s.phase!=ShowPhase.Conversation||s.agi<SkillCost("acquire")||s.acquired)return false;
            s.agi-=SkillCost("acquire");s.acquired=true;RecordDeckEvent("skill:acquire");
            if(s.deckMechanics)
            {
                // カード撤去後の「新しいカード」は、話題を1つその場で提案する能力として再設計。
                DescribeSupport("沙織が新しい話題の種を見つけてきた。会話の選択肢に加わります。（AGI −1）");
                s.actions.RemoveAll(a=>a.id=="ai_topic");
                s.actions.Insert(0,new TalkAction{id="ai_topic",line=$"{Clip(s.topic,16)}のこと、もっと深く聞いてみたい。",tag=s.preferredTag,basePower=7});
                Log(ShowSupportVoice.Name,"新しい話の糸口を見つけました。下の選択肢に加えておきましたので、よろしければどうぞ。");
                return true;
            }
            DescribeSupport("沙織が新しい言葉の支えになるカードを3枚探してきた。一枚を選んでデッキへ加えよう。（AGI −1）");Draft(true);Log(ShowSupportVoice.Name,"お言葉を支えるカードを探してまいりました。読書のお供に選ぶ栞のように、一枚どうぞ。私のおすすめは……いえ、ひまりさんのお好みで。 ");return true;
        }
        void Draft(bool ability)
        {
            State.rewardFromAbility=ability;State.offers.Clear();var pool=State.deckMechanics?Balance.cards.Where(c=>c.archetype==State.archetype&&(ability?c.rarity==0:c.rarity>0)).Select(c=>c.id).ToList():Content.cards.Select(c=>c.id).ToList();
            for(int i=0;i<3;i++){int n=State.deckMechanics?WeightedCardIndex(pool):Next(pool.Count);State.offers.Add(pool[n]);pool.RemoveAt(n);}
            if(!State.deckMechanics&&State.memoryCards.Count>0)State.offers[0]=State.memoryCards[State.memoryCards.Count-1].id;
            State.phase=ShowPhase.Reward;
        }
        public void TakeReward(int index)
        {
            var s=State;if(s.phase!=ShowPhase.Reward||index<0||index>=s.offers.Count)return;
            string id=s.offers[index];if(s.deckMechanics&&Card(id).rarity>0&&!s.ownedRare.Contains(id))s.ownedRare.Add(id);s.deck.Add(id);s.draw.Add(id);s.offers.Clear();
            Log(ShowSupportVoice.Name,$"「{Card(id).title}」を山札にお入れしました。素敵な一言のお供になりますように。……褒めすぎでしょうか。少し照れますね。");
            if(s.rewardFromAbility)s.phase=ShowPhase.Conversation;
            else if(s.deckMechanics&&s.chapter>=2){s.phase=ShowPhase.Victory;s.epilogue=DeckEpilogue();Bank();Log("悠斗",s.epilogue);}
            else{s.chapter++;if(s.deckMechanics)ResetDeckChapter();s.turn=0;s.agi=Math.Min(12,s.agi+3);s.stress=Math.Max(0,s.stress-12);s.phase=ShowPhase.Route;}
        }
        // 最初の到達点までの距離: まだ遠い→共通の話題→笑顔にキュン→手をつなぐ。
        static int RelationshipStage(int trust)=>trust>=36?3:trust>=20?2:trust>=8?1:0;
        static readonly string[] RelationshipStageNames={"まだ遠い","共通の話題","笑顔にキュン","心が近い"};
        // Stage-up moments no longer narrate: the poetic asides read as out of place and kept resurfacing.
        static readonly string[] StageMoments={"","","",""};
        public static string RelationshipStageLabel(int stage)=>RelationshipStageNames[Math.Clamp(stage,0,RelationshipStageNames.Length-1)];
        static readonly string[] RelationshipToneJa={
            "まだ距離がある段階。丁寧だが少し控えめで、言葉を選びながら話す。質問には答えるが、自分から深い話は振らない。",
            "共通の話題が見つかった段階。声のトーンが少し明るくなり、相手の話に前のめりで食いつく。「それで？」「もっと聞きたい」など、自分から質問を返すことが増える。",
            "笑顔にキュンとする段階。反応がわかりやすく柔らかくなり、照れや小さな笑いが台詞に滲む。相手のことをもっと知りたがり、「あなたは？」と逆質問したり、相手の過去の発言を覚えていて触れたりする。",
            "心が近い段階。言葉に親密さと温かさが出て、将来の話題にも自然に触れ、相手を気遣う言葉が増える。手をつなぐなどの身体的な接触は、プレイヤーがそう言った場合にだけ応じ、自分からは始めない。"
        };
        static readonly string[] RelationshipToneEn={
            "Still distant. Polite but a little reserved, choosing words carefully. Answers questions but doesn't volunteer anything deep unprompted.",
            "Found something in common. Voice warms up a notch; leans into what the player says. Asks things back more often, like \"And then?\" or \"I want to hear more.\"",
            "Smile-flutter stage. Reactions turn visibly softer, with a little shyness or a small laugh showing through the line. Genuinely curious about the player now -- asks \"What about you?\" and brings up things the player said earlier.",
            "Close-at-heart stage. Words carry real warmth, touch naturally on the future, and check in on how the player is feeling more often. Physical closeness such as holding hands only happens if the player says so; never initiate it."
        };
        public static string RelationshipTone(int stage,bool english)=>(english?RelationshipToneEn:RelationshipToneJa)[Math.Clamp(stage,0,3)];
        // Hidden rhythm bonus: once a compliment has landed, alternating between complimenting the other
        // person and opening up about yourself, in roughly equal measure, multiplies the gain by 1.5.
        // Deliberately never surfaced in the UI; the player discovers it through the numbers.
        static string BalanceKind(TalkAction a)
        {
            if(a==null||string.IsNullOrEmpty(a.id))return null;
            if(a.id=="compliment"||a.id=="first_compliment"||a.id.StartsWith("tech_"))return "compliment";
            if(a.id.StartsWith("group_"))return null; // group talk has its own scoring
            if(a.id.StartsWith("final_like"))return "compliment";
            if(a.id.StartsWith("final_"))return "disclosure";
            switch(a.id){case "introduce":case "share_more":case "share_hobby":case "impression":case "honest":case "promise":case "recall":case "romance":case "purpose":return "disclosure";}
            return null;
        }
        int ApplyBalanceBonus(TalkAction action,int power,bool complimentLandedNow)
        {
            var s=State;string kind=BalanceKind(action);if(kind==null)return power;
            int c=s.balanceCompliments+(kind=="compliment"?1:0),d=s.balanceDisclosures+(kind=="disclosure"?1:0);
            bool alternating=!string.IsNullOrEmpty(s.lastBalanceKind)&&s.lastBalanceKind!=kind;
            if(s.complimentLanded&&alternating&&Math.Abs(c-d)<=1)power=power*3/2;
            if(complimentLandedNow)s.complimentLanded=true;
            s.balanceCompliments=c;s.balanceDisclosures=d;s.lastBalanceKind=kind;
            return power;
        }
        // "Understand first, then approach": listening lines (pickups, answers, techniques, memory recalls)
        // set up the next turn. A strong romantic approach right after one, once hearts are close, lands hard;
        // the same approach while still distant lands half as well.
        static bool IsStrongApproach(TalkAction a)=>a!=null&&(a.id=="hold_hands"||a.id=="purpose"||a.id=="spark"||a.id=="spark_ai"||a.id=="romance"||a.id.StartsWith("final_word")||a.id.StartsWith("final_future"));
        static bool IsUnderstanding(TalkAction a)=>a!=null&&(a.id=="pickup"||a.id=="answer"||a.id=="crafted"||a.id=="ask_name"||a.id=="curious"||a.id.StartsWith("tech_")||!string.IsNullOrEmpty(a.memoryA)||!string.IsNullOrEmpty(a.recallA));
        public void ResolveTalk(int index,ShowReply reply)
        {
            var s=State;if(s.phase!=ShowPhase.Conversation||index<0||index>=s.actions.Count)throw new InvalidOperationException("Conversation action is no longer available.");
            var action=s.actions[index];int power=Power(action);int stageBefore=RelationshipStage(s.Player.trust);
            Log("ひまり",action.line);
            if(RiskFails()){Log("実況レン","賭けた言葉は、今の二人には早すぎた。ひまり、脱落。");s.discard.AddRange(s.played);s.discard.AddRange(s.hand);s.played.Clear();s.hand.Clear();EliminatePlayer(English?"The gamble's condition was not met":"危険札の条件未達");return;}
            bool isCompliment=action.id=="compliment"||action.id=="first_compliment";bool complimentSuccess=true;
            if(isCompliment)
            {
                s.complimented=true;
                int chance=55+Math.Min(30,s.Player.trust/2);
                complimentSuccess=Next(100)<chance;
                if(!complimentSuccess)power=Math.Max(1,power/3);
            }
            bool isSpark=action.id=="spark"||action.id=="spark_ai";bool sparkSuccess=true;
            if(isSpark)
            {
                if(action.id=="spark_ai"){s.agi=Math.Max(0,s.agi-2);sparkSuccess=true;}
                else{int chance=Math.Min(90,30+s.loop*15+Math.Min(20,s.Player.trust/3));sparkSuccess=Next(100)<chance;}
                if(sparkSuccess){s.firstPartCleared=true;power+=10;}else power=Math.Max(1,power/3);
            }
            if(action.id=="introduce")s.introduced=true;if(action.id=="greet")s.greeted=true;
            s.usedActions??=new List<string>();if(!s.usedActions.Contains(UsedKey(action)))s.usedActions.Add(UsedKey(action));
            s.nameTip="";
            if(action.id=="ask_name"){s.nameKnown=true;s.nameTip="沙織：名前は大切です。呼ぶだけで注意を引けて、好感度も上がります。褒めるときも、名前を添えてみてください。";}
            if(action.id=="pickup"||action.id=="answer"||action.id=="crafted")s.deepened=true;
            if(action.id=="romance")s.romanceTalked=true;
            if(action.id=="impression")s.impressionShared=true;
            foreach(string id in new[]{action.memoryA,action.memoryB}.Where(x=>!string.IsNullOrEmpty(x)).Distinct()){var m=s.memories.Find(x=>x.id==id);if(m!=null)
                {
                    m.used++;
                    if(!s.memoryCards.Any(c=>c.memoryId==m.id))s.memoryCards.Add(new MemoryCard{id="imprint_"+m.id,memoryId=m.id,title=Clip(m.detail,9)+"の余韻",owner=m.owner,detail=m.detail});
                }}
            if(!string.IsNullOrEmpty(action.memoryB))s.combined=true;
            power=ApplyBalanceBonus(action,power,isCompliment&&complimentSuccess);
            bool approachLanded=false;
            if(IsStrongApproach(action))
            {
                if(stageBefore>=3&&s.lastActionUnderstood){power+=8;approachLanded=true;}
                else if(stageBefore<2)power=Math.Max(1,power/2);
            }
            s.lastActionUnderstood=IsUnderstanding(action);
            // Group talk (round 2): kind is a steady +3; cool swings -5..+7 with the mood; a comeback turns the
            // last reaction into material and lands with the mood. The lead's reaction delta (-5..+5) and the
            // room bonus from ApplyGroupRound are added on top. Numbers stay out of the narration.
            string groupNote="";
            if(action.id=="group_kind"){power=3;s.groupMood=Math.Clamp(s.groupMood+.15f,-1f,1f);groupNote=English?"○ Your kind words landed gently.":"○ 優しい言葉が、穏やかに届いた。";}
            else if(action.id=="group_cool")
            {
                float t=Math.Clamp(Next(1000)/1000f+s.groupMood*.35f,0f,1f);power=(int)Math.Round(-5+12*t);
                s.groupMood=Math.Clamp(s.groupMood+(power>=4?.3f:power<0?-.3f:0),-1f,1f);
                groupNote=power>=5?(English?"✨ That cool line took the room.":"✨ クールな一言が、空気を持っていった。"):power<0?(English?"△ The cool line fell flat.":"△ 決めに行った一言が、空回りした。"):(English?"○ It came across as composed.":"○ 落ち着いた一言として届いた。");
            }
            else if(action.id=="group_comeback")
            {
                power=Math.Clamp((int)Math.Round(s.groupMood*5)+(!string.IsNullOrEmpty(s.lastGroupWinner)&&s.lastGroupWinner!="himari"?2:0)+(string.IsNullOrEmpty(s.lastGroupReaction)?1:0),-5,5);
                groupNote=power>=3?(English?"💥 The comeback landed!":"💥 切り返しが刺さった！"):power<0?(English?"△ The comeback missed the mood.":"△ 切り返しが、空気に合わなかった。"):(English?"○ The comeback kept the flow going.":"○ 切り返しで、流れをつないだ。");
            }
            if(GroupTalk&&action.id=="crafted"){power=5;s.groupMood=Math.Clamp(s.groupMood+.2f,-1f,1f);groupNote=English?"💡 An answer built from this round's memory landed squarely.":"💡 この回の記憶を活かした答えが、しっかり届いた。";}
            if(action.id.StartsWith("group_")||(GroupTalk&&action.id=="crafted")){power+=s.groupPlayerDelta;s.groupPlayerDelta=0;}
            s.Player.trust=Math.Max(0,s.Player.trust+(s.target=="yuto"?power:(power+1)/2));
            if(s.target!="yuto"){var ally=s.contestants.Find(c=>c.id==s.target);ally.alliance+=power/3;s.Player.alliance+=power/3;}
            s.Player.stars+=s.starsBonus+power*350+(action.tag=="public"?(s.weather=="fog"?3000:6000):1000);
            s.stress=Math.Clamp(s.stress+Math.Max(0,s.intentPressure+s.stressBonus-s.guard-(s.weather=="fog"?3:0))-(action.tag=="empathy"?2:0),0,100);
            if(s.deckMechanics)ResolveDeckTurn(action,power);
            else foreach(var rival in s.contestants.Where(x=>x.id!="himari"&&!x.eliminated)){rival.trust+=6+Next(7)+s.chapter+(int)s.difficulty*2;rival.stars+=2000+Next(6000);}
            RecordConversation(action,reply,s.target=="yuto"?power:(power+1)/2);
            if(TechniqueBonus(action)>0)s.usedTechniques.Add(action.id);
            s.turnsSpoken++;s.turn++;
            Log(Content.Person(s.target).displayName,reply.reply);s.lastNpc=reply.reply;s.followup=reply.followup;
            if(string.IsNullOrEmpty(s.followup)&&(reply.reply.Contains("？")||reply.reply.Contains("?")))s.followup=ShowDialogue.FallbackAnswer(reply.reply,this);
            foreach(var fact in reply.facts??Array.Empty<ShowFact>())if(!string.IsNullOrWhiteSpace(fact.detail)&&fact.detail.Length<=80&&reply.reply.Contains(fact.detail))Remember(s.target,fact.category,fact.detail,"聞いた");
            if(!string.IsNullOrEmpty(reply.topic)&&reply.reply.Contains(reply.topic)){s.topic=reply.topic;Remember(s.target,"topic",reply.topic,"聞いた");}
            if(action.id=="introduce")Remember("himari","self",Content.customParticipant?string.Join("、",Content.Person("himari").topics.Take(3)):"童話と小さな生き物","伝えた");
            int stageAfter=RelationshipStage(s.Player.trust);bool stageUp=stageAfter>stageBefore;s.stageMoment="";if(stageUp){s.relationshipStage=stageAfter;s.stageMoment=StageMoments[stageAfter];}
            string vibe=power>=action.basePower+8?"◎ 手ごたえ十分":power>=action.basePower+2?"○ 順調に進んでる":"△ 反応は控えめ";
            Log("解説ミサ",$"信頼 +{(s.target=="yuto"?power:(power+1)/2)}。{(!string.IsNullOrEmpty(action.memoryA)?"以前の言葉を拾ったことが、会話をつないだ。":"次に何を知りたくなるかが、彼女の言葉を変える。")}");
            if(isCompliment)Log("解説ミサ",complimentSuccess?"✓ 褒め言葉が届いた。表情がやわらいだ。":"△ 少し照れさせただけで、空気は変わらなかった。");
            if(isSpark)Log("解説ミサ",sparkSuccess?"💗 ドキッとさせた！ファーストパート・クリア！":"△ 空気は変わったけど、決め手には欠けた。もう一度チャンスを探ろう。");
            if(stageUp&&!string.IsNullOrEmpty(s.stageMoment))Log("地の文",s.stageMoment);
            s.discard.AddRange(s.played);s.discard.AddRange(s.hand);s.played.Clear();s.hand.Clear();
            if(s.stress>=100){EliminatePlayer(English?"Nerves hit their limit":"緊張が限界に達した");return;}
            // The fourth reply is shown first; the ceremony runs when the player presses next (RunPendingCeremony).
            if(s.turn>=4)s.ceremonyPending=true;else BeginTurn();
            string headline=isSpark?(sparkSuccess?"💗 ドキッとさせた！ファーストパート・クリア！\n":"△ 決め手には欠けた。もう一押し。\n"):isCompliment?(complimentSuccess?"✓ 褒め言葉が届いた。\n":"△ 少し照れさせただけで終わった。\n"):"";
            if(approachLanded)headline+=English?"💗 Because you listened first, that step landed deep.\n":"💗 相手を受け止めたあとの一歩が、深く届いた。\n";
            if(!string.IsNullOrEmpty(groupNote))headline+=groupNote+"\n";
            if(stageUp&&!string.IsNullOrEmpty(s.stageMoment))headline+=s.stageMoment+"\n";
            s.notice=GroupTalk?headline+(English?$"The room feels {GroupMoodLabel}.":$"場の空気は、{GroupMoodLabel}。")
                :headline+vibe+"\n"+s.memoryFeedback+"\n"+$"好感度 +{(s.target=="yuto"?power:(power+1)/2)}。{(!string.IsNullOrEmpty(action.memoryA)?"記憶から話をつなぎました。":"相手の返答から、新しい話題が浮かびました。")}\n続き：{Clip(s.topic,24)}";
        }
        static readonly string[] CeremonyProse={
            "{0}の椅子にだけ、スポットライトが残らない。拍手はすぐ次の話題へ流れていく。",
            "{0}は最後まで微笑んでいた。カメラが離れても、その微笑みだけがしばらく画面に残る。",
            "{0}が舞台袖へ消えていく。誰も呼び止めなかった。窓の外は、もう暗い。",
        };
        public void RunPendingCeremony(){if(!State.ceremonyPending)return;State.ceremonyPending=false;Ceremony();}
        void Ceremony()
        {
            var s=State;var loser=s.contestants.Where(x=>!x.eliminated).OrderBy(x=>x.Merit).ThenBy(x=>x.id=="himari"?0:1).First();
            loser.eliminated=true;s.lastEliminated=loser.id;string loserName=Content.Person(loser.id).displayName;
            if(loser.id=="himari"){Log("実況レン",$"第{s.chapter+1}回の選考。信頼を比べた結果……{loserName}が脱落。");EliminatePlayer(English?"Lowest selection score among the survivors":"選考スコアが生存者の中で最下位");return;}
            if(s.chapter>=2)
            {
                // The final is framed as being chosen, not as someone else going out.
                string lead=Content.Person("yuto").displayName,me=Content.Person("himari").displayName;
                Log("実況レン",English?$"The final choice. {lead} chose... {me}!":$"ファイナル。{lead}が選んだのは……{me}！");
                s.phase=ShowPhase.Ceremony;
                s.notice=English?$"{lead} chose {me}.\n{loserName} bows out with a smile, and the last spotlight settles on the two of you.":$"{lead}が選んだのは、{me}。\n{loserName}は微笑んで舞台を降りた。最後のスポットライトは、ふたりの上に落ちる。";
                return;
            }
            Log("実況レン",$"第{s.chapter+1}回の選考。信頼を比べた結果……{loserName}が脱落。");
            s.phase=ShowPhase.Ceremony;s.notice=$"{loserName}はOUT。残るのは{s.contestants.Count(c=>!c.eliminated)}人。\n"+string.Format(CeremonyProse[Next(CeremonyProse.Length)],loserName);
        }
        void EliminatePlayer(string reason){State.Player.eliminated=true;State.phase=ShowPhase.Eliminated;State.lastEliminated="himari";State.notice=reason;Bank();}
        void Bank(){if(State.banked)return;State.bank=Math.Min(24,State.bank+State.agi);State.banked=true;}
        public void RecordPresented(string speaker,string text)
        {
            if(string.IsNullOrWhiteSpace(text))return;
            State.presentedLog??=new List<ShowBeat>();State.learnedTechniques??=new List<string>();State.usedTechniques??=new List<string>();State.rivalConversations??=new List<RivalConversationBeat>();State.comebackReviews??=new List<string>();
            var last=State.presentedLog.LastOrDefault();
            if(last!=null&&last.speaker==speaker&&last.text==text&&last.loop==State.loop)return;
            State.presentedLog.Add(new ShowBeat{speaker=speaker,text=text,loop=State.loop,chapter=State.chapter});
            if(State.presentedLog.Count>180)State.presentedLog.RemoveAt(0);
        }
        public void Log(string speaker,string text){State.log.Add(new ShowBeat{speaker=Content.AdaptText(speaker),text=Content.AdaptText(text),loop=State.loop,chapter=State.chapter});if(State.log.Count>180)State.log.RemoveAt(0);}
        public ShowMemory Remember(string owner,string category,string detail,string source)
        {
            if(string.IsNullOrWhiteSpace(detail))return null;detail=detail.Trim();category=string.IsNullOrWhiteSpace(category)?"topic":category;
            var old=State.memories.Find(m=>m.owner==owner&&m.category==category&&(m.detail.Contains(detail)||detail.Contains(m.detail)));
            if(old!=null){if(old.loop<State.loop){old.loop=State.loop;old.source=source;}return old;}
            var memory=new ShowMemory{id=Guid.NewGuid().ToString("N"),owner=owner,category=category,detail=detail,source=source,loop=State.loop};
            State.memories.Add(memory);return memory;
        }
        public string Motivation=>!State.introduced?"自分のことを伝えたい":!State.crafted?"相手が話したことを、もう少し知りたい":!State.combined?"二つの記憶を結んで、気持ちを伝えたい":State.Player.trust<35?"この出会いを、もう少し深めたい":"この先の約束を交わしたい";
    }
}
