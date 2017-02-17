Shader "RimVolumetric/Rim-Clouds" {
	Properties{
		_MainColor("Main Color", Color) = (0.26,0.19,0.16,0.0)
		_SecondaryColor("Secondary Color", Color) = (0.7,0.19,0.5,0.0)
		_RimPower("Rim Power", Range(0,30)) = 3.0

		_NoiseScale("Noise Scale", vector) = (0,0,0,0)
		_Seed("Seed", float) = .1

		_Hardness("Hardness", Range(0,1)) = 1
		_Cutoff("Cutoff", Range(0, 1)) = 1

		_HeightMultiplier("Height Multiplier", Range(0,.5)) = .15
	}

		SubShader{
		Tags{ "Queue" = "Geometry" }
		Cull Off
		CGPROGRAM
#include "Noise.cginc"

#pragma surface surf WrapLambert alphatest:_Cutoff addshadow nofog

	float _Hardness;

	half4 LightingWrapLambert(SurfaceOutput s, half3 lightDir, half atten) {
		s.Normal = normalize(s.Normal);

		half distAtten;
		if (_WorldSpaceLightPos0.w == 0.0)
			distAtten = 1.0;
		else
			distAtten = saturate(1.0 / length(lightDir));

		atten = lerp(atten, cnoise(atten), .45);

		half diff = (max(0, dot(s.Normal, lightDir)) * atten + 1 - _Hardness);

		half4 c;
		c.rgb = (s.Albedo * diff * _LightColor0) * distAtten;
		c.a = s.Alpha;
		return c;
	}

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
	float _HeightMultiplier;

	void surf(Input IN, inout SurfaceOutput o) {
		//float depth = length(IN.worldPos - _WorldSpaceCameraPos) / 150;
		depth = 1;

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

		o.Alpha = 1 - pow(rim, _RimPower) + cnoise(n + _Seed) + cnoise(n *50 + _Seed)*.25 + pow(oppositeside, _RimPower);
	}
	ENDCG
	}
		FallBack "Diffuse"
}