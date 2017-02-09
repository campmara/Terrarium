Shader "Custom/Water" 
{
	Properties 
	{
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}

		//distortion
		//_DistAmt("Distortion", range(0,128)) = 13
		//_ColorFog("Fogginess", range(0, 1)) = 0.78
	}
	SubShader 
	{
		GrabPass{}

		Tags{ "IgnoreProjector" = "True" "RenderType" = "Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf NoLighting vertex:vert addshadow
		#pragma target 3.0

		#include "Noise.cginc"

		sampler2D _MainTex;

		struct Input 
		{
			float2 uv_MainTex;
			//float3 viewDir;
			//float4 proj : TEXCOORD;
		};

		fixed4 _Color;

		/*
		//distortion
		float _DistAmt;
		float _ColorFog;
		sampler2D _GrabTexture;
		float4 _GrabTexture_TexelSize;
		*/

		fixed4 LightingNoLighting(SurfaceOutput s, fixed3 lightDir, fixed atten)
		{
			fixed4 c;
			c.rgb = s.Albedo + s.Emission;
			c.a = s.Alpha;
			return c;
		}

		void vert(inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);

			/*
			//https://forum.unity3d.com/threads/refraction-example.78750/
			float4 oPos = mul(UNITY_MATRIX_MVP, v.vertex);
			#if UNITY_UV_STARTS_AT_TOP
				float scale = -1.0;
			#else
				float scale = 1.0;
			#endif
			o.proj.xy = (float2(oPos.x, oPos.y*scale) + oPos.w) * 0.5;
			o.proj.zw = oPos.zw;
			*/
		}

		void surf (Input IN, inout SurfaceOutput o) 
		{
			/*
			//refraction from above url
			half3 nor = cnoise((o.Normal) + _Time);
			float2 offset = nor  * _DistAmt * _GrabTexture_TexelSize.xy;
			IN.proj.xy = offset * IN.proj.z + IN.proj.xy; 
			half4 col = tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(IN.proj));
			*/
			
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;

			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
