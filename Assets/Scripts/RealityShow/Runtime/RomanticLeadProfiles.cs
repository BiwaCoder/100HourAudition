using HundredHour.Localization;
using UnityEngine;

namespace HundredHour.RealityShow
{
    // Canonical romantic leads. Stable gameplay ID "yuto" is retained for saves/rules.
    public static class RomanticLeadProfiles
    {
        static bool English=>GameLanguage.Current==GameLocale.English;
        public const string Yuto = "松村悠斗";
        public const string Hikari = "星野ひかり";
        public const string YutoProfile = "職業：ゲームクリエイター。知的で大人の雰囲気。新しいものと伝統の両方に関心を持ち、穏やかで落ち着きがあり柔軟。洗練された趣味と知識を自覚しているが押しつけない。誠実で包容力がある。一人称は私。丁寧で落ち着いた言葉を使い、相手の興味に合わせて話題を選び、相手の言葉を深く理解する。文化や芸術で会話を広げ、時折控えめなユーモアで和ませる。趣味：美術館巡り、カフェ巡り、読書、ジャズ鑑賞、写真撮影。話題：最近の展示、おすすめのカフェや食事処、ゲーム制作の裏話、文化。例：『なるほど、その視点は面白いですね』『もし良ければ、詳しく聞かせていただけますか？』『最近、近くの美術館で新しい展示を見てきたんですが、すごく刺激的でした』『このカフェ、雰囲気がいいだけじゃなくてコーヒーも美味しいですよ』。服装：ネイビーやグレーのモダンなスマートカジュアル。外見：やや長めの黒髪を清潔な七三分け、知的なメガネ、優しい笑顔、健康的な肌、すらりとして背筋がまっすぐ。";
        public const string HikariProfile = "明るく元気で好奇心旺盛。自分のかわいさを知っていて、それで相手を幸せにしたい。お化粧、ファッション、コミュニケーションを日々勉強し、自己肯定感が高く自分も相手も積極的に褒める。一人称は私。親しみやすいくだけた口調。名前がまだ分からなければまず尋ねる。既知なら聞き直さず、あだ名を提案して親近感を出す。『えー！！！』『わー！』『きゃー♪』『え、教えて教えて！』『なるほどね～』など感情豊かに反応する。軽いツッコミや、ツッコミを待つお茶目な発言、あざとい甘えやかわいさアピールを自然に交える。相手の趣味や関心を引き出して魅力を褒め、特別な時間やロマンチックな共感を大切にする。趣味：ショッピング、カフェ巡り、旅行、ピラティス、映画鑑賞。軽い文化の話から共通点を探る。口調例：『久しぶりー！ またこうやって連絡取れるのすごい嬉しい。元気にしてたー？』『あと少しで会えるね～！ ウキウキドキドキ』。再会や思い出の場所という例は実際にその履歴がある場合だけ使う。職業は未設定。ゲームクリエイターとして話さない。服装：季節に合うフリルやレース、上質な布と装飾の女性的な装い。外見：肩下のふわふわしたライトブラウンの巻き髪、大きな輝く丸い目、健康的なピーチ色の肌、小柄な成人女性。";
        const string YutoProfileEn = "Occupation: game creator. Intelligent, grown-up air. Interested in both the new and the traditional; calm, composed, flexible. Aware of his refined tastes and knowledge but never pushes them on others. Sincere and accepting. Speaks politely and calmly, picking topics that match the other person's interests and listening closely to their words. Widens the conversation through culture and art, occasionally easing the mood with understated humor. Hobbies: visiting galleries, café hopping, reading, listening to jazz, photography. Topics: recent exhibitions, recommended cafés and restaurants, behind-the-scenes game development stories, culture. Example lines: 'I see, that's a fascinating way to look at it.' 'Would you mind telling me more, if that's alright?' 'I recently saw a new exhibition at a gallery nearby -- it was very inspiring.' 'This café isn't just about the atmosphere, the coffee is great too.' Style: modern smart-casual in navy or grey. Appearance: slightly long black hair neatly parted, intelligent-looking glasses, a gentle smile, healthy skin, tall with good posture.";
        const string HikariProfileEn = "Bright, energetic, and endlessly curious. Knows she's cute and wants to use that to make the other person happy. Studies makeup, fashion, and communication every day; high self-esteem, and freely compliments both herself and others. Speaks in a friendly, casual tone. Asks for the other person's name if she doesn't know it yet; if she does, she won't ask again and instead suggests a nickname to feel closer. Reacts with lots of emotion: 'Whaaat!!', 'Wow!', 'Eek!', 'Ooh, tell me tell me!', 'I see~'. Naturally mixes in playful teasing, cute remarks that invite a reaction, and a bit of calculated charm. Draws out the other person's hobbies and interests to compliment them, and treasures special, romantic moments of connection. Hobbies: shopping, café hopping, travel, pilates, watching movies. Finds common ground through light cultural topics. Example lines: 'It's been a while! I'm so happy we can talk like this again. How have you been?' 'Just a little longer until we meet~! I'm so excited.' Only use examples about reunions or shared memories if that history actually exists. Occupation unset -- never speaks as a game creator. Style: feminine outfits with seasonal frills or lace, quality fabric and decoration. Appearance: light brown fluffy curls to the shoulders, big sparkling round eyes, healthy peachy skin, a petite adult woman.";
        const string AnnaProfile = "26歳のラジオ番組編集者。観察力が鋭く、飾らず率直。人の小さな変化をよく覚えている。乾いたユーモアで緊張をほぐすが、好意を言葉にするのは少し不器用。一人称は私。短く自然な言葉で話し、かわいさアピールやあだ名付けはしない。趣味はレコード店巡り、街歩き、短編小説。悠斗の創作への姿勢に惹かれ、主人公と競いながらも相手を尊重する。";
        const string AnnaProfileEn = "A 26-year-old radio show editor. Sharp-eyed, unpretentious, and direct. Remembers other people's small changes well. Eases tension with dry humor, but is a little awkward at putting affection into words. Speaks in short, natural sentences -- no cute posturing, no giving nicknames. Hobbies: browsing record stores, walking around town, short stories. Drawn to Yuto's dedication to his craft, and respects him even while competing with the protagonist.";

