Shader "Custom/GroundHoleSplatMask" 
{
	SubShader 
	{
	    Tags {"Queue" = "Transparent+10" } // earlier = hides stuff later in queue
	    ZTest LEqual
	    ZWrite On
		Blend SrcAlpha OneMinusSrcAlpha
	    //ColorMask 0
		
		CGPROGRAM
		#pragma surface surf Standard
		#pragma target 3.0

		struct Input 
		{
			float3 worldPos;
		};

		void surf (Input IN, inout SurfaceOutputStandard o) 
		{
			discard;
		}
		ENDCG
	}


}
