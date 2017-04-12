Shader "Custom/Particles/SplatParticle" 
{
	Properties 
	{
		_TintColor ("Tint Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
	}
	SubShader 
	{
		Tags { "RenderType"="Transparent" "Queue"="AlphaTest"}
		ColorMask RGB
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Standard
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input 
		{
			float2 uv_MainTex;
			float4 color : COLOR;
		};

		fixed4 _TintColor;

		void surf (Input IN, inout SurfaceOutputStandard o) 
		{
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex);

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
