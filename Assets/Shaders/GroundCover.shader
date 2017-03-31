Shader "Custom/GroundCover"
{
	Properties
	{
		//_Color("Color", Color) = (1,1,1,1)
		//_Color2("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_Hardness("Hardness", Range(0, 1)) = 1
		_Cutoff("Alpha Cutoff", Range(0, 1)) = 1
	}
		SubShader
		{
			Tags{ "RenderType" = "Geometry" "Queue" = "AlphaTest" }
			LOD 200

			Cull Off
			//Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM
			// Physically based Standard lighting model, and enable shadows on all light types
	#pragma surface surf WrapLambert vertex:vert alphatest:_Cutoff addshadow

			// Use shader model 3.0 target, to get nicer looking lighting
	#pragma target 3.0

			float _Hardness;	

			half4 LightingWrapLambert(SurfaceOutput s, half3 lightDir, half atten)
		{
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

	sampler2D _MainTex;

	struct Input
	{
		float2 uv_MainTex;
	};

	fixed4 _Color;
	fixed4 _Color2;
	float _ToggleBillboard;

	//global variable for ground color
	uniform float4 _GroundColorPrimary;
	uniform float4 _GroundColorSecondary;

	void vert(inout appdata_full v, out Input o)
	{
		//throw wind in here eventually??
		UNITY_INITIALIZE_OUTPUT(Input, o);
		fixed4 worldPos = mul(transpose(unity_ObjectToWorld), float4(0, 1, 0, 1));
		v.normal = worldPos;
	}

	void surf(Input IN, inout SurfaceOutput o)
	{
		fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
		c.rgb = lerp(_GroundColorPrimary, _GroundColorSecondary, c.r);
		o.Albedo = c.rgb;
		o.Alpha = c.a;
	}
	ENDCG
	}
		FallBack "Transparent/Cutout/Diffuse"
}
