Shader "Custom/Terrain"
{
	Properties{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_WaveTex("Wave (R)", 2D) = "white" {}
		_TerrainTex("Terrain Tex (R)", 2D) = "white" {}
		_DiscRadius("Disc Radius", range(0,1)) = .025
		_DiscFade("Disc Fade", range(0,250)) = 5
			_HeightmapStrength("Heightmap Strength", Float) = 1.0
			_HeightmapDimX("Heightmap Width", Float) = 2048
			_HeightmapDimY("Heightmap Height", Float) = 2048
		_Hardness("Hardness", Range(.25, 1)) = 0.5
	}
		SubShader{
		Tags{ "Queue" = "Geometry" }
		//Cull Off
		CGPROGRAM

		#pragma surface surf WrapLambert vertex:vert
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
	sampler2D _WaveTex;
	sampler2D _TerrainTex;
	
	float _DiscRadius;
	float _DiscFade;

	half4 _Color;
	//global variable for ground color
	uniform float4 _GroundColorPrimary;
	uniform float4 _GroundColorSecondary;
	
	uniform sampler2D _TerrainHeightMap;
	uniform float _TerrainHeightMultiplier;

	void vert(inout appdata_full v)
	{
		float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
		float4 mainTex = tex2Dlod(_TerrainHeightMap, float4(v.texcoord.xy, 0, 0));
		worldPos.y -= (mainTex.r * _TerrainHeightMultiplier);
		v.vertex = mul(unity_WorldToObject, worldPos);
	}

	float4 mix(float4 x, float4 y, float4 a) {
		return x * (1 - a) + y * a;
	}

	float _HeightmapStrength, _HeightmapDimX, _HeightmapDimY;

	void surf(Input IN, inout SurfaceOutput o) {

		float len = length(IN.worldPos * (1  -  _DiscRadius));
		float4 mainTex = tex2D(_MainTex, IN.uv_MainTex);

		float discLen = clamp(pow(len*mainTex.r, _DiscFade), 0, 1);
		//float4 terrainTex = tex2D(_TerrainTex, float2(len * .5 + _Time.x * 5, len));
		float4 waveTex = tex2D(_WaveTex, float2(len * .01 + _Time.x * 1, len * .01));

		//
		//float4 outside = lerp(unity_FogColor, _GroundColorPrimary, waveTex.r);
		//float4 col = lerp(_GroundColorSecondary, outside, discLen);
		float4 terrainColor = lerp(_GroundColorPrimary, _GroundColorSecondary, mainTex.r);
		float4 fogWaves = lerp(_GroundColorSecondary, unity_FogColor, waveTex.r * mainTex.r);
		float4 col = fogWaves;
		o.Albedo = col.rgb;

		//http://polycount.com/discussion/117185/creating-normals-from-alpha-heightmap-inside-a-shader
		float3 normal = float3(.5,.5,.5);

		float me = tex2D(_TerrainHeightMap, IN.uv_MainTex).x;
		float n = tex2D(_TerrainHeightMap, float2(IN.uv_MainTex.x, IN.uv_MainTex.y + 1.0 / _HeightmapDimY)).x;
		float s = tex2D(_TerrainHeightMap, float2(IN.uv_MainTex.x, IN.uv_MainTex.y - 1.0 / _HeightmapDimY)).x;
		float e = tex2D(_TerrainHeightMap, float2(IN.uv_MainTex.x - 1.0 / _HeightmapDimX, IN.uv_MainTex.y)).x;
		float w = tex2D(_TerrainHeightMap, float2(IN.uv_MainTex.x + 1.0 / _HeightmapDimX, IN.uv_MainTex.y)).x;

		float3 norm = normal;
		float3 temp = norm; //a temporary vector that is not parallel to norm
		if (norm.x == 1)
			temp.y += 0.5;
		else
			temp.x += 0.5;

		//form a basis with norm being one of the axes:
		float3 perp1 = normalize(cross(norm, temp));
		float3 perp2 = normalize(cross(norm, perp1));

		//use the basis to move the normal in its own space by the offset
		float3 normalOffset = -_HeightmapStrength * (((n - me) - (s - me)) * perp1 + ((e - me) - (w - me)) * perp2);
		norm += normalOffset;
		norm = normalize(norm);

		o.Normal = norm;
	}

	ENDCG
	}
		FallBack "Standard"
}
