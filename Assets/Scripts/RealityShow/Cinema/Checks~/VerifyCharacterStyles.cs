System.Collections.IEnumerator Check() {
var d=UnityEngine.Object.FindFirstObjectByType<HundredHour.Environments.MansionArrivalDirector>();
var v=UnityEngine.Object.FindFirstObjectByType<HundredHour.RealityShow.Cinema.ShowCharacterVisual>();
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
report.Add("Exclusive mode "+v.mode+": "+((int)v.mode==i&&v.cube.enabled==cube&&v.silhouette.enabled==fill&&v.wire.enabled==wire&&v.model.gameObject.activeSelf==(i>0&&i<4)&&v.streetToon.gameObject.activeSelf==(i==4)&&v.storybookToon.gameObject.activeSelf==(i==5)));
report.Add("Physics and portrait "+i+": "+(col.enabled&&col.size==size&&System.Linq.Enumerable.All(v.originalPortraitGraphics,g=>g.enabled==cube)));
var start=body.position;
var e=new UnityEngine.EventSystems.PointerEventData(es);e.position=UnityEngine.RectTransformUtility.WorldToScreenPoint(null,joy.transform.position)+UnityEngine.Vector2.right*140*joy.GetComponentInParent<UnityEngine.Canvas>().scaleFactor;
joy.OnPointerDown(e);yield return new UnityEngine.WaitForSeconds(.28f);
report.Add("Move "+i+": "+(UnityEngine.Vector3.Distance(start,body.position)>.2f));
if(i==4||i==5){var rig=i==4?v.streetToon:v.storybookToon;report.Add("Pose "+i+": "+(UnityEngine.Quaternion.Angle(rig.leftLeg.localRotation,UnityEngine.Quaternion.identity)>.1f));}
joy.OnPointerUp(e);yield return new UnityEngine.WaitForSeconds(.15f);
report.Add("Stop "+i+": "+(UnityEngine.Vector3.ProjectOnPlane(body.linearVelocity,UnityEngine.Vector3.up).magnitude<.05f));
body.position=original;body.linearVelocity=UnityEngine.Vector3.zero;
v.model.rotation=v.streetToon.transform.rotation=v.storybookToon.transform.rotation=UnityEngine.Quaternion.Euler(0,180,0);
yield return new UnityEngine.WaitForSeconds(.2f);
if(i==2||i==4||i==5){UnityEngine.ScreenCapture.CaptureScreenshot("Docs/CharacterPrototype/StyleV2/Game"+v.mode+".png");yield return new UnityEngine.WaitForSeconds(.2f);}
}
v.SetMode(HundredHour.RealityShow.Cinema.ShowCharacterVisual.VisualMode.Wire);
bool oldAnimate=v.animate;v.animate=false;int oldLayer=v.wire.gameObject.layer;v.wire.gameObject.layer=31;
var go=new UnityEngine.GameObject("Wire temporal verification");var cam=go.AddComponent<UnityEngine.Camera>();cam.enabled=false;cam.cullingMask=1<<31;
cam.transform.position=v.transform.position+new UnityEngine.Vector3(1.1f,.8f,-3.3f);cam.transform.LookAt(v.transform.position+new UnityEngine.Vector3(0,.35f,0));cam.fieldOfView=35;cam.clearFlags=UnityEngine.CameraClearFlags.SolidColor;cam.backgroundColor=UnityEngine.Color.black;cam.allowHDR=true;
var glow=go.AddComponent<HundredHour.Environments.MansionSoftGlow>();glow.shader=UnityEngine.Shader.Find("Hidden/HundredHour/Mansion Soft Glow");
var rt=new UnityEngine.RenderTexture(600,700,24,UnityEngine.RenderTextureFormat.DefaultHDR);cam.targetTexture=rt;
UnityEngine.Color32[] first=null;int changed=0;
try {
for(int frame=0;frame<3;frame++){
yield return new UnityEngine.WaitForSeconds(.8f);
cam.Render();var previous=UnityEngine.RenderTexture.active;UnityEngine.RenderTexture.active=rt;
var tex=new UnityEngine.Texture2D(600,700,UnityEngine.TextureFormat.RGB24,false);tex.ReadPixels(new UnityEngine.Rect(0,0,600,700),0,0);tex.Apply();
if(UnityEngine.QualitySettings.activeColorSpace==UnityEngine.ColorSpace.Linear){tex.SetPixels(System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Select(tex.GetPixels(),c=>c.gamma)));tex.Apply();}
var pixels=tex.GetPixels32();if(frame==0)first=pixels;else for(int n=0;n<pixels.Length;n++){if(System.Math.Abs(pixels[n].r-first[n].r)+System.Math.Abs(pixels[n].g-first[n].g)+System.Math.Abs(pixels[n].b-first[n].b)>30)changed++;}
System.IO.File.WriteAllBytes("Docs/CharacterPrototype/StyleV2/WireFlow"+frame+".png",tex.EncodeToPNG());UnityEngine.RenderTexture.active=previous;UnityEngine.Object.Destroy(tex);
}
report.Add("Wire temporal pixel change (frozen pose): "+(changed>100)+" ("+changed+" changed samples)");
}finally{v.wire.gameObject.layer=oldLayer;v.animate=oldAnimate;cam.targetTexture=null;rt.Release();UnityEngine.Object.Destroy(rt);UnityEngine.Object.Destroy(go);}
v.SetMode(modeBefore);body.position=original;body.linearVelocity=UnityEngine.Vector3.zero;
System.IO.File.WriteAllLines("Docs/CharacterPrototype/StyleV2/PlayVerification.txt",report);
}
UnityEngine.Object.FindFirstObjectByType<HundredHour.RealityShow.Cinema.ShowCharacterVisual>().StartCoroutine(Check());return "Checking six styles, movement, UI, and temporal wire rendering";
