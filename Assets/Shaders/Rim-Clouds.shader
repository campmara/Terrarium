﻿Shader "RimVolumetric/Rim-Clouds" {
	Properties{
		_MainColor("Main Color", Color) = (0.26,0.19,0.16,0.0)
		_SecondaryColor("Secondary Color", Color) = (0.7,0.19,0.5,0.0)
		_RimPower("Rim Power", Range(0,30)) = 3.0

		_NoiseScale("Noise Scale", vector) = (0,0,0,0)
		_Seed("Seed", float) = .1

		_Cutoff("Cutoff", Range(0, 1)) = 1

		_HeightMultiplier("Height Multiplier", Range(0,.5)) = .15
	}

		SubShader{
		Tags{ "Queue" = "Geometry" }
		Cull Off
		CGPROGRAM

#pragma surface surf Clouds alphatest:_Cutoff addshadow nofog

	struct Input {
		float3 viewDir;
		float3 worldPos;
		float4 screenPos;
		float4 pos : SV_POSITION;
		float4 projPos : TEXCOORD1;
	};

	float4 _MainColor;
	float4 _SecondaryColor;
	float _RimPower;
	float _Seed;
	float4 _NoiseScale;
	sampler2D _CameraDepthTexture;
	sampler2D _LightTextureB0;
	float _HeightMultiplier;

	float hash(float n)
	{
		return frac(sin(n)*43758.5453);
	}

	float noise(float3 x)
	{
		// The noise function returns a value in the range -1.0f -> 1.0f

		float3 p = floor(x);
		float3 f = frac(x);

		f = f*f*(3.0 - 2.0*f);
		float n = p.x + p.y*57.0 + 113.0*p.z;

		return lerp(lerp(lerp(hash(n + 0.0), hash(n + 1.0), f.x),
			lerp(hash(n + 57.0), hash(n + 58.0), f.x), f.y),
			lerp(lerp(hash(n + 113.0), hash(n + 114.0), f.x),
				lerp(hash(n + 170.0), hash(n + 171.0), f.x), f.y), f.z);
	}

	void surf(Input IN, inout SurfaceOutput o) {
		float depth = length(IN.worldPos ) / 150; // - _WorldSpaceCameraPos
		//depth = 1;

		float t = _Seed;

		//method for sampling from the noise based on world position.
		half4 n = 0;
		n.x = IN.worldPos.x * _NoiseScale.x;
		n.y = IN.worldPos.y * _NoiseScale.y;
		n.z = IN.worldPos.z * _NoiseScale.z;

		float heightmul = clamp(IN.worldPos.y * _HeightMultiplier, 0, 1);
		float4 fog = lerp(_MainColor, _SecondaryColor, heightmul*heightmul*clamp(depth, 0, 1));
		o.Albedo = fog;
		//o.Albedo = depth;
		//o.Albedo = _MainColor;

		//the normals used in this rim function are deformed by our rimtexture giving part of the main effect of the shader
		half rim = 1 - saturate(dot(normalize(IN.viewDir), o.Normal));
		half oppositeside = saturate(dot(normalize(IN.viewDir), -o.Normal));
		o.Alpha = 1 - pow(rim, _RimPower) + noise(n * 5 + _Seed)*.01 + noise(n * 25 + _Seed) * .25 + pow(oppositeside, _RimPower); // 
	}

	half4 LightingClouds(SurfaceOutput s, half3 lightDir, half atten) {
		half4 c;
		c.rgb = atten * s.Albedo;
		c.a = s.Alpha;
		return c;
	}

	ENDCG
	}
		FallBack "Diffuse"
}