using UnityEngine;

namespace HundredHour.UI.Participants
{
    /// <summary>Inspector設定とゲーム側APIをViewへ接続。シーンやゲーム進行への依存なし。</summary>
    [ExecuteAlways, DisallowMultipleComponent, RequireComponent(typeof(ParticipantCardView))]
    [AddComponentMenu("UI/Reality Show/Participant Card")]
    public sealed class ParticipantCardController : MonoBehaviour
    {
        [SerializeField] ParticipantCardData settings=new ParticipantCardData();
        ParticipantCardView view;
        bool dirty=true;
        public ParticipantCardData Settings => settings;
        public void SetData(ParticipantCardData data) { settings=data ?? new ParticipantCardData(); Refresh(); }
        public void SetEliminated(bool value) { settings.eliminated=value; Refresh(); }
        public void SetStars(long stars, float support) { settings.stars=System.Math.Max(0,stars); settings.support=Mathf.Clamp01(support); Refresh(); }
        public void SetPortrait(Sprite image) { settings.portrait=image; Refresh(); }
        public void Refresh()
        {
            if(view==null) view=GetComponent<ParticipantCardView>();
            if(settings==null) settings=new ParticipantCardData();
            view.Render(settings); dirty=false;
        }
        void OnEnable() => dirty=true;
        void OnValidate() => dirty=true;
        void Update() { if(dirty) Refresh(); }
    }
}
