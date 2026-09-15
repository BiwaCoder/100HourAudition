Shader "HundredHour/Fountain Cascade"
{
 Properties {_Color("Water",Color)=(.36,.72,.76,.4)}
 SubShader {Tags {"Queue"="Transparent" "RenderType"="Transparent"} Cull Off ZWrite Off
 CGPROGRAM
 #pragma surface surf Standard alpha:premul
 #pragma target 3.0
 fixed4 _Color;
 struct Input {float3 worldPos;};
 void surf(Input IN,inout SurfaceOutputStandard o){float flow=sin(IN.worldPos.y*44+_Time.y*13+IN.worldPos.x*7)*.5+.5; o.Albedo=_Color.rgb; o.Metallic=.25;o.Smoothness=.92;o.Alpha=_Color.a*(.65+flow*.35);o.Emission=_Color.rgb*pow(flow,12)*.16;}
 ENDCG
 } FallBack "Standard"
}
