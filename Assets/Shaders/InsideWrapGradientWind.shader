Shader "InsideWrap/Hardness (Half Lambert) with Gradient and Wind"
{
	Properties{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_SecondaryTex("Secondary (RGB)", 2D) = "white" {}
		_NormalTex("Normal Map", 2D) = "bump" {}

		_ColorTop("Top Color", Color) = (1,1,1,1)
		_ColorMid("Mid Color", Color) = (1,1,1,1)
		_ColorBot("Bot Color", Color) = (1,1,1,1)
		_Middle("Middle", Range(0.001, 0.999)) = 0.5

		_Hardness("Hardness", Range(.25, 1)) = 0.5

		_Sensitivity("Height Sensitivity", Range(.000001, 1000)) = 15

		//these values are made uniform lower in the shader
		//I kept them in for debugging later on.
			//_WaveDir("Wind Direction", Vector) = (0,0,0,0)
			//_WaveSpeed("Wind Speed", float) = 0
			//_WaveNoise("Wind Noisiness", float) = 0
			//_WaveScale("Wind Scale", float) = 0 
			//_WaveAmount("Wind Amount", float) = 0

	}
		SubShader{
		Tags{ "Queue" = "Geometry" }
		//Cull Off
		CGPROGRAM
		
	#pragma surface surf WrapLambert vertex:vert addshadow
	#include "Noise.cginc"

	half _Hardness;
	half4 _ShadowColor;

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
		float2 uv_SecondaryTex;
		float2 uv_NormalTex;
		float4 color : COLOR;
	};

	sampler2D _MainTex;
	sampler2D _SecondaryTex;
	sampler2D _NormalTex;

	fixed4 _ColorTop;
	fixed4 _ColorMid;
	fixed4 _ColorBot;
	float  _Middle;

	//wind
	float _Sensitivity;

	//these values are uniform so that they'll work globally
	uniform fixed4 _WaveDir;
	uniform float _WaveSpeed;
	uniform float _WaveNoise;
	uniform float _WaveAmount;
	uniform float _WaveScale;

	void vert(inout appdata_full v) {

		//this effect causes the material to disappear if the amount is 0, this won't run the code if that's the case
		if (_WaveAmount == 0 || _WaveDir.x == 0 && _WaveDir.y == 0 && _WaveDir.z == 0) { return; }
		
		//put it in world space
		float4 worldPos = mul(unity_ObjectToWorld, v.vertex);

		//Chris's wind function modified to take in to account world height and more directional wind
		//.......................

		float noiseOffset = cnoise(worldPos.xyz) * _WaveNoise;
		//clamped to prevent extreme results at higher branches, needs tweaking, it would be nice if lower plants shook more in here
		float heightSensitivity = clamp(worldPos.y * worldPos.y, 0, _Sensitivity) / _Sensitivity;
		//oscillation value adds on to the direction of the wind, it's length is measured with _WaveScale
		float4 oscillation = sin(_Time.y * _WaveSpeed + noiseOffset) * _WaveScale * normalize(_WaveDir) * heightSensitivity; // * v.color.r
		//wave direction and oscillation combined are then scaled overall by the _WaveAmount
		float4 wind = (normalize(_WaveDir) + oscillation) * _WaveAmount * heightSensitivity; //* v.color.r

		worldPos += wind;
		//''''''''''''''''''''''

		//take it back to object space
		float4 objectSpaceVertex = mul(unity_WorldToObject, worldPos);
		v.vertex = objectSpaceVertex;

		v.color.r = wind;
		v.color.g = wind;
		v.color.b = wind;
	}

	void surf(Input IN, inout SurfaceOutput o) {

		fixed4 gradient = lerp(_ColorBot, _ColorMid, IN.uv_MainTex.y / _Middle) * step(IN.uv_MainTex.y, _Middle);
		gradient += lerp(_ColorMid, _ColorTop, (IN.uv_MainTex.y - _Middle) / (1 - _Middle)) * step(_Middle, IN.uv_MainTex.y);

		//o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb * tex2D(_SecondaryTex, IN.uv_SecondaryTex).rgb * gradient;
		o.Albedo = IN.color.r;
		o.Normal = UnpackNormal(tex2D(_NormalTex, IN.uv_NormalTex));
	}

	ENDCG
	}
		FallBack "Standard"
}
