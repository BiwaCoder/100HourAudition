using TMPro;
using UnityEngine;
namespace HundredHour.UI.Audition
{
    public sealed class AuditionClockView : MonoBehaviour
    {
        [SerializeField] TMP_Text hours,minutes,seconds;
        long lastSeconds=-1;
        public void Configure(TMP_Text h,TMP_Text m,TMP_Text s){hours=h;minutes=m;seconds=s;lastSeconds=-1;}
        public void Render(long totalSeconds)
        {
            if(hours==null||minutes==null||seconds==null)return;
            totalSeconds=System.Math.Max(0,totalSeconds);
            if(lastSeconds==totalSeconds)return;
            lastSeconds=totalSeconds;
            hours.text=(totalSeconds/3600).ToString("D2");
            minutes.text=(totalSeconds/60%60).ToString("D2");
            seconds.text=(totalSeconds%60).ToString("D2");
        }
    }
}
