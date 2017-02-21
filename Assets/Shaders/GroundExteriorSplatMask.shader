Shader "Custom/GroundExteriorSplatMask" 
{
	Properties 
	{
		// Disc Clipping
		_Center("Center of Disc", Vector) = (0, 0, 0, 0)
		_Radius("Disc Radius", Range(0.0, 1.0)) = 1.0
		_ScaleFactor("Scale Factor", Float) = 4.0
	}
	SubShader 
	{
		Tags {"Queue" = "Transparent+10" } // earlier = hides stuff later in queue
	    ZTest LEqual
	    ZWrite On
	    //ColorMask 0
		
		CGPROGRAM
		#pragma surface surf Standard
		#pragma target 3.0

		#include "Noise.cginc"

		fixed4 _Center;
		float _Radius;
		float _ScaleFactor;

		struct Input 
		{
			float3 worldPos;
		};

		void surf (Input IN, inout SurfaceOutputStandard o) 
		{
			float len = length(IN.worldPos - _Center) + cnoise(IN.worldPos) + cnoise(IN.worldPos * 4);
			float rad = _Radius * (5 * _ScaleFactor);
			if (len < rad)
			{
				discard;
			}
		}
		ENDCG
	}
}
