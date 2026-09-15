using System;
using System.Collections.Generic;
using System.Linq;
using HundredHour.Localization;

namespace HundredHour.RealityShow
{
    public sealed partial class ShowGame
    {
        public static readonly string[] TopicGoals={"深く知る","自分も伝える","一緒の未来"};
        // Round 2 keeps its own memory: only what was said in this group talk is on the table there,
        // and those group memories stay out of the other rounds.
        bool MemoryVisible(ConversationMemory m)=>GroupTalk?(m.chapter==1&&m.loop==State.loop&&m.timeline==State.timeLeaps):FinalTalk?m.effect=="final":m.effect!="group"&&m.effect!="final";
        public List<ConversationMemory> SharedMemories()=>State.conversations.Where(m=>m.partner==State.target&&m.loop==State.loop&&m.timeline==State.timeLeaps&&MemoryVisible(m)).ToList();
        public List<ConversationMemory> RecallMemories()=>State.conversations.Where(m=>m.partner==State.target&&m.loop<=State.loop&&MemoryVisible(m)).ToList();
        public bool IsPastMemory(ConversationMemory m)=>m!=null&&(m.loop!=State.loop||m.timeline!=State.timeLeaps);
        public string MemorySummary(ConversationMemory m)
        {
            if(m==null)return "";
            string speech=m.reply??"";int at=speech.IndexOf(m.topic??"",StringComparison.Ordinal);
            int start=at<0?0:Math.Max(0,at-8);
            bool english=GameLanguage.Current==GameLocale.English;
            // English: never start mid-word ("'s your favorite…"); snap back to the sentence start or a word boundary.
            if(english&&start>0){int space=speech.LastIndexOf(' ',start);start=at<=40||space<0?0:space+1;}
            string excerpt=Clip(speech.Substring(start).Replace("\n"," "),44);
            string prefix=IsPastMemory(m)?(english?"Heard last time: ":"前の時間で聞いた："):(english?"They said: ":"相手の言葉：");
            return prefix+excerpt;
        }
        public void RememberFirstImpression(string playerLine,string reply)
        {
            State.conversations.Add(new ConversationMemory{id=Guid.NewGuid().ToString("N"),partner=State.target,loop=State.loop,timeline=State.timeLeaps,chapter=0,turn=0,playerLine=playerLine,reply=reply,topic="第一印象",meaning=PartnerMeaningFromLines(playerLine,reply,"第一印象"),effect="第一印象として残った"});
        }
        public static string TopicLabel(string topic)=>topic=="第一印象"&&GameLanguage.Current==GameLocale.English?"First impression":topic;
        public string DescribedMeaning(ConversationMemory m)=>m==null?"":(string.IsNullOrEmpty(m.meaning)?PartnerMeaningFromLines(m.playerLine,m.reply,m.topic):m.meaning);
        public string DescribedEffect(ConversationMemory m)=>m==null?"":(string.IsNullOrEmpty(m.effect)?"交わした言葉として残った":m.effect);
        static bool Has(string text,params string[] cues)=>!string.IsNullOrEmpty(text)&&cues.Any(text.Contains);
        string CueFromSpeech(string playerLine,string topic)
        {
            if(Has(playerLine,"食事","料理","食べ"))return "食事の好み";
            if(Has(playerLine,"童話","本","読書","物語"))return "好きな物語";
            if(Has(playerLine,"音楽","歌"))return "音楽の好み";
            if(Has(playerLine,"絵","写真","創作"))return "創作の話";
            if(Has(playerLine,"散歩","自然","花","植物"))return "自然の話";
            if(!string.IsNullOrWhiteSpace(topic)&&topic!="第一印象")return Clip(topic,14);
            return "この人柄";
        }
        string PartnerMeaningFromLines(string playerLine,string reply,string topic)
        {
            string cue=CueFromSpeech(playerLine,topic);
            if(Has(reply,"私も","同じ","大好き","目がない"))return $"{cue}を、自分と同じ好みとして受け取った。";
            if(Has(reply,"好き","覚え","名前"))return $"{cue}を、相手の人柄として覚えた。";
            if(Has(reply,"？","?"))return $"{cue}について、もう少し知りたいと思った。";
            return "名前と人柄を、第一印象として受け取った。";
        }
        string PartnerMeaning(TalkAction action,ShowReply reply)
        {
            string spoken=reply?.reply??"";
            string topic=Clip(string.IsNullOrWhiteSpace(reply?.topic)?State.topic:reply.topic,14);
            var recalled=SharedMemory(action.recallA);
            if(recalled!=null)
            {
                string prior=Clip(recalled.topic,12);
                if(action.goal==TopicGoals[2])return $"「{prior}」から、一緒に過ごす時間を想像した。";
                if(action.goal==TopicGoals[1])return $"「{prior}」に、あなたの経験を重ねて受け取った。";
                return $"「{prior}」の理由や気持ちを聞かれ、自分の話として返した。";
            }
            if(action.id=="introduce")return PartnerMeaningFromLines(action.line,spoken,topic);
            if(action.id=="greet")return "声をかけられたことを歓迎した。";
            if(action.id=="call_name")return "名前を呼ばれて、意識がこちらに向いた。";
            if(action.id=="ask_name")return "自分の名前を覚えてもらえて、少し嬉しかった。";
            if(action.id=="compliment"||action.id=="first_compliment")return LineUsesName(action.line)?"名前を添えた褒め言葉が、まっすぐ届いた。":"褒め言葉として受け止め、距離が近づいた。";
            if(Has(spoken,"私も","同じ","大好き","目がない"))return $"{topic}を、自分と同じ好みとして受け取った。";
            if(Has(spoken,"覚えて","拾って"))return "以前の言葉を覚えていることを喜んだ。";
            if(Has(spoken,"一緒","ふたり","二人"))return "これからを一緒に考える話として受け取った。";
            if(Has(spoken,"？","?"))return $"{topic}について、もう少し知りたいと思った。";
            return $"{topic}を、自分の経験として返した。";
        }
        string MemoryEffect(TalkAction action,int trustGain,int insight,int bonus,bool stageUp,int stage)
        {
            bool english=GameLanguage.Current==GameLocale.English;
            var parts=new List<string>();
            if(trustGain>0)parts.Add(english?$"Affection +{trustGain}":$"好感度 +{trustGain}");
            parts.Add(english?$"Understanding +{insight}":$"相互理解 +{insight}");
            if(NameBonus(action)>0)parts.Add(english?"Called by name":"名前呼び");
            if(bonus>0)parts.Add(bonus>=7?(english?"Connected two memories":"ふたつの記憶をつないだ"):(english?"Deepened a memory":"以前の記憶を深めた"));
            if(stageUp)parts.Add((english?"Closeness → ":"距離感 → ")+GameLanguage.Text(RelationshipStageLabel(stage)));
            return string.Join(" ／ ",parts);
        }
        public ConversationMemory SharedMemory(string id)=>RecallMemories().Find(m=>m.id==id);
        public bool SelectRecall(string id,bool second=false)
        {
            if(State.phase!=ShowPhase.Conversation||!string.IsNullOrEmpty(State.pendingTopic)||SharedMemory(id)==null)return false;
            if(second){if(id==State.recallA)return false;State.recallB=id;}
            else{State.recallA=id;if(State.recallB==id)State.recallB="";}
            return true;
        }
        public string RecallContext()
        {
            string Describe(string id){var m=SharedMemory(id);return m==null?"なし":$"{(IsPastMemory(m)?"過去の時間の記憶。相手は覚えていない。以前話したと言わず、初めての質問にする":"現在ふたりで共有する記憶")}／話題：{m.topic}／相手：{m.reply}";}
            return "選んだ話題1："+Describe(State.recallA)+"\n選んだ話題2："+Describe(State.recallB);
        }
        public bool BeginTopic(int goal)
        {
            var s=State;
            if(s.phase!=ShowPhase.Conversation||goal<0||goal>=TopicGoals.Length||s.acquired||s.agi<SkillCost("acquire")||!string.IsNullOrEmpty(s.pendingTopic))return false;
            s.agi-=SkillCost("acquire");s.acquired=true;s.topicGoal=TopicGoals[goal];
            s.pendingTopic="conversation";s.pendingTopicSeed=Clip(SharedMemory(s.recallA)?.topic??s.topic,24);
            return true;
        }
        public string LocalTopicLine()
        {
            bool english=GameLanguage.Current==GameLocale.English;
            string a=Clip(TopicLabel(SharedMemory(State.recallA)?.topic??State.topic),18),b=Clip(TopicLabel(SharedMemory(State.recallB)?.topic),14);
            string seed=string.IsNullOrEmpty(b)?a:a+(english?" and ":"と")+b;
            if(IsFinalDate)
            {
                string question=english
                    ?(State.turn<=0?"I want to know your true feelings. Could you see me as your partner?":State.turn==1?"I want a future with you after the show. What life would you want us to build?":State.turn==2?"I want us to face the hard days together too. What promise would matter most to you?":"I want to share my life with you. Will you marry me?")
                    :(State.turn<=0?"あなたの本当の気持ちが知りたい。私を恋人として考えてくれる？":State.turn==1?"番組のあとも、あなたと一緒にいたい。ふたりでどんな毎日を築きたい？":State.turn==2?"つらい日も一緒に乗り越えたい。ふたりで守りたい大切な約束は何？":"これからの人生を、あなたと歩みたい。私と結婚してくれますか？");
                return WithName((english?$"Thinking of {seed}, ":$"「{seed}」から考えたんだけど、")+question);
            }
            string mine=Clip(Content.Person("himari").topics?.FirstOrDefault()??(english?"quiet time to talk":"ゆっくり話す時間"),12);
            string line=english
                ?(State.topicGoal==TopicGoals[1]?$"I love {mine} too, and I feel it connects to {seed}. Where do you think we're alike?":State.topicGoal==TopicGoals[2]?$"Starting from {seed}, want to try something together sometime? What kind of time would you like?":$"What first made {seed} matter to you? I'd love to know how you felt then, too.")
                :(State.topicGoal==TopicGoals[1]?$"私も{mine}が好きで、{seed}に通じる気がする。どこが似ていると思う？":State.topicGoal==TopicGoals[2]?$"{seed}をきっかけに、今度ふたりで何かしてみない？ どんな時間にしたい？":$"{seed}が大切になったきっかけは？ その時の気持ちも知りたい。");
            return WithName(line);
        }
        public bool IsFinalDate=>State.chapter==2;
        public string FinalDateTopicDirection(bool english)
        {
            if(!IsFinalDate)return "";
            if(english)return State.turn<=0?"Confess attraction and ask directly about their true romantic feelings.":State.turn==1?"Ask about choosing each other and building a concrete life together after the show.":State.turn==2?"Discuss a lifelong commitment, a promise, or a concern that matters before marriage.":"Make a sincere, explicit marriage proposal and leave the answer to the other person.";
            return State.turn<=0?"好意を自分の言葉で伝え、恋人としてどう思っているか、本当の気持ちを率直に尋ねる。":State.turn==1?"番組のあとも互いを選び、ふたりで築く具体的な暮らしや将来を話す。":State.turn==2?"生涯をともにする覚悟、守りたい約束、結婚前に向き合いたい不安の核心に踏み込む。":"自分の意志として、結婚してほしいと明確にプロポーズする。返事は相手に委ねる。";
        }
        public bool TopicMatchesGoal(string line)
        {
            if(string.IsNullOrWhiteSpace(line))return false;
            string[] cues=State.topicGoal==TopicGoals[0]?new[]{"理由","きっかけ","気持ち","なぜ","どうして","どんな思い","惹かれ","どう感じ","どこが","何が","どういう","どんなところ","どうやって"}:
                State.topicGoal==TopicGoals[1]?new[]{"私","僕","俺","自分","わたし","ぼく"}:new[]{"一緒","ふたり","二人"};
            return cues.Any(line.Contains);
        }
        public bool ComposeRecall()
        {
            var s=State;if(s.phase!=ShowPhase.Conversation||SharedMemory(s.recallA)==null||s.composedThisTurn||s.focus<1||!string.IsNullOrEmpty(s.pendingTopic))return false;
            s.focus--;s.composedThisTurn=true;s.topicGoal=TopicGoals[0];
            s.actions.RemoveAll(a=>a.id=="recall_memory");
            s.actions.Insert(0,new TalkAction{id="recall_memory",line=LocalTopicLine(),tag=s.preferredTag,basePower=6,goal=s.topicGoal,recallA=s.recallA,recallB=s.recallB});
            return true;
        }
        string SynergyKey(TalkAction a)
        {
            if(SharedMemory(a.recallA)==null)return null;
            var ids=new[]{a.recallA,SharedMemory(a.recallB)?.id}.Where(x=>!string.IsNullOrEmpty(x)).OrderBy(x=>x);
            return State.loop+":"+State.timeLeaps+":"+State.target+":"+string.Join("+",ids)+":"+a.goal;
        }
        public int MemoryBonus(TalkAction a)
        {
            string key=SynergyKey(a);if(key==null||State.memorySynergies.Contains(key))return 0;
            // A memory can support at most three fresh angles, even with new partners in a pair.
            if(new[]{a.recallA,a.recallB}.Where(id=>!string.IsNullOrEmpty(id)).Any(id=>State.memorySynergies.Count(k=>k.StartsWith(State.loop+":"+State.timeLeaps+":")&&k.Contains(id))>=3))return 0;
            bool past=IsPastMemory(SharedMemory(a.recallA))||IsPastMemory(SharedMemory(a.recallB));
            return SharedMemory(a.recallB)==null?(past?6:4):(past?10:7);
        }
        // Choice labels carry no bonus annotations any more; the numbers show up in the result instead.
        public string MemoryPreview(TalkAction a)=>"";
        public string MemoryPreviewDetailed(TalkAction a)
        {
            bool english=GameLanguage.Current==GameLocale.English;
            string nameMark=NameBonus(a)>0?(english?$"[Name +{NameBonus(a)}] ":$"［名前呼び +{NameBonus(a)}］ "):"";
            if(SharedMemory(a.recallA)==null)return TechniquePreview(a)+nameMark;
            int bonus=MemoryBonus(a);
            return nameMark+(bonus==0?(english?"[Already used] ":"［既出のつながり］ "):english?$"[Memory{(SharedMemory(a.recallB)==null?"":"x2")} +{bonus}] ":$"［記憶{(SharedMemory(a.recallB)==null?"":"×記憶")} +{bonus}］ ");
        }
        void RecordConversation(TalkAction action,ShowReply reply,int trustGain)
        {
            var s=State;int bonus=MemoryBonus(action);int insight=bonus>=7?6:bonus>0?4:1;
            if(bonus>0)s.memorySynergies.Add(SynergyKey(action));
            int before=s.understanding;s.understanding=Math.Min(100,before+insight);
            int stageAfter=RelationshipStage(s.Player.trust);bool stageUp=stageAfter>s.relationshipStage;
            bool english=GameLanguage.Current==GameLocale.English;
            s.memoryFeedback=bonus>0?
                (english?$"\"{SharedMemory(action.recallA).topic}\"{(bonus>=7?" and \""+SharedMemory(action.recallB).topic+"\"":"")} deepened the conversation. Affection +{bonus} / Understanding +{s.understanding-before}"
                    :$"「{SharedMemory(action.recallA).topic}」{(bonus>=7?"と「"+SharedMemory(action.recallB).topic+"」":"")}から会話が深まった。好感度に +{bonus} ／ 相互理解 +{s.understanding-before}")
                :(english?$"Saved to shared memory. Understanding +{s.understanding-before}":$"ふたりの記憶に保存。相互理解 +{s.understanding-before}");
            s.conversations.Add(new ConversationMemory{id=Guid.NewGuid().ToString("N"),partner=s.target,loop=s.loop,timeline=s.timeLeaps,chapter=s.chapter,turn=s.turn,playerLine=action.line,reply=reply.reply,topic=string.IsNullOrWhiteSpace(reply.topic)?s.topic:reply.topic,meaning=PartnerMeaning(action,reply),effect=MemoryEffect(action,trustGain,s.understanding-before,bonus,stageUp,stageAfter)});
            s.recallA=s.recallB="";
            Log(english?"Shared memory":"ふたりの記憶",s.memoryFeedback);
        }
        public bool SenseFeelings()
        {
            var s=State;if(s.phase!=ShowPhase.Conversation||s.perspective||s.agi<1)return false;
            s.agi--;s.perspective=true;
            string hint=s.preferredTag=="empathy"?"評価や提案を急がず、なぜ大切なのかを聞いてほしそうです。":"相手の話を受け止めたうえで、あなた自身の経験も聞きたそうです。";
            s.notice="沙織：直前の言葉からの推測です。"+hint+"\n上部の記憶から以前の会話を選び、本人に確かめてみましょう。";
            s.notice+="\n"+LearnAdviceTechnique();DescribeSupport(s.notice);return true;
        }
        // 相手が今どれくらい心を開いているかの簡易指標。会話が進むペースに対して信頼が伸びていなければ「警戒気味」とみなす。
        public bool Wary=>State.turnsSpoken>=1&&State.Player.trust<State.turnsSpoken*5;
        public (string headline,string advice) MascotHint()
        {
            var s=State;bool english=GameLanguage.Current==GameLocale.English;
            if(FinalTalk)return english?
                ("This is the last one-on-one. Say it in words.","Include the word \"like\" when you talk. "+FinalThemeTip(FinalThemeIndex)):
                ("最後のツーショットです。ちゃんと言葉で伝えることが大切です。","『好き』という言葉を含めて話しましょう。"+FinalThemeTip(FinalThemeIndex));
            if(GroupTalk)return english?
                ("In a group, character is everything.",$"The reliable play: open the memory list (top right), pick something said in this round, and build an answer to the theme from it -- that always lands. Otherwise, kind answers earn a little in any mood, cool answers swing big with the room, and a comeback that uses the last reaction can land hard. Right now the room feels {GroupMoodLabel}."):
                ("複数人の会話では、キャラが大事です。",$"定石は、右上の記憶からこの回で出た発言を選び、それを材料にお題への答えを作ることです。これは確実に届きます。それ以外では、優しい答えはどんな空気でも少しずつ、クールな答えは空気次第で大きく上がるか外すか、直前の反応をネタにした切り返しは刺されば大きいです。今の場の空気は、{GroupMoodLabel}。");
            if(RecallMemories().Any(IsPastMemory)&&string.IsNullOrEmpty(s.recallA))return english?
                ("Try a conversation built on a memory, from the top right.","You can pick more than one memory to make a topic."):
                ("右上から、記憶を活かした会話をしてみましょう。","記憶は複数選んで話題を作れます。");
            if(!s.nameKnown)return english?("A name really matters, you know!","Just calling it gets their attention, and affection goes up too. Try asking their name first!"):("名前は、とても大切なのです！","呼ぶだけで注意を引けて、好感度も上がるのです。まずは名前を聞いてみるのですよ！");
            if(!s.introduced)return english?("Try sharing your own name too!","Once you can call them by name, casually introduce yourself as well."):("自分の名前も、伝えてみるのです！","相手の名前を呼べるようになったら、自分のこともさらっと名乗るのです。");
            if(Wary)return english?("They seem a little guarded right now...","No need to rush — try adding \""+CallName()+"\" before asking again."):("ちょっと身構えられてる気がするのです……","急がず、"+CallName()+"さん、と名前を添えてから聞き返すのがいいと思うのです。");
            if(!s.complimented)return english?("Add their name when you compliment them!","Calling them \""+CallName()+"\" before a compliment makes it really land♪"):("褒めるときは、名前を添えるのです！",CallName()+"さん、と呼んでから褒めると、ちゃんと届くのです♪");
            return english?("Talk while using their name!","Saying \""+CallName()+"\" while you talk makes a combo. You can combine it with a memory above too."):("名前を読んで、会話するのです！",CallName()+"さん、と呼びながら話すとコンボになるのです。上の記憶と組み合わせてもいいのですよ。");
        }
        public bool MascotAdvice()
        {
            var s=State;if(s.phase!=ShowPhase.Conversation)return false;
            var hint=MascotHint();bool english=GameLanguage.Current==GameLocale.English;
            // A newly learned technique is folded into Saori's line as one more sentence; otherwise nothing is appended.
            if(FinalTalk){State.notice=english?$"Saori: \"{hint.headline} {hint.advice}\"":$"沙織「{hint.headline} {hint.advice}」";}
            else if(GroupTalk)
            {
                // Round 2: advice follows the current theme instead of teaching a one-on-one technique.
                string tip=GroupThemeTip(GroupThemeIndex);
                State.notice=english?$"Saori: \"{hint.headline} {hint.advice} {tip}\"":$"沙織「{hint.headline} {hint.advice} {tip}」";
            }
            else
            {
                int known=State.learnedTechniques.Count;string tech=LearnAdviceTechnique();bool learned=State.learnedTechniques.Count>known;
                State.notice=english?$"Saori: \"{hint.headline} {hint.advice}{(learned?" And "+tech:"")}\"":$"沙織「{hint.headline} {hint.advice}{(learned?" そして、"+tech:"")}」";
            }
            DescribeSupport(State.notice);
            return true;
        }
    }
}
