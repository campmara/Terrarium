// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Water"
{
	Properties
	{
		_Color1("Color 1", Color) = (1,1,1,1)
		_Color2("Color 2", Color) = (1,1,1,1)
		//_MainTex("Albedo (RGB)", 2D) = "white" {}
		_SpecTex("Specular (R)", 2D) = "white" {}

		//distortion
		_DistAmt("Distortion", range(0,128)) = 13
		_ColorFog("Fogginess", range(0, 1)) = 0.78

		// depth
		_DepthClip("Depth Clip", Range(0, 1)) = 0.1

		[Header(Splatmap Values)]
		[Toggle]_SplatmapEnabled("Splatmap Enabled", float) = 0
		_ImprintAmount("Imprint Amount", Range(-3, 3)) = 1
	}

	SubShader
	{
		GrabPass{}

		Tags{ "Queue" = "Geometry-5" "IgnoreProjector" = "True" "RenderType" = "Opaque" }
		LOD 200

		CGPROGRAM
		#pragma surface surf Water vertex:vert addshadow
		#pragma target 3.0

		#include "UnityCG.cginc"
		#include "Noise.cginc"

		sampler2D _MainTex;
		sampler2D _SpecTex;
		sampler2D _CameraDepthTexture;

		fixed4 _Color1;
		fixed4 _Color2;

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
			float4 tangent : TANGENT;
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

		fixed4 LightingWater(SurfaceOutput s, fixed3 lightDir, half3 viewDir, fixed atten)
		{
			fixed4 c;
			half3 h = normalize(lightDir + viewDir);

			half diff = max(0, dot(s.Normal, lightDir));

			float nh = max(0, dot(s.Normal, h));
			float spec = pow(nh, 148.0) * 1;

			c.rgb = s.Albedo;
			c.rgb += spec * s.Specular;
			c.a = s.Alpha;
			return c;
		}

		//...splatmap...
		float _ImprintAmount;
		float _SplatmapEnabled;

		uniform sampler2D _ClipEdges;
		uniform sampler2D _SplatMap;
		uniform float3 _CameraWorldPos;
		uniform float _OrthoCameraScale;
		//..............

		void vert(inout appdata v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);

			float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
			float4 worldNormal = mul(unity_ObjectToWorld, float4(v.normal, 0));

			//https://forum.unity3d.com/threads/refraction-example.78750/
			//refraction distorting uvs
			float4 oPos = UnityObjectToClipPos(v.vertex);
			#if UNITY_UV_STARTS_AT_TOP
				float scale = -1.0;
			#else
				float scale = 1.0;
			#endif
			o.proj.xy = (float2(oPos.x, oPos.y*scale) + oPos.w) * 0.5;
			o.proj.zw = oPos.zw;

			//splatmap deformation
			//...........
			if (_SplatmapEnabled != 0) {
				_OrthoCameraScale *= 2;
				float2 worldUV = float2(((worldPos.x - _CameraWorldPos.x) / _OrthoCameraScale + .5f), ((worldPos.z - _CameraWorldPos.z) / _OrthoCameraScale + .5f)); //find a way to center this
				float4 border = tex2Dlod(_ClipEdges, float4(worldUV, 0, 0)).rgba;
				float4 d = (tex2Dlod(_SplatMap, float4(worldUV, 0, 0)));

				d.a = clamp(d.a - border.a, 0, 1);
				d.rg = (d.rg - .5f) * d.a;
				d.b *= d.a;

				worldPos.x -= d.r;
				worldPos.y -= (d.b) * _ImprintAmount;
				worldPos.z += d.g;

				worldNormal.xz += d.rg;
			}
			//......

			//o.worldUV = worldUV;
			v.vertex = mul(unity_WorldToObject, worldPos);
			v.normal = mul(unity_WorldToObject, worldNormal.xyz);

			o.grabPos = ComputeGrabScreenPos(v.vertex);
		}

		void surf(Input IN, inout SurfaceOutput o)
		{
			//refraction from above url
			IN.proj = lerp(IN.proj, cnoise(IN.worldPos + _Time), 0.1);
			half4 col = tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(IN.proj));
			
			float distToCenter = distance(IN.uv_MainTex, float2(.5, .5));

			// Albedo comes from a texture tinted by color
			fixed4 gradient = lerp(_Color1, _Color2, distToCenter);

			//half depth = saturate((LinearEyeDepth(tex2D(_CameraDepthTexture, IN.proj.xy / IN.proj.w).r) - IN.proj.z));

			float depth = Linear01Depth(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(IN.grabPos)).r);
			/* if (depth < _DepthClip)
			{
				gradient = fixed4(1, 1, 1, 1);
				col = half4(1, 1, 1, 1);
			}
			*/

			float2 warpUV = lerp(IN.uv_MainTex, cnoise(IN.worldPos*10 + _Time), 0.005);

			float spec = step(.5f + sin(_Time * 5)*.05f, 1-tex2D(_SpecTex, warpUV).r).r;
			float4 mainTex = tex2D(_MainTex, warpUV);

			fixed4 finalCol = lerp(gradient, col * gradient, _ColorFog);
			//finalCol = lerp(finalCol, mainTex, mainTex.a);
			fixed4 c = finalCol;
			//c = depth;
			o.Albedo = c.rgb;
			o.Specular = spec;
			o.Alpha = c.a;
		}
		ENDCG
	}

		FallBack "Diffuse"
}