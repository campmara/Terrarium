// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

//Current issues: 
// - I haven't really researched what the vertex part of the distortion shader does
// - general tidying of files
// - lighting model includes unity's default which already reflects the world, it could easily be discarded and replaced with something on here:
// https://docs.unity3d.com/Manual/SL-SurfaceShaderLighting.html
// - Needs squashing from http://diary.conewars.com/melting-shader-part-2/
// - Some things might want to be sliders instead of values (vertex stuff)
// - I'm cheating rn with Chris's script in a lot of ways and it should be replaced SmearEFfectWaterBall.cs

Shader "Custom/Tree Bubble"
{
	Properties
	{
		[Header(Color)]
	//gradient plus
	_Color("Color 1", Color) = (1,1,1,1)
	_Color2("Color 2", Color) = (1,1,1,1)


	[Header(Constant Noise)]
	//general noise stuff
	_MainNoiseScale("Main Noise Scale", Float) = 1
		_MainNoiseHeight("Main Noise Height", Float) = 0
		_NoiseOffset("Noise Offset", Vector) = (0,0,0,0)
		_NoiseSpeed("Noise Speed", float) = 1
		[Header(Lighting)]
	_Hardness("Hardness", Range(0.001, 0.999)) = 0.5
		[Header(Wind Values)]
	[Toggle]_WindEnabled("Wind Enabled", float) = 1
		_Sensitivity("Height Sensitivity to Wind", Range(.000001, 1000)) = 15

		[Header(Melt)]
	//conewars melt
	_MeltY("Melt Y", Float) = 0.0
		_MeltDistance("Melt Distance", Float) = 1.0
		_MeltCurve("Melt Curve", Range(1.0,10.0)) = 2.0
		_MeltAmount("Melt Amount", Range(0.0, 1.0)) = 1.0
	}

		SubShader
	{

		Tags{ "Queue" = "Geometry" "IgnoreProjector" = "True" "RenderType" = "Opaque" }
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
		float4 color : COLOR;
	};

	fixed4 _Color;
	fixed4 _Color2;

	half _MainNoiseScale;
	half _MainNoiseHeight;

	fixed4 _NoiseOffset;
	float _NoiseSpeed;

	//melting variables from conewars
	half _MeltY;
	half _MeltDistance;
	half _MeltCurve;
	half _MeltAmount;

	//...wind...
	float _WindEnabled;
	float _Sensitivity;

	uniform fixed4 _WaveDir;
	uniform float _WaveNoise;
	uniform float _WaveAmount;
	uniform float _WaveScale;
	uniform float _WaveTime;
	uniform float3 _WaveTimeVec;
	uniform sampler2D _WindTex;
	//......

	void vert(inout appdata_full v, out Input o) {
		UNITY_INITIALIZE_OUTPUT(Input, o);

		float mainTex = tex2Dlod(_MainTex, v.texcoord);

		//calculate normals and apply deformation
		//...............
		//Vertex extrusion-
		float power = mainTex.r;
		float4 vertexExtrusion = abs(cnoise(v.normal * _MainNoiseScale + _NoiseOffset.xyz + _Time.xyz * _NoiseSpeed)) * _MainNoiseHeight * power;

		v.vertex.xyz += v.normal * vertexExtrusion;
		//--

		float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
		float4 worldNormal = mul(unity_ObjectToWorld, float4(v.normal, 0));

		//conewars melt-
		// these could cause problems in the future with differences in elevation
		float melt = (worldPos.y - _MeltY) / _MeltDistance;
		melt = 1 - saturate(melt);
		melt = pow(melt, _MeltCurve) * _MeltAmount;
		worldPos.xz += worldNormal.xz * melt * power;
		//--
			
		//Chris's wind function modified to take in to account world height and more directional wind
		//.......................
		//this effect causes the material to disappear if the amount is 0, this won't run the code if that's the case
		if (_WindEnabled != 0 && _WaveAmount != 0 && length(_WaveDir != 0)) {
			float noiseOffset = cnoise(worldPos.xyz) * _WaveNoise;
			//clamped to prevent extreme results at higher branches, needs tweaking, it would be nice if lower plants shook more in here
			float heightSensitivity = clamp(worldPos.y * worldPos.y, 0, _Sensitivity) / _Sensitivity;
			//oscillation value adds on to the direction of the wind, it's length is measured with _WaveScale
			float4 oscillation = sin(_WaveTime + noiseOffset  * heightSensitivity) * _WaveScale * normalize(_WaveDir) * heightSensitivity; // * v.color.r																																   //wave direction and oscillation combined are then scaled overall by the _WaveAmount
			float4 wind = (normalize(_WaveDir) + oscillation) * _WaveAmount * heightSensitivity; //* v.color.r
			float turbulenceScale = .025;
			float4 turbulence = tex2Dlod(_WindTex, (float4(worldPos.x, worldPos.z, 0, 0) + float4(_WaveTimeVec.x, _WaveTimeVec.z, 0, 0)) * .025);
			wind *= turbulence;
			worldPos += wind;
			//o.turbulence = turbulence;
		}
		//''''''''''''''''''''''

		v.vertex = mul(unity_WorldToObject, worldPos);
		//..............
		v.color = vertexExtrusion;
		o.pos = v.vertex.xyz;
	}

	void surf(Input IN, inout SurfaceOutput o)
	{
		float4 mainTex = tex2D(_MainTex, IN.uv_MainTex);

		fixed4 finalCol = _Color;

		//o.Albedo = _Color;
		o.Albedo = lerp(_Color, _Color2, 1 - IN.color.r);
	}
	ENDCG
	}

		FallBack "Diffuse"
}