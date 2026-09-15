using System;
using System.Collections.Generic;
using System.Linq;
using HundredHour.Localization;

namespace HundredHour.RealityShow
{
    [Serializable] public sealed class FinalMemoryIdea {public string[] likes,charms,together;}
    // Round 3 (chapter 2): the final one-on-one. Each turn has a theme that pulls the earlier rounds
    // back in -- the day you met, what you like about them, the future, the words you owe them --
    // and the memory list is rebuilt into what the player likes, finds charming, and wants to do together.
    public sealed partial class ShowGame
    {
        static readonly string[] FinalThemesJa={"出会った日のこと","あなたの好きなところ","ふたりの未来","伝えたい言葉"};
        static readonly string[] FinalThemesEn={"The day we met","What I like about you","Our future","The words I owe you"};
        public bool FinalTalk=>State.chapter==2&&State.communicationMechanics;
        public int FinalThemeIndex=>Math.Clamp(State.turn,0,3);
        public string FinalTheme(int index)=>(EnglishUI?FinalThemesEn:FinalThemesJa)[Math.Clamp(index,0,3)];
        IEnumerable<ConversationMemory> FinalMemories=>State.conversations.Where(m=>m.effect=="final"&&m.loop==State.loop&&m.timeline==State.timeLeaps);
        string FinalPick(string prefix,string fallback,int index=0)
        {
            var list=FinalMemories.Where(m=>m.topic.StartsWith(prefix)).Select(m=>m.reply).Where(x=>!string.IsNullOrWhiteSpace(x)).ToList();
            return list.Count==0?fallback:list[Math.Abs(index)%list.Count];
        }
        // Seeds the final-round memory from what the game already knows (called when the final route is chosen);
        // the AI pass in the director adds more from the actual conversations when it succeeds.
        public void EnsureFinalMemories()
        {
            if(FinalMemories.Any())return;
            var lead=Content.Person("yuto");bool h=RomanticLeadProfiles.IsHikari(lead);
            var likes=EnglishUI?new[]{h?"your smile":"your calm way of listening",h?"how you say what you feel":"how you notice the small things"}:new[]{h?"笑顔":"落ち着いた聞き方",h?"気持ちをそのまま言葉にするところ":"小さなことに気づいてくれるところ"};
            var charms=EnglishUI?new[]{Clip(lead.personality,60)}:new[]{Clip(lead.personality,40)};
            var together=(lead.topics??Array.Empty<string>()).Take(2).ToArray();
            AddFinalMemories(likes,charms,together);
        }
        public void AddFinalMemories(IEnumerable<string> likes,IEnumerable<string> charms,IEnumerable<string> together)
        {
            var s=State;
            void Add(string prefix,IEnumerable<string> items){foreach(var x in (items??Array.Empty<string>()).Where(v=>!string.IsNullOrWhiteSpace(v)).Select(v=>v.Trim()).Take(4))
                if(!FinalMemories.Any(m=>m.topic==prefix+x))s.conversations.Add(new ConversationMemory{id=Guid.NewGuid().ToString("N"),partner=s.target,loop=s.loop,timeline=s.timeLeaps,chapter=2,turn=s.turn,playerLine="",reply=x,topic=prefix+x,meaning="",effect="final"});}
            Add(EnglishUI?"What I like: ":"好きなところ：",likes);Add(EnglishUI?"Charm: ":"魅力：",charms);Add(EnglishUI?"Together: ":"一緒にしたい：",together);
        }
        public string FinalMemoryPrompt()
        {
            var lead=Content.Person("yuto");var me=Content.Person("himari");
            var history=State.conversations.Where(m=>m.partner==State.target&&m.loop==State.loop&&m.timeline==State.timeLeaps&&m.effect!="final").TakeLast(14)
                .Select(m=>$"{(string.IsNullOrEmpty(m.playerLine)?"":m.playerLine+" / ")}{lead.displayName}: {Clip(m.reply,70)}");
            return EnglishUI
                ?$"From this conversation history between the player {me.displayName} and {lead.displayName} ({lead.personality}), extract in the player's voice: \"likes\" (3 short things the player likes about {lead.displayName}, each at most 8 words), \"charms\" (2 charming traits, at most 8 words), \"together\" (3 things the player would want to do with {lead.displayName}, at most 8 words, e.g. a trip or a birthday). Ground every item in the history; do not invent events. JSON only: {{\"likes\":[],\"charms\":[],\"together\":[]}}\n\nHistory:\n{string.Join("\n",history)}"
                :$"プレイヤー{me.displayName}と{lead.displayName}（{lead.personality}）のこれまでの会話から、プレイヤー視点で次を抽出する。likes：{lead.displayName}の好きなところ3つ（各12字以内）、charms：魅力的だと感じたこと2つ（各12字以内）、together：{lead.displayName}と一緒にしたいこと3つ（各14字以内、旅行や誕生日など具体的に）。すべて会話の内容に根拠があるものだけ。出来事を捏造しない。JSONのみ：{{\"likes\":[],\"charms\":[],\"together\":[]}}\n\n会話：\n{string.Join("\n",history)}";
        }
        string FirstTopicThisLoop=>State.conversations.Where(m=>m.partner==State.target&&m.loop==State.loop&&m.timeline==State.timeLeaps&&m.chapter==0&&m.topic!="第一印象"&&!string.IsNullOrEmpty(m.topic)).Select(m=>Clip(m.topic,12)).FirstOrDefault();
        public TalkAction[] FinalChoices(int theme)
        {
            var s=State;string lead=Lead;string me=I;
            string likeA=FinalPick(EnglishUI?"What I like: ":"好きなところ：",EnglishUI?"your smile":"笑顔",0),likeB=FinalPick(EnglishUI?"What I like: ":"好きなところ：",EnglishUI?"how you listen":"話を聞いてくれるところ",1);
            string tripTo=FinalPick(EnglishUI?"Together: ":"一緒にしたい：",EnglishUI?"a town by the sea":"海の見える街",0);
            string first=FirstTopicThisLoop??(EnglishUI?"what you love":"好きなもの");
            string meet=s.firstApproach=="compliment"?(EnglishUI?$"The day we met I told you our eyes met and you looked beautiful -- I meant it, and I still do.":$"出会った日、目が合ってきれいだと言ったのは本気だった。今も、その気持ちのまま好きでいる。")
                :s.firstApproach=="greet"?(EnglishUI?$"The day we met I could barely say hello. Even then, {lead}, I think I already liked you.":$"出会った日はこんにちはと言うのが精一杯だった。あの時からもう、{lead}さんのことが好きだったんだと思う。")
                :(EnglishUI?$"The day we met I gave you my name and my heart was pounding. From that day, {lead}, I've been on the edge of liking you.":$"出会った日、名乗りながら胸が鳴っていた。あの日から、{lead}さんを好きになる入口にいた気がする。");
            string[] ja={
                meet,
                $"最初の会話で「{first}」の話をしたね。あの時から、{lead}さんの話し方が好きだった。",
                $"正直、最初は緊張で名前もうまく言えなかった。でも今は、{lead}さんの隣が一番落ち着く。",
                $"{lead}さんの{likeA}が好き。気づいてた？",
                $"みんなといる時の{lead}さんも好きだけど、ふたりの時の{lead}さんはもっと好き。",
                $"{lead}さんの{likeB}、ずっと見てた。それが好きで、ここまで来た。",
                $"番組が終わったら、{lead}さんと{tripTo}に出かけたい。時間を気にせず、ふたりで。",
                $"{lead}さんの誕生日は、海辺で小さなサプライズパーティーにしよう。準備は{me}が全部やるから、好きなだけ笑ってて。",
                $"来年の今日も、{lead}さんと一緒に笑っていたい。それが{me}の、一番の未来。",
                $"{lead}さん、好きです。番組の続きじゃなくて、ふたりの続きを一緒に作ってほしい。",
                $"百時間、ずっと{lead}さんを見てた。好きだよ。この言葉だけは、ちゃんと届いてほしい。",
                $"答えは急がなくていい。でも{me}の気持ちは決まってる。{lead}さんが好き。"};
            string[] en={
                meet,
                $"Our first conversation was about {first}. I liked the way you talk from that moment, {lead}.",
                $"Honestly, I was so nervous at first I could barely say my name. Now, {lead}, beside you is where I'm calmest.",
                $"I like {likeA}, {lead}. Had you noticed?",
                $"I like you when everyone's around, {lead}, but I like you even more when it's just us.",
                $"I kept watching {likeB}. Liking that is what brought me here.",
                $"When the show ends, I want to head out for {tripTo} with you, {lead}. Just the two of us, no clock.",
                $"For your birthday, {lead}, let's do a small surprise party by the sea. I'll handle everything; you just laugh as much as you like.",
                $"A year from today, I want to still be laughing with you, {lead}. That's the future I want most.",
                $"{lead}, I like you. Not a sequel to the show -- I want us to write what comes next together.",
                $"For a hundred hours I never stopped watching you, {lead}. I like you. Let this one line reach you.",
                $"You don't have to answer now. But my heart is set, {lead}. I like you."};
            var lines=EnglishUI?en:ja;int t=Math.Clamp(theme,0,3);string themeName=FinalTheme(t);
            string[] ids={"final_past","final_like","final_future","final_word"};
            int basePower=t==3?10:t==1?8:7;
            return new[]{
                new TalkAction{id=ids[t]+"_a",line=lines[t*3],tag="empathy",basePower=basePower,goal=themeName},
                new TalkAction{id=ids[t]+"_b",line=lines[t*3+1],tag="creation",basePower=basePower,goal=themeName},
                new TalkAction{id=ids[t]+"_c",line=lines[t*3+2],tag="empathy",basePower=basePower,goal=themeName}
            };
        }
        public string FinalThemeTip(int theme)
        {
            string lead=Lead;bool h=RomanticLeadProfiles.IsHikari(Content.Person("yuto"));
            string[] ja={
                $"出会った日に感じたことを、そのまま言葉にしましょう。{(h?"彼女":"彼")}は、あの日のあなたを覚えています。",
                $"{lead}さんのどこが好きか、具体的に一つ。『好き』という言葉を、ちゃんと入れて伝えるのです。",
                "未来のことを話すとイメージが湧きます。ふたりで行く旅行や、どんな誕生日パーティーにするか、を口にしてみましょう。",
                "最後は、ちゃんと言葉で伝えることが大切です。『好き』を含めた一言で締めましょう。"};
            string[] en={
                $"Say what you felt the day you met, as it was. {(h?"She":"He")} remembers you from that day.",
                $"Name one specific thing you like about {lead}, and actually use the word \"like\".",
                "Talking about the future makes it real. Put a trip for the two of you, or the birthday party you'd throw, into words.",
                "In the end, saying it out loud is what matters. Close with a line that includes \"like\"."};
            return (EnglishUI?en:ja)[Math.Clamp(theme,0,3)];
        }
    }
}
