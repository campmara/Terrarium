// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/PlayerMouth" {
	Properties {
		_MainColor("Color Tint", Color) = (1,1,1,1)
		_Color1("Color 1", Color) = (1,1,1,1)
		_Color2("Color 2", Color) = (1,1,1,1)

		_Outline("Outline Thickness", float) = 1
		_OutlineColor("Outline Color", Color) = (1,1,1,1)

		_MainTex("Outside Window (RGB)", 2D) = "white" {}
		_Scale("Simulated Distance", Range(0, 5)) = 0.1
	}
	SubShader {
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
		half _Scale;
		fixed4 _MainColor;
		fixed4 _Color1;
		fixed4 _Color2;

		float _Outline;
		float4 _OutlineColor;

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
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void vert(inout appdata_base v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input, o);
			o.pos = v.vertex;
			o.normal = v.normal;
		}

		void surf (Input IN, inout SurfaceOutput o) {
			//still needs to be fully integrated
			//half4 col = tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(IN.proj));
			
			float3 localPos = IN.pos;
			localPos += _Scale;

			half3 norm = o.Normal;
			half scale = (1 - _Scale);
			half coeff = dot(abs(normalize(IN.viewDir)), norm);
			half3 offset = (IN.viewDir + norm) * scale / 2 * (2 - coeff);
			half2 uv = half2(IN.uv_MainTex.x * _Scale + scale / 2 - offset.x, IN.uv_MainTex.y * _Scale + scale / 2 - offset.y);
			half4 color = lerp(_Color1, _Color2, clamp(pow(coeff, 2),0,1)) * _MainColor;
			o.Albedo = color.rgb;
			if (distance(IN.uv_MainTex, float2(.5,.5)) > _Outline) {
				o.Albedo = _OutlineColor;
			}
			o.Alpha = 1;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
