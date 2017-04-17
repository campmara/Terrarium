// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/DepthGrayscale" {
	Properties{
		_MainTex("", 2D) = "white" {}
		_ClipColor("Clip Color", color) = (0,0,0,0)
		_CameraProximity("Camera Proximity", float) = 1800000
	}
SubShader {
Tags { "RenderType"="Opaque" }

Pass{
CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"

//sampler2D _CameraDepthNormalsTexture;
sampler2D _CameraDepthTexture;
sampler2D _MainTex;
float4 _ClipColor;
float _CameraProximity;

struct v2f {
   float4 pos : SV_POSITION;
   float4 scrPos:TEXCOORD1;
};

//Vertex Shader
v2f vert (appdata_base v){
   v2f o;
   o.pos = UnityObjectToClipPos (v.vertex);
   o.scrPos=ComputeScreenPos(o.pos);
   //for some reason, the y position of the depth texture comes out inverted
   //o.scrPos.y = 1 - o.scrPos.y;
   return o;
}

//Fragment Shader
half4 frag (v2f i) : COLOR{
	fixed4 combinedColor;
	fixed4 orgColor = tex2Dproj(_MainTex, i.scrPos); //Get the orginal rendered color
   float depthValue = Linear01Depth (tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.scrPos)).r);
	//float depthValue;
	//float3 normalValues;
   //DecodeDepthNormal(tex2D(_CameraDepthNormalsTexture, i.scrPos.xy), depthValue, normalValues);
   half4 depth;

   depth.r = depthValue;
   depth.g = depthValue;
   depth.b = depthValue;
   depth.a = 1 - depthValue;

   float reallyCloseToCamera = clamp(depthValue*depthValue * _CameraProximity, 0, 1);

   combinedColor = lerp(_ClipColor*orgColor, orgColor, reallyCloseToCamera);
   //combinedColor = reallyCloseToCamera;//float4(depthValue,0);
   /*
   if (dot(o.Normal, IN.viewDir) < -.25) {
	   o.Albedo = float4(0, 0, 0, 1);
   }*/
   return combinedColor;
}
ENDCG
}
}
FallBack "Diffuse"
}