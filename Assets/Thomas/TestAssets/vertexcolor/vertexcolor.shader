// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Custom/VertexColorBlendshape" {
	Properties{
		_Color("Main Color", Color) = (0.5, 0.5, 0.5, 1)
		_MainTex("Base (RGB) Gloss (A)", 2D) = "white" {}
		_Deformation("Deformation", Range(0.00, 1)) = 0
	}

		SubShader{
		Tags{ "Queue" = "Geometry"
		"IgnoreProjector" = "True"
		"RenderType" = "Geometry"
	}
		LOD 400
		CGPROGRAM
#pragma surface surf BlinnPhong vertex:vert
#include "UnityCG.cginc"

	sampler2D _MainTex;
	float4 _Color;
	float _Deformation;

	struct Input {
		float2 uv_MainTex;
		float4 color : COLOR;
	};

	void vert(inout appdata_full v)
	{
		//3ds max flip vertex to rbga to account for difference 
		float3 colorToPosition = v.color.rgb;
		colorToPosition.r = 1 - colorToPosition.r;
		colorToPosition.g = colorToPosition.g;
		colorToPosition.b = colorToPosition.b;

		float centerOffset = .5;
		float scale = 2;

		v.vertex.xyz = lerp(v.vertex.xyz, (colorToPosition - centerOffset) * scale, _Deformation);
	}

	void surf(Input IN, inout SurfaceOutput o) {
		half4 tex = tex2D(_MainTex, IN.uv_MainTex);
		half4 c = IN.color;
		o.Albedo = c.rgb;
		o.Alpha = 1;
	}
	ENDCG
	}
}