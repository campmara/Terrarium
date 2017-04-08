Shader "Custom/GroundHoleSplatMask" 
{
	Properties 
	{
		_MainTex("Texture", 2D) = "white" {}
	}
	SubShader 
	{
	    Tags {"Queue" = "AlphaTest-5" "IgnoreProjector"="True"} // earlier = hides stuff later in queue
	    ZWrite On
	    ColorMask 0
		
		CGPROGRAM
		#pragma surface surf Standard
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input 
		{
			float2 uv_MainTex;
			float3 worldPos;
		};

		void surf (Input IN, inout SurfaceOutputStandard o) 
		{
			discard;

			o.Albedo = 0;
			o.Alpha = 0;
		}
		ENDCG
	}
}
