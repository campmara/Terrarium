Shader "Custom/TransparentColor" 
{
	Properties 
	{
		_Color ("Color", Color) = (1,1,1,1)
		_Color2("Color", Color) = (1,1,1,1)

		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Cutoff ("Alpha Cutoff", Range(0, 1)) = 1

		[MaterialToggle] _ToggleBillboard("Toggle Billboard Effect", Float) = 0
	}
	SubShader 
	{
		//if performance is bad it's probably because of  "DisableBatching"="True" !!! 
		Tags { "RenderType"="Transparent" "Queue"="AlphaTest" "DisableBatching"="True" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Lambert vertex:vert alphatest:_Cutoff

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

		struct Input 
		{
			float2 uv_MainTex;
		};

		fixed4 _Color;
		fixed4 _Color2;
		float _ToggleBillboard;

		void vert (inout appdata_full v, out Input o)
		{
			
			UNITY_INITIALIZE_OUTPUT(Input, o);

			if (_ToggleBillboard == 1) {
				//code via https://gist.github.com/renaudbedard/7a90ec4a5a7359712202

				// get the camera basis vectors
				float3 forward = -normalize(UNITY_MATRIX_V._m20_m21_m22);
				//float3 up = float3(0, 1, 0); //normalize(UNITY_MATRIX_V._m10_m11_m12); //rotate on all axises 
				float3 up = UNITY_MATRIX_IT_MV[1].xyz;
				float3 right = normalize(UNITY_MATRIX_V._m00_m01_m02);

				// rotate to face camera
				float4x4 rotationMatrix = float4x4(right, 0,
					up, 0,
					forward, 0,
					0, 0, 0, 1);

				//float offset = _Object2World._m22 / 2;
				float offset = 0;
				v.vertex = mul(v.vertex + float4(0, offset, 0, 0), rotationMatrix) + float4(0, -offset, 0, 0);
			}

			fixed4 worldPos = mul(transpose(unity_ObjectToWorld), float4(0, 1, 0, 1));
			v.normal = worldPos;
		}

		void surf (Input IN, inout SurfaceOutput o) 
		{
			// Albedo comes from a texture tinted by color
			//fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
			c.rgb = lerp(_Color, _Color2, c.r);
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Transparent/Cutout/Diffuse"
}
