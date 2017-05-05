// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

//Current issues: 
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
		[Header(Color)]
		//gradient plus
		_ColorTop("Top Color", Color) = (1,1,1,1)
		_ColorMid("Mid Color", Color) = (1,1,1,1)
		_ColorBot("Bot Color", Color) = (1,1,1,1)
		_Middle("Middle", Range(0.001, 0.999)) = 0.5
		_MainTex("Albedo (RGB)", 2D) = "white" {}

		[Header(Rim Lighting)]
		// Rim Lighting
		_RimColor ("Rim Color", Color) = (0.26,0.19,0.16,0.0)
      	_RimPower ("Rim Power", Range(0.5,8.0)) = 0.9

		[Header(Specular Lighting)]
		// Rim Lighting
		_SpecularColor("Specular Color", Color) = (0.26,0.19,0.16,0.0)
		_SpecularPower("Specular Power", float) = 128
		
		[Header(Distortion)]
		//distortion
		_DistAmt("Distortion", range(0,128)) = 13
		_ColorFog("Fogginess", range(0, 1)) = 0.78

		[Header(Smear)]
		//delay stuff
		[HideInInspector]_Position("Position", Vector) = (0, 0, 0, 0)
		[HideInInspector]_PrevPosition("Prev Position", Vector) = (0, 0, 0, 0)
		_SmearNoiseScale("Smear Noise Scale", Float) = 1
		_SmearNoiseHeight("Smear Noise Height", Float) = 1.3

		[Header(Constant Noise)]
		//general noise stuff
		_MainNoiseScale("Main Noise Scale", Float) = 1
		_MainNoiseHeight("Main Noise Height", Float) = 0
		_NoiseOffset("Noise Offset", Vector) = (0,0,0,0)

		[Header(Wind Values)]
		[Toggle]_WindEnabled("Wind Enabled", float) = 1
		_Sensitivity("Height Sensitivity to Wind", Range(.000001, 1000)) = 15

		[Header(Melt)]
		//conewars melt
		_MeltY("Melt Y", Float) = 0.0
		_MeltDistance("Melt Distance", Float) = 1.0
		_MeltCurve("Melt Curve", Range(1.0,10.0)) = 2.0
		_MeltAmount("Melt Amount", Range(0.0, 1.0)) = 1.0

		[Header(Spherification)]
		_SphereCenter("Center", vector) = (0,0,0,0)
		_SphereScale("Sphere Scale", float) = 1
		_Spherification("Spherify Amount", Range(0,1)) = 0

		[Header(Blur)]
		_BlurSize("Blur Size", float) = 1
	}

	SubShader
	{
		GrabPass{}

		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf WaterPlayer vertex:vert addshadow
#pragma target 3.0

		#include "Noise.cginc"

		sampler2D _MainTex;

		struct Input
		{
			float2 uv_MainTex; // for gradient
			float3 viewDir; // for rim lighting
			float4 proj : TEXCOORD; // for refraction
			float3 pos; // for melt and gradient
		};

		float4 _RimColor;
	    float _RimPower;

		float4 _SpecularColor;
		float _SpecularPower;

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

		//wind
		float _WindEnabled;
		float _Sensitivity;

		//these values are uniform so that they'll work globally
		uniform fixed4 _WaveDir;
		uniform float _WaveNoise;
		uniform float _WaveAmount;
		uniform float _WaveScale;
		uniform float _WaveTime;

		//melting variables from conewars
		half _MeltY;
		half _MeltDistance;
		half _MeltCurve;
		half _MeltAmount;

		//spherification values
		float _Spherification;
		float _SphereScale;
		float4 _SphereCenter;

		//blur values
		float _BlurSize;

		fixed4 LightingWaterPlayer(SurfaceOutput s, fixed3 lightDir, fixed atten, fixed3 viewDir)
		{
			fixed4 c;

			half3 h = normalize(lightDir + viewDir);

			half diff = max(0, dot(s.Normal, lightDir));

			float nh = max(0, dot(s.Normal, h));
			float spec = pow(nh, _SpecularPower);

			c.rgb = s.Albedo.rgb + spec*_SpecularColor.rgb;
			c.a = s.Alpha;
			return c;
		}

		//http://diary.conewars.com/vertex-displacement-shader/ for normal recalculation
		//deformation function
		float4 getNewVertPosition(float4 objectSpacePosition, float3 objectSpaceNormal)
		{
			//Vertex extrusion-
			objectSpacePosition.xyz += objectSpaceNormal * abs(cnoise(objectSpaceNormal * _MainNoiseScale + _NoiseOffset.xyz)) * _MainNoiseHeight;
			//--

			//Chris's smear-
			float4 worldSpacePosition = mul(unity_ObjectToWorld, objectSpacePosition);
			float4 worldSpaceNormal = mul(unity_ObjectToWorld, float4(objectSpaceNormal, 0));

			fixed3 worldOffset = _Position.xyz - _PrevPosition.xyz;
			fixed3 localOffset = worldSpacePosition.xyz - _Position.xyz;

			// World offset should only be behind swing
			float dirDot = dot(normalize(worldOffset), normalize(localOffset));
			fixed3 unitVec = fixed3(1, 1, 1) * _SmearNoiseHeight;
			worldOffset = clamp(worldOffset, unitVec * -1, unitVec);
			worldOffset *= -clamp(dirDot, -1, 0) * lerp(1, 0, step(length(worldOffset), 0));

			fixed3 smearOffset = -worldOffset.xyz * lerp(1, cnoise(worldSpacePosition.xyz * _SmearNoiseScale), step(0, _SmearNoiseScale));
			worldSpacePosition.xyz += smearOffset;
			//--

			//Wind-
			if (_WindEnabled == 0 || _WaveAmount == 0 || _WaveDir.x == 0 && _WaveDir.y == 0 && _WaveDir.z == 0) {
			}
			else {
				//Chris's wind function modified to take in to account world height and more directional wind
				//.......................

				float noiseOffset = cnoise(worldSpacePosition.xyz) * _WaveNoise;
				//clamped to prevent extreme results at higher branches, needs tweaking, it would be nice if lower plants shook more in here
				float heightSensitivity = clamp(worldSpacePosition.y * worldSpacePosition.y, 0, _Sensitivity) / _Sensitivity;
				//oscillation value adds on to the direction of the wind, it's length is measured with _WaveScale
				float4 oscillation = sin(_WaveTime + noiseOffset  * heightSensitivity) * _WaveScale * normalize(_WaveDir) * heightSensitivity; // * v.color.r
				//wave direction and oscillation combined are then scaled overall by the _WaveAmount
				float4 wind = (normalize(_WaveDir) + oscillation) * _WaveAmount * heightSensitivity; //* v.color.r

				worldSpacePosition += wind;
				//''''''''''''''''''''''
			}
			//--

			//conewars melt-
			// these could cause problems in the future with differences in elevation
			float melt = ( worldSpacePosition.y - _MeltY ) / _MeltDistance;
			melt = 1 - saturate(melt);
			melt = pow(melt, _MeltCurve) * _MeltAmount;
			worldSpacePosition.xz += worldSpaceNormal.xz * melt;
			//--

			return mul(unity_WorldToObject, worldSpacePosition);
		}

		void vert(inout appdata_tan v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input, o);
			
			//spherification
			float3 directionToCenter = normalize(_SphereCenter.xyz + v.vertex.xyz);
			float3 pointOnSphere = directionToCenter * _SphereScale;
			v.vertex.xyz = lerp(v.vertex.xyz, pointOnSphere, _Spherification);
			v.normal = lerp(v.normal, directionToCenter, _Spherification);

			//calculate normals and apply deformation
			//...............
			float4 vertPosition = getNewVertPosition(v.vertex, v.normal);
			float4 bitangent = float4(cross(v.normal, v.tangent), 0);

			float vertOffset = 0.01;

			float4 v1 = getNewVertPosition(v.vertex + v.tangent * vertOffset, v.normal);
			float4 v2 = getNewVertPosition(v.vertex + bitangent * vertOffset, v.normal);

			float4 newTangent = v1 - vertPosition;
			float4 newBitangent = v2 - vertPosition;
			v.normal = cross(newTangent, newBitangent);
			v.vertex = vertPosition; 
			//..............
			
			o.pos = v.vertex.xyz;
			//o.worldPos.xyz = mul(unity_ObjectToWorld, v.vertex.xyz);

			//https://forum.unity3d.com/threads/refraction-example.78750/
			//refraction distorting uvs
			float4 oPos = UnityObjectToClipPos(v.vertex);
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
			float4 worldPos = mul(unity_ObjectToWorld, float4(IN.pos,0));

			float4 mainTex = tex2D(_MainTex, IN.uv_MainTex);

			//grad uv
			float gradUV = IN.pos.y*.5 + .5;

			//refraction from above url
			half3 nor = cnoise((o.Normal) + _Time.xyz);
			float2 offset = nor  * _DistAmt * _GrabTexture_TexelSize.xy;
			IN.proj.xy = offset * IN.proj.z + IN.proj.xy; 
			float4 projCoords = UNITY_PROJ_COORD(IN.proj);
			//half4 col = tex2Dproj(_GrabTexture, projCoords); replaced by new blur

			//https://gist.github.com/mandarinx/c7e8f555a48b9f38e852
			half4 sum = half4(0, 0, 0, 0);
			_BlurSize *= mainTex.r;

			#define GRABPIXEL(weight,kernelx) tex2Dproj( _GrabTexture, UNITY_PROJ_COORD(float4(IN.proj.x + _GrabTexture_TexelSize.x * kernelx * _BlurSize, IN.proj.y, IN.proj.z, IN.proj.w))) * weight

			sum += GRABPIXEL(0.05, -4.0);
			sum += GRABPIXEL(0.09, -3.0);
			sum += GRABPIXEL(0.12, -2.0);
			sum += GRABPIXEL(0.15, -1.0);
			sum += GRABPIXEL(0.18, 0.0);
			sum += GRABPIXEL(0.15, +1.0);
			sum += GRABPIXEL(0.12, +2.0);
			sum += GRABPIXEL(0.09, +3.0);
			sum += GRABPIXEL(0.05, +4.0);

			half4 col = sum;
			
			//rim lighting
			half rim = 1 - saturate(dot(IN.viewDir, normalize(o.Normal)));
			float4 rimInfluence = _RimColor*pow(rim, _RimPower);

			//murray's gradient
			fixed4 gradient = lerp(_ColorBot, _ColorMid, gradUV / _Middle) * step(gradUV, _Middle);
			gradient += lerp(_ColorMid, _ColorTop, (gradUV - _Middle) / (1 - _Middle)) * step(_Middle, gradUV);

			//mix gradient with mouth color
			gradient = lerp(_ColorBot, gradient, mainTex.r);

			//mix em'
			fixed4 finalCol = lerp(gradient, col * gradient, _ColorFog);
			finalCol = finalCol + rimInfluence * clamp(worldPos.y, 0, .1) * 10;

			//put it all together and make the rim lighting ramp up from the ground
			o.Albedo = finalCol;
			o.Alpha = finalCol.a;
		}
		ENDCG
	}

		FallBack "Diffuse"
}