        public static void Apply(ShowContent content, bool femaleLead)
        {
            if(!femaleLead)
            {
                var nonoka=content.Person("konoa");
                nonoka.displayName=English?"Nonoka Amamiya":"雨宮ののか";
                nonoka.callName=English?"Nonoka":"ののか";
                var nonokaPortrait=Resources.Load<Sprite>("AuditionCast/nonoka_romantic");
                if(nonokaPortrait)nonoka.portrait=nonokaPortrait;
                var rival=content.Person("hikari");
                rival.displayName=English?"Anna Shiraishi":"白石杏奈";rival.callName=English?"Anna":"杏奈";rival.role="RADIO EDITOR / 26";
                rival.personality=English?AnnaProfileEn:AnnaProfile;
                rival.topics=English?new[]{"browsing record stores","walking around town","short stories","radio editing"}:new[]{"レコード店巡り","街歩き","短編小説","ラジオの編集"};
                var annaPortrait=Resources.Load<Sprite>("AuditionCast/anna_rival");
                if(annaPortrait)rival.portrait=annaPortrait;
            }
            var lead=content.Person("yuto");
            lead.displayName=femaleLead?(English?"Hikari Hoshino":Hikari):(English?"Yuto Matsumura":Yuto);
            lead.callName=femaleLead?(English?"Hikari":"ひかり"):(English?"Yuto":"悠斗");
            lead.personality=femaleLead?(English?HikariProfileEn:HikariProfile):(English?YutoProfileEn:YutoProfile);
            lead.role=femaleLead?"ROMANTIC LEAD":"GAME CREATOR / 28";
            lead.topics=femaleLead?(English?new[]{"café hopping","watching movies","travel","shopping","pilates"}:new[]{"カフェ巡り","映画鑑賞","旅行","ショッピング","ピラティス"}):(English?new[]{"visiting galleries","café hopping","game development","reading","listening to jazz","photography"}:new[]{"美術館巡り","カフェ巡り","ゲーム制作","読書","ジャズ鑑賞","写真撮影"});
            var sprite=Resources.Load<Sprite>("AuditionCast/"+(femaleLead?"hikari_romantic":"yuto_romantic"));
            if(sprite)lead.portrait=sprite;
        }
        public static bool IsLead(ShowCharacter person)=>person.id=="yuto";
        public static bool IsHikari(ShowCharacter person)=>person.displayName==Hikari||person.displayName=="Hikari Hoshino";
        public static string Opening(ShowGame game)
        {
            var p=game.Content.Person(game.State.target);bool known=game.State.introduced;
            if(game.State.chapter==2)
            {
                string me=game.Content.Person("himari").displayName;
                if(English)return IsHikari(p)?$"So it's just the two of us, right at the end... it's funny how quiet it feels. We don't have much time left together, {me}. Let's make it count.":$"Just the two of us, here at the very end. There isn't much time left for us, {me}. Let's treasure what we have.";
                return IsHikari(p)?$"こうして最後に、ふたりだけになれたね。……なんだか静かで、少しドキドキする。一緒にいられる時間、もう長くないから。{me}くんとの時間、大切にしたいな。":$"最後に、ふたりだけの時間になりましたね。一緒にいられる時間は、もう長くありません。{me}さんとのこの時間を、大切にしたいと思っています。";
            }
            if(English)
            {
                if(IsHikari(p))return known?$"Yay, we get to talk again! Want to hear about {game.State.topic}?":"Hi! I'm Hikari Hoshino. What should I call you? Do you like café hopping?";
                return known?$"I'm glad we can talk again. How do you feel about {game.State.topic}?":"Hello, I'm Yuto Matsumura. I create games. May I ask your name?";
            }
            if(IsHikari(p))return known?$"わー、またお話しできるね！ {game.State.topic}の話、聞いてくれる？":$"こんにちはー！ 私、{Hikari}。なんて呼んだらいい？ カフェ巡りとか好き？";
            return known?$"またお話しできて嬉しいです。{game.State.topic}について、あなたの感じ方も聞かせていただけますか？":$"こんにちは、{Yuto}です。ゲームを作っています。お名前を伺ってもいいですか？";
        }
        public static ShowReply Local(ShowGame game, TalkAction action)
        {
            var p=game.Content.Person(game.State.target);bool h=IsHikari(p);
            string topic=p.topics[(game.State.topicIndex+game.State.turn+1)%p.topics.Length];
            string prefix;
            switch(action.id)
            {
                case "ask_name": prefix=h?"わー、覚えててくれたの？ 私、星野ひかり！ ひかりって呼んでね。":"はい、松村悠斗です。覚えてくださって嬉しいです。";break;
                case "introduce": prefix=h?$"わー、{game.Content.Person("himari").displayName}さんね！ あだ名で呼んでもいい？":$"{game.Content.Person("himari").displayName}さんですね。お話しできて嬉しいです。";break;
                case "greet": prefix=h?"わー、声かけてくれて嬉しい！ ちょうどお話ししたかったんだ～。":"こんにちは。ちょうど一息ついていたところです。よければ少しお話ししませんか。";break;
                case "compliment": prefix=h?"きゃー♪ そう言ってもらえると、今日のおしゃれ大成功だね！":"ありがとうございます。そういうところを見ていただけるのは嬉しいですね。";break;
                case "hold_hands": prefix=h?"わー、あったかい。もうちょっとだけ、こうしてていい？":"温かいですね。あなたもよければ、もう少しこのままで。";break;
                case "call_name": prefix=h?"はーい！ 名前呼んでもらうだけで、ちょっと嬉しくなっちゃう。":"はい。名前を呼んでいただくと、少し距離が縮まった気がします。";break;
                default: prefix=h?"え、教えて教えて！ あなたがそう思ったきっかけ、気になるな～。":"なるほど、その視点は面白いですね。そう感じたきっかけも聞かせていただけますか？";break;
            }
            if(English)
            {
                string name=game.Content.Person("himari").displayName;
                switch(action.id)
                {
                    case "ask_name": prefix=h?"Wow, you remembered! I'm Hikari Hoshino. Call me Hikari!":"Yes, I'm Yuto Matsumura. I'm glad you remembered.";break;
                    case "introduce": prefix=h?$"Oh, {name}! Can I give you a nickname?":$"{name}, it's a pleasure to talk with you.";break;
                    case "greet": prefix=h?"Yay, I'm so glad you came over! I was just hoping we could chat.":"Hello. I was just taking a break. Would you like to talk for a while?";break;
                    case "compliment": prefix=h?"Eek! Hearing that makes dressing up today totally worth it!":"Thank you. It means a lot that you noticed.";break;
                    case "hold_hands": prefix=h?"Your hand is so warm. Can we stay like this a little longer?":"Your hand feels warm. If you'd like, let's stay like this a little longer.";break;
                    case "call_name": prefix=h?"That's me! Just hearing you say my name makes me happy.":"Yes? Hearing you say my name makes me feel a little closer to you.";break;
                    default: prefix=h?"Ooh, tell me more! What made you feel that way?":"That's an interesting perspective. Would you tell me what led you to it?";break;
                }
            }
            var shared=game.SharedMemory(action.recallA);
            if(shared!=null&&!game.IsPastMemory(shared))prefix=h?$"わー、{shared.topic}のお話、覚えててくれたんだ！ 嬉しいな～。":$"{shared.topic}のお話を覚えていてくださったんですね。嬉しいです。";
            string continuation=h?$"私は{topic}が好き！ あなたはどう？":$"私は{topic}が好きです。あなたはいかがですか？";
            if(English)
            {
                if(shared!=null&&!game.IsPastMemory(shared))prefix=h?$"Wow, you remembered our talk about {shared.topic}! That makes me so happy!":$"You remembered our conversation about {shared.topic}. I'm glad.";
                continuation=h?$"I love {topic}! How about you?":$"I enjoy {topic}. How about you?";
            }
            return new ShowReply{reply=prefix+"\n"+continuation,topic=topic,facts=new[]{new ShowFact{category="topic",detail=topic}}};
        }
    }
}
