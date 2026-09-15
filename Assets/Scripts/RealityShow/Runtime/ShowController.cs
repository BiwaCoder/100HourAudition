using System;
using System.Collections;
using UnityEngine;
using HundredHour.AIChat;
namespace HundredHour.RealityShow
{
    [RequireComponent(typeof(ShowView),typeof(ChatApiClient))]
    public sealed class ShowController : MonoBehaviour
    {
        public ShowContent content;
        public CanvasGroup transitionGroup;
        public ShowDifficulty difficulty=ShowDifficulty.Easy;
        [Tooltip("ON: PythonAPIを使用。失敗時は会話をローカル生成して続行。")]
        public bool useAI;
        public ShowGame Game {get;private set;}
        public bool Busy {get;private set;}
        public bool PresentationLocked {get;set;}
        public event Action Refreshed;
        public string LastDialogueSource {get;private set;}="local";
        public string LastError {get;private set;}
        public string LastRawReply {get;private set;}
        ShowView view;ChatApiClient api;bool choosingWeather;int revision;
        void Awake(){view=GetComponent<ShowView>();api=GetComponent<ChatApiClient>();}
        void OnEnable(){view.Choice+=Choose;view.Card+=PlayCard;view.Command+=Command;}
        void Start(){Game=new ShowGame(content);Refresh();}
        void OnDisable(){revision++;StopAllCoroutines();Busy=false;view.Choice-=Choose;view.Card-=PlayCard;view.Command-=Command;if(transitionGroup!=null)transitionGroup.alpha=1;}
        public void Refresh(string message=null){if(Game!=null){view.Render(Game,Busy||PresentationLocked,difficulty,useAI,message);Refreshed?.Invoke();}}
        void Save(){if(Game.State.phase==ShowPhase.Title)return;try{ShowSaveStore.Save(Game.State);}catch(Exception e){LastError=e.Message;Game.State.notice="保存できませんでした："+e.Message;}}
        public void Command(string command)
        {
            if(Busy||PresentationLocked||Game==null)return;
            choosingWeather=false;
            switch(command)
            {
                case "easy":difficulty=ShowDifficulty.Easy;break;case "normal":difficulty=ShowDifficulty.Normal;break;case "hard":difficulty=ShowDifficulty.Hard;break;case "ai":useAI=!useAI;break;
                case "start":Game.Begin(Environment.TickCount,difficulty);LastError=null;break;
                case "continue":if(ShowSaveStore.TryLoad(content,out var loaded,out var error)){Game=new ShowGame(content,loaded);difficulty=loaded.difficulty;}else{LastError=error;Refresh("セーブを読み込めませんでした："+error);return;}break;
                case "home":Save();Game=new ShowGame(content);break;
                case "ask":Game.Compose(view.MemoryA,"ask");break;case "share":Game.Compose(view.MemoryA,"share");break;case "connect":Game.Compose(view.MemoryA,"connect",view.MemoryB);break;
                case "leap":if(Game.TimeLeap()){Save();Refresh();}return;
                case "perspective":Game.UsePerspective();break;case "future":Game.Forecast();break;case "acquire":Game.Acquire();break;
                case "weather":choosingWeather=true;view.ShowWeatherChoices();return;
            }
            Save();Refresh();
        }
        public void PlayCard(int index){if(Busy||PresentationLocked||Game==null)return;choosingWeather=false;if(Game.PlayCard(index)){Save();Refresh();}}
        public void Choose(int index)
        {
            if(Busy||PresentationLocked||Game==null)return;
            if(choosingWeather){choosingWeather=false;if(index>=0&&index<5)Game.SetWeather(new[]{"clear","rain","wind","fog","blackout"}[index]);Save();Refresh();return;}
            var phase=Game.State.phase;
            if(phase==ShowPhase.Conversation){Speak(index);return;}
            Action change=()=>{
                switch(phase)
                {
                    case ShowPhase.FirstChoice:Game.FirstChoice(index);break;
                    case ShowPhase.Route:Game.ChooseRoute(index);break;
                    case ShowPhase.Reward:Game.TakeReward(index);break;
                    case ShowPhase.Eliminated:if(index==0&&Game.State.CanLoop)Game.Loop();else Game.Leave();break;
                    case ShowPhase.Victory:case ShowPhase.GameOver:Game=new ShowGame(content);break;
                    default:Game.Advance();break;
                }
            };
            StartCoroutine(Change(change));
        }
        void Speak(int index)
        {
            if(index<0||index>=Game.State.actions.Count)return;var action=Game.State.actions[index];var fallback=ShowDialogue.Local(Game,action);
            if(!useAI){LastDialogueSource="local";StartCoroutine(Change(()=>Game.ResolveTalk(index,fallback)));return;}
            Game.DescribeSupport("沙織がこれまでの会話と記憶を整理して送った。相手から届く返答を、一緒に待っている。");
            Busy=true;Refresh();int request=++revision;
            api.SendChatMessage(ShowDialogue.Context(Game,action),ShowDialogue.SystemPrompt,result=>{
                if(!isActiveAndEnabled||request!=revision)return;
                LastRawReply=result?.response;bool valid=ShowDialogue.TryParse(result,out var reply,out var error);LastDialogueSource=valid?"PythonAPI":"local fallback";LastError=valid?null:error;
                Debug.Log($"[RealityShow AI] source={LastDialogueSource} accepted={valid} reason={error}",this);
                StartCoroutine(Change(()=>{Game.ResolveTalk(index,valid?reply:fallback);Game.DescribeSupport(valid?"沙織が届いた返答を整理した。相手が話した言葉を、次の会話に使える記憶としてノートに残してくれた。":"沙織は通信結果を使えなかったため、手元の会話と記憶を使って進行を支えた。今回はローカル会話で続けている。");if(!valid)Game.State.notice="AI通信を利用できないため、記憶を引き継いでローカル会話で続けています。";}));
            });
        }
        IEnumerator Change(Action mutation)
        {
            Busy=true;Refresh();yield return Fade(1,.25f);
            try{mutation();Save();}catch(Exception e){LastError=e.Message;Debug.LogException(e);}
            Refresh();yield return Fade(.25f,1);Busy=false;Refresh();
        }
        IEnumerator Fade(float from,float to)
        {
            float elapsed=0;while(elapsed<.14f){if(transitionGroup!=null)transitionGroup.alpha=Mathf.Lerp(from,to,elapsed/.14f);elapsed+=Time.unscaledDeltaTime;yield return null;}
            if(transitionGroup!=null)transitionGroup.alpha=to;
        }
    }
}
