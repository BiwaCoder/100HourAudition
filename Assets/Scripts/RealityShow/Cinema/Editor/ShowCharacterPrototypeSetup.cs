using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace HundredHour.RealityShow.Cinema.Editor
{
    public static class ShowCharacterPrototypeSetup
    {
        public const string ScenePath = "Assets/Scenes/GameScene/SeasideMansionCharacterTest.unity";
        const string ArtPath = "Assets/RealityShow/CharacterPrototype";
        static readonly List<Transform> bones = new List<Transform>();
        static Geometry solid, lines;
        static Transform root;

        [MenuItem("Tools/100Hour/Create Mansion Character Test")]
        public static string Create()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play first.");
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null)
                throw new InvalidOperationException("Test scene already exists; open it instead of overwriting.");
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (scene.path != "Assets/Scenes/GameScene/SeasideMansion.unity" || scene.isDirty)
                throw new InvalidOperationException("Open the saved SeasideMansion source first.");
            AssetDatabase.CopyAsset(scene.path, ScenePath);
            EditorSceneManager.OpenScene(ScenePath);
            if (!AssetDatabase.IsValidFolder(ArtPath)) AssetDatabase.CreateFolder("Assets/RealityShow", "CharacterPrototype");
            var player = GameObject.Find("MansionPlayerCube");
            var visual = player.AddComponent<ShowCharacterVisual>();
            visual.cube = player.GetComponent<Renderer>();
            root = new GameObject("Himari Abstract Character").transform;
            root.SetParent(player.transform, false);
            // Compensate for the existing cube's non-uniform scale; feet meet its bottom.
            root.localScale = new Vector3(1 / player.transform.lossyScale.x, 1 / player.transform.lossyScale.y, 1 / player.transform.lossyScale.z);
            root.localPosition = new Vector3(0, -.5f, 0);
            root.localRotation = Quaternion.identity;
            bones.Clear(); solid = new Geometry(); lines = new Geometry();
            var hips = Bone("Hips", root, new Vector3(0, .89f, 0));
            visual.torso = Bone("Chest", hips, new Vector3(0, .35f, 0));
            visual.head = Bone("Head", visual.torso, new Vector3(0, .34f, 0));
            visual.leftArm = Bone("Left shoulder", visual.torso, new Vector3(-.245f, .14f, 0));
            visual.rightArm = Bone("Right shoulder", visual.torso, new Vector3(.245f, .14f, 0));
            visual.leftLeg = Bone("Left hip", hips, new Vector3(-.115f, 0, 0));
            visual.rightLeg = Bone("Right hip", hips, new Vector3(.115f, 0, 0));
            // A plain long jacket, blank oval head and loose sleeves: no face, hair or texture.
            Rings(visual.torso, new[]{new Vector3(0,-.43f,0),new Vector3(0,-.07f,0),new Vector3(0,.17f,0),new Vector3(0,.25f,0)},
                new[]{new Vector2(.265f,.14f),new Vector2(.20f,.12f),new Vector2(.245f,.125f),new Vector2(.085f,.075f)}, 6);
            Rings(visual.head, new[]{new Vector3(0,-.1f,0),Vector3.zero,new Vector3(0,.16f,0),new Vector3(0,.24f,0)},
                new[]{new Vector2(.06f,.06f),new Vector2(.125f,.10f),new Vector2(.13f,.105f),new Vector2(.055f,.055f)}, 6);
            foreach (var arm in new[]{visual.leftArm,visual.rightArm})
            {
                float sign = arm == visual.leftArm ? -1 : 1;
                Rings(arm,new[]{Vector3.zero,new Vector3(sign*.055f,-.27f,.005f),new Vector3(sign*.025f,-.51f,.045f),new Vector3(sign*.02f,-.60f,.06f)},
                    new[]{new Vector2(.072f,.078f),new Vector2(.059f,.06f),new Vector2(.043f,.045f),new Vector2(.034f,.03f)}, 5);
            }
            foreach (var leg in new[]{visual.leftLeg,visual.rightLeg})
            {
                Rings(leg,new[]{Vector3.zero,new Vector3(0,-.39f,0),new Vector3(0,-.79f,0),new Vector3(0,-.85f,.045f)},
                    new[]{new Vector2(.085f,.095f),new Vector2(.066f,.068f),new Vector2(.048f,.055f),new Vector2(.060f,.12f)}, 5);
            }
            var dark = MakeMaterial("Ink", new Color(.045f,.055f,.075f));
            var chalk = MakeMaterial("Chalk", new Color(.83f,.80f,.69f));
            visual.model = root;
            visual.silhouette = Skin("Silhouette",solid,dark);
            visual.wire = Skin("Wire contours",lines,chalk);
            root.localRotation = Quaternion.Euler(0,180,0);
            var signalView=UnityEngine.Object.FindFirstObjectByType<HundredHour.Environments.MansionSignalView>(FindObjectsInactive.Include);
            visual.originalPortraitGraphics=signalView.selfWindow.GetComponentsInChildren<UnityEngine.UI.Graphic>(true);
            AddButton(visual);
            visual.ApplyMode();
            EditorSceneManager.MarkSceneDirty(root.gameObject.scene);
            EditorSceneManager.SaveScene(root.gameObject.scene);
            AssetDatabase.SaveAssets();
            Selection.activeGameObject = player;
            return $"Created {ScenePath}: solid {solid.triangles.Count/3} triangles, wire {lines.triangles.Count/3} triangles, {bones.Count} bones, two skinned renderers.";
        }

        static Transform Bone(string name, Transform parent, Vector3 position)
        {
            var t = new GameObject(name).transform; t.SetParent(parent,false); t.localPosition=position; bones.Add(t); return t;
        }
        static Material MakeMaterial(string name, Color color)
        {
            var m = new Material(Shader.Find("Unlit/Color")) {name=name, color=color};
            AssetDatabase.CreateAsset(m,ArtPath+"/"+name+".mat"); return m;
        }
        static SkinnedMeshRenderer Skin(string name, Geometry data, Material material)
        {
            var mesh = new Mesh {name=name}; mesh.SetVertices(data.vertices); mesh.SetTriangles(data.triangles,0);
            mesh.boneWeights=data.weights.ToArray();
            mesh.bindposes=bones.Select(b=>b.worldToLocalMatrix*root.localToWorldMatrix).ToArray();
            mesh.RecalculateNormals(); mesh.RecalculateBounds();
            AssetDatabase.CreateAsset(mesh,ArtPath+"/"+name+".asset");
            var go=new GameObject(name); go.transform.SetParent(root,false);
            var r=go.AddComponent<SkinnedMeshRenderer>(); r.sharedMesh=mesh; r.sharedMaterial=material;
            r.bones=bones.ToArray(); r.rootBone=bones[0]; r.localBounds=new Bounds(new Vector3(0,.95f,0),new Vector3(1.5f,2.2f,1.6f));
            r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off; r.receiveShadows=false;
            return r;
        }
        static void Rings(Transform bone, Vector3[] centers, Vector2[] radii, int sides)
        {
            if(centers[0].y > centers[centers.Length-1].y) { Array.Reverse(centers); Array.Reverse(radii); }
            int index=bones.IndexOf(bone); var rings=new Vector3[centers.Length][];
            for(int j=0;j<centers.Length;j++)
            {
                rings[j]=new Vector3[sides];
                for(int i=0;i<sides;i++)
                {
                    float a=2*Mathf.PI*i/sides;
                    rings[j][i]=root.InverseTransformPoint(bone.TransformPoint(centers[j]+new Vector3(Mathf.Cos(a)*radii[j].x,0,Mathf.Sin(a)*radii[j].y)));
                }
            }
            for(int j=0;j<rings.Length;j++) for(int i=0;i<sides;i++)
            {
                int next=(i+1)%sides;
                // Outline the end rings and longitudinal edges; no triangulation diagonals.
                if(j==0 || j==rings.Length-1) Tube(rings[j][i],rings[j][next],index);
                if(j<rings.Length-1)
                {
                    solid.Quad(rings[j][i],rings[j+1][i],rings[j+1][next],rings[j][next],index);
                    Tube(rings[j][i],rings[j+1][i],index);
                }
            }
            for(int i=1;i<sides-1;i++)
            {
                solid.Triangle(rings[0][0],rings[0][i],rings[0][i+1],index);
                var end=rings[rings.Length-1]; solid.Triangle(end[0],end[i+1],end[i],index);
            }
        }
        static void Tube(Vector3 a, Vector3 b, int bone)
        {
            var axis=(b-a).normalized; var u=Vector3.Cross(axis,Mathf.Abs(axis.y)>.9f?Vector3.right:Vector3.up).normalized*.004f;
            var v=Vector3.Cross(axis,u); var offsets=new[]{u,v,-u,-v};
            for(int i=0;i<4;i++) {int n=(i+1)%4; lines.Quad(a+offsets[i],a+offsets[n],b+offsets[n],b+offsets[i],bone);}
        }
        sealed class Geometry
        {
            public readonly List<Vector3> vertices=new List<Vector3>();
            public readonly List<int> triangles=new List<int>();
            public readonly List<BoneWeight> weights=new List<BoneWeight>();
            public void Triangle(Vector3 a,Vector3 b,Vector3 c,int bone)
            {
                int start=vertices.Count; vertices.Add(a);vertices.Add(b);vertices.Add(c);
                for(int i=0;i<3;i++){triangles.Add(start+i);weights.Add(new BoneWeight{boneIndex0=bone,weight0=1});}
            }
            public void Quad(Vector3 a,Vector3 b,Vector3 c,Vector3 d,int bone){Triangle(a,b,c,bone);Triangle(a,c,d,bone);}
        }
        static void AddButton(ShowCharacterVisual visual)
        {
            var view=UnityEngine.Object.FindFirstObjectByType<HundredHour.Environments.MansionArrivalView>(FindObjectsInactive.Include);
            var canvas=view.GetComponentInParent<Canvas>();
            if(canvas==null) canvas=view.GetComponentInChildren<Canvas>(true);
            if(canvas==null) throw new InvalidOperationException("Arrival Canvas missing.");
            var go=new GameObject("Character style test",typeof(RectTransform),typeof(UnityEngine.UI.Image),typeof(UnityEngine.UI.Button));
            go.transform.SetParent(canvas.transform,false);
            var rt=(RectTransform)go.transform;rt.anchorMin=rt.anchorMax=rt.pivot=Vector2.one;rt.anchoredPosition=new Vector2(-28,-100);rt.sizeDelta=new Vector2(310,48);
            go.GetComponent<UnityEngine.UI.Image>().color=new Color(.025f,.045f,.065f,.94f);
            visual.switchButton=go.GetComponent<UnityEngine.UI.Button>();
            var label=new GameObject("Label",typeof(RectTransform),typeof(TMPro.TextMeshProUGUI));label.transform.SetParent(go.transform,false);
            var text=label.GetComponent<TMPro.TextMeshProUGUI>();text.font=view.navigationHint.font;text.fontSize=21;text.alignment=TMPro.TextAlignmentOptions.Center;text.color=new Color(.93f,.90f,.81f);text.raycastTarget=false;
            var tr=text.rectTransform;tr.anchorMin=Vector2.zero;tr.anchorMax=Vector2.one;tr.offsetMin=tr.offsetMax=Vector2.zero;
            visual.modeLabel=text;
        }
    }
}
