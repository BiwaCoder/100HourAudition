using UnityEngine;
namespace HundredHour.UI.Timeline
{
    [System.Serializable]
    public sealed class TimelineEntry
    {
        public Sprite avatar;
        public string speaker="Runa";
        public string action="shared a thought";
        [TextArea(2,6)] public string dialogue="ここから、私たちの100時間が始まる。";
        public string timeLabel="now";
        public bool showReactions=true;
        public long likes=12000,stars=3400,views=890;
        public TimelineEntry Copy()=>new TimelineEntry {avatar=avatar,speaker=speaker,action=action,dialogue=dialogue,timeLabel=timeLabel,showReactions=showReactions,likes=likes,stars=stars,views=views};
    }
}
