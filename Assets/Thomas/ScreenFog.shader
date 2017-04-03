// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/DepthGrayscale" {
	Properties{
		_MainTex("", 2D) = "white" {}
		_ClipColor("Clip Color", color) = (0,0,0,0)
		_CameraProximity("Camera Proximity", float) = 1800000
		_Cube("Skybox Cubemap", CUBE) = "" {}
	}
		SubShader{

		Tags{ "RenderType" = "Opaque" }

		Pass{
		CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"


		samplerCUBE _Cube;
		sampler2D _CameraDepthTexture;
	sampler2D _MainTex;
	float4 _MainTex_ST;
	float4 _ClipColor;
	float _CameraProximity;

	struct v2f {
		float4 pos : SV_POSITION;
		float3 viewDir : TEXCOORD0;
		float4 scrPos:TEXCOORD1;
	};

	//Vertex Shader
	v2f vert(appdata_full v) {
		v2f o;

		float4x4 modelMatrix = unity_ObjectToWorld;
		o.viewDir = mul(modelMatrix, v.vertex).xyz
			- _WorldSpaceCameraPos;

		o.pos = UnityObjectToClipPos(v.vertex);
		o.scrPos = ComputeScreenPos(o.pos);
		//for some reason, the y position of the depth texture comes out inverted
		//o.scrPos.y = 1 - o.scrPos.y;
		return o;
	}

	//Fragment Shader
	half4 frag(v2f i) : COLOR{
		fixed4 combinedColor;
	fixed4 orgColor = tex2Dproj(_MainTex, i.scrPos); //Get the orginal rendered color
	fixed4 skyColor = texCUBE(_Cube, i.viewDir);
	float depthValue = Linear01Depth(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.scrPos)).r);
	half4 depth;

	depth.r = depthValue;
	depth.g = depthValue;
	depth.b = depthValue;
	depth.a = 1 - depthValue;

	float reallyCloseToCamera = clamp(depthValue*depthValue * _CameraProximity, 0, 1);
	if (depthValue >= 1) {
		return orgColor;
	}
	combinedColor = lerp(orgColor, skyColor, 1);
	//combinedColor = skyColor;
	return combinedColor;
	}
		ENDCG
	}
	}
		FallBack "Diffuse"
}