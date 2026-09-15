using UnityEngine;

[ExecuteInEditMode]
public class MistFilterEffect : MonoBehaviour
{
    [Header("Mist Filter Settings")]
    [Range(0f, 1f)]
    public float mistIntensity = 0.3f;
    
    [Range(0.5f, 8f)]
    public float mistBlur = 2f;
    
    [Range(1f, 3f)]
    public float highlightBoost = 1.5f;
    
    [Range(0f, 2f)]
    public float softness = 1f;
    
    [Header("Color Settings")]
    public Color mistColor = Color.white;
    
    [Header("Shader")]
    public Shader mistFilterShader;
    
    private Material mistFilterMaterial;
    
    void Start()
    {
        if (mistFilterShader == null)
        {
            mistFilterShader = Shader.Find("Custom/MistFilter");
        }
        
        CreateMaterial();
    }
    
    void CreateMaterial()
    {
        if (mistFilterShader != null && mistFilterMaterial == null)
        {
            mistFilterMaterial = new Material(mistFilterShader);
            mistFilterMaterial.hideFlags = HideFlags.HideAndDontSave;
        }
    }
    
    void OnDestroy()
    {
        if (mistFilterMaterial != null)
        {
            DestroyImmediate(mistFilterMaterial);
        }
    }
    
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (mistFilterMaterial == null || mistFilterShader == null)
        {
            Graphics.Blit(source, destination);
            return;
        }
        
        mistFilterMaterial.SetFloat("_MistIntensity", mistIntensity);
        mistFilterMaterial.SetFloat("_MistBlur", mistBlur);
        mistFilterMaterial.SetFloat("_HighlightBoost", highlightBoost);
        mistFilterMaterial.SetFloat("_Softness", softness);
        mistFilterMaterial.SetColor("_MistColor", mistColor);
        
        Graphics.Blit(source, destination, mistFilterMaterial);
    }
    
    void OnValidate()
    {
        CreateMaterial();
    }
}