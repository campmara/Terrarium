Shader "InsideWrap/Hardness (Half Lambert)"
{
	Properties{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_Color("Color", Color) = (1, 1, 1, 1)

		_Hardness("Hardness", Range(.25, 1)) = 0.5
	}
		SubShader{
		Tags{ "Queue" = "Geometry" }
		//Cull Off
		CGPROGRAM

		#pragma surface surf WrapLambert 
		#include "noise.cginc"

		half _Hardness;

	half4 LightingWrapLambert(SurfaceOutput s, half3 lightDir, half atten) {
		s.Normal = normalize(s.Normal);

		half distAtten;
		if (_WorldSpaceLightPos0.w == 0.0)
			distAtten = 1.0;
		else
			distAtten = saturate(1.0 / length(lightDir));

		half diff = (max(0, dot(s.Normal, lightDir)) * atten + 1 - _Hardness) * _Hardness; ;

		half4 c;
		c.rgb = (s.Albedo * diff * _LightColor0) * distAtten;
		c.a = s.Alpha;
		return c;
	}

	struct Input {
		float2 uv_MainTex;
		float3 worldPos;
	};

	sampler2D _MainTex;

	half4 _Color;
	//global variable for ground color
	uniform float4 _GroundColorPrimary;
	uniform float4 _GroundColorSecondary;

	void surf(Input IN, inout SurfaceOutput o) {
		float len = length(IN.worldPos * (.025));//*sin(IN.worldPos*.05);
		len = clamp(pow(len,5), 0, 1);
		float4 col = lerp(_GroundColorSecondary, _Color, len);
		o.Albedo = col.rgb;
	}

	ENDCG
	}
		FallBack "Standard"
}
