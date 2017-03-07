Shader "Custom/GroundExteriorSplatMask" 
{
	Properties 
	{
		_MainTex("Texture", 2D) = "white" {}

		// Disc Clipping
		_Center("Center of Disc", Vector) = (0, 0, 0, 0)
		_Radius("Disc Radius", Range(0.0, 1.0)) = 1.0
		_ScaleFactor("Scale Factor", Float) = 4.0
	}
	SubShader 
	{
		Tags {"Queue" = "Transparent+10" "IgnoreProjector"="True"} // earlier = hides stuff later in queue
	    //ZTest LEqual
	    //ZWrite On
		Blend SrcAlpha OneMinusSrcAlpha
	    //ColorMask 0
		
		CGPROGRAM
		#pragma surface surf Standard
		#pragma target 3.0

		#include "Noise.cginc"

		sampler2D _MainTex;

		fixed4 _Center;
		float _Radius;
		float _ScaleFactor;

		struct Input 
		{
			float2 uv_MainTex;
			float3 worldPos;
		};

		void surf (Input IN, inout SurfaceOutputStandard o) 
		{
			float len = length(IN.worldPos - _Center) + cnoise(IN.worldPos) + cnoise(IN.worldPos * 4);
			float rad = _Radius * (5 * _ScaleFactor);
			if (len < rad)
			{
				//o.Alpha = 0;
				discard;
			}
			
			fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
			o.Albedo = 0;
			o.Alpha = 0;
		}
		ENDCG
	}
}
