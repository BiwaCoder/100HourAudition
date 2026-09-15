using System;
using System.Linq;
using System.Text;

namespace HundredHour.RealityShow
{
    public sealed partial class ShowGame
    {
        string RecordRivalConversation(ShowContestant rival,out int gain)
        {
            var s=State;bool h=RomanticLeadProfiles.IsHikari(Content.Person("yuto"));
            string topic=Clip(s.topic,24),line,response,technique;
            int style=(s.turn+(rival.id=="konoa"?0:rival.id=="hikari"?1:2))%4;
            switch(style)
            {
                case 0:
                    technique="気持ちを深く聞く";
                    line=$"「{topic}」のどんなところに惹かれるんですか？ きっかけも聞いてみたいです。";
                    response=h?$"え、聞いてくれるの嬉しい！ {topic}って、自分の好きなものを見つける時間が楽しいんだ♪":$"{topic}に触れると、普段と違う視点に気づけるのが面白いんです。";
                    break;
                case 1:
                    technique="自分の仕事とつなぐ";
                    var role=Content.Person(rival.id).personality;
                    string craft=role.Contains("俳優")?"演じるときの表情":role.Contains("写真")?"写真に残す瞬間":role.Contains("学芸")?"展示で伝えること":role.Contains("ラジオ")?"声で届ける言葉":"自分らしい表現";
                    line=$"「{topic}」の話を聞いて、{craft}にも通じる気がしました。大切にしたい感覚ってありますか？";
                    response=h?"わー、そのつなげ方おもしろい！ 私は、見てくれる人まで楽しくなる感じを大切にしたいな。":"その視点は面白いですね。私は、触れた人が自分なりに発見できる余白を大切にしたいです。";
                    break;
                case 2:
                    technique="一緒の景色を想像する";
                    line=h?"そのふんわりした髪、海辺の光にも似合いそうですね。散歩するなら、どんな時間帯が好きですか？":"写真がお好きなら、この海の夕暮れは撮ってみたくなりませんか？ どんな瞬間を残したいですか？";
                    response=h?"きゃー♪ 夕暮れの海って憧れる！ 風が気持ちいい時間に、ゆっくり歩きたいな。":"光が水面に残る瞬間を撮ってみたいですね。急がず眺める時間も含めて楽しみたいです。";
                    break;
                default:
                    technique="好きから未来を広げる";
                    line=h?"おしゃれへのこだわり、誰かに届ける仕事にもつながりそうですね。小さなお店を作るなら、どんな雰囲気にしたいですか？":"ゲームと写真へのこだわりを、一つの作品にできたら面白そうですね。小さな企画を始めるなら、何を伝えたいですか？";
                    response=h?"わー、想像すると楽しい！ 入っただけで気分が明るくなる、可愛い場所にしたいな♪":"良い問いですね。何気ない景色にも物語がある、と感じてもらえる企画を考えてみたいです。";
                    break;
            }
            // Same values as the player's technique choices: basic 6, advanced 8, and the no-memory understanding gain of 1.
            gain=style==2||style==3?8:6;int insight=1;
            int before=rival.understanding;rival.understanding=Math.Min(100,before+insight);
            var beat=new RivalConversationBeat{rival=rival.id,line=line,reply=response,technique=technique,loop=s.loop,chapter=s.chapter,turn=s.turn,timeline=s.timeLeaps,trustGain=gain,understandingGain=rival.understanding-before};
            s.rivalConversations.Add(beat);
            return FormatRivalBeat(beat);
        }
        string FormatRivalBeat(RivalConversationBeat b)=>
            $"{(English?"Talk":"会話")}{b.turn+1}・{b.technique}\n{Content.Person(b.rival).displayName}「{b.line}」\n{Content.Person("yuto").displayName}「{b.reply}」\n{(English?"Affection":"好感度")} +{b.trustGain} ／ {(English?"Understanding":"相互理解")} +{b.understandingGain}";
        static string MeritLine(ShowContestant c)=>English
            ?$"score (trust) {c.Merit}"
            :$"選考スコア（信頼） {c.Merit}";
        public string DefeatReview()
        {
            var s=State;var text=new StringBuilder(English?"Saori: Let's take a quick peek at the rivals' conversations.\n\n":"沙織：ちょっとだけ、ライバルの会話を覗いてみましょう。\n\n");
            text.AppendLine((English?"Result: ":"今回の結果：")+s.notice+"\n");
            var contenders=s.contestants.Where(c=>!c.eliminated||c.id==s.lastEliminated).OrderByDescending(c=>c.Merit).ThenBy(c=>c.id=="himari"?1:0).ToList();
            text.AppendLine(English?"―― Selection scores (lowest is eliminated; ties go against you) ――":"―― 選考スコア（最下位が脱落。同点ならあなたが脱落）――");
            for(int i=0;i<contenders.Count;i++){
                var c=contenders[i];bool self=c.id=="himari";
                text.AppendLine($"{i+1}. {Content.Person(c.id).displayName}{(self?" / YOU":"")}　{(c.eliminated?(English?"OUT":"脱落"):(English?"Survived":"生存"))}");
                text.AppendLine("　"+MeritLine(c));
            }
            var me=contenders.Find(c=>c.id=="himari");var lowestSurvivor=contenders.Where(c=>!c.eliminated).OrderBy(c=>c.Merit).FirstOrDefault();
            if(me!=null&&lowestSurvivor!=null&&me.eliminated)
                text.AppendLine(English?$"\nGap to the lowest survivor ({Content.Person(lowestSurvivor.id).displayName}): {lowestSurvivor.Merit-me.Merit} points (you needed {lowestSurvivor.Merit-me.Merit+1} more)."
                    :$"\n最下位の生存者（{Content.Person(lowestSurvivor.id).displayName}）との点差：{lowestSurvivor.Merit-me.Merit} 点（あと {lowestSurvivor.Merit-me.Merit+1} 点で残れました）");
            var mine=s.conversations.Where(m=>m.loop==s.loop&&m.chapter==s.chapter&&m.timeline==s.timeLeaps).ToList();
            text.AppendLine();
            text.AppendLine(English?$"―― Your conversations this chapter ({mine.Count}) ――":$"―― あなたの会話（この章 {mine.Count} 回）――");
            if(mine.Count==0)text.AppendLine(English?"No conversation record for this time.":"この時間の会話記録はありません。");
            foreach(var m in mine)text.AppendLine($"{(English?"Talk":"会話")}{m.turn+1}　{Clip(m.playerLine,28)}\n　→ {m.effect}");
            text.AppendLine();
            text.AppendLine(English?"―― The rivals' actual conversations ――":"―― 実際に進行したライバルの会話 ――");
            foreach(var c in contenders.Where(c=>c.id!="himari")){
                text.AppendLine("\n"+Content.Person(c.id).displayName+"　"+MeritLine(c));
                var beats=s.rivalConversations.Where(b=>b.rival==c.id&&b.loop==s.loop&&b.chapter==s.chapter&&b.timeline==s.timeLeaps).ToList();
                if(beats.Count==0)text.AppendLine(English?"No conversation record for this time. Old saves are not reconstructed.":"この時間の会話記録はありません。旧セーブの会話を後から作り直すことはしません。");
                foreach(var b in beats)text.AppendLine(FormatRivalBeat(b)+"\n");
                if(beats.Count>0)text.AppendLine(English?$"Total over {beats.Count} talks: affection +{beats.Sum(b=>b.trustGain)} / understanding +{beats.Sum(b=>b.understandingGain)} (running total: trust {c.trust}, understanding {c.understanding})"
                    :$"合計（{beats.Count}会話）：好感度 +{beats.Sum(b=>b.trustGain)} ／ 相互理解 +{beats.Sum(b=>b.understandingGain)}（累計：信頼 {c.trust}・相互理解 {c.understanding}）");
            }
            if(!s.rivalConversations.Any(b=>b.loop==s.loop&&b.chapter==s.chapter&&b.timeline==s.timeLeaps)&&!string.IsNullOrWhiteSpace(s.npcScene))
                text.AppendLine((English?"\nLast saved observation (detailed understanding not recorded)\n":"\n保存されていた直近の観測記録（詳細な相互理解は未記録）\n")+s.npcScene);
            return text.ToString();
        }
    }
}
