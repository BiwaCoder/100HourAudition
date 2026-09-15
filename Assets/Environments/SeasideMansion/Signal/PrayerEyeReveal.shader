Shader "100Hour/UI/PrayerEyeReveal"
{
    Properties
    {
        [PerRendererData] _MainTex ("Closed eyes", 2D) = "white" {}
        _HalfTex ("Half-open eyes", 2D) = "white" {}
        _OpenTex ("Open eyes", 2D) = "white" {}
        _EyeOpen ("Opening", Range(0,1)) = 0
        _EyeBand ("Eye region UV", Vector) = (0.459,0.835,0.526,0.872)
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="False"}
        Cull Off Lighting Off ZWrite Off ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct appdata {float4 vertex:POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;};
            struct v2f {float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;};
            sampler2D _MainTex,_HalfTex,_OpenTex;
            float _EyeOpen;float4 _EyeBand;
            v2f vert(appdata v){v2f o;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.color=v.color;return o;}
            fixed4 frag(v2f i):SV_Target
            {
                fixed4 base=tex2D(_MainTex,i.uv);
                fixed4 halfEye=tex2D(_HalfTex,i.uv),openEye=tex2D(_OpenTex,i.uv);
                float t=saturate(_EyeOpen)*2;
                fixed4 eye=t<1?lerp(base,halfEye,smoothstep(0,1,t)):lerp(halfEye,openEye,smoothstep(0,1,t-1));
                float2 edge=smoothstep(_EyeBand.xy,_EyeBand.xy+.003,i.uv)*(1-smoothstep(_EyeBand.zw-.003,_EyeBand.zw,i.uv));
                // Only the eyes are composited. The prayer pose, body, hair and mansion never crossfade or warp.
                return lerp(base,eye,edge.x*edge.y)*i.color;
            }
            ENDCG
        }
    }
}
