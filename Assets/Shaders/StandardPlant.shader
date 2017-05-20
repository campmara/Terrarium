Shader "TerrariumPlant/Standard"
{
	Properties
	{
		[Header(Colors)]
		_MainTex("Base (RGB)", 2D) = "white" {}
		_ColorTop("Top Color", Color) = (1,1,1,1)
		_ColorMid("Mid Color", Color) = (1,1,1,1)
		_ColorBot("Bot Color", Color) = (1,1,1,1)
		_Middle("Middle", Range(0.001, 0.999)) = 0.5

		[Header(Light Values)]
		_Hardness("Hardness", Range(.25, 1)) = 0.5

		[Header(Wind Values)]
		[Toggle]_WindEnabled("Wind Enabled", float) = 1
		_Sensitivity("Height Sensitivity to Wind", Range(.000001, 1000)) = 15

		[Header(Splatmap Values)]
		[Toggle]_SplatmapEnabled("Splatmap Enabled", float) = 0
		_SplatTex("Splat Texture (RGBA)", 2D) = "white" {}
		_ImprintAmount("Imprint Amount", Range(-3, 3)) = 1

		[Header(Cutoff Values)]
		_Dissolve("Dissolve Amount", Range(0, 1)) = 0
		_CutoffNoiseScale("Cutoff Noise Scale", float) = 10
		_CutoffEdgeScale("Cutoff Edge Scale", Range(0,.5)) = .45

		//_CutoffPos("Cutoff Position", Range(0,1)) = 1. for vanishing from top
	}
	SubShader
	{
		Tags{ "Queue" = "Geometry" }
		Cull Off
		CGPROGRAM
		
		#pragma surface surf WrapLambert vertex:vert addshadow alphatest:zero
		#include "Noise.cginc"

		half _Hardness;
		half4 _ShadowColor;

		half4 LightingWrapLambert(SurfaceOutput s, half3 lightDir, half3 viewDir, half atten)
		{
			s.Normal = normalize(s.Normal);
			half distAtten;
			if (_WorldSpaceLightPos0.w == 0.0)
				distAtten = 1.0;
			else
				distAtten = saturate(1.0 / length(lightDir));

			half diff = (max(0, dot(s.Normal, lightDir)) * atten + 1 - _Hardness) * _Hardness;

			half4 c;
			c.rgb = (s.Albedo * diff * _LightColor0) * distAtten;
			c.a = s.Alpha;
			return c;
		}

		struct Input 
		{
			float2 uv_MainTex;
			//float4 color : COLOR;
			float3 localPos;
			float3 localNormal;
			//float4 turbulence;
		};

		sampler2D _MainTex;
		//...cutoff...
		float _CutoffEdgeScale;
		float _CutoffNoiseScale;
		float _Dissolve;
		//.....

		//...gradient...
		fixed4 _ColorTop;
		fixed4 _ColorMid;
		fixed4 _ColorBot;
		float  _Middle;
		//......

		//...wind...
		float _WindEnabled;
		float _Sensitivity;

		uniform fixed4 _WaveDir;
		uniform float _WaveNoise;
		uniform float _WaveAmount;
		uniform float _WaveScale;
		uniform float _WaveTime;
		uniform float3 _WaveTimeVec;
		uniform sampler2D _WindTex;
		//......

		//...splatmap...
		float _ImprintAmount;
		float _SplatmapEnabled;

		uniform sampler2D _ClipEdges;
		uniform sampler2D _SplatMap;
		uniform float4 _SplatmapNeutralColor;
		uniform float3 _CameraWorldPos;
		uniform float _OrthoCameraScale;
		//..............

		//splatmap+
		sampler2D _SplatTex;

		void vert(inout appdata_base v, out Input o) 
		{

			UNITY_INITIALIZE_OUTPUT(Input, o);

			o.localPos = v.vertex;
			o.localNormal = v.normal;
			float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
			float4 worldNormal = mul(unity_ObjectToWorld, float4(v.normal, 0));

			//Chris's wind function modified to take in to account world height and more directional wind
			//.......................
			//this effect causes the material to disappear if the amount is 0, this won't run the code if that's the case
			if (_WindEnabled != 0 && _WaveAmount != 0 && length(_WaveDir != 0)) {
				float noiseOffset = cnoise(worldPos.xyz) * _WaveNoise;
				//clamped to prevent extreme results at higher branches, needs tweaking, it would be nice if lower plants shook more in here
				float heightSensitivity = clamp(worldPos.y * worldPos.y, 0, _Sensitivity) / _Sensitivity;
				//oscillation value adds on to the direction of the wind, it's length is measured with _WaveScale
				float4 oscillation = sin(_WaveTime + noiseOffset  * heightSensitivity) * _WaveScale * normalize(_WaveDir) * heightSensitivity; // * v.color.r																																   //wave direction and oscillation combined are then scaled overall by the _WaveAmount
				float4 wind = (normalize(_WaveDir) + oscillation) * _WaveAmount * heightSensitivity; //* v.color.r
				float turbulenceScale = .025;
				float4 turbulence = tex2Dlod(_WindTex, (float4(worldPos.x, worldPos.z, 0, 0) + float4(_WaveTimeVec.x, _WaveTimeVec.z, 0,0)) * .025);
				wind *= turbulence;
				worldPos += wind;
				//o.turbulence = turbulence;
			}
			//''''''''''''''''''''''

			//splatmap deformation
			//...........
			if (_SplatmapEnabled != 0) {
				_OrthoCameraScale *= 2;
				float2 worldUV = float2(((worldPos.x - _CameraWorldPos.x) / _OrthoCameraScale + .5f), ((worldPos.z - _CameraWorldPos.z) / _OrthoCameraScale + .5f)); //find a way to center this
				float4 border = tex2Dlod(_ClipEdges, float4(worldUV, 0, 0)).rgba;
				float4 uv = (tex2Dlod(_SplatMap, float4(worldUV, 0, 0)));

				float4 duv = (tex2Dlod(_SplatTex, float4(uv.xy, 0, 0)));
				float4 d = duv;
				d = lerp(_SplatmapNeutralColor, d, uv.a);

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
		}

		void surf(Input IN, inout SurfaceOutput o) 
		{

			//...
			//https://github.com/keijiro/TriplanarPBS/blob/master/Assets/TriplanarPBS/Shaders/TriplanarPBS.shader#L69
			// Calculate a blend factor for triplanar mapping.
			float3 bf = normalize(abs(IN.localNormal));
			bf /= dot(bf, (float3)1);

			// Get texture coordinates.
			float2 tx = IN.localPos.yz * _CutoffNoiseScale;
			float2 ty = IN.localPos.zx * _CutoffNoiseScale;
			float2 tz = IN.localPos.xy * _CutoffNoiseScale;

			// Base color
			half4 cx = wnoise(float3(tx,0)) * bf.x;
			half4 cy = wnoise(float3(ty,0)) * bf.y;
			half4 cz = wnoise(float3(tz,0)) * bf.z;
			half4 color = (cx + cy + cz);
			float alpha = color.r;
			//...

			fixed4 gradient = lerp(_ColorBot, _ColorMid, IN.uv_MainTex.y / _Middle) * step(IN.uv_MainTex.y, _Middle);
			gradient += lerp(_ColorMid, _ColorTop, (IN.uv_MainTex.y - _Middle) / (1 - _Middle)) * step(_Middle, IN.uv_MainTex.y);

			//crispy edges for fading
			gradient = lerp(gradient, _ColorMid, _Dissolve * round(1 - (alpha - _Dissolve + _CutoffEdgeScale)));

			o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb * gradient;
			o.Alpha = alpha - _Dissolve;

			//o.Albedo = IN.turbulence.rgb;
			//code for fading from the top of the mesh
			//o.Alpha = lerp(0, 1, ((1 - IN.uv_MainTex.y) * 2) - (_CutoffPos + color.r/4 * _CutoffPos));
		}

		ENDCG
	}
	FallBack "Transparent/Cutout/Diffuse"
}
