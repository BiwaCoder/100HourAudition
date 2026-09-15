System.Collections.IEnumerator Check() {
var d=UnityEngine.Object.FindFirstObjectByType<HundredHour.Environments.MansionArrivalDirector>();
var v=UnityEngine.Object.FindFirstObjectByType<HundredHour.RealityShow.Cinema.ShowCharacterVisual>();
var report=new System.Collections.Generic.List<string>();
float until=UnityEngine.Time.realtimeSinceStartup+60;
while(d.Phase!=HundredHour.Environments.MansionArrivalDirector.ArrivalPhase.Walking && UnityEngine.Time.realtimeSinceStartup<until) yield return null;
report.Add("Intro to Walking: "+(d.Phase==HundredHour.Environments.MansionArrivalDirector.ArrivalPhase.Walking));
yield return null; yield return null;
var body=v.GetComponent<UnityEngine.Rigidbody>();var collider=v.GetComponent<UnityEngine.BoxCollider>();
var position=body.position;var colliderSize=collider.size;
for(int i=0;i<4;i++) {
var es=UnityEngine.EventSystems.EventSystem.current;
var pointer=new UnityEngine.EventSystems.PointerEventData(es);
pointer.position=UnityEngine.RectTransformUtility.WorldToScreenPoint(null, v.switchButton.transform.position);
var hits=new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();es.RaycastAll(pointer,hits);
report.Add("Button raycast "+i+": "+(hits.Count>0&&hits[0].gameObject==v.switchButton.gameObject));
UnityEngine.EventSystems.ExecuteEvents.Execute(v.switchButton.gameObject,pointer,UnityEngine.EventSystems.ExecuteEvents.pointerClickHandler);
yield return null;
bool cube=v.mode==HundredHour.RealityShow.Cinema.ShowCharacterVisual.VisualMode.Cube;
bool fill=v.mode==HundredHour.RealityShow.Cinema.ShowCharacterVisual.VisualMode.Silhouette||v.mode==HundredHour.RealityShow.Cinema.ShowCharacterVisual.VisualMode.SilhouetteAndWire;
bool wire=v.mode==HundredHour.RealityShow.Cinema.ShowCharacterVisual.VisualMode.Wire||v.mode==HundredHour.RealityShow.Cinema.ShowCharacterVisual.VisualMode.SilhouetteAndWire;
report.Add("Portrait "+v.mode+": "+System.Linq.Enumerable.All(v.originalPortraitGraphics,g=>g.enabled==cube));
report.Add("Mode "+v.mode+": "+(v.cube.enabled==cube&&v.silhouette.enabled==fill&&v.wire.enabled==wire&&collider.enabled&&collider.size==colliderSize));
}
var joy=UnityEngine.Object.FindFirstObjectByType<FixedJoystick>();
var center=UnityEngine.RectTransformUtility.WorldToScreenPoint(null,joy.transform.position);
var e=new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current);
e.position=center+UnityEngine.Vector2.right*140*joy.GetComponentInParent<UnityEngine.Canvas>().scaleFactor;
var before=body.position;joy.OnPointerDown(e);
yield return new UnityEngine.WaitForSeconds(.45f);
report.Add("Joystick movement: "+(UnityEngine.Vector3.Distance(before,body.position)>.2f));
report.Add("Leg animation: "+(UnityEngine.Quaternion.Angle(v.leftLeg.localRotation,UnityEngine.Quaternion.identity)>.1f));
joy.OnPointerUp(e);yield return new UnityEngine.WaitForSeconds(.25f);
report.Add("Stops: "+(UnityEngine.Vector3.ProjectOnPlane(body.linearVelocity,UnityEngine.Vector3.up).magnitude<.05f));
body.position=position;body.linearVelocity=UnityEngine.Vector3.zero;
v.SetMode(HundredHour.RealityShow.Cinema.ShowCharacterVisual.VisualMode.SilhouetteAndWire);
v.model.rotation=UnityEngine.Quaternion.Euler(0,180,0);
yield return new UnityEngine.WaitForSeconds(.2f);
UnityEngine.ScreenCapture.CaptureScreenshot("Docs/CharacterPrototype/WalkingGame.png");
System.IO.File.WriteAllLines("Docs/CharacterPrototype/PlayVerification.txt",report);
}
UnityEngine.Object.FindFirstObjectByType<HundredHour.RealityShow.Cinema.ShowCharacterVisual>().StartCoroutine(Check());
return "Checking intro, four button modes, joystick motion and pose";
