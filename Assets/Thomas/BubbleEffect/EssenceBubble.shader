Shader "Custom/Essence Bubble" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_SpecularColor ("Specular Color", Color) = (0.25,0.25,0.25,1)
		_SpecularPower ("Specular Power", float) = 148
		_Cutoff ("Cutoff", Range(0,1)) = 1
	}
	SubShader {

		Tags { "RenderType"="Transparent" "Queue" = "Transparent" }
		LOD 200
		//Cull off 
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Water fullforwardshadows alphatest:_Cutoff //vertex:vert

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
			float4 color : COLOR;
			float4 proj : TEXCOORD;
			float4 grabPos : TEXCOORD1;
		};

		struct appdata
		{
			float4 vertex : POSITION;
			float4 color : COLOR;
			float4 texcoord : TEXCOORD0;
			float3 normal : NORMAL;
		};

		fixed4 _Color;
		float _SpecularPower;

		fixed4 LightingWater(SurfaceOutput s, fixed3 lightDir, half3 viewDir, fixed atten)
		{
			fixed4 c;
			half3 h = normalize(lightDir + viewDir);

			half diff = max(0, dot(s.Normal, lightDir));

			float nh = max(0, dot(s.Normal, h));
			float spec = pow(nh, _SpecularPower) * 1;

			c.rgb = s.Albedo;
			c.rgb += spec * s.Specular;
			c.a = s.Alpha;
			return c;
		}
		
		void vert(inout appdata v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);
			o.color = v.color;
		}

		float4 _SpecularColor;

		void surf (Input IN, inout SurfaceOutput o) {
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color * IN.color;
			o.Albedo = c;// * tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(IN.proj)).rgb;
			o.Alpha = c.a; // * IN.color.a
			o.Specular = _SpecularColor;
		}
		ENDCG
	}
	//FallBack "Transparent/Cutout/Diffuse"
}