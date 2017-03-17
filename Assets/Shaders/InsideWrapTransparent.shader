Shader "InsideWrap/Hardness (Half Lambert) with Transparency"
{
	Properties{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_Color("Color 1", Color) = (1,1,1,1)
		_Color2("Color 2", Color) = (1,1,1,1)
		_Hardness("Hardness", Range(.25, 1)) = 0.5
	}
		SubShader{
		Tags{ "Queue" = "Transparent" }
		//Cull Off
		Zwrite Off
		CGPROGRAM
		
	#pragma surface surf WrapLambert alpha:auto
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
	};

	sampler2D _MainTex;
	float4 _Color;
	float4 _Color2;

	void surf(Input IN, inout SurfaceOutput o) {
		float4 c = tex2D(_MainTex, IN.uv_MainTex);
		c.rgb = lerp(_Color, _Color2, c.r);
		o.Albedo = c.rgb;
		o.Alpha = c.a;
	}

	ENDCG
	}
		FallBack "Transparent/Cutout/Diffuse"
}
