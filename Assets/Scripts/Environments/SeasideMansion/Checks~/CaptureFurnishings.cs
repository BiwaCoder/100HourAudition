var dir="Docs/MansionDetailV3";System.IO.Directory.CreateDirectory(dir);
var main=UnityEngine.Camera.main;var go=new UnityEngine.GameObject("Temporary V3 verification camera");var cam=go.AddComponent<UnityEngine.Camera>();cam.CopyFrom(main);cam.enabled=false;cam.fieldOfView=48;cam.allowHDR=true;cam.targetTexture=new UnityEngine.RenderTexture(1440,900,24,UnityEngine.RenderTextureFormat.DefaultHDR);var previous=UnityEngine.RenderTexture.active;
var mist=main.GetComponent<MistFilterEffect>();if(mist)UnityEditor.EditorUtility.CopySerialized(mist,go.AddComponent<MistFilterEffect>());
var glow=main.GetComponent<HundredHour.Environments.MansionSoftGlow>();if(glow)UnityEditor.EditorUtility.CopySerialized(glow,go.AddComponent<HundredHour.Environments.MansionSoftGlow>());
try{
var names=new[]{"lantern","lounge","architecture","courtyard"};
var positions=new[]{new UnityEngine.Vector3(3.5f,1.15f,8.4f),new UnityEngine.Vector3(4.8f,1.65f,-11.2f),new UnityEngine.Vector3(7,2.1f,7),new UnityEngine.Vector3(5,2.5f,8)};
var targets=new[]{new UnityEngine.Vector3(4.5f,.86f,7),new UnityEngine.Vector3(5,.7f,-14.6f),new UnityEngine.Vector3(12,2,1),new UnityEngine.Vector3(0,1.7f,-3)};
for(int i=0;i<names.Length;i++){cam.transform.SetPositionAndRotation(positions[i],UnityEngine.Quaternion.LookRotation(targets[i]-positions[i]));cam.Render();UnityEngine.RenderTexture.active=cam.targetTexture;var tex=new UnityEngine.Texture2D(1440,900,UnityEngine.TextureFormat.RGB24,false);tex.ReadPixels(new UnityEngine.Rect(0,0,1440,900),0,0);tex.Apply();System.IO.File.WriteAllBytes(dir+"/"+names[i]+".png",tex.EncodeToPNG());UnityEngine.Object.DestroyImmediate(tex);}
}finally{UnityEngine.RenderTexture.active=previous;UnityEngine.Object.DestroyImmediate(cam.targetTexture);UnityEngine.Object.DestroyImmediate(go);}
return "Four V3 views captured with mansion post processing";
