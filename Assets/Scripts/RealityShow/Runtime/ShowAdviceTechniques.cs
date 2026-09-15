using System;
using System.Linq;
using HundredHour.Localization;

namespace HundredHour.RealityShow
{
    public sealed partial class ShowGame
    {
        sealed class AdviceTechnique
        {
            public string id,topic,line,reply,lesson;
            public AdviceTechnique(string id,string topic,string line,string reply,string lesson)
            {this.id=id;this.topic=topic;this.line=line;this.reply=reply;this.lesson=lesson;}
        }
        AdviceTechnique[] AdviceTechniques()
        {
            var person=Content.Person(State.target);
            if(!RomanticLeadProfiles.IsLead(person))return Array.Empty<AdviceTechnique>();
            bool h=RomanticLeadProfiles.IsHikari(person);
            string prefix=h?"hikari_":"yuto_";
            if(GameLanguage.Current==GameLocale.English)return EnglishTechniques(h,prefix);
            var basics=h?new[]{
                new AdviceTechnique("hikari_hair","髪型","ふんわりした髪型、よく似合っています。巻き方で大切にしていることはありますか？","わー！ 気づいてくれたの嬉しい♪ ふわっと見えるように、顔まわりを少しずつ巻いてるんだ。","見た目だけでなく、本人が工夫したところを具体的に褒めてみましょう。"),
                new AdviceTechnique("hikari_nails","ネイル","おしゃれを大切にしているところ、素敵ですね。ネイルも楽しみますか？ 色を選ぶときのこだわりを聞きたいです。","え、教えていいの？ ネイルも好き！ お洋服と合わせて色を考える時間が楽しいんだ～。","見えていないネイルの色は決めつけず、まず楽しんでいるか聞いてみましょう。"),
                new AdviceTechnique("hikari_clothes","服の素材","レースの雰囲気が素敵ですね。服を選ぶときは、デザインと着心地、どちらを大切にしていますか？","きゃー♪ レースって細かい模様まで可愛いよね！ でも着心地も大切。楽しく過ごせるお洋服が好き！","持ち物の値段より、選んだ理由や心地よさに目を向けましょう。"),
                new AdviceTechnique("hikari_effort","おしゃれの工夫","似合うものを日々研究しているところ、素敵だと思います。最近試して楽しかった工夫はありますか？","わー、頑張ってるところまで見てくれるんだ！ 最近は髪型とアクセサリーの組み合わせを研究中なの♪","かわいさを支える努力を褒め、本人の新しい挑戦を聞いてみましょう。")
            }:new[]{
                new AdviceTechnique("yuto_clothes","服のこだわり","落ち着いた服の合わせ方が素敵ですね。色や素材を選ぶとき、どんなところを大切にしていますか？","ありがとうございます。落ち着いた色を中心に、着ていて自然に過ごせる素材を選んでいます。","服そのものだけでなく、組み合わせや選び方を褒めてみましょう。"),
                new AdviceTechnique("yuto_camera","写真とカメラ","写真で日常の魅力を見つける趣味、素敵ですね。カメラを選ぶなら、どんな撮り心地を大切にしますか？","写真は、いつもの景色を見直すきっかけになりますね。カメラは持ち歩きやすく、撮りたい瞬間に自然に使えるものが好きです。","機種や所有を決めつけず、撮りたいものや道具の選び方を聞きましょう。"),
                new AdviceTechnique("yuto_work","ゲーム制作の工夫","ゲーム作りで新しいものと伝統の両方を大切にする姿勢、素敵ですね。仕事で特に工夫していることは何ですか？","ありがとうございます。新しい仕組みでも、触れる人が自然に理解できることを大切にしています。その両立を考える時間が面白いですね。","肩書きだけでなく、仕事への姿勢や工夫を具体的に褒めましょう。"),
                new AdviceTechnique("yuto_car","車と出かけ方","展示や写真を楽しむ時間の使い方が素敵ですね。出かけるときは車も使いますか？ 移動の仕方にもこだわりはありますか？","展示を見たあとに少し歩いて、街の表情を探すのも好きです。移動はその日の目的に合わせて、余裕を持てる方法を選びたいですね。","車好き・所有者とは決めつけず、出かけ方から好みを確かめましょう。")
            };
            return basics.Concat(new[]{
                new AdviceTechnique(prefix+"future","好きから仕事・起業へ",
                    h?"おしゃれやカフェへのこだわり、誰かを喜ばせる仕事にもつながりそうですね。小さなお店を始めるなら、どんな空間にしてみたいですか？":"ゲームと写真へのこだわりを、小さな新しい事業にもつなげられそうですね。もし自分のスタジオで自由な企画を始めるなら、何を作ってみたいですか？",
                    h?"わー、考えたことなかった！ 好きな服やスイーツに囲まれて、来た人が笑顔になれる空間にしたいな♪":"面白いですね。写真で見つけた日常の美しさを、遊びながら発見できる作品にしてみたいです。",
                    "好きなこと→本人の工夫→誰に届けたいか、と未来を広げましょう。仕事にするかどうかは本人の希望を聞きます。"),
                new AdviceTechnique(prefix+"scenery","魅力と景色を結ぶ",
                    h?"その美しい髪、海辺の夕暮れの光にぴったりだと思います。ふたりで散歩するなら、どんな景色を見てみたいですか？":"落ち着いた服の色、この海の夕暮れにもよく似合いそうですね。ふたりで写真を撮るなら、どんな景色を残したいですか？",
                    h?"きゃー♪ そんなふうに想像してくれるの嬉しい！ 波がきらきらしてるところを、ゆっくり一緒に歩きたいな。":"ありがとうございます。夕暮れの柔らかい光を残したいですね。写真のあとに、同じ景色を眺める時間も楽しそうです。",
                    "具体的な魅力に、場所・光・一緒の過ごし方を重ねます。最後は相手が思い描く景色を聞きましょう。"),
                new AdviceTechnique(prefix+"travel","好きから世界旅行へ",
                    h?"カフェ巡りや旅行が好きなところ、素敵ですね。もし世界を旅するなら、まずどの国のカフェに行ってみたいですか？":"写真と美術館が好きなところ、素敵ですね。もし世界を旅するなら、どの街の美術館やカフェを撮ってみたいですか？",
                    h?"えー！ 行きたい行きたい！ パリのカフェでクロワッサン食べて、その次はイタリアでジェラート巡りかな♪":"ありがとうございます。まずはパリの小さな美術館と、その近くのカフェを撮ってみたいですね。リスボンの光も気になります。",
                    "好きなこと→行ってみたい場所→一緒に見たい景色、と旅の想像を広げましょう。行き先は本人に選んでもらいます。"),
                new AdviceTechnique(prefix+"family","家族との休日を描く",
                    h?"人を笑顔にするのが得意なところ、素敵ですね。いつか家族ができたら、どんな休日を過ごしてみたいですか？":"相手の話を丁寧に聞くところ、素敵ですね。いつか家族ができたら、どんな休日を過ごしてみたいですか？",
                    h?"わー、想像しちゃった♪ 日曜の朝にみんなでパンケーキを焼いて、公園でシャボン玉とか、そういうのがいいな。":"ありがとうございます。朝は一緒にパンを焼いて、午後は近くの公園で写真を撮る。そんな静かな休日がいいですね。",
                    "本人の優しさを褒め、家族との休日の過ごし方を聞いてみましょう。結婚や子どもを前提と決めつけず、本人の希望として尋ねます。"),
                new AdviceTechnique(prefix+"wedding","海辺の結婚式を想像する",
                    h?"レースの似合うあなたなら、海辺の式もきっと絵になりますね。もし結婚式を挙げるなら、どんな場所がいいですか？":"落ち着いた色の似合うあなたなら、海辺の式も絵になりそうですね。もし結婚式を挙げるなら、どんな場所がいいですか？",
                    h?"きゃー♪ 海沿いの白いチャペルで、夕日を見ながらがいいな！ ドレスは絶対レースがいい！":"海辺の小さなチャペルで、夕暮れに。写真も自分たちで一枚残せたら嬉しいですね。",
                    "似合う装いから、憧れの場所へ想像を広げましょう。結婚式は本人の憧れとして聞き、押しつけません。")
            }).ToArray();
        }
        // Same ids as the Japanese set, so learned/used technique state carries across a language switch.
        static AdviceTechnique[] EnglishTechniques(bool h,string prefix)
        {
            var basics=h?new[]{
                new AdviceTechnique("hikari_hair","Hairstyle","That soft, wavy hairstyle really suits you. Is there something you pay special attention to when you curl it?","Wow! I'm so happy you noticed! I curl the strands around my face little by little so it looks fluffy.","Compliment not just the look but the specific effort they put in."),
                new AdviceTechnique("hikari_nails","Nails","I love how much you care about style. Do you enjoy doing your nails too? I'd like to hear how you choose the colors.","Oh, can I tell you? I love nails! Picking colors to match my outfits is such a fun time!","Don't assume a nail color you can't see; first ask whether they enjoy it."),
                new AdviceTechnique("hikari_clothes","Fabric","The lace has such a lovely feel. When you choose clothes, which matters more, the design or the comfort?","Eek! Lace is so cute right down to the tiny patterns! But comfort matters too. I love clothes I can have fun in!","Look at why they chose it and how it feels, not at the price tag."),
                new AdviceTechnique("hikari_effort","Style effort","I think it's wonderful how you keep studying what suits you. Any recent experiment you enjoyed?","Wow, you even notice the effort! Lately I'm experimenting with hairstyle and accessory combinations!","Praise the effort behind the cuteness, and ask about their newest challenge.")
            }:new[]{
                new AdviceTechnique("yuto_clothes","Style choices","The calm way you put your outfit together is lovely. What do you value when choosing colors and fabrics?","Thank you. I lean toward calm colors and fabrics that feel natural to wear.","Compliment the combination and the choices, not just the clothes themselves."),
                new AdviceTechnique("yuto_camera","Photos and cameras","Finding beauty in everyday life through photos is a lovely hobby. What feel do you look for in a camera?","Photos give me a reason to look at familiar scenery again. I like a camera I can carry easily and use naturally the moment I want to.","Don't assume gear or ownership; ask what they want to shoot and how they choose tools."),
                new AdviceTechnique("yuto_work","Craft in game-making","I admire how you value both the new and the traditional in game-making. What do you put the most care into at work?","Thank you. Even with a new mechanic, I want anyone to understand it naturally. Balancing those two is the fun part.","Praise their attitude and specific craft, not just the job title."),
                new AdviceTechnique("yuto_car","Cars and outings","I love how you spend time on exhibitions and photos. Do you drive when you go out? Any preference in how you travel?","After an exhibition I like to walk a little and look for the town's expressions. I choose whatever way of getting there leaves room to breathe.","Don't assume they own or love cars; learn their preferences from how they go out.")
            };
            return basics.Concat(new[]{
                new AdviceTechnique(prefix+"future","From passion to work",
                    h?"Your eye for style and cafés could turn into work that makes people happy. If you opened a small shop, what kind of space would you create?":"Your care for games and photography could grow into a small venture of its own. If you started a free project at your own studio, what would you make?",
                    h?"Wow, I never thought about it! I'd love a place full of my favorite clothes and sweets, where everyone leaves smiling!":"Interesting. I'd like to make something where players discover the everyday beauty I find in photos.",
                    "Widen the future: what they love, how they refine it, who they want to reach. Ask whether they'd want to make it a job."),
                new AdviceTechnique(prefix+"scenery","Tie charm to scenery",
                    h?"Your beautiful hair would look perfect in the evening light by the sea. If we took a walk together, what scenery would you want to see?":"The calm colors you wear would suit this seaside sunset. If we took a photo together, what scenery would you want to keep?",
                    h?"Eek! I love that you imagine that! I'd want to walk slowly along the sparkling waves with you.":"Thank you. I'd want to keep the soft evening light. Looking at the same view after the photo sounds lovely too.",
                    "Layer a place, light, and time together onto a specific charm. End by asking what scenery they picture."),
                new AdviceTechnique(prefix+"travel","From passion to world travel",
                    h?"It's lovely that you enjoy cafés and travel. If you traveled the world, which country's café would you visit first?":"It's lovely that you enjoy photos and galleries. If you traveled the world, which city's galleries and cafés would you want to photograph?",
                    h?"Ooh, I want to go! A croissant at a café in Paris, then gelato-hopping in Italy!":"Thank you. First a small gallery in Paris and the café next to it. The light in Lisbon intrigues me too.",
                    "Widen the journey: what they love, where they'd go, what you'd see together. Let them pick the destination."),
                new AdviceTechnique(prefix+"family","Picture a family weekend",
                    h?"You're so good at making people smile. If you had a family someday, what would your ideal weekend look like?":"I love how carefully you listen. If you had a family someday, what would your ideal weekend look like?",
                    h?"Wow, now I'm picturing it! Pancakes together on Sunday morning, then bubbles in the park. Something like that.":"Thank you. Baking bread together in the morning, then photos at the nearby park. A quiet weekend like that.",
                    "Praise their kindness and ask how they'd spend a family weekend. Don't assume marriage or children; ask as their own wish."),
                new AdviceTechnique(prefix+"wedding","Imagine a seaside wedding",
                    h?"Lace suits you so well that a seaside ceremony would look like a painting. If you had a wedding, where would you want it?":"Calm colors suit you so well that a seaside ceremony would look like a painting. If you had a wedding, where would you want it?",
                    h?"Eek! A white chapel by the sea, watching the sunset! And the dress absolutely has to be lace!":"A small chapel by the sea, at dusk. It would be nice to take one photo of it ourselves too.",
                    "Move from the outfit that suits them to the place they dream of. Ask about a wedding as their wish, never as pressure.")
            }).ToArray();
        }
        public string LearnComebackTechnique()
        {
            if(State.phase!=ShowPhase.Eliminated)return "";
            string key=State.loop+":"+State.chapter+":"+State.timeLeaps+":"+State.target;
            // Pick from every advanced technique still unlearned, so successive defeats teach different
            // futures (work, scenery, travel, family, wedding) instead of always starting with work.
            var pool=AdviceTechniques().Skip(4).Where(t=>!State.learnedTechniques.Contains(t.id)).ToArray();
            var next=pool.Length==0?null:pool[Next(pool.Length)];
            bool english=GameLanguage.Current==GameLocale.English;
            if(State.comebackReviews.Contains(key)||next==null)return english?"Try the technique you learned this time in the next attempt. Add their name and connect a memory to their reply, and the conversation opens up further.":"今回覚えたテクニックを、次の挑戦で試してみましょう。名前を添え、相手の返答から記憶をつなぐと、さらに話が広がります。";
            State.comebackReviews.Add(key);State.learnedTechniques.Add(next.id);
            State.techniqueNotice=english?$"Learned the advanced technique \"{next.topic}\"!\n{next.lesson}\nA strong choice is added to the next attempt. First-time bonus +12.\nExample: \"{next.line}\""
                :$"応用テク「{next.topic}」を覚えました！\n{next.lesson}\n次の挑戦に強い選択肢が追加されます。初回ボーナス+12。\n例：「{next.line}」";
            return State.techniqueNotice;
        }
        public string LearnAdviceTechnique()
        {
            var available=AdviceTechniques().Take(4).ToArray();
            int allowance=Math.Min(available.Length,1+RecallMemories().Count/2);
            var next=available.Take(allowance).FirstOrDefault(t=>!State.learnedTechniques.Contains(t.id));
            bool english=GameLanguage.Current==GameLocale.English;
            if(next==null){State.techniqueNotice=english?"Techniques you've learned can be used from the choices. Keep talking, then check in again.":"覚えたテクニックは選択肢で使えます。会話を重ねたら、また相談してみましょう。";return State.techniqueNotice;}
            State.learnedTechniques.Add(next.id);
            AppendLearnedTechniques();
            // One gentle sentence, phrased for the lead's gender; the new choice simply appears in the list.
            bool femaleLead=RomanticLeadProfiles.IsHikari(Content.Person("yuto"));
            State.techniqueNotice=english
                ?$"{(femaleLead?"Women":"Men")} love it when you notice their {next.topic}. Why not bring it up? (Added to your choices.)"
                :$"{(femaleLead?"女性":"男性")}は{next.topic}について触れられるとうれしいのです。話題に使ってみてはどうですか？（選択肢に追加しました）";
            return State.techniqueNotice;
        }
        void AppendLearnedTechniques()
        {
            if(GroupTalk||FinalTalk)return; // themed rounds have their own lines
            // Once its one-time bonus is spent, a technique choice stays in the list forever with
            // the exact same line and no reward -- exactly the "same line, no progress" complaint.
            // Consume it: once used, it drops out of the choices instead of lingering as a dupe.
            foreach(var t in AdviceTechniques().Where(t=>State.learnedTechniques.Contains(t.id)&&!State.usedTechniques.Contains("tech_"+t.id)))
                if(!State.actions.Any(a=>a.id=="tech_"+t.id))
                    State.actions.Add(new TalkAction{id="tech_"+t.id,line=WithName(t.line),tag="empathy",basePower=IsAdvanced(t.id)?8:6});
        }
        public int TechniqueBonus(TalkAction action)=>action!=null&&
            AdviceTechniques().Any(t=>"tech_"+t.id==action.id&&State.learnedTechniques.Contains(t.id))&&
            !State.usedTechniques.Contains(action.id)?(IsAdvanced(action.id)?12:6):0;
        static readonly string[] AdvancedSuffixes={"_future","_scenery","_travel","_family","_wedding"};
        static bool IsAdvanced(string id)=>AdvancedSuffixes.Any(id.EndsWith);
        public string TechniquePreview(TalkAction action)
        {
            var t=AdviceTechniques().FirstOrDefault(t=>"tech_"+t.id==action.id);
            if(t==null)return "";
            bool english=GameLanguage.Current==GameLocale.English;
            return english?$"[Technique: {t.topic}{(TechniqueBonus(action)>0?$" first-time+{TechniqueBonus(action)}":"")}] ":$"［テク：{t.topic}{(TechniqueBonus(action)>0?$" 初回+{TechniqueBonus(action)}":"")}］ ";
        }
        public ShowReply TechniqueReply(TalkAction action)
        {
            var t=AdviceTechniques().FirstOrDefault(t=>"tech_"+t.id==action.id);
            return t==null?null:new ShowReply{reply=t.reply,topic=t.topic,followup=GameLanguage.Current==GameLocale.English?"Could you tell me what first made you care about that?":"そのこだわりが生まれたきっかけも、聞かせてもらえますか？",facts=Array.Empty<ShowFact>()};
        }
    }
}
