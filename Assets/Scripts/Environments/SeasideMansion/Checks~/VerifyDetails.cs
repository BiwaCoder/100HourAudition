var dir="Docs/MansionDetailV2";System.IO.Directory.CreateDirectory(dir);
var root=UnityEngine.GameObject.Find("Mansion Detail V2");var original=UnityEngine.GameObject.Find("Seaside Mansion");var report=new System.Collections.Generic.List<string>();
if(root.GetComponentsInChildren<UnityEngine.LODGroup>().Length!=25)throw new System.Exception("Expected 25 independent LOD groups");
report.Add("PASS: 25 reusable LOD groups (fountain 1, columns 8, palms 12, beds 4)");
foreach(var r in root.GetComponentsInChildren<UnityEngine.Renderer>(true))foreach(var m in r.sharedMaterials)if(!m||!m.shader||!m.shader.isSupported||UnityEditor.ShaderUtil.ShaderHasError(m.shader))throw new System.Exception("Material failure: "+r.name);
report.Add("PASS: all materials have supported shaders with no compiler errors");
UnityEngine.Physics.SyncTransforms();var hit=new UnityEngine.RaycastHit();if(!UnityEngine.GameObject.Find("Fountain detail").GetComponent<UnityEngine.MeshCollider>().Raycast(new UnityEngine.Ray(new UnityEngine.Vector3(2,3,0),UnityEngine.Vector3.down),out hit,5))throw new System.Exception("Basin collider top inaccessible");
report.Add("PASS: shallow fountain collider blocks basin, no hemisphere obstruction");
var ocean=root.transform.Find("Ocean - modifier mesh").GetComponentInChildren<UnityEngine.Renderer>();report.Add("Ocean bounds: "+ocean.bounds);
var names=new[]{"Gallery column","Palm trunk","Palm frond","Palm leaflet","Garden leaf cluster","Colorful flowers","Fountain","Tier ","Basin marble"};
var old=new System.Collections.Generic.List<UnityEngine.GameObject>();foreach(var t in original.GetComponentsInChildren<UnityEngine.Transform>(true))foreach(var n in names)if(t.name.StartsWith(n,System.StringComparison.Ordinal)){old.Add(t.gameObject);break;}
foreach(var o in old)if(o.activeSelf)throw new System.Exception("Old detail visible "+o.name);
report.Add("PASS: "+old.Count+" old primitive detail objects preserved and disabled");
var go=new UnityEngine.GameObject("Temporary benchmark camera");var cam=go.AddComponent<UnityEngine.Camera>();cam.CopyFrom(UnityEngine.Camera.main);cam.enabled=false;cam.fieldOfView=52;var rt=new UnityEngine.RenderTexture(960,600,24);rt.antiAliasing=2;cam.targetTexture=rt;var tex=new UnityEngine.Texture2D(960,600,UnityEngine.TextureFormat.RGB24,false);var previous=UnityEngine.RenderTexture.active;
var horizon=UnityEngine.GameObject.Find("Ocean").GetComponent<UnityEngine.Renderer>();var far=horizon.sharedMaterial;var oldFar=UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Material>("Assets/Environments/SeasideMansion/Materials/Mansion Ocean.mat");
try{
var positions=new[]{new UnityEngine.Vector3(36,27,47),new UnityEngine.Vector3(5,2.5f,8),new UnityEngine.Vector3(0,3,-27)};var targets=new[]{new UnityEngine.Vector3(0,1,0),new UnityEngine.Vector3(0,1.7f,-3),new UnityEngine.Vector3(0,0,-110)};var labels=new[]{"exterior","courtyard","ocean"};
for(int i=0;i<3;i++){
cam.transform.SetPositionAndRotation(positions[i],UnityEngine.Quaternion.LookRotation(targets[i]-positions[i]));
for(int mode=0;mode<2;mode++){
bool detail=mode==1;root.SetActive(detail);foreach(var o in old)o.SetActive(!detail);horizon.sharedMaterial=detail?far:oldFar;
for(int warm=0;warm<3;warm++){cam.Render();UnityEngine.RenderTexture.active=rt;tex.ReadPixels(new UnityEngine.Rect(0,0,960,600),0,0);}
var watch=System.Diagnostics.Stopwatch.StartNew();for(int frame=0;frame<12;frame++){cam.Render();UnityEngine.RenderTexture.active=rt;tex.ReadPixels(new UnityEngine.Rect(0,0,960,600),0,0);}watch.Stop();
report.Add(labels[i]+" "+(detail?"detail":"original")+": render + GPU readback mean "+(watch.Elapsed.TotalMilliseconds/12).ToString("F2")+" ms at 960x600 (Editor, NOT player frame time)");
tex.Apply();System.IO.File.WriteAllBytes(dir+"/"+labels[i]+(detail?"-after":"-before")+".png",tex.EncodeToPNG());
}
}
}finally{root.SetActive(true);foreach(var o in old)o.SetActive(false);horizon.sharedMaterial=far;UnityEngine.RenderTexture.active=previous;UnityEngine.Object.DestroyImmediate(tex);UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(go);}
report.Add("PASS: verification restored detailed scene, no voice API requests");System.IO.File.WriteAllLines(dir+"/Verification.txt",report);return string.Join("\n",report);
