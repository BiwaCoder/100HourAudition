using System;
namespace HundredHour.UI.Audition
{
    /// <summary>ゲーム時間のカウントダウン。システム時刻・Unity・セーブ機構から独立。</summary>
    public sealed class AuditionClockModel
    {
        public double RemainingSeconds {get;private set;}
        public bool Running {get;private set;}
        public event Action Completed;
        public AuditionClockModel(double seconds){SetRemaining(seconds);}
        public void SetRemaining(double seconds)
        {
            if(double.IsNaN(seconds)||double.IsInfinity(seconds))throw new ArgumentOutOfRangeException(nameof(seconds));
            RemainingSeconds=Math.Max(0,Math.Min(seconds,35999999));
            if(RemainingSeconds==0)Running=false;
        }
        public void Resume(){if(RemainingSeconds>0)Running=true;}
        public void Pause()=>Running=false;
        public void AddTime(double seconds)=>SetRemaining(RemainingSeconds+seconds);
        public void Tick(double seconds)
        {
            if(!Running||seconds<=0||double.IsNaN(seconds)||double.IsInfinity(seconds))return;
            RemainingSeconds=Math.Max(0,RemainingSeconds-seconds);
            if(RemainingSeconds==0){Running=false;Completed?.Invoke();}
        }
        public long DisplaySeconds=>(long)Math.Ceiling(RemainingSeconds);
    }
}
