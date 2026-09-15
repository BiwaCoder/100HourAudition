var root=UnityEngine.GameObject.Find("Mansion Detail V3");var sb=new System.Text.StringBuilder();
sb.AppendLine("Scene: "+UnityEngine.SceneManagement.SceneManager.GetActiveScene().name+"; Play="+UnityEditor.EditorApplication.isPlaying);
sb.AppendLine("LOD groups: "+root.GetComponentsInChildren<UnityEngine.LODGroup>(true).Length);
sb.AppendLine("Lights: "+root.GetComponentsInChildren<UnityEngine.Light>(true).Length);
foreach(var n in new[]{"Lantern","Sofa","ThrowPillow","Table"})sb.AppendLine(n+": "+root.transform.Cast<UnityEngine.Transform>().Count(t=>t.name==n+" crafted"));
var bad=root.GetComponentsInChildren<UnityEngine.Renderer>(true).Where(r=>r.sharedMaterials.Any(m=>!m||!m.shader||!m.shader.isSupported)).Select(r=>r.name).ToArray();sb.AppendLine("Missing/unsupported materials: "+bad.Length);
foreach(var n in new[]{"HundredHour/Coastal Furnishing","Hidden/HundredHour/Mansion Soft Glow"})sb.AppendLine(n+" shader messages: "+UnityEditor.ShaderUtil.GetShaderMessages(UnityEngine.Shader.Find(n)).Length);
var camGo=new UnityEngine.GameObject("Temporary V3 performance camera");var cam=camGo.AddComponent<UnityEngine.Camera>();cam.CopyFrom(UnityEngine.Camera.main);cam.enabled=false;cam.fieldOfView=48;cam.transform.SetPositionAndRotation(new UnityEngine.Vector3(4.8f,1.65f,-11.2f),UnityEngine.Quaternion.LookRotation(new UnityEngine.Vector3(.2f,-.95f,-3.4f)));cam.targetTexture=new UnityEngine.RenderTexture(960,600,24,UnityEngine.RenderTextureFormat.DefaultHDR);
var glow=camGo.AddComponent<HundredHour.Environments.MansionSoftGlow>();glow.shader=UnityEngine.Shader.Find("Hidden/HundredHour/Mansion Soft Glow");var previous=UnityEngine.RenderTexture.active;var tex=new UnityEngine.Texture2D(960,600,UnityEngine.TextureFormat.RGB24,false);
try{
 foreach(bool enabled in new[]{false,true}){
  glow.enabled=enabled;for(int i=0;i<3;i++)cam.Render();var watch=System.Diagnostics.Stopwatch.StartNew();
  for(int i=0;i<12;i++){cam.Render();UnityEngine.RenderTexture.active=cam.targetTexture;tex.ReadPixels(new UnityEngine.Rect(0,0,960,600),0,0);tex.Apply();}
  watch.Stop();sb.AppendLine("960x600 Camera.Render + synchronous readback, glow "+enabled+": "+(watch.Elapsed.TotalMilliseconds/12).ToString("F2")+" ms (not player FPS)");
 }
}finally{UnityEngine.RenderTexture.active=previous;UnityEngine.Object.DestroyImmediate(tex);UnityEngine.Object.DestroyImmediate(cam.targetTexture);UnityEngine.Object.DestroyImmediate(camGo);}
System.IO.File.WriteAllText("Docs/MansionDetailV3/Verification.txt",sb.ToString());return sb.ToString();
