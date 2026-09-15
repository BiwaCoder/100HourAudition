System.Collections.IEnumerator Check() {
var d=UnityEngine.Object.FindFirstObjectByType<HundredHour.Environments.MansionArrivalDirector>();
var v=UnityEngine.GameObject.Find("MansionPlayerCube").GetComponent<HundredHour.RealityShow.Cinema.ShowCharacterVisual>();
var report=new System.Collections.Generic.List<string>();
float until=UnityEngine.Time.realtimeSinceStartup+65;
while(d.Phase!=HundredHour.Environments.MansionArrivalDirector.ArrivalPhase.Walking&&UnityEngine.Time.realtimeSinceStartup<until)yield return null;
yield return null;yield return null;
report.Add("Intro to Walking: "+(d.Phase==HundredHour.Environments.MansionArrivalDirector.ArrivalPhase.Walking));
var body=v.GetComponent<UnityEngine.Rigidbody>();var original=body.position;var col=v.GetComponent<UnityEngine.BoxCollider>();var size=col.size;
var joy=UnityEngine.Object.FindFirstObjectByType<FixedJoystick>();
var es=UnityEngine.EventSystems.EventSystem.current;
var pointer=new UnityEngine.EventSystems.PointerEventData(es);
var modeBefore=v.mode;v.SetMode(HundredHour.RealityShow.Cinema.ShowCharacterVisual.VisualMode.StorybookToon);
for(int i=0;i<6;i++) {
pointer.position=UnityEngine.RectTransformUtility.WorldToScreenPoint(null,v.switchButton.transform.position);
var hits=new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();es.RaycastAll(pointer,hits);
report.Add("Button raycast "+i+": "+(hits.Count>0&&hits[0].gameObject==v.switchButton.gameObject));
UnityEngine.EventSystems.ExecuteEvents.Execute(v.switchButton.gameObject,pointer,UnityEngine.EventSystems.ExecuteEvents.pointerClickHandler);
yield return null;
bool cube=i==0;bool fill=i==1||i==3;bool wire=i==2||i==3;
report.Add("Exclusive mode "+v.mode+": "+((int)v.mode==i&&v.cube.enabled==cube&&v.linkedVisual.mode==v.mode&&v.importedModels[0].gameObject.activeSelf==(i>0&&i<4)&&v.importedModels[1].gameObject.activeSelf==(i==4)&&v.importedModels[2].gameObject.activeSelf==(i==5)));
report.Add("Physics and portrait "+i+": "+(col.enabled&&col.size==size&&System.Linq.Enumerable.All(v.originalPortraitGraphics,g=>g.enabled==cube)));
var start=body.position;
var e=new UnityEngine.EventSystems.PointerEventData(es);e.position=UnityEngine.RectTransformUtility.WorldToScreenPoint(null,joy.transform.position)+UnityEngine.Vector2.right*140*joy.GetComponentInParent<UnityEngine.Canvas>().scaleFactor;
joy.OnPointerDown(e);yield return new UnityEngine.WaitForSeconds(.28f);
report.Add("Move "+i+": "+(UnityEngine.Vector3.Distance(start,body.position)>.2f));
if(i>0){var item=v.importedModels[i<4?0:i-3];report.Add("Walk animation "+i+": "+item.motion.IsPlaying(item.walkClip));}
joy.OnPointerUp(e);yield return new UnityEngine.WaitForSeconds(.15f);
report.Add("Stop "+i+": "+(UnityEngine.Vector3.ProjectOnPlane(body.linearVelocity,UnityEngine.Vector3.up).magnitude<.05f));
body.position=original;body.linearVelocity=UnityEngine.Vector3.zero;
foreach(var item in v.importedModels)item.transform.rotation=UnityEngine.Quaternion.Euler(0,180,0);
yield return new UnityEngine.WaitForSeconds(.2f);
if(i==2||i==4||i==5){UnityEngine.ScreenCapture.CaptureScreenshot("Docs/CharacterPrototype/FemaleIntegration/Game"+v.mode+".png");yield return new UnityEngine.WaitForSeconds(.2f);}
}
v.SetMode(modeBefore);body.position=original;body.linearVelocity=UnityEngine.Vector3.zero;
System.IO.File.WriteAllLines("Docs/CharacterPrototype/FemaleIntegration/PlayVerification.txt",report);
}
UnityEngine.GameObject.Find("MansionPlayerCube").GetComponent<HundredHour.RealityShow.Cinema.ShowCharacterVisual>().StartCoroutine(Check());return "Checking female and bachelor integration";
