using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
namespace HundredHour.Environments.Editor
{
 public static class MansionDetailSetup
 {
  const string Root="Assets/Environments/SeasideMansion/DetailV2";
  [Serializable] class Palette {public Entry[] materials;}
  [Serializable] class Entry {public string name;public float[] color;public float metallic,roughness;}
  [MenuItem("Tools/100Hour/Apply Mansion Detail V2")]
  public static void Apply()
  {
   var scene=SceneManager.GetActiveScene();
   if(EditorApplication.isPlaying||scene.path!="Assets/Scenes/GameScene/SeasideMansion.unity")throw new InvalidOperationException("Open SeasideMansion outside Play mode.");
   if(GameObject.Find("Mansion Detail V2"))throw new InvalidOperationException("Details already applied.");
   Directory.CreateDirectory("Backups/MansionDetailV2");File.Copy(scene.path,"Backups/MansionDetailV2/BeforeDetails.unity",false);
   Directory.CreateDirectory(Root+"/Materials");AssetDatabase.Refresh();
   var palette=JsonUtility.FromJson<Palette>(File.ReadAllText(Root+"/Palette.json"));var mats=new Dictionary<string,Material>();
   foreach(var e in palette.materials)
   {
    bool water=e.name.Contains("Water")||e.name.Contains("Cascade");var path=Root+"/Materials/"+e.name+".mat";
    var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(!m){m=new Material(Shader.Find(water?"HundredHour/Coastal Water":"HundredHour/Coastal Detail"));AssetDatabase.CreateAsset(m,path);}
    m.color=new Color(e.color[0],e.color[1],e.color[2]).gamma;m.SetFloat("_Glossiness",1-e.roughness);m.enableInstancing=true;
    if(water){m.SetFloat("_Wave",e.name.Contains("Cascade")?0:.013f);m.SetFloat("_Cascade",e.name.Contains("Cascade")?1:0);}
    else {m.SetFloat("_Metallic",e.metallic);bool leaf=e.name.Contains("Leaves")||e.name.Contains("Leaf")||e.name.Contains("leaves")||e.name.Contains("petals");m.SetFloat("_Cull",leaf?0:2);m.SetFloat("_Wind",leaf?.035f:0);m.SetFloat("_Grain",e.name.Contains("Limestone")?1:0);}
    EditorUtility.SetDirty(m);mats[e.name]=m;
   }
   foreach(var file in Directory.GetFiles(Root,"*.fbx"))
   {var imp=(ModelImporter)AssetImporter.GetAtPath(file);imp.importAnimation=false;imp.importCameras=false;imp.importLights=false;imp.isReadable=false;imp.materialImportMode=ModelImporterMaterialImportMode.ImportStandard;foreach(var m in mats)imp.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material),m.Key),m.Value);imp.SaveAndReimport();}
   var root=new GameObject("Mansion Detail V2");
   string[] targets={"Gallery column","Palm trunk","Palm frond","Palm leaflet","Garden leaf cluster","Colorful flowers","Fountain","Tier ","Basin marble"};
   var original=GameObject.Find("Seaside Mansion");int hidden=0;
   foreach(var t in original.GetComponentsInChildren<Transform>(true))if(targets.Any(p=>t.name.StartsWith(p,StringComparison.Ordinal))){t.gameObject.SetActive(false);hidden++;}
   Create("Fountain",Vector3.zero,1,0,root.transform);
   foreach(float x in new[]{-10f,-6,6,10})foreach(float z in new[]{-9f,9})Create("Column",new Vector3(x,0,z),1,0,root.transform);
   int k=0;
   foreach(float x in new[]{-8.5f,8.5f})foreach(float z in new[]{-7.8f,7.8f})Create("Palm",new Vector3(x,0,z),.75f,k++*137.5f,root.transform);
   foreach(float x in new[]{-22f,22f})foreach(float z in new[]{17f,5,-8,-18})Create("Palm",new Vector3(x,0,z),1,k++*137.5f,root.transform);
   foreach(float x in new[]{-6.8f,6.8f})foreach(float z in new[]{-5.7f,5.7f})Create("PlantBed",new Vector3(x,0,z),1,0,root.transform);
   var sea=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/OceanSurface.fbx"));sea.name="Ocean - modifier mesh";sea.transform.SetParent(root.transform,false);
   var ocean=new Material(mats["D2 Water"]){name="D2 Ocean"};ocean.SetFloat("_Ocean",1);ocean.SetFloat("_Wave",.085f);ocean.color=new Color(.075f,.25f,.30f);AssetDatabase.CreateAsset(ocean,Root+"/Materials/D2 Ocean.mat");
   foreach(var r in sea.GetComponentsInChildren<Renderer>()){r.sharedMaterial=ocean;r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;}
   var horizon=GameObject.Find("Ocean");var far=new Material(ocean){name="D2 Horizon"};far.SetFloat("_Ocean",0);far.SetFloat("_Horizon",1);far.SetFloat("_Wave",0);AssetDatabase.CreateAsset(far,Root+"/Materials/D2 Horizon.mat");horizon.GetComponent<Renderer>().sharedMaterial=far;horizon.transform.position=new Vector3(0,-1.15f,0);horizon.GetComponent<Renderer>().shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
   // Keep the original walkable foundation while bringing the coast into sight at either side.
   var beach=GameObject.Find("Beach");if(beach)beach.transform.localScale=new Vector3(90,.3f,85);
   PrefabUtility.SaveAsPrefabAsset(root,Root+"/MansionDetails.prefab");AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
   Debug.Log("Mansion Detail V2 applied; old detail objects hidden="+hidden);
  }
  static void Create(string name,Vector3 position,float scale,float angle,Transform parent)
  {
   var g=new GameObject(name+" detail");g.transform.SetParent(parent,false);g.transform.SetPositionAndRotation(position,Quaternion.Euler(0,angle,0));g.transform.localScale=Vector3.one*scale;
   var lods=new LOD[2];
   for(int i=0;i<2;i++){
    var o=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>($"{Root}/{name}LOD{i}.fbx"));o.transform.SetParent(g.transform,false);o.name="LOD"+i;
    var rs=o.GetComponentsInChildren<Renderer>();lods[i]=new LOD(i==0?.18f:.012f,rs);
   }
   var group=g.AddComponent<LODGroup>();group.SetLODs(lods);group.RecalculateBounds();
   if(name=="Column"){var c=g.AddComponent<CapsuleCollider>();c.radius=.31f;c.height=4.6f;c.center=new Vector3(0,2.3f,0);}
   if(name=="Palm"){var c=g.AddComponent<CapsuleCollider>();c.radius=.24f;c.height=6;c.center=new Vector3(.18f,3,0);}
   if(name=="Fountain"){var mc=g.AddComponent<MeshCollider>();mc.sharedMesh=MakeBasinCollider();}
  }
  static Mesh MakeBasinCollider(){const int n=48;var v=new Vector3[n*2+2];var t=new List<int>();v[n*2]=new Vector3(0,0,0);v[n*2+1]=new Vector3(0,.66f,0);for(int i=0;i<n;i++){float a=i*Mathf.PI*2/n;v[i]=new Vector3(Mathf.Cos(a)*3.1f,0,Mathf.Sin(a)*3.1f);v[i+n]=v[i]+Vector3.up*.66f;int j=(i+1)%n;t.AddRange(new[]{i,j,i+n,j,j+n,i+n,n*2,j,i,n*2+1,i+n,j+n});}var path=Root+"/BasinCollision.asset";var m=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(!m){m=new Mesh{name="Fountain shallow collision"};m.vertices=v;for(int i=0;i<t.Count;i+=3){int swap=t[i];t[i]=t[i+2];t[i+2]=swap;}m.triangles=t.ToArray();m.RecalculateNormals();AssetDatabase.CreateAsset(m,path);}return m;}
 }
}
