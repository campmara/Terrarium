Shader "TerrariumPlant/PlantSpriteSheet" {
	Properties {
		[Header(Color)]
		_ColorTop("Color Top", Color) = (1,1,1,1)
		_ColorBot("Color Bot", Color) = (1,1,1,1)

		[Header(Colorset)]
		_ColorSetSeed("Colorset Seed", float) = 0
		[Toggle]_ColorSetEnabled("Colorset Enabled", float) = 0
		_CurrentColorSet("Current Colorset (int: 0 - 10)", int) = 0

		[Header(Spritesheet)]
		_MainTex ("Spritesheet", 2D) = "white" {}

		_Columns("Columns", int) = 8
		_Rows("Rows", int) = 3

		_FrameNumber ("Frame Number", int) = 0
		_TotalFrames ("Total Number of Frames", int) = 1
		//_FrameScale ("Frame Scale (for testing)", float) = 1
		_Cutoff ("Alpha Cutoff", Range(0,1)) = 1
		[MaterialToggle] _ToggleBillboard("Toggle Billboard Effect", Float) = 0
		[MaterialToggle] _ToggleVertexColorAnim("Toggle Vertex Color Animation", Float) = 0
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
			float4 color : COLOR;
		};

		half _Glossiness;
		half _Metallic;
		int _Columns;
		int _Rows;
		fixed4 _ColorTop;
		fixed4 _ColorBot;
		int _FrameNumber;
		int _TotalFrames;
		float _FrameScale;
		float _ToggleBillboard;
		float _ToggleVertexColorAnim;

		//...colorset coloring...
		float _ColorSetSeed;
		float _ColorSetEnabled;
		int _CurrentColorSet;
		//......

		//...colorsets...

		//0
		uniform float4 _PlantColorSet0_Top1;
		uniform float4 _PlantColorSet0_Top2;
		uniform float4 _PlantColorSet0_Bot1;
		uniform float4 _PlantColorSet0_Bot2;

		//1 
		uniform float4 _PlantColorSet1_Top1;
		uniform float4 _PlantColorSet1_Top2;
		uniform float4 _PlantColorSet1_Bot1;
		uniform float4 _PlantColorSet1_Bot2;

		//2 
		uniform float4 _PlantColorSet2_Top1;
		uniform float4 _PlantColorSet2_Top2;
		uniform float4 _PlantColorSet2_Bot1;
		uniform float4 _PlantColorSet2_Bot2;

		//3
		uniform float4 _PlantColorSet3_Top1;
		uniform float4 _PlantColorSet3_Top2;
		uniform float4 _PlantColorSet3_Bot1;
		uniform float4 _PlantColorSet3_Bot2;

		//4
		uniform float4 _PlantColorSet4_Top1;
		uniform float4 _PlantColorSet4_Top2;
		uniform float4 _PlantColorSet4_Bot1;
		uniform float4 _PlantColorSet4_Bot2;

		//5
		uniform float4 _PlantColorSet5_Top1;
		uniform float4 _PlantColorSet5_Top2;
		uniform float4 _PlantColorSet5_Bot1;
		uniform float4 _PlantColorSet5_Bot2;

		//6
		uniform float4 _PlantColorSet6_Top1;
		uniform float4 _PlantColorSet6_Top2;
		uniform float4 _PlantColorSet6_Bot1;
		uniform float4 _PlantColorSet6_Bot2;

		//7
		uniform float4 _PlantColorSet7_Top1;
		uniform float4 _PlantColorSet7_Top2;
		uniform float4 _PlantColorSet7_Bot1;
		uniform float4 _PlantColorSet7_Bot2;

		//8
		uniform float4 _PlantColorSet8_Top1;
		uniform float4 _PlantColorSet8_Top2;
		uniform float4 _PlantColorSet8_Bot1;
		uniform float4 _PlantColorSet8_Bot2;

		//9
		uniform float4 _PlantColorSet9_Top1;
		uniform float4 _PlantColorSet9_Top2;
		uniform float4 _PlantColorSet9_Bot1;
		uniform float4 _PlantColorSet9_Bot2;

		//.......

		void vert(inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);

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
			o.color = v.color;
		}

		void surf (Input IN, inout SurfaceOutput o) {

			float frame = clamp(_FrameNumber, 0, _TotalFrames);
			
			if (_ToggleVertexColorAnim != 0) {
				frame = clamp(round(IN.color.a * _TotalFrames), 0, _TotalFrames);
			}

			float2 offPerFrame = float2((1 / (float)_Columns), (1 / (float)_Rows));

			float2 spriteSize = IN.uv_MainTex;
			spriteSize.x = (spriteSize.x / _Columns);
			spriteSize.y = (spriteSize.y / _Rows);

			float2 currentSprite = float2(0,  1 - offPerFrame.y);
			currentSprite.x += frame * offPerFrame.x;
			
			float rowIndex;
			float mod = modf(frame / (float)_Columns, rowIndex);
			currentSprite.y -= rowIndex * offPerFrame.y;
			currentSprite.x -= rowIndex * _Columns * offPerFrame.x;
			
			float2 spriteUV = (spriteSize + currentSprite); // * _FrameScale


			float seed = frac(_ColorSetSeed);

			if (_ColorSetEnabled != 0) {
				//this is probably better off being an array
				if (_CurrentColorSet == 0) {
					_ColorTop = lerp(_PlantColorSet0_Top1, _PlantColorSet0_Top2, seed);
					_ColorBot = lerp(_PlantColorSet0_Bot1, _PlantColorSet0_Bot2, seed);
				}
				else if (_CurrentColorSet == 1) {
					_ColorTop = lerp(_PlantColorSet1_Top1, _PlantColorSet1_Top2, seed);
					_ColorBot = lerp(_PlantColorSet1_Bot1, _PlantColorSet1_Bot2, seed);
				}
				else if (_CurrentColorSet == 2) {
					_ColorTop = lerp(_PlantColorSet2_Top1, _PlantColorSet2_Top2, seed);
					_ColorBot = lerp(_PlantColorSet2_Bot1, _PlantColorSet2_Bot2, seed);
				}
				else if (_CurrentColorSet == 3) {
					_ColorTop = lerp(_PlantColorSet3_Top1, _PlantColorSet3_Top2, seed);
					_ColorBot = lerp(_PlantColorSet3_Bot1, _PlantColorSet3_Bot2, seed);
				}
				else if (_CurrentColorSet == 4) {
					_ColorTop = lerp(_PlantColorSet4_Top1, _PlantColorSet4_Top2, seed);
					_ColorBot = lerp(_PlantColorSet4_Bot1, _PlantColorSet4_Bot2, seed);
				}
				else if (_CurrentColorSet == 5) {
					_ColorTop = lerp(_PlantColorSet5_Top1, _PlantColorSet5_Top2, seed);
					_ColorBot = lerp(_PlantColorSet5_Bot1, _PlantColorSet5_Bot2, seed);
				}
				else if (_CurrentColorSet == 6) {
					_ColorTop = lerp(_PlantColorSet6_Top1, _PlantColorSet6_Top2, seed);
					_ColorBot = lerp(_PlantColorSet6_Bot1, _PlantColorSet6_Bot2, seed);
				}
				else if (_CurrentColorSet == 7) {
					_ColorTop = lerp(_PlantColorSet7_Top1, _PlantColorSet7_Top2, seed);
					_ColorBot = lerp(_PlantColorSet7_Bot1, _PlantColorSet7_Bot2, seed);
				}
				else if (_CurrentColorSet == 8) {
					_ColorTop = lerp(_PlantColorSet8_Top1, _PlantColorSet8_Top2, seed);
					_ColorBot = lerp(_PlantColorSet8_Bot1, _PlantColorSet8_Bot2, seed);
				}
				else if (_CurrentColorSet == 9) {
					_ColorTop = lerp(_PlantColorSet9_Top1, _PlantColorSet9_Top2, seed);
					_ColorBot = lerp(_PlantColorSet9_Bot1, _PlantColorSet9_Bot2, seed);
				}
			}

			fixed4 c = tex2D (_MainTex, spriteUV);
			c.rgb = lerp(_ColorTop, _ColorBot, c.r) * IN.color.rgb;

			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Transparent/Cutout/Diffuse"
}
