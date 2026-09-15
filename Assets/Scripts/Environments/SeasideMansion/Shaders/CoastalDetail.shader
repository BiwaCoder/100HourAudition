Shader "HundredHour/Coastal Detail"
{
 Properties { _Color("Color",Color)=(1,1,1,1) _Metallic("Metallic",Range(0,1))=0 _Glossiness("Smoothness",Range(0,1))=.4 _Wind("Leaf wind",Float)=0 _Grain("Stone grain",Float)=0 [Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull",Float)=2 }
 SubShader {
 Tags { "RenderType"="Opaque" } LOD 200 Cull [_Cull]
 CGPROGRAM
 #pragma surface surf Standard fullforwardshadows vertex:vert addshadow
 #pragma target 3.0
 #pragma multi_compile_instancing
 fixed4 _Color; half _Metallic,_Glossiness;float _Wind,_Grain;
 struct Input { float3 worldPos; float4 color:COLOR; };
 void vert(inout appdata_full v) { float3 w=mul(unity_ObjectToWorld,v.vertex).xyz; float weight=saturate((w.y-unity_ObjectToWorld._m13)*.25); float sway=sin(w.x*.9+w.z*.6+_Time.y*1.3)*sin(_Time.y*.7+w.z)*_Wind*weight; v.vertex.xyz+=mul((float3x3)unity_WorldToObject,float3(sway,0,sway*.3)); }
 void surf(Input IN,inout SurfaceOutputStandard o) { float grain=sin(IN.worldPos.x*97+sin(IN.worldPos.z*61))*sin(IN.worldPos.y*83+IN.worldPos.z*39); o.Albedo=_Color.rgb*IN.color.rgb*(1+grain*_Grain*.018);o.Metallic=_Metallic;o.Smoothness=_Glossiness;o.Alpha=1; }
 ENDCG
 }
 FallBack "Standard"
}
