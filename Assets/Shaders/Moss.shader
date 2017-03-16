Shader "Custom/BlendFaceUp" {
	Properties{
		_MainTex("Base (RGB)", 2D) = "white" {}
	_Color("Color", color) = (1,1,1,0)

		_MeltTex("MeltTex (RGB)", 2D) = "white" {}
	_MeltColor("Melt Color", color) = (1,1,1,0)

		_MeltY("MeltY", float) = 1
		_MeltDistance("Melt Distance", float) = 1
		_Scale("Scale", float) = 1
	}
		SubShader{
		Tags{ "RenderType" = "Geometry" }
		LOD 300
		//cull off

		CGPROGRAM
#pragma surface surf BlinnPhong vertex:disp nolightmap addshadow fullforwardshadows
#pragma target 4.6

		struct appdata {
		float4 vertex : POSITION;
		float4 color : COLOR;
		float4 tangent : TANGENT;
		float3 normal : NORMAL;
		float2 texcoord : TEXCOORD0;
	};

	float _MeltDistance;
	float _MeltY;
	sampler2D _FallOffTex;
	float _Scale;

	void disp(inout appdata v)
	{
		//http://diary.conewars.com/melting-shader-part-2/
		float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
		//float melt = (worldPos.y - _MeltY) / _MeltDistance;
		//float edge = abs(sin(tex2Dlod(_FallOffTex, float4(0, 0, worldPos.x * _Scale, worldPos.y * _Scale)).r));
		if (worldPos.y < .25) {
			float melt = (worldPos.y - _MeltY) / _MeltDistance;
			melt = smoothstep(1, 0, melt);
			//melt = 1 - saturate(melt);
			fixed4 worldUp = mul(transpose(unity_ObjectToWorld), float4(0, 1, 0, 1));
			v.normal = lerp(v.normal, worldUp, melt * melt);
			v.color = lerp(0, 1, melt * melt);
		}

	}

	struct Input {
		float2 uv_MainTex;
		float3 worldPos;
		float3 worldNormal;
		float4 color : COLOR;
		float3 viewDir;
	};

	sampler2D _MeltTex;
	sampler2D _MainTex;
	fixed4 _Color;
	fixed4 _MeltColor;

	void surf(Input IN, inout SurfaceOutput o) {
		//https://ravingbots.com/2015/09/02/how-to-improve-unity-terrain-texturing-tutorial/
		fixed4 cX = tex2D(_MeltTex, IN.worldPos.yz * _Scale);
		fixed4 cY = tex2D(_MeltTex, IN.worldPos.xz * _Scale);
		fixed4 cZ = tex2D(_MeltTex, IN.worldPos.xy * _Scale);

		// we drop the sign because we do not distinguish positive and negative directions
		float3 blend = abs(IN.worldNormal);

		// the values should sum to 1 but we must avoid dividing by 0
		blend /= blend.x + blend.y + blend.z + 0.001f;

		// blending
		fixed4 meltfinal = blend.x * cX + blend.y  * cY  + blend.z * cZ;

		//edge and melting
		float edge = (abs(sin(meltfinal.r))) * 5;
		float melt = (((IN.worldPos.y) - _MeltY) / _MeltDistance) - (edge - 1.5);
		melt = smoothstep(1, 0, melt);

		float4 baseColor = tex2D(_MainTex, IN.uv_MainTex)* _Color;

		float interp = smoothstep(.0, .5,meltfinal.r);
		o.Albedo = lerp(baseColor, _MeltColor, (melt * melt));
		o.Albedo = lerp(baseColor, o.Albedo, interp);
	}

	ENDCG
	}
		FallBack "Diffuse"
}