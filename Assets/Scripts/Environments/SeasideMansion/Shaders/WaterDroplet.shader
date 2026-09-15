Shader "HundredHour/Water Droplet" {
 Properties {_Color("Color",Color)=(.7,.9,1,.7)}
 SubShader {Tags {"Queue"="Transparent" "RenderType"="Transparent"} Blend SrcAlpha OneMinusSrcAlpha Cull Off ZWrite Off
 Pass {CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 struct a {float4 vertex:POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;};struct v {float4 pos:SV_POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;};fixed4 _Color;
 v vert(a i){v o;o.pos=UnityObjectToClipPos(i.vertex);o.uv=i.uv;o.color=i.color*_Color;return o;}
 fixed4 frag(v i):SV_Target{float r=length(i.uv*2-1);i.color.a*=smoothstep(1,.3,r);return i.color;}
 ENDCG}
 }
}
