using System;
using System.Linq;
using HundredHour.Localization;
using HundredHour.RealityShow;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HundredHour.Environments
{
    // Uses the existing canvas and font; no scene/prefab migration is needed.
    public sealed class MansionMemoryJournal : MonoBehaviour
    {
        static bool English=>GameLanguage.Current==GameLocale.English;
        static string L(string ja,string en)=>English?en:ja;
        MansionSignalDirector director;
        MansionSignalView view;
        GameObject panel;
        Button toggle, clear, compose, generate;
        RectTransform rows;
        ScrollRect scroll;
        readonly System.Collections.Generic.List<GameObject> rowObjects=new System.Collections.Generic.List<GameObject>();
        TMP_Text title, selection;

        public bool IsOpen=>panel&&panel.activeSelf;
        public RectTransform ToggleRect=>toggle?toggle.transform as RectTransform:null;
        static void Rect(RectTransform r,float x,float y,float right,float top)
        {r.anchorMin=new Vector2(x,y);r.anchorMax=new Vector2(right,top);r.offsetMin=r.offsetMax=Vector2.zero;}
        TMP_Text Text(Transform parent,string name,float x,float y,float right,float top,int size)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(TextMeshProUGUI));go.transform.SetParent(parent,false);
            var t=go.GetComponent<TMP_Text>();t.font=view.body.font;t.fontSharedMaterial=view.body.fontSharedMaterial;t.fontSize=size;t.color=new Color(.91f,.95f,1);t.text="";t.richText=false;t.raycastTarget=false;
            Rect(t.rectTransform,x,y,right,top);return t;
        }
        Button Button(Transform parent,string name,string label,float x,float y,float right,float top,Action action)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Image),typeof(Button));go.transform.SetParent(parent,false);
            Rect((RectTransform)go.transform,x,y,right,top);go.GetComponent<Image>().color=new Color(.06f,.20f,.25f,1);
            var b=go.GetComponent<Button>();b.targetGraphic=go.GetComponent<Image>();var colors=b.colors;colors.disabledColor=new Color(.35f,.35f,.35f);b.colors=colors;
            var t=Text(go.transform,"Label",.035f,.04f,.965f,.96f,24);t.text=label;t.alignment=TextAlignmentOptions.Center;t.enableAutoSizing=true;t.fontSizeMin=19;t.fontSizeMax=24;
            b.onClick.AddListener(()=>action());return b;
        }
        public void Initialize(MansionSignalDirector d,MansionSignalView v)
        {
            director=d;view=v;
            toggle=Button(view.stage,"MemoryToggle",L("記憶・話題","Topics"),.79f,.925f,.98f,.985f,()=>{if(IsOpen)Close();else Open();});
            panel=new GameObject("ConversationMemoryWindow",typeof(RectTransform),typeof(Image));panel.transform.SetParent(view.stage,false);
            Rect((RectTransform)panel.transform,.06f,.12f,.94f,.91f);panel.GetComponent<Image>().color=new Color(.015f,.045f,.065f,1);
            title=Text(panel.transform,"Title",.035f,.90f,.79f,.98f,28);
            Button(panel.transform,"CloseMemory",L("閉じる","Close"),.83f,.90f,.97f,.98f,Close);
            Text(panel.transform,"Instructions",.035f,.825f,.97f,.895f,23).text=L("話題をクリックして選択（最大2件）。もう一度クリックで解除。","Click a topic to pick it (up to 2). Click again to deselect.");
            var viewport=new GameObject("MemoryList",typeof(RectTransform),typeof(Image),typeof(RectMask2D),typeof(ScrollRect));viewport.transform.SetParent(panel.transform,false);
            Rect((RectTransform)viewport.transform,.035f,.40f,.97f,.82f);viewport.GetComponent<Image>().color=new Color(.01f,.025f,.04f,1);
            var content=new GameObject("Rows",typeof(RectTransform));content.transform.SetParent(viewport.transform,false);rows=(RectTransform)content.transform;
            rows.anchorMin=new Vector2(0,1);rows.anchorMax=Vector2.one;rows.pivot=new Vector2(.5f,1);rows.offsetMin=rows.offsetMax=Vector2.zero;
            scroll=viewport.GetComponent<ScrollRect>();scroll.viewport=(RectTransform)viewport.transform;scroll.content=rows;scroll.horizontal=false;scroll.vertical=true;scroll.scrollSensitivity=45;scroll.movementType=ScrollRect.MovementType.Clamped;
            selection=Text(panel.transform,"Selection",.035f,.155f,.97f,.385f,23);
            clear=Button(panel.transform,"ClearMemories",L("選択解除","Deselect"),.035f,.045f,.20f,.14f,()=>{director.Game.State.recallA=director.Game.State.recallB="";director.RecallChanged();});
            compose=Button(panel.transform,"RecallWithoutAI",L("同じ話題を深める / 集中1","Deepen this topic / Focus 1"),.22f,.045f,.58f,.14f,()=>director.SpeakFromMemory());
            generate=Button(panel.transform,"ExpandMemory",L("新しい魅力的な話題を作る / AI1","Create an engaging topic / AI 1"),.60f,.045f,.97f,.14f,()=>director.GenerateFromMemories());
            panel.SetActive(false);toggle.gameObject.SetActive(false);
        }
        void Select(string id)
        {
            var s=director.Game.State;
            if(id==s.recallA){s.recallA=s.recallB;s.recallB="";}
            else if(id==s.recallB)s.recallB="";
            else if(string.IsNullOrEmpty(s.recallA))director.Game.SelectRecall(id);
            else if(string.IsNullOrEmpty(s.recallB))director.Game.SelectRecall(id,true);
            director.RecallChanged();
        }
        public void Open(){view.CloseChoiceDialog();panel.SetActive(true);panel.transform.SetAsLastSibling();Refresh();scroll.verticalNormalizedPosition=1;}
        // Opening hid the choice panel; closing must redraw the dialogue so its buttons ("次へ" etc.) come back.
        public void Close(){panel.SetActive(false);Refresh();if(director)director.Refresh();}
        public void Refresh()
        {
            if(director.Game==null)return;
            var game=director.Game;var s=game.State;var memories=game.RecallMemories();
            toggle.gameObject.SetActive(director.Started);toggle.GetComponentInChildren<TMP_Text>().text=English?$"Topics {memories.Count} {(IsOpen?"Close":"Open")}":$"記憶・話題 {memories.Count} {(IsOpen?"閉じる":"開く")}";
            if(!IsOpen)return;
            compose.GetComponentInChildren<TMP_Text>().text=L("同じ話題を深める / 集中1","Deepen this topic / Focus 1");
            generate.GetComponentInChildren<TMP_Text>().text=L("新しい魅力的な話題を作る / AI1","Create an engaging topic / AI 1");
            title.text=English?$"Topics for {director.content.Person(s.target).displayName}  /  {memories.Count} memories":$"{director.content.Person(s.target).displayName}への話題  /  記憶 {memories.Count}件";
            bool can=s.phase==ShowPhase.Conversation&&!director.Busy&&string.IsNullOrEmpty(s.pendingTopic);
            float position=rows.anchoredPosition.y;
            foreach(var go in rowObjects){go.SetActive(false);Destroy(go);}rowObjects.Clear();
            int index=0;
            foreach(var m in memories.AsEnumerable().Reverse())
            {
                string id=m.id;bool chosen=id==s.recallA||id==s.recallB;
                // A memory that has already earned its three fresh-angle bonuses this loop gives no
                // further benefit if picked again (see ShowGame.MemoryBonus); mark it
                // used and lock it instead of leaving it selectable with no visible difference.
                bool exhausted=!chosen&&s.memorySynergies.Count(k=>k.StartsWith(s.loop+":"+s.timeLeaps+":")&&k.Contains(id))>=3;
                var button=Button(rows,"Memory_"+id,"",0,0,1,1,()=>Select(id));rowObjects.Add(button.gameObject);
                var r=(RectTransform)button.transform;r.anchorMin=new Vector2(0,1);r.anchorMax=Vector2.one;r.pivot=new Vector2(.5f,1);r.sizeDelta=new Vector2(0,64);r.anchoredPosition=new Vector2(0,-index*72);
                button.GetComponent<Image>().color=chosen?new Color(.10f,.36f,.40f,1):exhausted?new Color(.05f,.05f,.06f,1):new Color(.035f,.10f,.14f,1);
                button.interactable=can&&!exhausted&&(chosen||string.IsNullOrEmpty(s.recallA)||string.IsNullOrEmpty(s.recallB));
                var label=button.GetComponentInChildren<TMP_Text>();Rect(label.rectTransform,.015f,.12f,.31f,.9f);label.alignment=TextAlignmentOptions.MidlineLeft;label.text=(exhausted?L("✓ 使用済み：","✓ Used: "):chosen?"✓ ":"□ ")+ShowGame.TopicLabel(m.topic);label.color=exhausted?new Color(.5f,.55f,.55f):label.color;label.enableAutoSizing=false;label.fontSize=23;
                label.textWrappingMode=TextWrappingModes.NoWrap;label.overflowMode=TextOverflowModes.Ellipsis;
                var summary=Text(r,"Summary",.33f,.12f,.985f,.9f,21);summary.alignment=TextAlignmentOptions.MidlineLeft;summary.text=game.MemorySummary(m);summary.textWrappingMode=TextWrappingModes.NoWrap;summary.overflowMode=TextOverflowModes.Ellipsis;
                index++;
            }
            rows.sizeDelta=new Vector2(0,Math.Max(72,index*72));rows.anchoredPosition=new Vector2(0,Mathf.Clamp(position,0,Mathf.Max(0,rows.sizeDelta.y-scroll.viewport.rect.height)));
            var a=game.SharedMemory(s.recallA);var b=game.SharedMemory(s.recallB);
            int bonus=game.MemoryBonus(new TalkAction{recallA=s.recallA,recallB=s.recallB,goal=ShowGame.TopicGoals[0]});
            selection.text=a==null?(memories.Count==0?L("話した内容から、使える話題がここに増えます。","Topics you can use will appear here as you talk."):L("1件で話を深める。2件で話題をつなぐ。前の時間の記憶も使えます。","Pick one to go deeper, or two to connect them. Memories from earlier timelines work too.")):
                (English?$"Selected: {ShowGame.Clip(ShowGame.TopicLabel(a.topic),20)}{(b==null?"":" + "+ShowGame.Clip(ShowGame.TopicLabel(b.topic),20))}  /  memory bonus +{bonus}\n"
                    +(game.IsPastMemory(a)||game.IsPastMemory(b)?"From an earlier timeline: they won't remember it. It comes up as a first-time question.":"Try asking why, or how they felt, about a topic they remember too.")
                :$"選択：{ShowGame.Clip(a.topic,20)}{(b==null?"":" ＋ "+ShowGame.Clip(b.topic,20))}  /  記憶ボーナス +{bonus}\n"
                    +(game.IsPastMemory(a)||game.IsPastMemory(b)?"前の時間の記憶：相手は覚えていません。初めての質問として話します。":"相手も覚えている話題から、理由や気持ちを聞いてみましょう。"));
            if(!can)selection.text+=L("\n今は閲覧のみ。相手の話を読み終えてから選べます。","\nViewing only for now. You can choose once you've finished reading their reply.");
            else if(s.composedThisTurn||s.acquired)selection.text+=L("\n作成済みの話題は下の選択肢から話せます。次の会話で再び作成できます。","\nA prepared topic is ready in the choices below. You can prepare another next turn.");
            // Always-visible, regardless of what's selected: the two buttons cost different
            // resources and do genuinely different things, which wasn't otherwise explained anywhere.
            selection.text+=game.IsFinalDate?L("\n最後のデート：集中1で本音を深める。AI1で記憶から将来・約束・プロポーズへ広げる。","\nFinal date: Focus 1 explores true feelings. AI 1 connects memories to your future, commitment, or a proposal."):L("\n自分で集中して、同じ話題の理由や気持ちを深める（集中1・毎ターン回復）。AIは選んだ記憶をつなぎ、新しい視点から広がり深まる話題を考える（AI1）。",
                "\nUse your focus to explore the reasons and feelings within the same topic (Focus 1, refills each turn). AI connects your chosen memories to a fresh topic with more breadth and depth (AI 1).");
            clear.interactable=can&&a!=null;compose.interactable=can&&a!=null&&!s.composedThisTurn&&s.focus>=1;
            generate.interactable=can&&a!=null&&!s.acquired&&s.agi>=game.SkillCost("acquire");
        }
    }
}
