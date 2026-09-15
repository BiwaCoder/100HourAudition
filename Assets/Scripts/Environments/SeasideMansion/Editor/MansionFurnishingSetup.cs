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
 public static class MansionFurnishingSetup
 {
  const string Root="Assets/Environments/SeasideMansion/DetailV3";
  [Serializable] class Palette {public Entry[] materials;}
  [Serializable] class Entry {public string name;public float[] color;public float metallic,roughness,emission;public int kind;}
  [Serializable] class Layout {public Placement[] placements;public string[] architecture;}
  [Serializable] class Placement {public string asset;public float[] position,scale,rotation;public float angle;}
  [MenuItem("Tools/100Hour/Apply Mansion Furnishings V3")]
  public static void Apply()
  {
   var scene=SceneManager.GetActiveScene();
   if(EditorApplication.isPlaying||scene.path!="Assets/Scenes/GameScene/SeasideMansion.unity")throw new InvalidOperationException("Open SeasideMansion outside Play mode.");
   if(GameObject.Find("Mansion Detail V3"))throw new InvalidOperationException("V3 already applied.");
   Directory.CreateDirectory("Backups/MansionDetailV3");EditorSceneManager.SaveScene(scene,"Backups/MansionDetailV3/BeforeFurnishings.unity",true);
   Directory.CreateDirectory(Root+"/Materials");AssetDatabase.Refresh();
   var palette=JsonUtility.FromJson<Palette>(File.ReadAllText(Root+"/Palette.json"));var mats=new Dictionary<string,Material>();
   foreach(var e in palette.materials){
    var path=Root+"/Materials/"+e.name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
    if(!m){m=new Material(Shader.Find("HundredHour/Coastal Furnishing"));AssetDatabase.CreateAsset(m,path);}
    m.color=new Color(e.color[0],e.color[1],e.color[2]).gamma;m.SetFloat("_Metallic",e.metallic);m.SetFloat("_Glossiness",1-e.roughness);m.SetFloat("_Kind",e.kind);
    m.SetColor("_EmissionColor",new Color(e.color[0],e.color[1],e.color[2])*e.emission);m.enableInstancing=true;EditorUtility.SetDirty(m);mats[e.name]=m;
   }
   foreach(var file in Directory.GetFiles(Root,"*.fbx")){
    var imp=(ModelImporter)AssetImporter.GetAtPath(file);imp.importAnimation=false;imp.importCameras=false;imp.importLights=false;imp.isReadable=false;imp.materialImportMode=ModelImporterMaterialImportMode.ImportStandard;
    foreach(var m in mats)imp.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material),m.Key),m.Value);imp.SaveAndReimport();
   }
   var root=new GameObject("Mansion Detail V3");var original=GameObject.Find("Seaside Mansion");
   string[] prefixes={"Lounge sofa","Teal cushion","Table pedestal","Round walnut table","Garden lantern","Dining tabletop"};int hidden=0;
   foreach(var t in original.GetComponentsInChildren<Transform>(true))if(prefixes.Any(p=>t.name.StartsWith(p,StringComparison.Ordinal))){t.gameObject.SetActive(false);hidden++;}
   // Existing wall geometry and collision stay in place; their materials acquire tactile surface shading.
   foreach(var r in original.GetComponentsInChildren<Renderer>(true)){
    var a=r.sharedMaterials;bool changed=false;
    for(int i=0;i<a.Length;i++)if(a[i]){
     if(a[i].name.StartsWith("Warm ivory stucco")){a[i]=mats["D3 Plaster"];changed=true;}
     else if(a[i].name.StartsWith("White marble trim")){a[i]=mats["D3 Limestone"];changed=true;}
    }
    if(changed)r.sharedMaterials=a;
   }
   var layout=JsonUtility.FromJson<Layout>(File.ReadAllText(Root+"/Layout.json"));
   foreach(var p in layout.placements){
    var g=new GameObject(p.asset+" crafted");g.transform.SetParent(root.transform,false);g.transform.position=new Vector3(p.position[0],p.position[2],-p.position[1]);g.transform.localScale=new Vector3(p.scale[0],p.scale[2],p.scale[1]);
    // Blender rotations are XYZ; conjugate by +90 degrees about X to map Z-up into Unity Y-up.
    if(p.rotation!=null&&p.rotation.Length==3){var basis=Quaternion.Euler(-90,0,0);var q=Quaternion.AngleAxis(p.rotation[2]*Mathf.Rad2Deg,Vector3.forward)*Quaternion.AngleAxis(p.rotation[1]*Mathf.Rad2Deg,Vector3.up)*Quaternion.AngleAxis(p.rotation[0]*Mathf.Rad2Deg,Vector3.right);g.transform.rotation=basis*q*Quaternion.Inverse(basis);}
    else g.transform.rotation=Quaternion.Euler(0,p.angle,0);
    var lods=new LOD[2];for(int i=0;i<2;i++){var o=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>($"{Root}/{p.asset}LOD{i}.fbx"));o.transform.SetParent(g.transform,false);lods[i]=new LOD(i==0?.14f:.009f,o.GetComponentsInChildren<Renderer>());}
    var group=g.AddComponent<LODGroup>();group.SetLODs(lods);group.RecalculateBounds();
    if(p.asset=="Sofa"){var c=g.AddComponent<BoxCollider>();c.center=new Vector3(0,.59f,0);c.size=new Vector3(2.6f,1.18f,.95f);}
    if(p.asset=="Table"){var c=g.AddComponent<BoxCollider>();c.size=new Vector3(1.6f,.75f,1.6f);c.center=new Vector3(0,.375f,0);}
    if(p.asset=="Lantern"){
     var lightObject=new GameObject("Warm opal spill");lightObject.transform.SetParent(g.transform,false);lightObject.transform.localPosition=new Vector3(0,1.02f,0);var l=lightObject.AddComponent<Light>();l.type=LightType.Point;l.color=new Color(1,.70f,.37f);l.intensity=.8f;l.range=2.3f;l.shadows=LightShadows.None;l.renderMode=LightRenderMode.ForcePixel;
    }
   }
   foreach(var n in layout.architecture){var o=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/"+n+".fbx"));o.transform.SetParent(root.transform,false);}
   var cam=Camera.main;if(cam){cam.allowHDR=true;var glow=cam.GetComponent<MansionSoftGlow>()??cam.gameObject.AddComponent<MansionSoftGlow>();glow.shader=Shader.Find("Hidden/HundredHour/Mansion Soft Glow");glow.intensity=.35f;glow.threshold=1.05f;}
   PrefabUtility.SaveAsPrefabAsset(root,Root+"/MansionFurnishings.prefab");AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);Debug.Log("Furnishings V3 complete; replaced "+hidden+" original objects.");
  }
 }
}
