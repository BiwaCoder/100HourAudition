System.Collections.IEnumerator Check(){
yield return new UnityEngine.WaitForSeconds(.3f);
var joy=UnityEngine.Object.FindFirstObjectByType<FixedJoystick>();var cube=UnityEngine.GameObject.Find("PlayerCube");var body=cube.GetComponent<UnityEngine.Rigidbody>();var start=body.position;
var r=(UnityEngine.RectTransform)joy.transform;var center=UnityEngine.RectTransformUtility.WorldToScreenPoint(null,r.position);var canvas=joy.GetComponentInParent<UnityEngine.Canvas>();var e=new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current);e.position=center+UnityEngine.Vector2.right*180*canvas.scaleFactor;joy.OnPointerDown(e);
yield return new UnityEngine.WaitForSeconds(.6f);var moved=body.position;joy.OnPointerUp(e);yield return new UnityEngine.WaitForSeconds(.2f);var stopped=body.linearVelocity;
e.position=center+UnityEngine.Vector2.up*180*canvas.scaleFactor;joy.OnPointerDown(e);yield return new UnityEngine.WaitForSeconds(.5f);var forward=body.position;joy.OnPointerUp(e);yield return new UnityEngine.WaitForSeconds(.2f);
System.IO.File.WriteAllText("Temp/JoystickCubeCheck.json",UnityEngine.JsonUtility.ToJson(new UnityEngine.Vector3(moved.x-start.x,forward.z-moved.z,new UnityEngine.Vector2(stopped.x,stopped.z).magnitude)));
body.position=new UnityEngine.Vector3(0,.5f,0);body.linearVelocity=UnityEngine.Vector3.zero;yield return new UnityEngine.WaitForSeconds(.3f);UnityEngine.ScreenCapture.CaptureScreenshot("Screenshots/JoystickCubeDemo.png");}
UnityEngine.Object.FindFirstObjectByType<HundredHour.JoystickDemo.JoystickCubeMover>().StartCoroutine(Check());return "Testing joystick pointer events";
