Shader "Custom/MistFilter"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MistIntensity ("Mist Intensity", Range(0.0, 1.0)) = 0.3
        _MistBlur ("Mist Blur", Range(0.5, 8.0)) = 2.0
        _HighlightBoost ("Highlight Boost", Range(1.0, 3.0)) = 1.5
        _MistColor ("Mist Color", Color) = (1, 1, 1, 1)
        _Softness ("Softness", Range(0.0, 2.0)) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            float _MistIntensity;
            float _MistBlur;
            float _HighlightBoost;
            fixed4 _MistColor;
            float _Softness;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float3 SoftBlur(sampler2D tex, float2 uv, float2 texelSize, float blurSize)
            {
                float3 color = float3(0, 0, 0);
                float totalWeight = 0;
                
                int samples = 6;
                
                for (int x = -samples; x <= samples; x++)
                {
                    for (int y = -samples; y <= samples; y++)
                    {
                        float2 offset = float2(x, y) * texelSize * blurSize;
                        float distance = length(float2(x, y)) / samples;
                        float weight = 1.0 - distance * distance;
                        weight = max(0, weight);
                        
                        color += tex2D(tex, uv + offset).rgb * weight;
                        totalWeight += weight;
                    }
                }
                
                return color / totalWeight;
            }

            float3 GetLuminance(float3 color)
            {
                return dot(color, float3(0.299, 0.587, 0.114));
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 originalColor = tex2D(_MainTex, i.uv);
                
                float3 blurredColor = SoftBlur(_MainTex, i.uv, _MainTex_TexelSize.xy, _MistBlur);
                
                float luminance = GetLuminance(originalColor.rgb);
                
                float highlightMask = pow(luminance, 2.0);
                highlightMask = saturate(highlightMask * _HighlightBoost);
                
                float3 mistEffect = lerp(originalColor.rgb, blurredColor, _MistIntensity);
                
                float3 highlightedColor = mistEffect + (highlightMask * _MistColor.rgb * _MistIntensity);
                
                float3 softened = lerp(originalColor.rgb, mistEffect, _Softness);
                
                float3 finalColor = lerp(softened, highlightedColor, highlightMask * _MistIntensity);
                
                finalColor = lerp(finalColor, finalColor * _MistColor.rgb, _MistIntensity * 0.3);
                
                return fixed4(finalColor, originalColor.a);
            }
            ENDCG
        }
    }
}