Shader "HundredHour/Character Painted Cel"
{
 Properties { _Outline("Outline",Range(0,.025))=.007 _Ink("Ink",Color)=(.025,.025,.045,1) }
 SubShader {
  Tags {"RenderType"="Opaque"}
  Pass {
   Cull Front
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "UnityCG.cginc"
   float _Outline;float4 _Ink;
   float4 vert(appdata_base v):SV_POSITION {return UnityObjectToClipPos(float4(v.vertex.xyz+v.normal*_Outline,1));}
   float4 frag():SV_Target {return _Ink;}
   ENDCG
  }
  Pass {
   Cull Back
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "UnityCG.cginc"
   struct appdata {float4 vertex:POSITION;float3 normal:NORMAL;float4 color:COLOR;};
   struct v2f {float4 pos:SV_POSITION;float3 world:TEXCOORD0;float3 normal:TEXCOORD1;float4 color:COLOR;};
   v2f vert(appdata v) {v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.world=mul(unity_ObjectToWorld,v.vertex).xyz;o.normal=UnityObjectToWorldNormal(v.normal);o.color=v.color;return o;}
   float4 frag(v2f i):SV_Target {
    float3 n=normalize(cross(ddy(i.world),ddx(i.world)));if(dot(n,i.normal)<0)n=-n;
    float light=dot(n,normalize(float3(-.5,.8,.6)));
    float shade=light>.45?1.04:light>-.15?.85:.61;
    return float4(i.color.rgb*shade,1);
   }
   ENDCG
  }
 }
}
