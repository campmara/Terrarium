Shader "Custom/SinisterMan" 
{
	Properties 
	{
		_Color ("Color", Color) = (1,1,1,1)
		_Color2("Color", Color) = (1,1,1,1)

		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Cutoff ("Alpha Cutoff", Range(0, 1)) = 1

		[Header(Noise)]
		_NoiseScale("Scale", Float) = 0.5
		_NoiseAmplitude("Amplitude", Float) = 0.1
		_NoiseSpeed("Speed", Float) = 1.0
	}
	SubShader 
	{
		//if performance is bad it's probably because of  "DisableBatching"="True" !!! 
		Tags { "RenderType"="Transparent" "Queue"="AlphaTest" "DisableBatching"="True" }
		LOD 200

		Cull Off
		
		CGPROGRAM
		#pragma surface surf Lambert vertex:vert alphatest:_Cutoff addshadow
		#pragma target 3.0
		#include "Noise.cginc"

		fixed4 LightingNoLighting(SurfaceOutput s, fixed3 lightDir, fixed atten)
		{
			fixed4 c;
			c.rgb = s.Albedo;
			c.a = s.Alpha;
			return c;
		}

		sampler2D _MainTex;

		struct Input 
		{
			float2 uv_MainTex;
		};

		fixed4 _Color;
		fixed4 _Color2;

		half _NoiseScale;
		half _NoiseAmplitude;
		half _NoiseSpeed;

		void vert (inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);

			fixed4 worldPos = mul(transpose(unity_ObjectToWorld), float4(0, 1, 0, 1));

			v.vertex.xy += wnoise(v.vertex.xyz * _NoiseScale + (_Time.y * _NoiseSpeed)) * _NoiseAmplitude;
			v.normal = worldPos;
		}

		void surf (Input IN, inout SurfaceOutput o) 
		{
			// Albedo comes from a texture tinted by color
			//fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
			c.rgb = lerp(_Color, _Color2, c.r);
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Transparent/Cutout/Diffuse"
}