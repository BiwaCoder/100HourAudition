Shader "Custom/MistFilter"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BlurTex ("Blur Texture", 2D) = "black" {}
        _MistIntensity ("Mist Intensity", Range(0.0, 1.0)) = 0.3
        _MistBlur ("Mist Blur", Range(0.5, 8.0)) = 2.0
        _HighlightBoost ("Highlight Boost", Range(1.0, 3.0)) = 1.5
        _MistColor ("Mist Color", Color) = (1, 1, 1, 1)
        _Softness ("Softness", Range(0.0, 2.0)) = 1.0
        _HazeAmount ("Haze Amount", Range(0.0, 0.5)) = 0.12
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Cull Off
        ZWrite Off
        ZTest Always

        // Pass 0: one-dimensional Gaussian blur.
        Pass
        {
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment fragBlur
            
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
            float2 _BlurDirection;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 fragBlur(v2f i) : SV_Target
            {
                fixed4 color = tex2D(_MainTex, i.uv) * 0.227027;
                color += tex2D(_MainTex, i.uv + _BlurDirection * 1.384615) * 0.316216;
                color += tex2D(_MainTex, i.uv - _BlurDirection * 1.384615) * 0.316216;
                color += tex2D(_MainTex, i.uv + _BlurDirection * 3.230769) * 0.070270;
                color += tex2D(_MainTex, i.uv - _BlurDirection * 3.230769) * 0.070270;
                return color;
            }

            ENDCG
        }

        // Pass 1: combine the original image and the blurred image.
        Pass
        {
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment fragComposite

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
            sampler2D _BlurTex;
            float4 _MainTex_ST;
            float _MistIntensity;
            float _HighlightBoost;
            fixed4 _MistColor;
            float _Softness;
            float _HazeAmount;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float3 GetLuminance(float3 color)
            {
                return dot(color, float3(0.299, 0.587, 0.114));
            }

            fixed4 fragComposite(v2f i) : SV_Target
            {
                fixed4 originalColor = tex2D(_MainTex, i.uv);
                float3 blurredColor = tex2D(_BlurTex, i.uv).rgb;
                
                float luminance = GetLuminance(originalColor.rgb);
                
                float highlightMask = pow(luminance, 2.0);
                highlightMask = saturate(highlightMask * _HighlightBoost);
                
                float3 mistEffect = lerp(originalColor.rgb, blurredColor, _MistIntensity);
                
                float3 highlightedColor = mistEffect + (highlightMask * _MistColor.rgb * _MistIntensity);
                
                float3 softened = lerp(originalColor.rgb, mistEffect, _Softness);
                
                float3 finalColor = lerp(softened, highlightedColor, highlightMask * _MistIntensity);
                
                finalColor = lerp(finalColor, finalColor * _MistColor.rgb, _MistIntensity * 0.3);

                // 光源だけでなく画面全体へ薄い色のベールを重ね、淡い空気感を作る。
                float haze = saturate(_HazeAmount * _MistIntensity);
                finalColor = 1.0 - (1.0 - finalColor) * (1.0 - _MistColor.rgb * haze);
                
                return fixed4(finalColor, originalColor.a);
            }
            ENDCG
        }
    }
}
