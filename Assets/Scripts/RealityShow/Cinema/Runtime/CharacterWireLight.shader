Shader "HundredHour/Character Wire Light"
{
 Properties {
  [HDR] _Color("Base light",Color)=(.3,.95,1,1)
  [HDR] _PulseColor("Traveling light",Color)=(1,.45,.12,1)
  _Intensity("Light intensity",Range(0,8))=3
  _Motion("Line drift",Range(0,.025))=.006
  _CoordinateScale("Mesh coordinate scale",Float)=1
  _Speed("Flow speed",Range(0,3))=.65
 }
 SubShader {
  Tags {"RenderType"="Opaque"}
  Pass {
   Cull Off
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "UnityCG.cginc"
   float4 _Color,_PulseColor; float _Intensity,_Motion,_Speed,_CoordinateScale;
   struct v2f {float4 pos:SV_POSITION;float3 localPosition:TEXCOORD0;};
   v2f vert(appdata_base v) {
    v2f o;float t=_Time.y*_Speed;float3 p=v.vertex.xyz*_CoordinateScale;
    p+=_Motion*float3(sin(p.y*13+t*1.7)+.4*sin(p.z*19-t),sin(p.x*17+t)*.25,cos(p.y*11-t*1.3));
    o.pos=UnityObjectToClipPos(float4(p/_CoordinateScale,1));o.localPosition=v.vertex.xyz*_CoordinateScale;return o;
   }
   float4 frag(v2f i):SV_Target {
    float t=_Time.y*_Speed;float3 p=i.localPosition;
    float wave=sin(p.y*11+p.x*9-t*2.1)*.5+.5;
    float irregular=sin(p.z*23-p.y*6+t*.73)*.5+.5;
    float streak=pow(saturate(wave*.72+irregular*.28),7);
    float flicker=.9+.1*sin(t*2.7+p.y*17);
    float3 tint=lerp(_Color.rgb,_PulseColor.rgb,smoothstep(.35,.9,irregular));
    return float4(tint*(.5+streak*_Intensity*3)*flicker,1);
   }
   ENDCG
  }
 }
}
