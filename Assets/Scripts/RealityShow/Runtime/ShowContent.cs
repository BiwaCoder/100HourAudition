using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HundredHour.RealityShow
{
    [Serializable] public sealed class ShowCharacter
    {
        public string id, displayName, role, personality;
        public string callName;
        public Sprite portrait;
        public Color accent = new Color(1,.48f,.42f);
        public string[] topics;
    }
    [Serializable] public sealed class ShowCard
    {
        public string id, title, description;
        public int cost=1;
        public string archetype,combo,trigger,trap,weather,topicSeed,topicTag;
        public int rarity,weight=10,power,warmth,wisdom,wisdomCost,agiCost,initiative,shield,sabotage,vulnerability,lingering,duration,comboPower,triggerPower,trapPower;
        public bool generate;
    }
    [CreateAssetMenu(menuName="100Hour/Reality Show Content")]
    public sealed class ShowContent : ScriptableObject
    {
        [NonSerialized] public bool customParticipant;
        [NonSerialized] public string playerGender="female";
        public string AdaptText(string text)
        {
            if(string.IsNullOrEmpty(text))return text;
            // The participant's own name (e.g. 久保田悠斗) must survive untouched: it may contain a
            // canonical cast name (悠斗) that would otherwise be swapped for the lead's name.
            string self=Person("himari")?.displayName;bool renamed=customParticipant||(!string.IsNullOrEmpty(self)&&self!="ひまり");
            if(!renamed)self=null;
            const string guard="\u0001";
            bool guarded=!string.IsNullOrEmpty(self)&&text.Contains(self);
            if(guarded)text=text.Replace(self,guard);
            if(renamed)text=Swap(text,"ひまり",self);
            if(playerGender!="male")text=Swap(Swap(text,"星野ひかり",Person("hikari").displayName),"ひかり",Person("hikari").displayName);
            else text=Swap(Swap(Swap(Swap(text,"松村悠斗",Person("yuto").displayName),"悠斗",Person("yuto").displayName),"雨宮ののか",Person("konoa").displayName),"ののか",Person("konoa").displayName)
                .Replace("ガール","ボーイ").Replace("ライバル女子","ライバル男子");
            return guarded?text.Replace(guard,self):text;
        }
        // Skips no-op swaps and swaps whose target already contains the source (星野ひかり ⊃ ひかり),
        // which would otherwise stack the surname twice.
        static string Swap(string text,string from,string to)=>string.IsNullOrEmpty(to)||from==to||to.Contains(from)?text:text.Replace(from,to);
        public List<ShowCharacter> cast=new List<ShowCharacter>();
        public List<ShowCard> cards=new List<ShowCard>();
        public ShowCharacter Person(string id)=>cast.Find(p=>p.id==id);
        public ShowCard Card(string id)=>cards.Find(c=>c.id==id);
        public static void Populate(ShowContent content)
        {
            content.cast=new List<ShowCharacter> {
                new ShowCharacter{id="himari",displayName="ひまり",role="YOU / 22",personality="童話と小さな生き物を愛する、素直で夢見がちな参加者。",topics=new[]{"押し花","童話","小さな生き物"}},
                new ShowCharacter{id="konoa",displayName="雨宮ののか",role="IDOL / 23",personality="場を明るくするアイドル。カメラの外では努力を認めてほしい。欲望：本当は誰よりもそばにいたい。秘密：大勢に好かれる仕事だからこそ「一番」になるのが怖くて、特別扱いされるとつい確かめるように距離を詰めてしまう。",topics=new[]{"舞台袖の緊張","歌の練習","青いリボン"}},
                new ShowCharacter{id="hikari",displayName="星野ひかり",role="PERFORMER / 22",personality="華やかで人懐っこい。自分の可愛さを自覚していて、話し方や仕草でそれを活かそうとする。相手の名前を聞いたらすぐあだ名で呼びたがる。「えー！！」「わー！」「きゃー」など感情豊かな擬音語から話し始めることが多い。時々ズレた発言でツッコミを誘い、自分を素直に褒めたり可愛さをアピールする。相手の趣味や好きなものを見つけると積極的に褒める。笑顔の裏に不安を隠している。欲望：私だけを見てほしい。秘密：可愛く振る舞うほど本命として見られているか不安になり、つい張り合うような一言を挟んでしまう。",topics=new[]{"雨の日のカフェ","旅先の写真","初舞台の失敗"}},
                new ShowCharacter{id="shiori",displayName="神埼沙織",role="CURATOR / 25",personality="知的で慎重。結論よりも考えた過程を大切にする。欲望：同じくらい大切にしてほしい、対等でいたい。秘密：誤解されるのが何より怖いから、慎重に言葉を選ぶ――本当は誰かに深く理解されたい。",topics=new[]{"未完成の展示","古い絵本","静かな図書室"}},
                new ShowCharacter{id="yuto",displayName="松村悠斗",role="CREATOR / 28",personality="穏やかで情熱的なゲームクリエイター。自分の作品にかけた想いを語る時は熱がこもる。相手の感性に共感を示し、繊細な表現と率直な本音を使い分ける。相手の話や趣味には真剣に耳を傾け、素直に感動を言葉にする。創作の迷いも隠さず共有したい。",topics=new[]{"未完成のゲーム","雨音の記憶","絵本の結末","美術館の迷路","小さな喫茶店"}}
            };
            string[][] definitions={
                new[]{"listen","静かな紅茶","緊張を6防ぎ、次の一言に信頼+2。"},
                new[]{"story","物語の糸","創作の話なら信頼+7＋同調Lv。別の話では+1。"},
                new[]{"purity","純粋さのかけら","純粋さ+1。3個以上なら誠実な会話に信頼+6。"},
                new[]{"rain","雨宿りの約束","次の一言に信頼+3。雨なら+10。"},
                new[]{"echo","口調の残響","同調Lv+1（周を越えて残る）。緊張を2防ぐ。"},
                new[]{"draw","好奇心の窓","追加で2枚引く。次の一言に信頼+1。"},
                new[]{"rest","深呼吸","緊張−10。落ち着いて、言葉を選び直す。"},
                new[]{"spotlight","スポットライト","SNSスター+12K。次の発言は緊張+5。"},
                new[]{"memory","記憶の栞","記憶を使った一言なら信頼+7。別の話では+1。"},
                new[]{"wind","風のリボン","風なら信頼+8。視点も使っていればさらに+5。通常+2。"},
                new[]{"confess","禁断の告白","信頼35以上か純粋さ2以上なら信頼+18。条件未達ならこの周は脱落。"},
                new[]{"blackout","闇の賭け","停電なら信頼+20。条件未達ならこの周は脱落。運命の棘+1。"}
            };
            content.cards=definitions.Select(d=>new ShowCard{id=d[0],title=d[1],description=d[2],cost=d[0]=="confess"||d[0]=="blackout"?2:1}).ToList();
        }
    }
}
