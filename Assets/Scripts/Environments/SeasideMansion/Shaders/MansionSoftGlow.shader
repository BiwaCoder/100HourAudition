Shader "Hidden/HundredHour/Mansion Soft Glow" {
 Properties { _MainTex("Source",2D)="white" {} _GlowTex("Glow",2D)="black" {} }
 SubShader { Cull Off ZWrite Off ZTest Always
 CGINCLUDE
 #include "UnityCG.cginc"
 sampler2D _MainTex,_GlowTex;float4 _MainTex_TexelSize;float2 _Direction;float _Threshold,_Intensity;
 half4 bright(v2f_img i):SV_Target {half3 c=tex2D(_MainTex,i.uv).rgb;half l=max(c.r,max(c.g,c.b));return half4(c*max(l-_Threshold,0)/max(l,.0001),1);}
 half4 blur(v2f_img i):SV_Target {return tex2D(_MainTex,i.uv)*.227027+(tex2D(_MainTex,i.uv+_Direction*1.384615)+tex2D(_MainTex,i.uv-_Direction*1.384615))*.316216+(tex2D(_MainTex,i.uv+_Direction*3.230769)+tex2D(_MainTex,i.uv-_Direction*3.230769))*.070270;}
 half4 composite(v2f_img i):SV_Target {half4 c=tex2D(_MainTex,i.uv);c.rgb+=tex2D(_GlowTex,i.uv).rgb*_Intensity;return c;}
 ENDCG
 Pass { CGPROGRAM
 #pragma vertex vert_img
 #pragma fragment bright
 ENDCG }
 Pass { CGPROGRAM
 #pragma vertex vert_img
 #pragma fragment blur
 ENDCG }
 Pass { CGPROGRAM
 #pragma vertex vert_img
 #pragma fragment composite
 ENDCG }
 }
}
