Shader "Custom/PlayerEyes" {
	Properties{
		_MainTex("Outside Window (RGB)", 2D) = "white" {}
		_MaskTex("Mask (R)", 2D) = "white" {}
		_MainColor("Color Tint", Color) = (1,1,1,1)
		_Color1("Color 1", Color) = (1,1,1,1)
		_Color2("Color 2", Color) = (1,1,1,1)

	_Scale("Simulated Distance", Range(0, 1)) = 0.1
	}
		SubShader{
		GrabPass{}

		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Opaque" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
#pragma surface surf NoLighting fullforwardshadows alpha:auto vertex:vert

		// Use shader model 3.0 target, to get nicer looking lighting
#pragma target 3.0

		fixed4 LightingNoLighting(SurfaceOutput s, fixed3 lightDir, fixed atten)
	{
		fixed4 c;
		c.rgb = s.Albedo;
		c.a = s.Alpha;
		return c;
	}

	sampler2D _MainTex;
	sampler2D _MaskTex;

	half _Scale;
	fixed4 _MainColor;
	fixed4 _Color1;
	fixed4 _Color2;

	sampler2D _GrabTexture;

	struct Input {
		float2 uv_MainTex;
		float2 uv_Detail;
		float4 proj : TEXCOORD; // for refraction
		float3 pos;
		float3 normal;
		half3 viewDir;
	};

	// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
	// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
	// #pragma instancing_options assumeuniformscaling
	UNITY_INSTANCING_CBUFFER_START(Props)
		// put more per-instance properties here
		UNITY_INSTANCING_CBUFFER_END

		void vert(inout appdata_base v, out Input o) {
		UNITY_INITIALIZE_OUTPUT(Input, o);
		v.vertex.z += sin(5.0f * v.vertex.x + _Time.x * 100)*.025;
		o.pos = v.vertex;
		o.normal = v.normal;
	}


	void surf(Input IN, inout SurfaceOutput o) {

		//https://forum.unity3d.com/threads/free-fake-window-shader.394971/
		half scale = (1 - _Scale);
		half coeff = dot(abs(normalize(IN.viewDir)), IN.normal);
		half3 offset = (IN.viewDir + IN.normal) * scale / 2 * (2 - coeff);
		half2 uv = half2(IN.uv_MainTex.x * _Scale + scale / 2 - offset.x, IN.uv_MainTex.y * _Scale + scale / 2 - offset.y);
		half4 color = lerp(_Color1, _Color2, tex2D(_MainTex, uv) * pow(coeff, 2));
		float4 maskTex = tex2D(_MaskTex, IN.uv_MainTex);
		o.Albedo = color.rgb * _MainColor.rgb;
		o.Alpha = maskTex.a;
	}
	ENDCG
	}
		FallBack "Transparent/Cutoff/Diffuse"
}
