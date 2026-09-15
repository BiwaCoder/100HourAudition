using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using Unity.Cinemachine;
namespace HundredHour.Environments.Editor
{
    public static class SeasideMansionSetup
    {
        const string Root = "Assets/Environments/SeasideMansion";
        const string ScenePath = "Assets/Scenes/GameScene/SeasideMansion.unity";
        [Serializable] class Palette { public Entry[] materials; }
        [Serializable] class Entry { public string name; public float[] color; public float metallic, roughness, alpha, emissionStrength; public float[] emission; }
        [MenuItem("Tools/100Hour/Configure New Seaside Mansion Scene")]
        public static void Configure()
        {
            var scene=SceneManager.GetActiveScene();
            if(EditorApplication.isPlaying || scene.path!=ScenePath) throw new InvalidOperationException("Open the new SeasideMansion scene outside Play mode.");
            if(GameObject.Find("Seaside Mansion")) throw new InvalidOperationException("Mansion already configured; edit its existing objects.");
            Directory.CreateDirectory(Root+"/Materials"); AssetDatabase.Refresh();
            var materials=new Dictionary<string,Material>();
            var palette=JsonUtility.FromJson<Palette>(File.ReadAllText(Root+"/MaterialPalette.json"));
            foreach(var e in palette.materials)
            {
                var m=new Material(Shader.Find("Standard")){name=e.name};
                // Blender stores linear colours; Unity material colour properties use sRGB.
                var c=new Color(e.color[0],e.color[1],e.color[2],1).gamma;c.a=e.alpha;m.color=c;
                m.SetFloat("_Metallic",e.metallic);m.SetFloat("_Glossiness",1-e.roughness);
                if(e.alpha<.99f)
                {
                    m.SetFloat("_Mode",3);m.SetInt("_SrcBlend",(int)BlendMode.One);m.SetInt("_DstBlend",(int)BlendMode.OneMinusSrcAlpha);m.SetInt("_ZWrite",0);
                    m.EnableKeyword("_ALPHAPREMULTIPLY_ON");m.renderQueue=3000;m.SetOverrideTag("RenderType","Transparent");
                }
                if(e.emissionStrength>0){m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",new Color(e.emission[0],e.emission[1],e.emission[2])*e.emissionStrength);}
                AssetDatabase.CreateAsset(m,Root+"/Materials/"+e.name+".mat");materials.Add(e.name,m);
            }
            var importer=(ModelImporter)AssetImporter.GetAtPath(Root+"/SeasideMansion.fbx");importer.isReadable=false;importer.importCameras=false;importer.importLights=false;importer.importAnimation=false;
            foreach(var p in materials)importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material),p.Key),p.Value);
            importer.SaveAndReimport();
            var model=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/SeasideMansion.fbx"));model.name="Seaside Mansion";
            // Only remove environment objects from the new copy of NatureStage.
            foreach(var name in new[]{"Terrain","_Dead_Bushes","_Flowers","_Plants","-----","Camera"})
            {var g=GameObject.Find(name);if(g!=null)UnityEngine.Object.DestroyImmediate(g);}
            foreach(var r in model.GetComponentsInChildren<MeshRenderer>())
            {
                r.sharedMaterials=r.sharedMaterials.Select(m=>m!=null&&materials.ContainsKey(m.name)?materials[m.name]:m).ToArray();
                string n=r.name;
                bool decoration=n.Contains("flower")||n.Contains("leaf")||n.Contains("Leaf")||n.Contains("frond")||n.Contains("inlay")||n.Contains("ribbon")||n.Contains("cascade")||n.Contains("water")||n.Contains("ornament")||n.Contains("cushion");
                if(!decoration){var mc=r.gameObject.AddComponent<MeshCollider>();mc.sharedMesh=r.GetComponent<MeshFilter>().sharedMesh;}
                GameObjectUtility.SetStaticEditorFlags(r.gameObject,StaticEditorFlags.BatchingStatic|StaticEditorFlags.ReflectionProbeStatic);
            }
            PrefabUtility.SaveAsPrefabAsset(model,Root+"/SeasideMansion.prefab");
            var sky=UnityEngine.Object.FindFirstObjectByType<Funly.SkyStudio.TimeOfDayController>();
            if(sky!=null)
            {
                var profile=UnityEngine.Object.Instantiate(sky.skyProfile);profile.name="Mansion Sunset";
                var skyMat=new Material(profile.skyboxMaterial);AssetDatabase.CreateAsset(skyMat,Root+"/Materials/Mansion Sky.mat");profile.skyboxMaterial=skyMat;
                AssetDatabase.CreateAsset(profile,Root+"/Mansion Sunset.asset");sky.copySkyProfile=false;sky.skyProfile=profile;sky.automaticTimeIncrement=false;sky.skyTime=.70f;
            }
            var sun=GameObject.Find("Sun").GetComponent<Light>();var rotation=sun.GetComponent<LowPolyVegetation_SunControl>();if(rotation)rotation.enabled=false;
            sun.transform.rotation=Quaternion.Euler(24,145,0);sun.intensity=1.05f;sun.color=new Color(1,.79f,.62f);sun.shadows=LightShadows.Soft;sun.shadowBias=.025f;
            var fill=GameObject.Find("Sun Ambient").GetComponent<Light>();fill.intensity=.2f;fill.shadows=LightShadows.None;
            RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.57f,.61f,.68f);RenderSettings.ambientEquatorColor=new Color(.42f,.38f,.35f);RenderSettings.ambientGroundColor=new Color(.25f,.23f,.22f);RenderSettings.fog=true;RenderSettings.fogMode=FogMode.ExponentialSquared;RenderSettings.fogDensity=.001f;RenderSettings.fogColor=new Color(.76f,.66f,.61f);
            var environment=new GameObject("Mansion Shore and Lighting");
            var sand=NewMaterial("Beach sand",new Color(.66f,.57f,.43f),.12f);
            Box("Beach",new Vector3(0,-.45f,10),new Vector3(180,.3f,85),sand,environment.transform);
            var waterSource=AssetDatabase.LoadAssetAtPath<Material>("Assets/Low Poly Vegetation Pack/Bonus Assets/Materials/Water_01.mat");
            var ocean=new Material(waterSource);ocean.name="Mansion Ocean";AssetDatabase.CreateAsset(ocean,Root+"/Materials/Mansion Ocean.mat");
            Box("Ocean",new Vector3(0,-.65f,-1030),new Vector3(4000,.1f,2000),ocean,environment.transform,false);
            foreach(var p in new[]{new Vector3(-5,3.4f,-14),new Vector3(5,3.4f,-14),new Vector3(-16,3.4f,0),new Vector3(16,3.4f,0),new Vector3(-7,3.4f,14),new Vector3(7,3.4f,14)})
            {var g=new GameObject("Warm interior fill");g.transform.SetParent(environment.transform);g.transform.position=p;var l=g.AddComponent<Light>();l.type=LightType.Point;l.color=new Color(1,.75f,.5f);l.intensity=1.4f;l.range=10;l.shadows=LightShadows.None;}
            var probeGo=new GameObject("Mansion reflection probe");probeGo.transform.SetParent(environment.transform);probeGo.transform.position=new Vector3(0,2,0);var probe=probeGo.AddComponent<ReflectionProbe>();probe.mode=ReflectionProbeMode.Realtime;probe.refreshMode=ReflectionProbeRefreshMode.OnAwake;probe.size=new Vector3(46,10,46);probe.resolution=128;probe.boxProjection=true;probe.intensity=.6f;
            var camera=Camera.main;camera.targetTexture=null;camera.allowHDR=true;camera.nearClipPlane=.08f;camera.farClipPlane=2500;
            var mist=camera.GetComponent<MistFilterEffect>();mist.mistIntensity=.28f;mist.mistBlur=1.6f;mist.highlightBoost=1.25f;mist.hazeAmount=.025f;
            var player=GameObject.Find("NaturePlayerCube");player.name="MansionPlayerCube";player.transform.position=new Vector3(0,.85f,23);
            var vcam=GameObject.Find("NatureFollowCamera");var follow=vcam.GetComponent<CinemachineFollow>();follow.FollowOffset=new Vector3(0,1.65f,2.6f);vcam.GetComponent<CinemachineCamera>().Lens.FieldOfView=62;
            var tour=new GameObject("Mansion Tour - keys 1 2 3 4").AddComponent<MansionSceneTour>();tour.outputCamera=camera;tour.brain=camera.GetComponent<CinemachineBrain>();tour.followCamera=vcam;tour.player=player.GetComponent<Rigidbody>();tour.mover=player.GetComponent<HundredHour.JoystickDemo.JoystickCubeMover>();tour.joystickCanvas=GameObject.Find("NatureJoystickCanvas");
            tour.viewpoints=new[]{View("01 Exterior",new Vector3(44,33,56),new Vector3(0,1,0),tour.transform),View("02 Courtyard",new Vector3(0,1.8f,8),new Vector3(0,2,-3),tour.transform),View("03 Ocean lounge",new Vector3(-5,1.7f,-10.2f),new Vector3(0,1.8f,-22),tour.transform)};
            tour.brain.enabled=false;vcam.SetActive(false);tour.joystickCanvas.SetActive(false);tour.mover.enabled=false;tour.player.isKinematic=true;
            camera.transform.SetPositionAndRotation(tour.viewpoints[0].position,tour.viewpoints[0].rotation);camera.fieldOfView=48;
            TuneSunset();AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);Selection.activeGameObject=model;
            SceneView.lastActiveSceneView?.LookAt(new Vector3(0,2,0),Quaternion.Euler(35,-140,0),65);
            Debug.Log("Seaside Mansion imported; keys 1 exterior / 2 courtyard / 3 lounge / 4 joystick walk.");
        }
        public static void TuneSunset()
        {
            var sky=UnityEngine.Object.FindFirstObjectByType<Funly.SkyStudio.TimeOfDayController>();
            var profile=sky.skyProfile;
            if(AssetDatabase.GetAssetPath(profile)!=Root+"/Mansion Sunset.asset")throw new InvalidOperationException("Only tune the mansion profile copy.");
            string[] keys={"SkyUpperColorKey","SkyMiddleColorKey","SkyLowerColorKey","FogColorKey"};
            Color[] colors={new Color(.24f,.34f,.52f),new Color(.94f,.65f,.48f),new Color(.57f,.38f,.32f),new Color(.78f,.65f,.54f)};
            for(int i=0;i<keys.Length;i++){var group=profile.GetGroup<Funly.SkyStudio.ColorKeyframeGroup>(keys[i]);foreach(var k in group.keyframes)k.color=colors[i];}
            foreach(var k in profile.GetGroup<Funly.SkyStudio.NumberKeyframeGroup>("FogDensityKey").keyframes)k.value=.06f;
            sky.skyTime=.70f;EditorUtility.SetDirty(profile);
            var water=AssetDatabase.LoadAssetAtPath<Material>(Root+"/Materials/Mansion Ocean.mat");water.color=new Color(.12f,.32f,.38f);water.SetFloat("_Glossiness",.65f);water.SetColor("_SpecColor",new Color(.2f,.2f,.2f));EditorUtility.SetDirty(water);
            var ocean=GameObject.Find("Ocean");ocean.transform.position=new Vector3(0,-.65f,0);ocean.transform.localScale=new Vector3(20000,.1f,20000);
            var marble=AssetDatabase.LoadAssetAtPath<Material>(Root+"/Materials/White marble trim.mat");marble.SetFloat("_Glossiness",.3f);EditorUtility.SetDirty(marble);
            var tour=UnityEngine.Object.FindFirstObjectByType<MansionSceneTour>();tour.viewpoints[0].position=new Vector3(36,27,47);tour.viewpoints[0].rotation=Quaternion.LookRotation(new Vector3(0,1,0)-tour.viewpoints[0].position);
            tour.outputCamera.farClipPlane=15000;tour.outputCamera.transform.SetPositionAndRotation(tour.viewpoints[0].position,tour.viewpoints[0].rotation);
            tour.followCamera.GetComponent<CinemachineCamera>().Lens.FarClipPlane=15000;
        }
        static Material NewMaterial(string name,Color color,float smooth){var m=new Material(Shader.Find("Standard")){color=color};m.SetFloat("_Glossiness",smooth);AssetDatabase.CreateAsset(m,Root+"/Materials/"+name+".mat");return m;}
        static void Box(string name,Vector3 p,Vector3 s,Material m,Transform parent,bool collide=true){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(parent);g.transform.position=p;g.transform.localScale=s;g.GetComponent<Renderer>().sharedMaterial=m;if(!collide)UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());}
        static Transform View(string name,Vector3 p,Vector3 target,Transform parent){var g=new GameObject(name);g.transform.SetParent(parent);g.transform.position=p;g.transform.rotation=Quaternion.LookRotation(target-p);return g.transform;}
    }
}
