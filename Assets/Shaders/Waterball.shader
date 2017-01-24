﻿//Current issues: 
// - I haven't really researched what the vertex part of the distortion shader does
// - general tidying of files
// - lighting model includes unity's default which already reflects the world, it could easily be discarded and replaced with something on here:
// https://docs.unity3d.com/Manual/SL-SurfaceShaderLighting.html
// - Needs squashing from http://diary.conewars.com/melting-shader-part-2/
// - Some things might want to be sliders instead of values (vertex stuff)
// - I'm cheating rn with Chris's script in a lot of ways and it should be replaced SmearEFfectWaterBall.cs

Shader "Custom/Waterball"
{
	Properties
	{
		_ColorTop("Top Color", Color) = (1,1,1,1)
		_ColorMid("Mid Color", Color) = (1,1,1,1)
		_ColorBot("Bot Color", Color) = (1,1,1,1)
		_Middle("Middle", Range(0.001, 0.999)) = 0.5
		_MainTex("Albedo (RGB)", 2D) = "white" {}

		// Rim Lighting
		_RimColor ("Rim Color", Color) = (0.26,0.19,0.16,0.0)
      	_RimPower ("Rim Power", Range(0.5,8.0)) = 3.0
		
		//distortion
		_DistAmt("Distortion", range(0,128)) = 10
		_ColorFog("Fogginess", range(0, 1)) = 0.5

		//delay stuff
		_Position("Position", Vector) = (0, 0, 0, 0)
		_PrevPosition("Prev Position", Vector) = (0, 0, 0, 0)
		_SmearNoiseScale("Smear Noise Scale", Float) = 15
		_SmearNoiseHeight("Smear Noise Height", Float) = 1.3

		//general noise stuff
		_MainNoiseScale("Main Noise Scale", Float) = 15
		_MainNoiseHeight("Main Noise Height", Float) = 1.3
		_NoiseOffset("Noise Offset", Vector) = (0,0,0,0)
	}

	SubShader
	{
		GrabPass{}

		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Opaque" }
		LOD 200

		CGPROGRAM
#pragma surface surf NoLighting vertex:vert addshadow
#pragma target 3.0

		#include "Noise.cginc"

		sampler2D _MainTex;

		struct Input
		{
			float2 uv_MainTex;
			float3 viewDir;
			float4 proj : TEXCOORD;
		};

		float4 _RimColor;
	    float _RimPower;

		fixed4 _ColorTop;
		fixed4 _ColorMid;
		fixed4 _ColorBot;
		float _Middle;

		fixed4 _PrevPosition;
		fixed4 _Position;

		half _SmearNoiseScale;
		half _SmearNoiseHeight;

		half _MainNoiseScale;
		half _MainNoiseHeight;

		fixed4 _NoiseOffset;

		//distortion
		float _DistAmt;
		float _ColorFog;
		sampler2D _GrabTexture;
		float4 _GrabTexture_TexelSize;

		fixed4 LightingNoLighting(SurfaceOutput s, fixed3 lightDir, fixed atten)
		{
			fixed4 c;
			c.rgb = s.Albedo + s.Emission;
			c.a = s.Alpha;
			return c;
		}

		void vert(inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);

			//Vertex extrusion
			v.vertex.xyz += v.normal * cnoise(v.normal * _MainNoiseScale + _NoiseOffset) * _MainNoiseHeight; // * _MainNoiseHeight _MainNoiseScale

			//Chris's smear
			fixed4 worldPos = mul(unity_ObjectToWorld, v.vertex);

			fixed3 worldOffset = _Position.xyz - _PrevPosition.xyz; // -5
			fixed3 localOffset = worldPos.xyz - _Position.xyz; // -5

															   // World offset should only be behind swing
			float dirDot = dot(normalize(worldOffset), normalize(localOffset));
			fixed3 unitVec = fixed3(1, 1, 1) * _SmearNoiseHeight;
			worldOffset = clamp(worldOffset, unitVec * -1, unitVec);
			worldOffset *= -clamp(dirDot, -1, 0) * lerp(1, 0, step(length(worldOffset), 0));

			fixed3 smearOffset = -worldOffset.xyz * lerp(1, cnoise(worldPos * _SmearNoiseScale), step(0, _SmearNoiseScale));
			worldPos.xyz += smearOffset;
			v.vertex = mul(unity_WorldToObject, worldPos);

			//https://forum.unity3d.com/threads/refraction-example.78750/
			//refraction distorting uvs
			float4 oPos = mul(UNITY_MATRIX_MVP, v.vertex);
			#if UNITY_UV_STARTS_AT_TOP
				float scale = -1.0;
			#else
				float scale = 1.0;
			#endif
			o.proj.xy = (float2(oPos.x, oPos.y*scale) + oPos.w) * 0.5;
			o.proj.zw = oPos.zw;
		}

		void surf(Input IN, inout SurfaceOutput o)
		{
			//refraction from above url
			half3 nor = cnoise((o.Normal) + _Time);
			float2 offset = nor  * _DistAmt * _GrabTexture_TexelSize.xy;
			IN.proj.xy = offset * IN.proj.z + IN.proj.xy; 
			half4 col = tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(IN.proj));
			
			// Albedo comes from a texture tinted by color
			fixed4 gradient = lerp(_ColorBot, _ColorMid, IN.uv_MainTex.y / _Middle) * step(IN.uv_MainTex.y, _Middle);
			gradient += lerp(_ColorMid, _ColorTop, (IN.uv_MainTex.y - _Middle) / (1 - _Middle)) * step(_Middle, IN.uv_MainTex.y);

			fixed4 finalCol = lerp(gradient, col * gradient, _ColorFog);
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * finalCol;
			o.Albedo = c.rgb;

			half rim = 1.0 - saturate(dot (normalize(IN.viewDir), nor));
	        o.Emission = _RimColor.rgb * pow (rim, _RimPower);

			o.Alpha = c.a;
		}
		ENDCG
	}

		FallBack "Diffuse"
}