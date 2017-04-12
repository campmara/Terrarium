Shader "Custom/LocalCutoff" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Scale("Texture Scale", float) = 1

		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
		_Cutoff("Alpha Cutoff", Range(0,1)) = 1
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		cull off

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows vertex:vert alphatest:_Cutoff
		
		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0
#include "noise.cginc"

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
			float3 localPos;
			float3 localNormal;
			//float3 worldNormal;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		half _Scale;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_CBUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_CBUFFER_END
		
		void vert(inout appdata_full v, out Input o){
			UNITY_INITIALIZE_OUTPUT(Input, o);
			o.localPos = v.vertex;
			o.localNormal = v.normal;
		}
		
		void surf (Input IN, inout SurfaceOutputStandard o) {

			//...
			//https://github.com/keijiro/TriplanarPBS/blob/master/Assets/TriplanarPBS/Shaders/TriplanarPBS.shader#L69
			// Calculate a blend factor for triplanar mapping.
			float3 bf = normalize(abs(IN.localNormal));
			bf /= dot(bf, (float3)1);

			// Get texture coordinates.
			float2 tx = IN.localPos.yz * _Scale;
			float2 ty = IN.localPos.zx * _Scale;
			float2 tz = IN.localPos.xy * _Scale;

			// Base color
			half4 cx = tex2D(_MainTex, tx) * bf.x;
			half4 cy = tex2D(_MainTex, ty) * bf.y;
			half4 cz = tex2D(_MainTex, tz) * bf.z;
			half4 color = (cx + cy + cz) * _Color;
			//...

			o.Albedo = color.rgb;
			o.Alpha = color.r;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
