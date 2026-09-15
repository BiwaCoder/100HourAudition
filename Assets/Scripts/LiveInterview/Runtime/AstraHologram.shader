Shader "Kumono/HologramBackground"
{
    Properties { _MainTex ("Texture", 2D) = "white" {} _FlowTime ("Time", Float) = 0 }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Cull Off ZWrite Off ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; fixed4 color:COLOR; };
            struct v2f { float4 vertex:SV_POSITION; float2 uv:TEXCOORD0; fixed4 color:COLOR; };
            float _FlowTime;
            v2f vert(appdata v){v2f o;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.color=v.color;return o;}
            float4 frag(v2f i):SV_Target
            {
                float t=_FlowTime*.075;
                float2 p=(i.uv-.5)*float2(3.5,2.0);
                // Slowly warped layers produce liquid interference instead of flashing grain.
                float2 q=p;
                q+=.42*float2(sin(p.y*2.3+t),cos(p.x*1.8-t*.7));
                q+=.20*float2(cos(q.y*3.1-t*.6),sin(q.x*2.7+t*.5));
                q+=.12*float2(sin(q.y*5.0+q.x*1.2+t*.4),cos(q.x*4.0-q.y*1.5-t*.3));
                float field=q.x*.8+q.y*.6+.44*sin(q.x*2.4-q.y*1.7+t*.35);
                float veins=pow(.5+.5*sin(field*10.0),7.0);
                float broad=.5+.5*sin(field*3.0+t*.3);
                float3 rainbow=.5+.5*cos(6.28318*(field*.25+t*.025+float3(0,.33,.67)));
                rainbow=lerp(float3(.35,.48,.65),rainbow,.65);
                float edges=smoothstep(.1,.8,length((i.uv-.5)*float2(1.25,1)));
                float brightness=(.045+.095*broad+.10*veins)*lerp(.48,1.0,edges);
                float grid=(pow(.5+.5*cos(i.uv.x*180),40)+pow(.5+.5*cos(i.uv.y*100),40))*.0025;
                float3 color=float3(.004,.006,.013)+rainbow*brightness*.48+grid*.5;
                return float4(color,i.color.a);
            }
            ENDCG
        }
    }
}
