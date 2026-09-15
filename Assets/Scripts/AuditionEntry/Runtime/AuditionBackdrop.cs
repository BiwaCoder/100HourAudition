using UnityEngine;
using UnityEngine.UI;
namespace HundredHour.AuditionEntry
{
    [ExecuteAlways]
    public sealed class AuditionBackdrop:MonoBehaviour
    {
        public Shader shader;
        Material material;
        void OnEnable(){if(shader){material=new Material(shader){hideFlags=HideFlags.HideAndDontSave};GetComponent<RawImage>().material=material;}}
        void Update(){if(material)material.SetFloat("_FlowTime",Time.unscaledTime);}
        void OnDisable()
        {
            if(!material)return;
            var image=GetComponent<RawImage>();
            if(image&&image.material==material)image.material=null;
            if(Application.isPlaying)Destroy(material);else DestroyImmediate(material);
            material=null;
        }
    }
}
