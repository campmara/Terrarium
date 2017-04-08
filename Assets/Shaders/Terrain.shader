Shader "Custom/Terrain" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _CameraDepthTexture;

		struct Input {
			float2 uv_MainTex;
			float2 depth : TEXCOORD0;
			float4 screenPos;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		sampler3D _DitherMaskLOD;

		half4 LightingSimpleSpecular(SurfaceOutput s, half3 lightDir, half3 viewDir, half atten) {
			half3 h = normalize(lightDir + viewDir);

			half diff = max(0, dot(s.Normal, lightDir));

			float nh = max(0, dot(s.Normal, h));
			float spec = pow(nh, 48.0);
			/*
			float dither = tex3D(_DitherMaskLOD, float3(.2, .2, .2)).a;
			if (diff < dither - .01) {
				spec = 1;
			} */ 

			half4 c;
			c.rgb = (s.Albedo * _LightColor0.rgb * diff + _LightColor0.rgb * spec) * atten;
			c.a = s.Alpha;
			return c;
		}

		void surf (Input IN, inout SurfaceOutputStandard o) {
			half depth = saturate((LinearEyeDepth(tex2D(_CameraDepthTexture, IN.screenPos.xy / IN.screenPos.w).r) - IN.screenPos.z));

			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;

			o.Albedo = depth * c.rgb;
			o.Emission = depth * c.rgb * .5;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
