using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
namespace HundredHour.Environments.Editor
{
 public static class MansionWaterPolish
 {
  public static void Apply()
  {
   if(EditorApplication.isPlaying||UnityEngine.SceneManagement.SceneManager.GetActiveScene().name!="SeasideMansion")throw new System.InvalidOperationException("SeasideMansion Edit mode required");
   const string root="Assets/Environments/SeasideMansion/DetailV2";
   var far=AssetDatabase.LoadAssetAtPath<Material>(root+"/Materials/D2 Horizon.mat");far.SetFloat("_Horizon",1);EditorUtility.SetDirty(far);
   var cascade=AssetDatabase.LoadAssetAtPath<Material>(root+"/Materials/D2 Cascade.mat");cascade.shader=Shader.Find("HundredHour/Fountain Cascade");cascade.color=new Color(.43f,.72f,.76f,.43f);EditorUtility.SetDirty(cascade);
   var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(root+"/BasinCollision.asset");
   if(mesh.normals[mesh.vertexCount-1].y<0){var t=mesh.triangles;for(int i=0;i<t.Length;i+=3){int swap=t[i];t[i]=t[i+2];t[i+2]=swap;}mesh.triangles=t;mesh.RecalculateNormals();EditorUtility.SetDirty(mesh);}
   foreach(var n in new[]{"Ocean","Ocean - modifier mesh"})foreach(var r in GameObject.Find(n).GetComponentsInChildren<Renderer>()){r.reflectionProbeUsage=UnityEngine.Rendering.ReflectionProbeUsage.Off;r.lightProbeUsage=UnityEngine.Rendering.LightProbeUsage.Off;}
   var fountain=GameObject.Find("Fountain detail");var mc=fountain.GetComponent<MeshCollider>();mc.sharedMesh=null;mc.sharedMesh=mesh;
   var material=AssetDatabase.LoadAssetAtPath<Material>(root+"/Materials/Water droplets.mat");if(!material){material=new Material(Shader.Find("HundredHour/Water Droplet"));AssetDatabase.CreateAsset(material,root+"/Materials/Water droplets.mat");}
   for(int i=0;i<3;i++)
   {
    string n="Fine splash "+i;if(fountain.transform.Find(n))continue;
    var go=new GameObject(n);go.transform.SetParent(fountain.transform,false);go.transform.localPosition=new Vector3(0,i==0?.47f:i==1?1.67f:2.63f,0);
    var ps=go.AddComponent<ParticleSystem>();ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
    var main=ps.main;main.loop=true;main.startLifetime=new ParticleSystem.MinMaxCurve(.3f,.65f);main.startSpeed=.18f;main.startSize=new ParticleSystem.MinMaxCurve(.012f,.04f);main.maxParticles=i==0?128:64;main.gravityModifier=.22f;main.startColor=new Color(.7f,.88f,.95f,.7f);main.simulationSpace=ParticleSystemSimulationSpace.Local;main.playOnAwake=true;
    var emission=ps.emission;emission.rateOverTime=i==0?100:45;
    var shape=ps.shape;shape.shapeType=ParticleSystemShapeType.Circle;shape.radius=i==0?2.62f:i==1?1.37f:.77f;shape.radiusThickness=.08f;shape.rotation=new Vector3(90,0,0);
    var vel=ps.velocityOverLifetime;vel.enabled=true;vel.space=ParticleSystemSimulationSpace.Local;vel.x=new ParticleSystem.MinMaxCurve(0,0);vel.y=new ParticleSystem.MinMaxCurve(.25f,.65f);vel.z=new ParticleSystem.MinMaxCurve(0,0);
    var size=ps.sizeOverLifetime;size.enabled=true;size.size=new ParticleSystem.MinMaxCurve(1,new AnimationCurve(new Keyframe(0,.4f),new Keyframe(.3f,1),new Keyframe(1,0)));
    var renderer=ps.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=material;renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;renderer.receiveShadows=false;
   }
   foreach(var ps in fountain.GetComponentsInChildren<ParticleSystem>()){var vel=ps.velocityOverLifetime;vel.x=new ParticleSystem.MinMaxCurve(0,0);vel.y=new ParticleSystem.MinMaxCurve(.25f,.65f);vel.z=new ParticleSystem.MinMaxCurve(0,0);}
   PrefabUtility.SaveAsPrefabAsset(GameObject.Find("Mansion Detail V2"),root+"/MansionDetails.prefab");AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
  }
 }
}
