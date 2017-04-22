// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Custom/VertexColorBlendshape" {
	Properties{
		_Color("Main Color", Color) = (0.5, 0.5, 0.5, 1)
		_MainTex("Base (RGB) Gloss (A)", 2D) = "white" {}
		_Deformation("Deformation", Range(0.00, 1)) = 0
		_ResultScale("Blend to Scale", float) = 2
		[Toggle]_Debug("Debug", float) = 0
	}

		SubShader{
		Tags{ "Queue" = "Geometry"
		"IgnoreProjector" = "True"
		"RenderType" = "Geometry"
	}
		LOD 400
		CGPROGRAM
#pragma surface surf BlinnPhong vertex:vert addshadow
#include "UnityCG.cginc"

	sampler2D _MainTex;
	float4 _Color;
	float _Deformation;
	float _ResultScale;

	struct Input {
		float2 uv_MainTex;
		float4 color : COLOR;
	};

	void vert(inout appdata_full v)
	{
		//3ds max flip vertex to rbga to account for difference 
		float3 colorToPosition = v.color.rgb;
		colorToPosition.r = (1 - colorToPosition.r) - .5;
		colorToPosition.g = colorToPosition.g;
		colorToPosition.b = colorToPosition.b - .5;

		colorToPosition.rgb *= _ResultScale;

		v.vertex.xyz = lerp(v.vertex.xyz, colorToPosition, _Deformation);
	}

	float _Debug;

	void surf(Input IN, inout SurfaceOutput o) {
		half4 c = 0;
		if (_Debug == 1) {
			c = IN.color;
		}
		else {
			half4 tex = tex2D(_MainTex, IN.uv_MainTex);
			c = tex;
		}
		o.Albedo = c.rgb;
		o.Alpha = 1;
	}
	ENDCG
	}
}