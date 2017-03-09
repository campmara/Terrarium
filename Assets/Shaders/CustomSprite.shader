Shader "Custom/ShaderSpriteSheet" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Columns("Columns", int) = 8
		_Rows("Rows", int) = 3
		_FrameNumber ("Frame Number", int) = 0
		_TotalFrames ("Total Number of Frames", int) = 1
		_Cutoff ("Alpha Cutoff", Range(0,1)) = 1
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		//Cull Off

		CGPROGRAM
		#pragma surface surf Lambert fullforwardshadows alphatest:_Cutoff
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
		int _FrameNumber;
		int _TotalFrames;

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
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
