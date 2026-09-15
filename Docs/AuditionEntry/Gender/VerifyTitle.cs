var p=UnityEngine.Object.FindFirstObjectByType<HundredHour.AuditionEntry.AuditionPortal>();
if(!p||!p.hub)throw new Exception("Title required");
p.Continue();
if(p.menuPanel.activeSelf||!p.transform.Find("Player Gender").gameObject.activeSelf)throw new Exception("Gender selection must precede menu");
ScreenCapture.CaptureScreenshot("Docs/AuditionEntry/Gender/title.png");
return "PASS language to gender selection before game menu";
