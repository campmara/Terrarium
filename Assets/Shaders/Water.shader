Shader "Custom/Water"
{
	Properties
	{
		_ColorTop("Top Color", Color) = (1,1,1,1)
		_ColorMid("Mid Color", Color) = (1,1,1,1)
		_ColorBot("Bot Color", Color) = (1,1,1,1)
		_Middle("Middle", Range(0.001, 0.999)) = 0.5
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		
		//distortion
		_DistAmt("Distortion", range(0,128)) = 13
		_ColorFog("Fogginess", range(0, 1)) = 0.78

		// depth
		_DepthClip("Depth Clip", Range(0, 1)) = 0.1
	}

	SubShader
	{
		GrabPass{}

		Tags{ "Queue" = "Geometry+5" "IgnoreProjector" = "True" "RenderType" = "Opaque" }
		LOD 200

		CGPROGRAM
		#pragma surface surf NoLighting vertex:vert addshadow
		#pragma target 3.0

		#include "UnityCG.cginc"
		#include "Noise.cginc"

		sampler2D _MainTex;
		sampler2D _CameraDepthTexture;

		fixed4 _ColorTop;
		fixed4 _ColorMid;
		fixed4 _ColorBot;
		float _Middle;

		//distortion
		float _DistAmt;
		float _ColorFog;
		sampler2D _GrabTexture;
		float4 _GrabTexture_TexelSize;

		// depth
		float _DepthClip;

		struct appdata 
		{
            float4 vertex : POSITION;
            float4 texcoord : TEXCOORD0;
            float3 normal : NORMAL;
        };

		struct Input
		{
			float2 uv_MainTex;
			float3 viewDir;
			float3 worldPos;
			float4 proj : TEXCOORD;
			float2 depth : TEXCOORD0;
			float4 grabPos : TEXCOORD1;
		};

		fixed4 LightingNoLighting(SurfaceOutput s, fixed3 lightDir, fixed atten)
		{
			fixed4 c;
			c.rgb = s.Albedo;
			c.a = s.Alpha;
			return c;
		}

		void vert(inout appdata v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);

			//https://forum.unity3d.com/threads/refraction-example.78750/
			//refraction distorting uvs
			float4 oPos = mul(UNITY_MATRIX_MVP, v.vertex);
			#if UNITY_UV_STARTS_AT_TOP
				float scale = -1.0;
			#else
				float scale = 1.0;
			#endif
			o.proj.xy = (float2(oPos.x, oPos.y*scale) + oPos.w) * 0.5;
			o.proj.zw = oPos.zw;

			o.grabPos = ComputeGrabScreenPos(v.vertex);
		}

		void surf(Input IN, inout SurfaceOutput o)
		{
			//refraction from above url
			//half3 nor = cnoise((o.Normal) + _Time);
			//float2 offset = nor  * _DistAmt * _GrabTexture_TexelSize.xy;
			//IN.proj.xy = offset * IN.proj.z + IN.proj.xy; 
			IN.proj = lerp(IN.proj, cnoise(IN.worldPos + _Time), 0.1);
			half4 col = tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(IN.proj));
			
			// Albedo comes from a texture tinted by color
			fixed4 gradient = lerp(_ColorBot, _ColorMid, IN.uv_MainTex.y / _Middle) * step(IN.uv_MainTex.y, _Middle);
			gradient += lerp(_ColorMid, _ColorTop, (IN.uv_MainTex.y - _Middle) / (1 - _Middle)) * step(_Middle, IN.uv_MainTex.y);

			//half depth = saturate((LinearEyeDepth(tex2D(_CameraDepthTexture, IN.screenPos.xy / IN.screenPos.w).r) - IN.screenPos.z));

			/*
			float depth = Linear01Depth(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(IN.grabPos)).r);
			if (depth < _DepthClip)
			{
				gradient = fixed4(1, 1, 1, 1);
				col = half4(1, 1, 1, 1);
			}
			*/

			fixed4 finalCol = lerp(gradient, col * gradient, _ColorFog);
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * finalCol;
			o.Albedo = c.rgb;

			o.Alpha = c.a;
		}
		ENDCG
	}

		FallBack "Diffuse"
}