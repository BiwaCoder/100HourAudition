var m=new HundredHour.UI.Audition.AuditionClockModel(360000);
int completed=0;m.Completed+=()=>completed++;
if(m.DisplaySeconds/3600!=100)throw new System.Exception("100-hour display wrapped");
m.Tick(1);if(m.RemainingSeconds!=360000)throw new System.Exception("Paused clock moved");
m.Resume();m.Tick(.25);
if(m.DisplaySeconds!=360000)throw new System.Exception("Ceiling display incorrect");
m.Tick(.75);if(m.DisplaySeconds!=359999)throw new System.Exception("Hour boundary incorrect");
m.Pause();m.Tick(10);if(m.DisplaySeconds!=359999)throw new System.Exception("Pause failed");
m.AddTime(60);if(m.DisplaySeconds!=360059)throw new System.Exception("Time leap failed");
m.SetRemaining(1);m.Resume();m.Tick(10);m.Tick(10);
if(completed!=1||m.Running||m.RemainingSeconds!=0)throw new System.Exception("Completion duplicated / went negative");
m.AddTime(2);m.Resume();m.Tick(2);if(completed!=2)throw new System.Exception("Completion did not rearm");
m.SetRemaining(-10);if(m.RemainingSeconds!=0)throw new System.Exception("Negative time accepted");
try{m.SetRemaining(double.NaN);throw new System.Exception("NaN accepted");}catch(System.ArgumentOutOfRangeException){}
return "PASS: 100-hour display, paused state, fractional seconds, hour boundary, rewind, zero clamp, one completion, rearm, invalid values";
