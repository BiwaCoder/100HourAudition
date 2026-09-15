Shader "HundredHour/Coastal Furnishing" {
 Properties {
 _Color("Tint",Color)=(1,1,1,1)
 _Metallic("Metallic",Range(0,1))=0
 _Glossiness("Smoothness",Range(0,1))=.4
 _Kind("0 Metal, 1 Cloth, 2 Walnut, 3 Stone, 4 Opal",Float)=0
 [HDR] _EmissionColor("Emission",Color)=(0,0,0,1)
 }
 SubShader {
 Tags {"RenderType"="Opaque"} LOD 250
 CGPROGRAM
 #pragma surface surf Standard fullforwardshadows
 #pragma target 3.0
 #include "UnityCG.cginc"
 fixed4 _Color;half _Metallic,_Glossiness,_Kind;half4 _EmissionColor;
 struct Input {float3 worldPos;};
 float hash(float3 p){return frac(sin(dot(p,float3(127.1,311.7,74.7)))*43758.5453);}
 float noise(float3 p){float3 i=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(lerp(hash(i),hash(i+float3(1,0,0)),f.x),lerp(hash(i+float3(0,1,0)),hash(i+float3(1,1,0)),f.x),f.y),lerp(lerp(hash(i+float3(0,0,1)),hash(i+float3(1,0,1)),f.x),lerp(hash(i+float3(0,1,1)),hash(i+float3(1,1,1)),f.x),f.y),f.z);}
 void surf(Input IN,inout SurfaceOutputStandard o){
 float3 p=IN.worldPos;float variation=1;float2 bump=0;float smoothness=_Glossiness;
 if(_Kind>.5&&_Kind<1.5){
 float3 freq=p*1400;float fade=1-saturate(length(fwidth(freq))*.28);
 float weave=sin(freq.x+freq.z)*sin(freq.y+freq.z);variation=.94+.06*weave*fade+.06*(noise(p*65)-.5);
 bump=float2(sin(freq.x+freq.z),cos(freq.y+freq.z))*.065*fade;
 }
 else if(_Kind>1.5&&_Kind<2.5){
 float3 q=p-unity_ObjectToWorld._m03_m13_m23;float angle=atan2(q.z,q.x);float sector=floor((angle+3.14159265)/.5235988)*.5235988;float grain=dot(q.xz,float2(cos(sector),sin(sector)));
 float n=noise(float3(q.x*3,q.y*7,q.z*3));float g=sin(grain*330+n*28+noise(q*float3(4,7,28))*9);float fine=sin(grain*900+n*63)*(1-saturate(length(fwidth(q))*200));variation=.92+.045*g+.015*fine+.055*n;smoothness+=.012*g;
 bump=float2(g,fine)*.006;
 }
 else if(_Kind>2.5&&_Kind<3.5){float n=noise(p*23);float vein=sin(p.y*15+noise(p*2)*8);variation=.975+.022*n+.012*vein;bump=float2(n-.5,noise(p*29)-.5)*.025;}
 o.Albedo=_Color.rgb*variation;o.Metallic=_Metallic;o.Smoothness=saturate(smoothness);o.Normal=normalize(float3(bump,1));o.Emission=_EmissionColor.rgb;o.Alpha=1;
 }
 ENDCG
 }
 FallBack "Standard"
}
