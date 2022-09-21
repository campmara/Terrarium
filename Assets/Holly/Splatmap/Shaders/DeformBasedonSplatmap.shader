// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/DeformBasedonSplatmap" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Displacement("Displacement", float) = 1
		_Inset("Inset Amount", float) = 1
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
		float _NormalMagnitude; // for testing not essential
		//..............

		struct Input {
			float2 uv_MainTex;
			float2 worldUV;
		};


		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

		float _Displacement;
		float _Inset;
		/*
		void vert(inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);

			_NormalMagnitude = 1; // for testing

			_OrthoCameraScale *= 2;
			float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
			float4 worldNormal = mul(unity_ObjectToWorld, float4(v.normal,0));
			
			float2 worldUV = float2(((worldPos.x - _CameraWorldPos.x) / _OrthoCameraScale + .5f), ((worldPos.z - _CameraWorldPos.z) / _OrthoCameraScale + .5f)); //find a way to center this
			
			float4 border = tex2Dlod(_ClipEdges, float4(worldUV, 0, 0)).rgba;
			float4 d = (tex2Dlod(_SplatMap, float4(worldUV, 0, 0)));
			

			d.rgb = (d.rgb - .5f) * _NormalMagnitude;// (d - .5f) * _Displacement;

			d.a = clamp(d.a - border.a, 0, 1);
			d.rgb *= d.a;

			//https://forum.unity3d.com/threads/normal-maps-and-importing-them-correctly.82652/#post-629189
			//get the dot product of r and g for the value b (or ra and ga)
			//d.b = dot(d.r, d.g);
			
			
			//for color normal maps
			worldPos.x -= d.r;
			worldPos.y -= (d.b);
			worldPos.z += d.g;

			worldNormal.xz += d.rg;
			
			worldNormal.x = worldNormal.x - d.r;
			worldNormal.z = worldNormal.z + d.g;
			

			
			//for optimized normal maps
			//float3 normal = UnpackNormal(d);
			//d.a = d.a - (.5f * d.a);
			//d.b = d.b - (.5f * d.b);
			//float height = dot(d.b, d.a) * 5;

			worldPos.x += normal.x;
			worldPos.z -= normal.y;
			//worldPos.y += normal.z;
			
			o.worldUV = worldUV;
			v.vertex = mul(unity_WorldToObject, worldPos);
			v.normal = mul(unity_WorldToObject, worldNormal.xyz);
		}
		*/
		void vert(inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);

			_OrthoCameraScale *= 2;
			float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
			float4 worldNormal = mul(unity_ObjectToWorld, float4(v.normal, 0));

			float2 worldUV = float2(((worldPos.x - _CameraWorldPos.x) / _OrthoCameraScale + .5f), ((worldPos.z - _CameraWorldPos.z) / _OrthoCameraScale + .5f)); //find a way to center this

			float4 border = tex2Dlod(_ClipEdges, float4(worldUV, 0, 0)).rgba;
			float4 d = (tex2Dlod(_SplatMap, float4(worldUV, 0, 0)));
			
			d.a = clamp(d.a - border.a, 0, 1);
			d.rgb = (d.rgb - .5f) * d.a;

			//for color normal maps
			worldPos.x -= d.r;
			worldPos.y -= (d.b);
			worldPos.z += d.g;

			worldNormal.xz += d.rg;

			o.worldUV = worldUV;
			v.vertex = mul(unity_WorldToObject, worldPos);
			v.normal = mul(unity_WorldToObject, worldNormal.xyz);
		}


		void surf (Input IN, inout SurfaceOutputStandard o) {
			fixed4 border = tex2D(_ClipEdges, IN.worldUV);
			fixed4 splat = tex2D (_SplatMap, IN.worldUV);
			splat.a = clamp(splat.a - border.a, 0, 1);
			o.Albedo = _Color.rgb;// * (1 - splat.a);
			//o.Albedo = lerp(_Color.rgb, splat.rgb, splat.a);
			//o.Albedo = _Color.rgb;
			//o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
