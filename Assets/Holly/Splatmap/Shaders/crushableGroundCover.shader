Shader "Custom/GroundCover - Crushable"
{
	Properties
	{
		//_Color("Color", Color) = (1,1,1,1)
		//_Color2("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_Hardness("Hardness", Range(0, 1)) = 1
		_Cutoff("Alpha Cutoff", Range(0, 1)) = 1
		_SplatTex("Splat Texture (RGBA)", 2D) = "white" {}
		_ImprintAmount("Imprint Amount", Range(0, 10)) = 1
		_WarpAmount("WarpAmount", Range(0, 10)) = 1
		_Pivot("Pivot", Vector) = (0,0,0,0)
		[Toggle]_BendFromPivot("Bend from Pivot", int) = 0
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
		float2 worldUV;
	};

	//fixed4 _Color;
	//fixed4 _Color2;
	
	//global variable for ground color
	uniform float4 _GroundColorPrimary;
	uniform float4 _GroundColorSecondary;
	
	float _WarpAmount;
	float _ImprintAmount;

	//...splatmap...
	uniform sampler2D _ClipEdges;
	uniform sampler2D _SplatMap;
	uniform float3 _CameraWorldPos;
	uniform float4 _SplatmapNeutralColor;
	uniform float _OrthoCameraScale;
	//..............

	//splatmap+
	sampler2D _SplatTex;

	//Pivot bending
	int _BendFromPivot;
	float4 _Pivot;

	void vert(inout appdata_full v, out Input o)
	{
		UNITY_INITIALIZE_OUTPUT(Input, o);


		fixed4 worldUp = mul(transpose(unity_ObjectToWorld), float4(0, 1, 0, 1));
		v.normal = worldUp;

		float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
		float4 worldNormal = mul(unity_ObjectToWorld, float4(v.normal, 0));
		
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

		if (_BendFromPivot != 0) {
			float distToPivot = v.vertex.y - _Pivot.y;
			d *= distToPivot;
		}

		//for color normal maps
		worldPos.x += d.r * _WarpAmount;
		worldPos.y -= (d.b) * _ImprintAmount;
		worldPos.z -= d.g * _WarpAmount;

		worldNormal.xz += d.rg;

		o.worldUV = worldUV;
		v.vertex = mul(unity_WorldToObject, worldPos);
		v.normal *= mul(unity_WorldToObject, worldNormal.xyz);
	}

	void surf(Input IN, inout SurfaceOutput o)
	{
		fixed4 border = tex2D(_ClipEdges, IN.worldUV);
		fixed4 ao = tex2D(_SplatMap, IN.worldUV);
		ao = clamp(ao.rgba - border.a, 0, 1);

		fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
		c.rgb = lerp(_GroundColorPrimary, _GroundColorSecondary, c.r);
		ao.b *= 1 - ao.a; 
		c.rgb -= ao.b * .1;
		//c.rgb = ao.b;
		//o.Albedo = clamp(c.rgb - border.a, 0, 1);
		o.Albedo = c.rgb;
		o.Alpha = c.a;
	}
	ENDCG
	}
		FallBack "Transparent/Cutout/Diffuse"
}
