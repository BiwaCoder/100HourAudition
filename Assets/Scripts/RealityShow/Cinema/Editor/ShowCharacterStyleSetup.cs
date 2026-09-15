using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace HundredHour.RealityShow.Cinema.Editor
{
    public static class ShowCharacterStyleSetup
    {
        const string Folder="Assets/RealityShow/CharacterPrototype/StyleV2";
        public static string RefreshStory()
        {
            if(EditorApplication.isPlaying || UnityEngine.SceneManagement.SceneManager.GetActiveScene().path!=ShowCharacterPrototypeSetup.ScenePath)
                throw new InvalidOperationException("Open the test scene in Edit mode.");
            var v=UnityEngine.Object.FindFirstObjectByType<ShowCharacterVisual>();var previous=v.storybookToon;
            v.storybookToon=new Builder(v.transform,true).Build();
            if(previous)UnityEngine.Object.DestroyImmediate(previous.gameObject);
            v.ApplyMode();EditorUtility.SetDirty(v);EditorSceneManager.MarkSceneDirty(v.gameObject.scene);EditorSceneManager.SaveScene(v.gameObject.scene);AssetDatabase.SaveAssets();
            return "Storybook face updated";
        }
        static T StoreAsset<T>(T value,string path) where T:UnityEngine.Object
        {
            var existing=AssetDatabase.LoadAssetAtPath<T>(path);
            if(!existing){AssetDatabase.CreateAsset(value,path);return value;}
            if(value is Mesh source && existing is Mesh target)
            {
                // Mesh buffers are native data; CopySerialized alone can retain old vertices.
                target.Clear();target.vertices=source.vertices;target.triangles=source.triangles;
                target.normals=source.normals;target.colors=source.colors;
                target.boneWeights=source.boneWeights;target.bindposes=source.bindposes;target.bounds=source.bounds;
            }
            else EditorUtility.CopySerialized(value,existing);
            UnityEngine.Object.DestroyImmediate(value);EditorUtility.SetDirty(existing);return existing;
        }
        [MenuItem("Tools/100Hour/Add Character Style Variants")]
        public static string Apply()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play first.");
            var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if(scene.path!=ShowCharacterPrototypeSetup.ScenePath)throw new InvalidOperationException("Open the character test scene.");
            var v=UnityEngine.Object.FindFirstObjectByType<ShowCharacterVisual>();
            if(v.streetToon||v.storybookToon)throw new InvalidOperationException("Variants already exist.");
            if(!AssetDatabase.IsValidFolder(Folder))AssetDatabase.CreateFolder("Assets/RealityShow/CharacterPrototype","StyleV2");
            v.streetToon=new Builder(v.transform,false).Build();
            v.storybookToon=new Builder(v.transform,true).Build();
            var glow=new Material(Shader.Find("HundredHour/Character Wire Light")){name="Living wire - cyan and amber"};
            glow.SetColor("_Color",new Color(.30f,.85f,1));glow.SetColor("_PulseColor",new Color(1,.55f,.18f));
            glow.SetFloat("_Intensity",3);glow.SetFloat("_Motion",.006f);glow.SetFloat("_Speed",.65f);
            AssetDatabase.CreateAsset(glow,Folder+"/LivingWire.mat");v.wire.sharedMaterial=glow;
            ((RectTransform)v.switchButton.transform).sizeDelta=new Vector2(365,48);
            v.SetMode(ShowCharacterVisual.VisualMode.Wire);
            EditorUtility.SetDirty(v);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            return string.Join("; ",new[]{v.streetToon,v.storybookToon}.Select(r=>r.name+": "+r.GetComponentInChildren<SkinnedMeshRenderer>(true).sharedMesh.triangles.Length/3+" triangles"));
        }
        sealed class Builder
        {
            readonly bool cute;
            readonly Transform root;
            readonly ShowCharacterRig rig;
            readonly List<Transform> bones=new List<Transform>();
            readonly List<Vector3> vertices=new List<Vector3>();
            readonly List<int> indices=new List<int>();
            readonly List<Color> colors=new List<Color>();
            readonly List<BoneWeight> weights=new List<BoneWeight>();
            readonly Color ink=new Color(.045f,.055f,.11f),skin=new Color(1,.76f,.53f),cream=new Color(1,.92f,.72f);
            readonly Color teal=new Color(.05f,.55f,.57f),orange=new Color(1,.30f,.10f),brown=new Color(.30f,.115f,.055f);
            readonly Color lime=new Color(.73f,.95f,.09f),pink=new Color(.91f,.07f,.38f),white=new Color(.96f,.98f,1);
            public Builder(Transform player,bool cute)
            {
                this.cute=cute;root=new GameObject(cute?"Himari Storybook Toon":"Himari Street Toon").transform;
                root.SetParent(player,false);root.localPosition=new Vector3(0,-.5f,0);
                root.localScale=new Vector3(1/player.lossyScale.x,1/player.lossyScale.y,1/player.lossyScale.z);
                rig=root.gameObject.AddComponent<ShowCharacterRig>();
            }
            Transform Bone(string name,Transform parent,Vector3 p){var t=new GameObject(name).transform;t.SetParent(parent,false);t.localPosition=p;bones.Add(t);return t;}
            public ShowCharacterRig Build()
            {
                var hips=Bone("Hips",root,new Vector3(0,cute?.72f:.89f,0));
                rig.torso=Bone("Chest",hips,new Vector3(0,cute?.29f:.35f,0));
                rig.head=Bone("Head",rig.torso,new Vector3(0,cute?.38f:.34f,0));
                rig.leftArm=Bone("Left shoulder",rig.torso,new Vector3(cute?-.235f:-.255f,.13f,0));
                rig.rightArm=Bone("Right shoulder",rig.torso,new Vector3(cute?.235f:.255f,.13f,0));
                rig.leftLeg=Bone("Left hip",hips,new Vector3(cute?-.12f:-.105f,0,0));
                rig.rightLeg=Bone("Right hip",hips,new Vector3(cute?.12f:.105f,0,0));
                if(cute)Story(hips);else Street(hips);
                var mesh=new Mesh{name=root.name};mesh.SetVertices(vertices);mesh.SetTriangles(indices,0);mesh.SetColors(colors);mesh.boneWeights=weights.ToArray();
                mesh.bindposes=bones.Select(b=>b.worldToLocalMatrix*root.localToWorldMatrix).ToArray();mesh.RecalculateNormals();
                // Smooth only the outline normals. The fill shader reconstructs each flat face.
                var normals=mesh.normals;var sums=new Dictionary<Vector3,Vector3>();
                for(int i=0;i<vertices.Count;i++){sums.TryGetValue(vertices[i],out var sum);sums[vertices[i]]=sum+normals[i];}
                for(int i=0;i<normals.Length;i++)normals[i]=sums[vertices[i]].normalized;mesh.normals=normals;mesh.RecalculateBounds();
                mesh=StoreAsset(mesh,Folder+"/"+root.name+".asset");
                var mat=new Material(Shader.Find("HundredHour/Character Painted Cel")){name=root.name+" palette"};mat.SetFloat("_Outline",cute?.0045f:.008f);mat.SetColor("_Ink",cute?new Color(.14f,.07f,.055f):ink);
                mat=StoreAsset(mat,Folder+"/"+root.name+".mat");
                var render=new GameObject("Painted low poly mesh");render.transform.SetParent(root,false);
                var sk=render.AddComponent<SkinnedMeshRenderer>();sk.sharedMesh=mesh;sk.sharedMaterial=mat;sk.bones=bones.ToArray();sk.rootBone=hips;
                sk.localBounds=new Bounds(new Vector3(0,.9f,0),new Vector3(1.7f,2.3f,1.7f));sk.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;sk.receiveShadows=false;
                root.localRotation=Quaternion.Euler(0,180,0);return rig;
            }
            void Street(Transform hips)
            {
                var chest=rig.torso;var head=rig.head;
                Ring(chest,new[]{new Vector3(0,-.29f,0),new Vector3(0,-.07f,0),new Vector3(0,.18f,0),new Vector3(0,.24f,0)},new[]{new Vector2(.17f,.11f),new Vector2(.16f,.10f),new Vector2(.28f,.15f),new Vector2(.10f,.085f)},6,lime);
                // Strong contrasting zipper/placket, angular hem and raised collar.
                Box(chest,new Vector3(0,-.055f,.126f),new Vector3(.09f,.36f,.032f),ink);
                Box(chest,new Vector3(.12f,.09f,.141f),new Vector3(.105f,.047f,.025f),pink,Quaternion.Euler(0,0,-13));
                Ring(chest,new[]{new Vector3(0,.19f,0),new Vector3(0,.30f,0)},new[]{new Vector2(.125f,.105f),new Vector2(.14f,.105f)},6,pink);
                Ring(hips,new[]{new Vector3(0,-.10f,0),new Vector3(0,.10f,0)},new[]{new Vector2(.24f,.15f),new Vector2(.17f,.105f)},6,ink);
                Box(hips,new Vector3(.10f,.095f,.115f),new Vector3(.16f,.04f,.02f),pink);
                Oval(head,new Vector3(0,.07f,.005f),new Vector3(.125f,.17f,.11f),6,4,skin);
                // Swept, pointed hair: deliberately large wedge silhouettes.
                Oval(head,new Vector3(0,.15f,-.035f),new Vector3(.15f,.145f,.125f),6,3,orange);
                Wedge(head,new Vector3(-.16f,.12f,.09f),new Vector3(.02f,.29f,.04f),new Vector3(-.05f,.065f,.14f),.035f,orange);
                Wedge(head,new Vector3(-.045f,.22f,.10f),new Vector3(.15f,.24f,.07f),new Vector3(.07f,.075f,.142f),.035f,orange);
                Wedge(head,new Vector3(.06f,.20f,-.02f),new Vector3(.23f,.24f,-.04f),new Vector3(.12f,.04f,.055f),.04f,orange);
                foreach(float sign in new[]{-1f,1f})
                {
                    Box(head,new Vector3(sign*.060f,.067f,.105f),new Vector3(.059f,.027f,.018f),white);
                    Box(head,new Vector3(sign*.055f,.065f,.117f),new Vector3(.023f,.031f,.011f),ink);
                    Box(head,new Vector3(sign*.062f,.102f,.112f),new Vector3(.069f,.014f,.014f),ink,Quaternion.Euler(0,0,sign*10));
                    Box(head,new Vector3(sign*.155f,.09f,-.008f),new Vector3(.055f,.13f,.11f),ink);
                    Box(head,new Vector3(sign*.187f,.09f,-.008f),new Vector3(.018f,.075f,.075f),teal);
                }
                Box(head,new Vector3(0,-.01f,.106f),new Vector3(.034f,.012f,.014f),brown);
                foreach(var arm in new[]{rig.leftArm,rig.rightArm})
                {
                    float s=arm==rig.leftArm?-1:1;
                    Ring(arm,new[]{new Vector3(s*.05f,-.43f,.045f),new Vector3(s*.045f,-.22f,0),Vector3.zero},new[]{new Vector2(.042f,.05f),new Vector2(.059f,.062f),new Vector2(.085f,.09f)},5,ink);
                    Ring(arm,new[]{new Vector3(0,-.16f,0),new Vector3(0,.035f,0)},new[]{new Vector2(.075f,.09f),new Vector2(.115f,.11f)},5,lime);
                    Box(arm,new Vector3(s*.05f,-.42f,.046f),new Vector3(.12f,.075f,.12f),pink);
                    Oval(arm,new Vector3(s*.05f,-.50f,.055f),new Vector3(.05f,.078f,.047f),5,3,skin);
                }
                foreach(var leg in new[]{rig.leftLeg,rig.rightLeg})
                {
                    Ring(leg,new[]{new Vector3(0,-.64f,0),new Vector3(0,-.35f,0),Vector3.zero},new[]{new Vector2(.046f,.06f),new Vector2(.055f,.064f),new Vector2(.075f,.09f)},5,ink);
                    Box(leg,new Vector3(0,-.39f,.059f),new Vector3(.10f,.11f,.024f),teal);
                    Ring(leg,new[]{new Vector3(0,-.86f,.08f),new Vector3(0,-.73f,.07f),new Vector3(0,-.60f,-.012f)},new[]{new Vector2(.105f,.21f),new Vector2(.115f,.19f),new Vector2(.062f,.075f)},6,white);
                    Box(leg,new Vector3(0,-.856f,.08f),new Vector3(.23f,.06f,.43f),ink);
                    Box(leg,new Vector3(0,-.733f,.16f),new Vector3(.19f,.05f,.09f),pink);
                    Box(leg,new Vector3(0,-.68f,.062f),new Vector3(.155f,.055f,.115f),teal);
                }
            }
            void Story(Transform hips)
            {
                var chest=rig.torso;var head=rig.head;
                Ring(chest,new[]{new Vector3(0,-.27f,0),new Vector3(0,0,0),new Vector3(0,.16f,0),new Vector3(0,.23f,0)},new[]{new Vector2(.22f,.15f),new Vector2(.205f,.14f),new Vector2(.24f,.15f),new Vector2(.10f,.085f)},8,teal);
                Box(chest,new Vector3(0,-.04f,.143f),new Vector3(.052f,.37f,.028f),cream);
                foreach(float y in new[]{-.17f,-.06f,.05f})Oval(chest,new Vector3(.035f,y,.169f),new Vector3(.016f,.016f,.011f),6,3,new Color(.95f,.62f,.19f));
                Ring(chest,new[]{new Vector3(0,.19f,0),new Vector3(0,.29f,0)},new[]{new Vector2(.13f,.105f),new Vector2(.11f,.085f)},8,orange);
                Wedge(chest,new Vector3(-.085f,.22f,.16f),new Vector3(.055f,.23f,.17f),new Vector3(-.06f,-.03f,.18f),.018f,orange);
                // Satchel strap and a simple brass clasp.
                Box(chest,new Vector3(.025f,-.025f,.183f),new Vector3(.056f,.50f,.023f),brown,Quaternion.Euler(0,0,32));
                Oval(hips,new Vector3(.265f,.08f,.015f),new Vector3(.11f,.145f,.11f),8,4,brown);
                Box(hips,new Vector3(.26f,.095f,.122f),new Vector3(.038f,.046f,.014f),cream);
                Oval(head,new Vector3(0,.025f,-.045f),new Vector3(.225f,.255f,.17f),8,5,brown);
                Oval(head,new Vector3(0,.005f,.045f),new Vector3(.193f,.216f,.155f),10,6,skin);
                // Rounded bob, scalloped fringe and soft teal cap.
                foreach(float x in new[]{-.125f,-.043f,.043f,.125f})
                    Wedge(head,new Vector3(x-.055f,.21f,.13f),new Vector3(x+.055f,.21f,.13f),new Vector3(x+.016f,.095f,.196f),.035f,brown);
                Oval(head,new Vector3(0,.208f,-.022f),new Vector3(.23f,.12f,.19f),10,4,teal);
                Oval(head,new Vector3(0,.193f,.10f),new Vector3(.24f,.032f,.165f),10,3,teal);
                Oval(head,new Vector3(-.12f,.252f,.132f),new Vector3(.038f,.039f,.018f),6,3,cream);
                foreach(float sign in new[]{-1f,1f})
                {
                    EyePatch(head,new Vector3(sign*.076f,.012f,.193f),new Vector2(.049f,.064f),white);
                    EyePatch(head,new Vector3(sign*.072f,.010f,.201f),new Vector2(.025f,.045f),new Color(.025f,.17f,.22f));
                    EyePatch(head,new Vector3(sign*.072f-.008f,.031f,.208f),new Vector2(.008f,.012f),white);
                    Box(head,new Vector3(sign*.081f,.095f,.19f),new Vector3(.071f,.013f,.015f),brown,Quaternion.Euler(0,0,sign*6));
                    Oval(head,new Vector3(sign*.194f,-.002f,.027f),new Vector3(.036f,.064f,.049f),6,4,skin);
                }
                Oval(head,new Vector3(0,-.05f,.202f),new Vector3(.018f,.020f,.020f),8,3,skin);
                Box(head,new Vector3(0,-.103f,.182f),new Vector3(.042f,.009f,.012f),brown);
                foreach(var arm in new[]{rig.leftArm,rig.rightArm})
                {
                    float s=arm==rig.leftArm?-1:1;
                    Oval(arm,new Vector3(s*.035f,-.09f,0),new Vector3(.105f,.16f,.108f),8,5,cream);
                    Ring(arm,new[]{new Vector3(s*.07f,-.36f,.03f),new Vector3(s*.05f,-.20f,0)},new[]{new Vector2(.055f,.06f),new Vector2(.075f,.075f)},8,cream);
                    Ring(arm,new[]{new Vector3(s*.068f,-.365f,.027f),new Vector3(s*.066f,-.30f,.026f)},new[]{new Vector2(.075f,.075f),new Vector2(.075f,.075f)},8,teal);
                    Oval(arm,new Vector3(s*.07f,-.412f,.04f),new Vector3(.065f,.075f,.055f),8,4,skin);
                }
                foreach(var leg in new[]{rig.leftLeg,rig.rightLeg})
                {
                    Oval(leg,new Vector3(0,-.115f,0),new Vector3(.11f,.22f,.12f),8,5,new Color(.18f,.28f,.43f));
                    Ring(leg,new[]{new Vector3(0,-.49f,0),new Vector3(0,-.26f,0)},new[]{new Vector2(.057f,.065f),new Vector2(.067f,.075f)},8,cream);
                    Oval(leg,new Vector3(0,-.575f,.052f),new Vector3(.10f,.135f,.18f),8,5,brown);
                    Ring(leg,new[]{new Vector3(0,-.56f,0),new Vector3(0,-.39f,0)},new[]{new Vector2(.083f,.097f),new Vector2(.095f,.10f)},8,brown);
                    Ring(leg,new[]{new Vector3(0,-.43f,0),new Vector3(0,-.37f,0)},new[]{new Vector2(.11f,.12f),new Vector2(.11f,.12f)},8,new Color(.62f,.33f,.13f));
                    Box(leg,new Vector3(0,-.676f,.052f),new Vector3(.19f,.035f,.30f),new Color(.18f,.08f,.045f));
                }
            }
            void Oval(Transform bone,Vector3 center,Vector3 radius,int sides,int rows,Color color)
            {
                var p=new Vector3[rows+1];var r=new Vector2[rows+1];
                for(int j=0;j<=rows;j++){float a=-Mathf.PI/2+Mathf.PI*j/rows;p[j]=center+Vector3.up*Mathf.Sin(a)*radius.y;float f=Mathf.Max(.035f,Mathf.Cos(a));r[j]=new Vector2(radius.x*f,radius.z*f);}
                Ring(bone,p,r,sides,color);
            }
            void EyePatch(Transform bone,Vector3 center,Vector2 radius,Color color)
            {
                const int sides=8;
                for(int i=0;i<sides;i++)
                {
                    float a=i*Mathf.PI*2/sides,b=(i+1)*Mathf.PI*2/sides;
                    var p=center+new Vector3(Mathf.Cos(a)*radius.x,Mathf.Sin(a)*radius.y,-.004f);
                    var q=center+new Vector3(Mathf.Cos(b)*radius.x,Mathf.Sin(b)*radius.y,-.004f);
                    Tri(bone,center,p,q,color);
                }
            }
            void Ring(Transform bone,Vector3[] centers,Vector2[] radii,int sides,Color color)
            {
                var rings=new Vector3[centers.Length][];
                for(int j=0;j<centers.Length;j++){rings[j]=new Vector3[sides];for(int i=0;i<sides;i++){float a=2*Mathf.PI*i/sides;rings[j][i]=centers[j]+new Vector3(Mathf.Cos(a)*radii[j].x,0,Mathf.Sin(a)*radii[j].y);}}
                for(int j=0;j<rings.Length-1;j++)for(int i=0;i<sides;i++){int n=(i+1)%sides;Quad(bone,rings[j][i],rings[j+1][i],rings[j+1][n],rings[j][n],color);}
                for(int i=1;i<sides-1;i++){Tri(bone,rings[0][0],rings[0][i],rings[0][i+1],color);var e=rings[rings.Length-1];Tri(bone,e[0],e[i+1],e[i],color);}
            }
            void Box(Transform bone,Vector3 center,Vector3 size,Color color,Quaternion? rotation=null)
            {
                var q=rotation??Quaternion.identity;var p=new Vector3[8];for(int i=0;i<8;i++)p[i]=center+q*Vector3.Scale(new Vector3((i&1)==0?-1:1,(i&2)==0?-1:1,(i&4)==0?-1:1),size*.5f);
                int[,] f={{0,2,3,1},{4,5,7,6},{0,4,6,2},{1,3,7,5},{0,1,5,4},{2,6,7,3}};
                for(int i=0;i<6;i++)Quad(bone,p[f[i,0]],p[f[i,1]],p[f[i,2]],p[f[i,3]],color);
            }
            void Wedge(Transform bone,Vector3 a,Vector3 b,Vector3 c,float depth,Color color)
            {
                var d=new Vector3(0,0,-depth);Tri(bone,a,c,b,color);Tri(bone,a+d,b+d,c+d,color);
                Quad(bone,a,b,b+d,a+d,color);Quad(bone,b,c,c+d,b+d,color);Quad(bone,c,a,a+d,c+d,color);
            }
            void Quad(Transform bone,Vector3 a,Vector3 b,Vector3 c,Vector3 d,Color color){Tri(bone,a,b,c,color);Tri(bone,a,c,d,color);}
            void Tri(Transform bone,Vector3 a,Vector3 b,Vector3 c,Color color)
            {
                int bi=bones.IndexOf(bone),start=vertices.Count;
                foreach(var p in new[]{a,b,c}){vertices.Add(root.InverseTransformPoint(bone.TransformPoint(p)));colors.Add(color);weights.Add(new BoneWeight{boneIndex0=bi,weight0=1});}
                indices.Add(start);indices.Add(start+1);indices.Add(start+2);
            }
        }
    }
}
