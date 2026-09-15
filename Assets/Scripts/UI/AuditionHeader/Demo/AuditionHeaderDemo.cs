using TMPro;
using UnityEngine;
namespace HundredHour.UI.Audition.Demo
{
    public sealed class AuditionHeaderDemo : MonoBehaviour
    {
        [SerializeField] AuditionClockController clock;
        [SerializeField] TMP_Text status;
        public void Configure(AuditionClockController timer,TMP_Text label){clock=timer;status=label;}
        public void Toggle(){if(clock.IsRunning){clock.Pause();status.text="PAUSED";}else{clock.Resume();status.text="LIVE";}}
        public void Rewind(){clock.AddTime(60);status.text="TIME LEAP  +60 SEC";}
        public void ResetClock(){clock.ResetClock();status.text="READY  /  100 HOURS";}
    }
}
