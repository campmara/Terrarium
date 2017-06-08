Shader "Custom/Terrain"
{
	Properties{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_WaveTex("Wave (R)", 2D) = "white" {}
		_TerrainTex("Terrain Tex (R)", 2D) = "white" {}
		_DiscRadius("Disc Radius", range(0,1)) = .025
		_DiscFade("Disc Fade", range(0,250)) = 5
		_Hardness("Hardness", Range(.25, 1)) = 0.5

		[Toggle]_SplatmapEnabled("Splatmap Enabled", float) = 0
		_SplatTex("Splat Texture (RGBA)", 2D) = "white" {}
		_ZeroY("Zero Y", float) = 0
	}
		SubShader{
		Tags{ "Queue" = "Geometry" }
		//Cull Off
		CGPROGRAM

		#pragma surface surf WrapLambert vertex:vert addshadow
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
	
	float _DiscRadius;
	float _DiscFade;

	half4 _Color;

	//global variable for ground color
	uniform float4 _GroundColorPrimary;
	uniform float4 _GroundColorSecondary;
	
	//...splatmap...
	float _ImprintAmount;
	float _SplatmapEnabled;

	uniform sampler2D _ClipEdges;
	uniform sampler2D _SplatMap;
	uniform float3 _CameraWorldPos;
	uniform float4 _SplatmapNeutralColor;
	uniform float _OrthoCameraScale;
	//..............

	//splatmap+
	float _ZeroY;
	sampler2D _SplatTex;

	void vert(inout appdata_full v, out Input o)
	{
		UNITY_INITIALIZE_OUTPUT(Input, o);

		float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
		float4 worldNormal = mul(unity_ObjectToWorld, v.normal);
		//splatmap deformation
		//...........
		if (_SplatmapEnabled != 0) {
			_OrthoCameraScale *= 2;
			float2 worldUV = float2(((worldPos.x - _CameraWorldPos.x) / _OrthoCameraScale + .5f), ((worldPos.z - _CameraWorldPos.z) / _OrthoCameraScale + .5f)); //find a way to center this
			float4 border = tex2Dlod(_ClipEdges, float4(worldUV, 0, 0)).rgba;
			float4 uv = (tex2Dlod(_SplatMap, float4(worldUV, 0, 0)));

			float4 duv = (tex2Dlod(_SplatTex, float4(uv.xy, 0, 0)));
			float4 d = duv;
			d = lerp(_SplatmapNeutralColor, d, uv.a);


			d.a = clamp(d.a - border.a, 0, 1);
			d.rg = (d.rg - .5f) * d.a;
			d.b *= d.a;

			worldPos.y = lerp(worldPos.y, 0 + _ZeroY, d.b);
			worldPos.x -= d.r;
			worldPos.z += d.g;
			worldNormal.xz = lerp(worldNormal.xz, d.rg, d.b * .5f); //this is rough
		}
		//......

		v.vertex = mul(unity_WorldToObject, worldPos);
		v.normal = mul(unity_WorldToObject, worldNormal.xyz);
	}

	void surf(Input IN, inout SurfaceOutput o) {

		float len = length(IN.worldPos * (1  -  _DiscRadius));
		float4 mainTex = tex2D(_MainTex, IN.uv_MainTex);
		float4 heightTex = tex2D(_MainTex, IN.uv_MainTex);


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
	}

	ENDCG
	}
		FallBack "Standard"
}
