Shader "Custom/Particles/SplatParticle"
{
	Properties
	{
		_TintColor("Tint Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
	}
		SubShader
	{
		Tags{ "RenderType" = "Transparent" "Queue" = "AlphaTest" }
		ColorMask RGB
		LOD 200

		CGPROGRAM
#pragma surface surf Unlit
#pragma target 3.0

		sampler2D _MainTex;

	half4 LightingUnlit(SurfaceOutput s, half3 lightDir, half atten) {
		//half NdotL = dot(s.Normal, lightDir);
		half4 c;
		c.rgb = s.Albedo * _LightColor0.rgb * (atten);
		c.a = s.Alpha;
		return c;
	}

	struct Input
	{
		float2 uv_MainTex;
		float4 color : COLOR;
	};

	fixed4 _TintColor;

	void surf(Input IN, inout SurfaceOutput o)
	{
		// Albedo comes from a texture tinted by color
		fixed4 c = tex2D(_MainTex, IN.uv_MainTex);

		if (c.a < 1 - IN.color.a)
		{
			discard;
		}

		c *= IN.color;

		o.Albedo = _TintColor.rgb * IN.color.rgb;
	}
	ENDCG
	}
		FallBack Off
}
