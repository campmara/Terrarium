Shader "Custom/Ground"
{
	Properties
	{
		// Disc Clipping
		_Center("Center of Disc", Vector) = (0, 0, 0, 0)
		_Radius("Disc Radius", Range(0.0, 1.0)) = 1.0
		_ScaleFactor("Scale Factor", Float) = 3.0
		
		// Textures
		_MainTex("Splat Map", 2D) = "white" {}
		_NormalTex("Normal Map", 2D) = "bump" {}

		// Color Samples
		_GroundColor("Ground Color", Color) = (1, 1, 1, 1)
		_GrassColor("Grass Color", Color) = (1, 1, 1, 1)
		_WorldDistortion("World Noise Distortion", Float) = 0.25

		// Wrap Lambert Lighting
		_Hardness("Hardness", Range(.25, 1)) = 0.5
	}
	SubShader
	{
		Tags{ "Queue" = "Geometry+20" }
		//Cull Off
		CGPROGRAM
			
		#pragma surface surf WrapLambert addshadow
		#include "Noise.cginc"

		half _Hardness;

		half4 LightingWrapLambert(SurfaceOutput s, half3 lightDir, half atten) 
		{
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

		struct Input 
		{
			float2 uv_MainTex;
			float2 uv_SecondaryTex;
			float2 uv_NormalTex;
			float3 worldPos;
		};

		fixed4 _Center;
		float _Radius;
		float _ScaleFactor;

		sampler2D _MainTex;
		sampler2D _SecondaryTex;
		sampler2D _NormalTex;

		fixed4 _GroundColor;
		fixed4 _GrassColor;
		float _WorldDistortion;

		//global variable for ground color
		uniform float4 _GroundColorPrimary;
		uniform float4 _GroundColorSecondary;

		void surf(Input IN, inout SurfaceOutput o) 
		{
			fixed3 noise = cnoise(IN.worldPos * _WorldDistortion);
			fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
			fixed4 finalTex = tex2D(_MainTex, lerp(IN.uv_MainTex, noise, 0.004));

            //fixed3 c = lerp(_GrassColor, _GroundColor, finalTex.a);
			fixed3 c = lerp(_GroundColorPrimary, _GroundColorSecondary, finalTex.a);
            o.Albedo = c;

			o.Normal = UnpackNormal(tex2D(_NormalTex, IN.uv_NormalTex));

			float len = length(IN.worldPos - _Center) + cnoise(IN.worldPos) + cnoise(IN.worldPos * 4 + _Time.y);
			float rad = _Radius * (5 * _ScaleFactor);
			
			if (len > rad)
			{
				discard;
			}
			
			//o.Alpha = lerp(1, 0, len*.025f);
		}

		ENDCG
	}
}
