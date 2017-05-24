// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

//Current issues: 
// - I haven't really researched what the vertex part of the distortion shader does
// - general tidying of files
// - lighting model includes unity's default which already reflects the world, it could easily be discarded and replaced with something on here:
// https://docs.unity3d.com/Manual/SL-SurfaceShaderLighting.html
// - Needs squashing from http://diary.conewars.com/melting-shader-part-2/
// - Some things might want to be sliders instead of values (vertex stuff)
// - I'm cheating rn with Chris's script in a lot of ways and it should be replaced SmearEFfectWaterBall.cs

Shader "Custom/Bunny"
{
	Properties
	{
		[Header(Color)]
	//gradient plus
	_ColorTop("Top Color", Color) = (1,1,1,1)
		_ColorMid("Mid Color", Color) = (1,1,1,1)
		_ColorBot("Bot Color", Color) = (1,1,1,1)
		_Middle("Middle", Range(0.001, 0.999)) = 0.5
		_MainTex("Albedo (RGB)", 2D) = "white" {}

		[Header(Constant Noise)]
	//general noise stuff
	_MainNoiseScale("Main Noise Scale", Float) = 1
		_MainNoiseHeight("Main Noise Height", Float) = 0
		_NoiseOffset("Noise Offset", Vector) = (0,0,0,0)
		_NoiseSpeed("Noise Speed", float) = 1
		[Header(Lighting)]
	_Hardness("Hardness", Range(0.001, 0.999)) = 0.5


		[Header(Melt)]
	//conewars melt
	_MeltY("Melt Y", Float) = 0.0
		_MeltDistance("Melt Distance", Float) = 1.0
		_MeltCurve("Melt Curve", Range(1.0,10.0)) = 2.0
		_MeltAmount("Melt Amount", Range(0.0, 1.0)) = 1.0
	}

		SubShader
	{

		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf WrapLambert vertex:vert addshadow
#pragma target 3.0

#include "Noise.cginc"

		sampler2D _MainTex;
	float _Hardness;


	half4 LightingWrapLambert(SurfaceOutput s, half3 lightDir, half3 viewDir, half atten)
	{
		s.Normal = normalize(s.Normal);
		half distAtten;
		if (_WorldSpaceLightPos0.w == 0.0)
			distAtten = 1.0;
		else
			distAtten = saturate(1.0 / length(lightDir));

		half diff = (max(0, dot(s.Normal, lightDir)) * atten + 1 - _Hardness) * _Hardness;

		half4 c;
		c.rgb = (s.Albedo * diff * _LightColor0) * distAtten;
		c.a = s.Alpha;
		return c;
	}

	struct Input
	{
		float2 uv_MainTex; // for gradient
		float3 viewDir; // for rim lighting
		float4 proj : TEXCOORD; // for refraction
		float3 pos; // for melt and gradient
	};

	fixed4 _ColorTop;
	fixed4 _ColorMid;
	fixed4 _ColorBot;
	float _Middle;

	half _MainNoiseScale;
	half _MainNoiseHeight;

	fixed4 _NoiseOffset;
	float _NoiseSpeed;

	//melting variables from conewars
	half _MeltY;
	half _MeltDistance;
	half _MeltCurve;
	half _MeltAmount;

	//http://diary.conewars.com/vertex-displacement-shader/ for normal recalculation
	//deformation function
	float4 getNewVertPosition(float4 objectSpacePosition, float3 objectSpaceNormal, float power)
	{

		//Vertex extrusion-
		objectSpacePosition.xyz += objectSpaceNormal * abs(cnoise(objectSpaceNormal * _MainNoiseScale + _NoiseOffset.xyz + _Time.xyz * _NoiseSpeed)) * _MainNoiseHeight * power;
		//--

		float4 worldSpacePosition = mul(unity_ObjectToWorld, objectSpacePosition);
		float4 worldSpaceNormal = mul(unity_ObjectToWorld, float4(objectSpaceNormal, 0));

		float yOffset = clamp(worldSpacePosition.y*1.5, 0, 1.25) / 3;

		worldSpacePosition.y += yOffset;

		//conewars melt-
		// these could cause problems in the future with differences in elevation
		float melt = (worldSpacePosition.y - _MeltY) / _MeltDistance;
		melt = 1 - saturate(melt);
		melt = pow(melt, _MeltCurve) * _MeltAmount;
		worldSpacePosition.xz += worldSpaceNormal.xz * melt * power;
		//--

		return mul(unity_WorldToObject, worldSpacePosition);
	}

	void vert(inout appdata_tan v, out Input o) {
		UNITY_INITIALIZE_OUTPUT(Input, o);

		float mainTex = tex2Dlod(_MainTex, v.texcoord);
		//calculate normals and apply deformation
		//...............
		float4 vertPosition = getNewVertPosition(v.vertex, v.normal, mainTex.r);
		float4 bitangent = float4(cross(v.normal, v.tangent), 0);

		float vertOffset = 0.01;

		float4 v1 = getNewVertPosition(v.vertex + v.tangent * vertOffset, v.normal, mainTex.r);
		float4 v2 = getNewVertPosition(v.vertex + bitangent * vertOffset, v.normal, mainTex.r);

		float4 newTangent = v1 - vertPosition;
		float4 newBitangent = v2 - vertPosition;
		v.normal = cross(newTangent, newBitangent);
		v.vertex = vertPosition;
		//..............

		o.pos = v.vertex.xyz;
	}

	void surf(Input IN, inout SurfaceOutput o)
	{
		float4 mainTex = tex2D(_MainTex, IN.uv_MainTex);

		float gradUV = IN.pos.y*.5 + .5;

		//murray's gradient
		fixed4 gradient = lerp(_ColorBot, _ColorMid, gradUV / _Middle) * step(gradUV, _Middle);
		gradient += lerp(_ColorMid, _ColorTop, (gradUV - _Middle) / (1 - _Middle)) * step(_Middle, gradUV);

		//mix em'
		fixed4 finalCol = gradient * mainTex;

		o.Albedo = finalCol;
	}
	ENDCG
	}

		FallBack "Diffuse"
}