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

    [Tooltip("画面全体に重ねる淡い霞の量")]
    [Range(0f, 0.5f)]
    public float hazeAmount = 0.12f;

    [Tooltip("Blurの広がり。負荷を増やさず霞を広くする")]
    [Range(0.5f, 3f)]
    public float blurSpread = 1.75f;
    
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
        CreateMaterial();

        if (mistFilterMaterial == null || mistFilterShader == null || !mistFilterShader.isSupported)
        {
            Graphics.Blit(source, destination);
            return;
        }
        
        mistFilterMaterial.SetFloat("_MistIntensity", mistIntensity);
        mistFilterMaterial.SetFloat("_HighlightBoost", highlightBoost);
        mistFilterMaterial.SetFloat("_Softness", softness);
        mistFilterMaterial.SetFloat("_HazeAmount", hazeAmount);
        mistFilterMaterial.SetColor("_MistColor", mistColor);

        RenderTextureDescriptor descriptor = source.descriptor;
        descriptor.depthBufferBits = 0;
        descriptor.msaaSamples = 1;

        RenderTexture horizontal = RenderTexture.GetTemporary(descriptor);
        RenderTexture vertical = RenderTexture.GetTemporary(descriptor);

        try
        {
            // 13x13 (169 samples) の一括Blurを、横7 + 縦7サンプルへ分割する。
            float blurRadius = mistBlur * blurSpread;

            mistFilterMaterial.SetVector("_BlurDirection", new Vector2(blurRadius / source.width, 0f));
            Graphics.Blit(source, horizontal, mistFilterMaterial, 0);

            mistFilterMaterial.SetVector("_BlurDirection", new Vector2(0f, blurRadius / source.height));
            Graphics.Blit(horizontal, vertical, mistFilterMaterial, 0);

            mistFilterMaterial.SetTexture("_BlurTex", vertical);
            Graphics.Blit(source, destination, mistFilterMaterial, 1);
        }
        finally
        {
            RenderTexture.ReleaseTemporary(horizontal);
            RenderTexture.ReleaseTemporary(vertical);
        }
    }
    
    void OnValidate()
    {
        CreateMaterial();
    }
}
