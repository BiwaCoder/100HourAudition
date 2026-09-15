using System;
using System.Linq;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using HundredHour.UI.Choices;
using HundredHour.UI.Participants;
using HundredHour.UI.Audition;
using HundredHour.UI.Timeline;
namespace HundredHour.RealityShow
{
    public sealed class ShowView : MonoBehaviour
    {
        public Cinema.ShowScenario scenario;
        public GameObject castPanel;
        public UnityEngine.UI.Button castTab,leapButton;
        public GameObject titleScreen,gameScreen,memoryPanel,feedPanel,deckPanel;
        public TMP_Text titleDescription,header,resources,chapterLabel,speaker,dialogue,notice,motivation,handLabel,deckText,memoryPageLabel,modeLabel;
        public UnityEngine.UI.Image speakerPortrait;
        public UnityEngine.UI.Button startButton,continueButton,aiButton,easyButton,normalButton,hardButton,memoryTab,feedTab,deckTab,memoryPrevious,memoryNext,askButton,shareButton,connectButton,perspectiveButton,futureButton,weatherButton,acquireButton,homeButton;
        public UnityEngine.UI.Button[] handButtons,memoryButtons;
        public TMP_Text[] handTexts,memoryTexts;
        public ParticipantCardController[] castCards;
        public ChoiceMenuController choices;
        public AuditionClockView clock;
        public TimelineFeedController feed;
        public event Action<int> Choice,Card;
        public event Action<string> Command;
        public string MemoryA {get;private set;} public string MemoryB {get;private set;}
        int memoryPage,tab,feedCount=-1;ShowState feedState;ShowGame game;bool busy;ShowDifficulty difficulty;bool ai;
        void Awake()
        {
            if(castTab!=null)castTab.onClick.AddListener(()=>SetTab(3));if(leapButton!=null)Bind(leapButton,"leap");
            choices.Confirmed+=i=>Choice?.Invoke(i);
            for(int i=0;i<handButtons.Length;i++){int n=i;handButtons[i].onClick.AddListener(()=>Card?.Invoke(n));}
            for(int i=0;i<memoryButtons.Length;i++){int n=i;memoryButtons[i].onClick.AddListener(()=>SelectMemory(n));}
            Bind(startButton,"start");Bind(continueButton,"continue");Bind(aiButton,"ai");Bind(easyButton,"easy");Bind(normalButton,"normal");Bind(hardButton,"hard");
            Bind(askButton,"ask");Bind(shareButton,"share");Bind(connectButton,"connect");Bind(perspectiveButton,"perspective");Bind(futureButton,"future");Bind(weatherButton,"weather");Bind(acquireButton,"acquire");Bind(homeButton,"home");
            memoryTab.onClick.AddListener(()=>SetTab(0));feedTab.onClick.AddListener(()=>SetTab(1));deckTab.onClick.AddListener(()=>SetTab(2));
            memoryPrevious.onClick.AddListener(()=>{memoryPage=Math.Max(0,memoryPage-1);RenderSide();});memoryNext.onClick.AddListener(()=>{memoryPage++;RenderSide();});
        }
        void Bind(UnityEngine.UI.Button button,string id)=>button.onClick.AddListener(()=>Command?.Invoke(id));
        void SetTab(int value){tab=value;RenderSide();}
        public void Render(ShowGame value,bool loading,ShowDifficulty selectedDifficulty,bool useAI,string externalNotice=null)
        {
            game=value;busy=loading;difficulty=selectedDifficulty;ai=useAI;var s=game.State;
            bool title=s.phase==ShowPhase.Title;titleScreen.SetActive(title);gameScreen.SetActive(!title);
            modeLabel.text=$"{(ai?"AI会話 ON":"ローカル会話")}  ·  {difficulty}";
            continueButton.interactable=System.IO.File.Exists(ShowSaveStore.Path);SetButton(aiButton,ai?"AI会話 ON":"AI会話 OFF");
            SetButton(easyButton,difficulty==ShowDifficulty.Easy?"● Easy":"Easy");SetButton(normalButton,difficulty==ShowDifficulty.Normal?"● Normal":"Normal");SetButton(hardButton,difficulty==ShowDifficulty.Hard?"● Hard":"Hard");
            if(title){choices.Close();return;}
            header.text=$"100 Hour <color=#FF9F8E>Audition Battle</color>     /     LOOP {s.loop}/{(s.difficulty==ShowDifficulty.Easy?"∞":s.LoopLimit.ToString())} · {s.difficulty}";
            resources.text=$"AGI {s.agi}  /  BANK {s.bank}      信頼 {s.Player.trust}   ·   緊張 {s.stress}/100   ·   同調 {s.attune}";
            clock.Render(s.RemainingHours*3600L);
            for(int i=0;i<castCards.Length;i++)
            {
                var c=s.contestants[i];var p=game.Content.Person(c.id);float total=s.contestants.Sum(x=>(float)x.stars);
                castCards[i].SetData(new ParticipantCardData{displayName=p.displayName,portrait=p.portrait,subtitle=$"{p.role} · 信頼 {c.trust}",number=i+1,stars=c.stars,support=total>0?c.stars/total:0,eliminated=c.eliminated,status=c.id=="himari"?"YOU":c.alliance>0?"ALLY":"ON AIR",accent=p.accent});
            }
            chapterLabel.text=$"EP. {s.chapter+1:00}  /  {ChapterName(s)}  /  TALK {Math.Min(s.turn+1,4)}/4";
            speaker.text=s.phase==ShowPhase.Conversation?game.Content.Person(s.target).displayName:PhaseSpeaker(s);
            if(scenario!=null&&scenario.Beat(s)!=null&&!string.IsNullOrEmpty(scenario.Beat(s).speaker))speaker.text=scenario.Beat(s).speaker;
            speakerPortrait.sprite=game.Content.Person(s.phase==ShowPhase.Conversation?s.target:"himari").portrait;
            string story=StoryText(s);if(!string.IsNullOrEmpty(s.supportNarration))story="【沙織のサポート】\n"+s.supportNarration+"\n\n"+story;if(dialogue.text!=story){dialogue.text=story;dialogue.rectTransform.anchoredPosition=Vector2.zero;UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(dialogue.rectTransform);dialogue.ForceMeshUpdate();}notice.text=externalNotice??s.notice;
            if(loading)notice.text=GetComponent<ShowController>().PresentationLocked?"放送中です。演出を見終えるか、スキップすると選択できます。":"相手が言葉を選んでいます……";
            motivation.text=$"いまの動機\n{game.Motivation}\n\n場：{WeatherName(s.weather)}  /  相手の圧力 {s.intentPressure}\n純粋さ {s.purity}  ·  運命の棘 {s.thorns}";
            handLabel.text=$"言葉を支える手札     集中力 {s.focus}/3     山札 {s.draw.Count} · 捨て札 {s.discard.Count}";
            bool talk=s.phase==ShowPhase.Conversation;
            for(int i=0;i<handButtons.Length;i++)
            {
                bool active=talk&&i<s.hand.Count;handButtons[i].gameObject.SetActive(active);if(!active)continue;
                var card=game.Card(s.hand[i]);var layout=handButtons[i].GetComponent<UnityEngine.UI.LayoutElement>();layout.preferredWidth=Math.Min(300,(1240-(s.hand.Count-1)*10f)/Math.Max(1,s.hand.Count));layout.flexibleWidth=0;handTexts[i].text=$"<color=#FFBBAA>{card.cost} FOCUS</color>\n<size=23><b>{card.title}</b></size>\n\n{card.description}";
                handButtons[i].interactable=!busy&&card.cost<=s.focus;
            }
            var labels=Labels(s).ToArray();if(busy)choices.Close();else choices.Show(labels);
            perspectiveButton.interactable=talk&&!busy&&s.agi>=1&&!s.perspective;
            futureButton.interactable=talk&&!busy&&s.agi>=2;
            weatherButton.interactable=talk&&!busy&&s.agi>=2;
            acquireButton.interactable=talk&&!busy&&s.agi>=1&&!s.acquired;
            homeButton.interactable=!busy;
            if(leapButton!=null)leapButton.interactable=talk&&!busy&&s.agi>=3&&!string.IsNullOrEmpty(s.chapterCheckpoint);
            RenderSide();
        }
        void RenderSide()
        {
            if(game==null)return;var s=game.State;
            // ChoiceMenu owns keyboard confirmation; a previously clicked Button must not submit again.
            if(s.phase!=ShowPhase.Title&&UnityEngine.EventSystems.EventSystem.current!=null)UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
            memoryPanel.SetActive(tab==0);feedPanel.SetActive(tab==1);deckPanel.SetActive(tab==2);
            if(castPanel!=null)castPanel.SetActive(tab==3);if(leapButton!=null)leapButton.gameObject.SetActive(tab==0);
            motivation.gameObject.SetActive(tab==0);
            foreach(var button in new[]{perspectiveButton,futureButton,weatherButton,acquireButton})button.gameObject.SetActive(tab==0);
            if(tab==1)
            {
                if(feedState==s&&feedCount==s.log.Count)return;feedState=s;feedCount=s.log.Count;
                feed.SetPageSize(2);feed.SetEntries(s.log.AsEnumerable().Reverse().Select(b=>new TimelineEntry{speaker=b.speaker,dialogue=b.text,action="",timeLabel=$"L{b.loop} / EP{b.chapter+1}",showReactions=false,avatar=game.Content.cast.Find(p=>p.displayName==b.speaker)?.portrait}).ToList());return;
            }
            if(tab==2){deckText.text="所持デッキ（周回で保持）\n\n"+string.Join("\n",s.deck.GroupBy(x=>x).Select(g=>$"{game.Card(g.Key).title}  ×{g.Count()}"))+"\n\n準備したカード\n"+string.Join(" / ",s.played.Select(x=>game.Card(x).title));return;}
            var memories=game.AvailableMemories();if(!memories.Any(m=>m.id==MemoryA))MemoryA=null;if(!memories.Any(m=>m.id==MemoryB))MemoryB=null;int pages=Math.Max(1,(memories.Count+memoryButtons.Length-1)/memoryButtons.Length);memoryPage=Math.Min(memoryPage,pages-1);
            memoryPageLabel.text=$"MEMORY {memoryPage+1}/{pages}  ·  {memories.Count}件\n一つ選んで動詞を重ねる。結ぶときは二つ選ぶ。";
            for(int i=0;i<memoryButtons.Length;i++)
            {
                int n=memoryPage*memoryButtons.Length+i;bool active=n<memories.Count;memoryButtons[i].gameObject.SetActive(active);if(!active)continue;
                var m=memories[n];memoryTexts[i].text=$"{(m.id==MemoryA?"① ":m.id==MemoryB?"② ":"")}{ShowGame.Clip(m.detail,23)}\n<size=14>{(m.loop<s.loop?"過去の周":m.source)} · 想起 {m.used}回</size>";
            }
            memoryPrevious.interactable=memoryPage>0;memoryNext.interactable=memoryPage<pages-1;
            bool craft=!busy&&s.phase==ShowPhase.Conversation&&s.turnsSpoken>0&&s.focus>=1&&!s.composedThisTurn&&memories.Any(m=>m.id==MemoryA);
            askButton.interactable=shareButton.interactable=craft;connectButton.interactable=craft&&s.crafted&&memories.Any(m=>m.id==MemoryB)&&MemoryA!=MemoryB;
        }
        void SelectMemory(int n)
        {
            var list=game.AvailableMemories();int index=memoryPage*memoryButtons.Length+n;if(index>=list.Count)return;
            string id=list[index].id;if(MemoryA==id)MemoryA=null;else if(MemoryB==id)MemoryB=null;else if(string.IsNullOrEmpty(MemoryA))MemoryA=id;else MemoryB=id;RenderSide();
        }
        public void ShowWeatherChoices()=>choices.Show(new[]{"晴れにする / AGI 2","雨を呼ぶ / AGI 2","風を起こす / AGI 2","霧で包む / AGI 2","停電を起こす / AGI 2","戻る"});
        public static string WeatherName(string w)=>w=="rain"?"雨":w=="wind"?"風":w=="fog"?"霧":w=="blackout"?"停電":"晴れ";
        IEnumerable<string> Labels(ShowState s)
        {
            switch(s.phase)
            {
                case ShowPhase.Opening:return new[]{"続きを見る  →"};
                case ShowPhase.FirstChoice:return new[]{"笑顔で、大きく手を振る","静かに、深く一礼する","自分の童話の話を始める"};
                case ShowPhase.FirstReview:return new[]{"スタジオの続きを見る  →"};
                case ShowPhase.FirstLoss:return new[]{"消えかけた声を、聞く"};
                case ShowPhase.Awakening:return new[]{"沙織に手伝ってもらい、もう一度挑む"};
                case ShowPhase.Route:return game.Routes();
                case ShowPhase.Conversation:return s.actions.Select((a,i)=>(s.forecast==i?"[予測] ":"")+a.line);
                case ShowPhase.Reward:return s.offers.Select(id=>game.Card(id).title+" / "+ShowGame.Clip(game.Card(id).description,30));
                case ShowPhase.Ceremony:return new[]{s.chapter>=2?"最後の言葉を聞く":"生き残った記憶から、カードを選ぶ"};
                case ShowPhase.Eliminated:return s.CanLoop?new[]{"もう一度、同じ舞台へ / 記憶とデッキを保持","ここで降りる"}:new[]{"この物語を終える"};
                case ShowPhase.Victory:case ShowPhase.GameOver:return new[]{"タイトルへ"};
                default:return Array.Empty<string>();
            }
        }
        static string ChapterName(ShowState s)=>s.phase==ShowPhase.Opening||s.phase==ShowPhase.FirstChoice?"FIRST IMPRESSION":s.chapter==0?"自己紹介":s.chapter==1?"グループデート":"一対一 / 最終選考";
        static string PhaseSpeaker(ShowState s)=>s.phase==ShowPhase.Awakening||s.phase==ShowPhase.Route||s.phase==ShowPhase.Reward?ShowSupportVoice.Label:s.phase==ShowPhase.Ceremony||s.phase==ShowPhase.Eliminated?"LIVE CEREMONY":"100Hour Audition";
        string StoryText(ShowState s)
        {
            var beat=scenario!=null?scenario.Beat(s):null;if(beat!=null&&!string.IsNullOrEmpty(beat.text))return beat.text;
            switch(s.phase)
            {
                case ShowPhase.Opening:return new[]{"鏡とライトに囲まれた舞台。残されたのは100時間。\nカメラは四人の呼吸を待ち続けている。","わたしは、ひまり。童話と小さな生き物が好き。\n自分の物語を信じて、この場所に来た。","相手はゲームクリエイター、松村悠斗。\nこの舞台で、四人の中からたった一人を選ぶ。"}[Math.Min(2,s.openingPage)];
                case ShowPhase.FirstChoice:return "最初のライトが点く。みんなの視線が、あなたに集まる。\nどんな自分を、最初に見せよう？";
                case ShowPhase.FirstReview:return s.openingPage==0?"実況レン：最初の関門は第一印象。四人の中から一人が、今ここで脱落します。":"解説ミサ：第一印象は優劣のすべてではありません。それでも今夜は、一人がこの舞台を去ります。";
                case ShowPhase.FirstLoss:return "「最初の脱落者は、ひまりさんです」\nまだ、何も伝えていないのに。時計の音だけが、遠くなる。";
                case ShowPhase.Awakening:return ShowSupportVoice.Awakening;
                case ShowPhase.Route:return ShowSupportVoice.Route(s.chapter);
                case ShowPhase.Conversation:return s.lastNpc;
                case ShowPhase.Reward:return ShowSupportVoice.Reward;
                case ShowPhase.Ceremony:return s.notice+"\n信頼 + スター/5000 + 同盟で選考。同点ではあなたが脱落します。";
                case ShowPhase.Eliminated:return "今回は、ここまで。\n"+s.notice+"。\n"+(s.CanLoop?"戻るか、降りるか。決めるのは、あなた。":"残された周はありません。この物語を閉じよう。");
                case ShowPhase.Victory:return "「君の言葉が、次の物語になった。一緒に続きを作りたい」\n100時間の終わりに、あなたの名前が呼ばれた。";
                case ShowPhase.GameOver:return "ライトが消える。けれど、この出会いは無駄じゃなかった。\nまた新しい物語を、始めることができる。";
                default:return "";
            }
        }
        static void SetButton(UnityEngine.UI.Button button,string text)=>button.GetComponentInChildren<TMP_Text>().text=text;
    }
}
