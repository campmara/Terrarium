// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/DeformBasedonSplatmap" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Displacement("Displacement", float) = 1
		_uvScale("UV Scale", float) = 1
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows vertex:vert addshadow

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;

		//this is potentially a good way to project shadows???
		//also storing normals in here probably wont be awful.
		//just use black values to represent height and colors for normals
		//...splatmap...
		uniform sampler2D _ClipEdges;
		uniform sampler2D _SplatMap;
		uniform float3 _CameraWorldPos;
		uniform float _OrthoCameraScale;
		//..............

		struct Input {
			float2 uv_MainTex;
			float2 worldUV;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

		float _Displacement;

		void vert(inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);

			_OrthoCameraScale *= 2;
			float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
			
			float2 worldUV = float2(((worldPos.x - _CameraWorldPos.x) / _OrthoCameraScale + .5f), ((worldPos.z - _CameraWorldPos.z) / _OrthoCameraScale + .5f)); //find a way to center this
			
			float4 border = tex2Dlod(_ClipEdges, float4(worldUV, 0, 0)).rgba;
			float4 d = (clamp((tex2Dlod(_SplatMap, float4(worldUV, 0, 0)) - border.a),0,1)) * _Displacement;

			worldPos.y -= d;
			v.vertex = mul(unity_WorldToObject, worldPos);
		}

		void surf (Input IN, inout SurfaceOutputStandard o) {
			fixed4 border = tex2D(_ClipEdges, IN.worldUV);
			fixed4 c = _Color;//fixed4 c = tex2D (_SplatMap, IN.worldUV);
			o.Albedo = c;//clamp(c.rgb - border.a, 0, 1);
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
