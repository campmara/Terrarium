Shader "Skybox/Gradient Sky in a Foggy Hole"
{
	Properties
	{
		_Color1("Color 1", Color) = (1, 1, 1, 0)
		_Color2("Color 2", Color) = (1, 1, 1, 0)
		_UpVector("Up Vector", Vector) = (0, 1, 0, 0)
		_Intensity("Intensity", Float) = 1.0
		_Exponent("Exponent", Float) = 1.0
		// The properties below are used in the custom inspector.
		_UpVectorPitch("Up Vector Pitch", float) = 0
		_UpVectorYaw("Up Vector Yaw", float) = 0
		_MainTex("MainTex", 2D) = "white" {}
		_WhispTexture("Whisp Texture", 2D) = "white" {}
		_FogLevel("Fog Level", Range(0,2)) = 0.5
		_FogBlend("Fog Blend", Range(0,100)) = 0.5
		_FogLightTransition("Fog Light Transition", Range(0, 3)) = 1
		//_FogColor("Fog Color", Color) = (1, 1, 1, 0)
	}

		CGINCLUDE

		#include "UnityCG.cginc"
		#include "Lighting.cginc"
		#include "Noise.cginc"

		struct appdata
	{
		float4 position : POSITION;
		float3 texcoord : TEXCOORD0;
	};

	struct v2f
	{
		float4 position : SV_POSITION;
		float3 texcoord : TEXCOORD0;
	};

	half4 _Color1;
	half4 _Color2;
	half4 _UpVector;
	half _Intensity;
	half _Exponent;

	half _FogLevel;
	half _FogBlend;

	sampler2D _WhispTexture;
	sampler2D _MainTex;
	float _FogLightTransition;

	v2f vert(appdata v)
	{
		v2f o;
		o.position = mul(UNITY_MATRIX_MVP, v.position);
		o.texcoord = v.texcoord;
		return o;
	}

	fixed4 frag(v2f i) : COLOR
	{
		_UpVector = _WorldSpaceLightPos0 * 2;
		float noiseHeight = .05;
		float noiseScale = 5;
		float time = _Time * 5;
		float3 uv = i.texcoord + _UpVector;

		float lineNoise = sin((uv.x*noiseScale) + time)*cos((uv.z*noiseScale) + time)*noiseHeight;
		float4 whispTex = tex2D(_WhispTexture, uv.xz) / 45;
		lineNoise -= whispTex;
		half d = dot(normalize(i.texcoord), 
			_UpVector)
			* 0.5f + 0.5f;
		half noise_d = dot(normalize(i.texcoord),
			_UpVector + lineNoise)
			* 0.5f + 0.5f;


		float melt = (noise_d - _FogLevel + lineNoise) * _FogBlend;
		melt = saturate(melt);
		//fixed4 tex = tex2D(_MainTex, i.texcoord.xyz * 5);

		fixed4 sky = clamp(lerp(_Color1, _Color2, pow(noise_d, _Exponent)) * _Intensity, 0, 1);
		fixed4 fog = lerp(unity_FogColor, _Color1, clamp(d + _FogLevel - _FogLightTransition + whispTex.r, 0, 1));

		float meltSun = (d*d - _FogLevel - .99) * 50;
		fixed4 sun = smoothstep(0, _LightColor0, meltSun);

		return lerp(fog, sky, melt) + sun;
		return sun;
	}

		ENDCG

		SubShader
	{
		Tags{ "RenderType" = "Background" "Queue" = "Background" }
			Pass
		{
			ZWrite Off
			Cull Off
			Fog{ Mode Off }
			CGPROGRAM
#pragma fragmentoption ARB_precision_hint_fastest
#pragma vertex vert
#pragma fragment frag
			ENDCG
		}
	}
	CustomEditor "GradientSkyboxInspector"
}