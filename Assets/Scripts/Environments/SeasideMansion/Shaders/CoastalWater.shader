Shader "HundredHour/Coastal Water"
{
 Properties { _Color("Water tint",Color)=(.04,.28,.32,1) _Glossiness("Smoothness",Range(0,1))=.88 _Wave("Wave amplitude",Float)=.06 _Ocean("Ocean patch",Float)=0 _Cascade("Falling water",Float)=0 _Horizon("Far horizon",Float)=0 }
 SubShader {
 Tags {"RenderType"="Opaque"} LOD 250 Cull Off
 CGPROGRAM
 #pragma surface surf Standard fullforwardshadows vertex:vert addshadow
 #pragma target 3.0
 fixed4 _Color;half _Glossiness;float _Wave,_Ocean,_Cascade,_Horizon;
 struct Input {float3 worldPos;};
 void vert(inout appdata_full v) {
 float3 w=mul(unity_ObjectToWorld,v.vertex).xyz;
 float edge=1;
 if(_Ocean>.5){if(abs(w.x)>159)w.x=sign(w.x)*160;if(abs(w.z+203)>159)w.z=sign(w.z+203)*160-203;edge=saturate((160-abs(w.x))/24)*saturate((160-abs(w.z+203))/24);w.y=lerp(-1.1,w.y,edge);}
 float a=w.x*.43+w.z*.27+_Time.y*.85;float b=w.x*-.31+w.z*.57-_Time.y*1.13;
 w.y+=(sin(a)+sin(b)*.45)*_Wave*edge;
 v.vertex=mul(unity_WorldToObject,float4(w,1));
 if(_Cascade<.5){float3 n=normalize(float3(-_Wave*edge*(cos(a)*.43-cos(b)*.1395),1,-_Wave*edge*(cos(a)*.27+cos(b)*.2565)));v.normal=normalize(mul(n,(float3x3)unity_ObjectToWorld));}
 }
 void surf(Input IN,inout SurfaceOutputStandard o){
 if(_Horizon>.5 && abs(IN.worldPos.x)<159.98 && abs(IN.worldPos.z+203)<159.98)clip(-1);
 float2 p=IN.worldPos.xz;float a=p.x*4.7+p.y*3.1+_Time.y*1.9;float b=p.x*-2.8+p.y*5.4-_Time.y*1.3;
 float ripple=sin(a+sin(b)*.6);float glint=pow(saturate(ripple*.5+.5),18)*.018;
 float streak=pow(saturate(sin(IN.worldPos.y*32+_Time.y*9+IN.worldPos.x*19)),5)*_Cascade;
 o.Albedo=lerp(_Color.rgb,_Color.rgb+float3(.13,.25,.24),glint)+streak*.18;
 float fade=1/(1+distance(_WorldSpaceCameraPos,IN.worldPos)*.015);o.Normal=float3(0,0,1);o.Metallic=.42;o.Smoothness=_Glossiness;o.Emission=_Color.rgb*(.07+streak*.08);o.Alpha=1;
 }
 ENDCG
 }
 FallBack "Standard"
}
