using UnityEngine;
namespace HundredHour.Environments
{
 /// <summary>Quarter-resolution HDR bloom. Only attached to the mansion camera.</summary>
 [ExecuteAlways,RequireComponent(typeof(Camera))]
 public sealed class MansionSoftGlow : MonoBehaviour
 {
  public Shader shader;
  [Range(0,2)] public float intensity=.32f;
  [Range(.5f,4)] public float threshold=1.1f;
  [Range(1,5)] public float radius=2.5f;
  Material material;
  void OnDisable(){if(material){if(Application.isPlaying)Destroy(material);else DestroyImmediate(material);material=null;}}
  void OnRenderImage(RenderTexture source,RenderTexture destination)
  {
   if(!shader||!shader.isSupported||intensity<=0){Graphics.Blit(source,destination);return;}
   if(!material)material=new Material(shader){hideFlags=HideFlags.HideAndDontSave};
   var d=source.descriptor;d.width=Mathf.Max(1,source.width/4);d.height=Mathf.Max(1,source.height/4);d.depthBufferBits=0;d.msaaSamples=1;
   var a=RenderTexture.GetTemporary(d);var b=RenderTexture.GetTemporary(d);a.filterMode=b.filterMode=FilterMode.Bilinear;
   try{
    material.SetFloat("_Threshold",threshold);material.SetFloat("_Intensity",intensity);
    Graphics.Blit(source,a,material,0);
    for(int i=0;i<2;i++){
     material.SetVector("_Direction",new Vector2(radius*(i+1)/d.width,0));Graphics.Blit(a,b,material,1);
     material.SetVector("_Direction",new Vector2(0,radius*(i+1)/d.height));Graphics.Blit(b,a,material,1);
    }
    material.SetTexture("_GlowTex",a);Graphics.Blit(source,destination,material,2);
   }finally{RenderTexture.ReleaseTemporary(a);RenderTexture.ReleaseTemporary(b);}
  }
 }
}
