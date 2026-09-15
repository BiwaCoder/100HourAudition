System.Collections.IEnumerator Check(){
var v=UnityEngine.GameObject.Find("MansionPlayerCube").GetComponent<HundredHour.RealityShow.Cinema.ShowCharacterVisual>();
var report=new System.Collections.Generic.List<string>();
v.importedModels[0].motion.enabled=false;
v.SetMode(HundredHour.RealityShow.Cinema.ShowCharacterVisual.VisualMode.Wire);
bool oldAnimate=v.animate;v.animate=false;int oldLayer=v.importedModels[0].contours.gameObject.layer;v.importedModels[0].contours.gameObject.layer=31;
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
System.IO.File.WriteAllBytes("Docs/CharacterPrototype/FemaleIntegration/WireFlow"+frame+".png",tex.EncodeToPNG());UnityEngine.RenderTexture.active=previous;UnityEngine.Object.Destroy(tex);
}
report.Add("Wire temporal pixel change (frozen pose): "+(changed>100)+" ("+changed+" changed samples)");
}finally{v.importedModels[0].contours.gameObject.layer=oldLayer;v.animate=oldAnimate;cam.targetTexture=null;rt.Release();UnityEngine.Object.Destroy(rt);UnityEngine.Object.Destroy(go);}
v.importedModels[0].motion.enabled=true;v.SetMode(HundredHour.RealityShow.Cinema.ShowCharacterVisual.VisualMode.StreetToon);
System.IO.File.WriteAllLines("Docs/CharacterPrototype/FemaleIntegration/WireVerification.txt",report);
}
UnityEngine.GameObject.Find("MansionPlayerCube").GetComponent<HundredHour.RealityShow.Cinema.ShowCharacterVisual>().StartCoroutine(Check());return "Wire test running";