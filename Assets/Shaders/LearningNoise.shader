Shader "Custom/Learning Noise" {
	Properties{
		_Tess("Tessellation", Range(1,32)) = 4
		_Displacement("Displacement", Range(0, 5.0)) = 0.3
		_Color("Color", color) = (1,1,1,0)
		_Scale("Noise Scale", vector) = (1,1,0,0)
		_Offset("Offset", vector) = (0,0,0,0)
		_Iterations("Iterations", int) = 2
	}
		SubShader{
		Tags{ "RenderType" = "Opaque" }
		LOD 300

		CGPROGRAM
#pragma surface surf BlinnPhong addshadow fullforwardshadows vertex:disp tessellate:tessFixed nolightmap
#pragma target 4.6
#include "Noise.cginc"

	struct appdata {
		float4 vertex : POSITION;
		float4 color : COLOR;
		float4 tangent : TANGENT;
		float3 normal : NORMAL;
		float2 texcoord : TEXCOORD0;
	};

	float _Tess;

	float4 tessFixed()
	{
		return _Tess;
	}

	sampler2D _DispTex;
	float _Displacement;
	float4 _Scale;
	float4 _Offset;
	int _Iterations;

	struct Input {
		float2 uv_MainTex;
		float4 color : COLOR;
	};

	//http://www.iquilezles.org/www/articles/morenoise/morenoise.htm
	float4 fbm(float3 x, int octaves)
	{
		float f = 1.98;  // could be 2.0
		float s = 0.49;  // could be 0.5
		float a = 0.0;
		float b = 0.5;
		float3 d = float3(0, 0, 0);
		float3x3   m = float3x3(1.0, 0.0, 0.0,
			0.0, 1.0, 0.0,
			0.0, 0.0, 1.0);
		for (int i = 0; i < octaves; i++)
		{
			float3 n = cnoise(x);
			a += b*n.x;          // accumulate values		
			d += b*mul(m,n.xyz);      // accumulate derivatives
			b *= s;
			x = f*mul(m,x);
			m = f*m*m;
		}
		return float4(a, d);
	}

	void disp(inout appdata v)
	{
		float3 uv = float3((v.texcoord.x * _Scale.x) + _Offset.x, (v.texcoord.y * _Scale.y) + _Offset.y, 0);
		float d = 1 -abs(fbm(uv, _Iterations)) * _Displacement;//1 - abs(cnoise(float3(v.texcoord.xy * _Scale, 0))) * _Displacement;
		v.vertex.xyz += v.normal * d;
		v.color = d;
	}

	fixed4 _Color;

	void surf(Input IN, inout SurfaceOutput o) {
		o.Albedo = IN.color.rgb * _Color;
		o.Specular = 0.2;
		o.Gloss = 1.0;
	}
	ENDCG
	}
		FallBack "Diffuse"
}