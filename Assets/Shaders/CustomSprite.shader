Shader "Custom/ShaderSpriteSheet" {
	Properties {
		_Color ("Color 1", Color) = (1,1,1,1)
		_Color2 ("Color 2", Color) = (1,1,1,1)

		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Columns("Columns", int) = 8
		_Rows("Rows", int) = 3
		_FrameNumber ("Frame Number", int) = 0
		_TotalFrames ("Total Number of Frames", int) = 1
		_Cutoff ("Alpha Cutoff", Range(0,1)) = 1
		[MaterialToggle] _ToggleBillboard("Toggle Billboard Effect", Float) = 0
	}
	SubShader {
		Tags { "RenderType"="Opaque" "DisableBatching"="True"}
		LOD 200
		
		Cull Off

		CGPROGRAM
		#pragma surface surf Lambert fullforwardshadows alphatest:_Cutoff vertex:vert addshadow
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
		};

		half _Glossiness;
		half _Metallic;
		int _Columns;
		int _Rows;
		fixed4 _Color;
		fixed4 _Color2;
		int _FrameNumber;
		int _TotalFrames;
		float _ToggleBillboard;

		void vert(inout appdata_base v)
		{
			//UNITY_INITIALIZE_OUTPUT(Input, o);

			if (_ToggleBillboard == 1) {
				//code via https://gist.github.com/renaudbedard/7a90ec4a5a7359712202

				// get the camera basis vectors
				float3 forward = -normalize(UNITY_MATRIX_V._m20_m21_m22);
				float3 up = float3(0, 1, 0); //normalize(UNITY_MATRIX_V._m10_m11_m12); //rotate on all axises 
				//float3 up = UNITY_MATRIX_IT_MV[1].xyz;
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

		void surf (Input IN, inout SurfaceOutput o) {
			float frame = clamp(_FrameNumber, 0, _TotalFrames);

			float xOffPerFrame = (1 / (float)_Columns);
			float yOffPerFrame = (1 / (float)_Rows);

			float2 spriteSize = IN.uv_MainTex;
			spriteSize.x = (spriteSize.x / _Columns);
			spriteSize.y = (spriteSize.y / _Rows);

			float2 currentSprite = float2(0,  1 - yOffPerFrame);
			currentSprite.x += frame * xOffPerFrame;
			
			float rowIndex;
			float mod = modf(frame / (float)_Columns, rowIndex);
			currentSprite.y -= rowIndex * yOffPerFrame;
			currentSprite.x -= rowIndex * _Columns * xOffPerFrame;

			fixed4 c = tex2D (_MainTex, spriteSize + currentSprite) * _Color;
			c.rgb = lerp(_Color, _Color2, c.r);

			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Transparent/Cutout/Diffuse"
}
