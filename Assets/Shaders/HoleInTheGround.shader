Shader "Custom/HoleInTheGround" 
{
	SubShader 
	{
	    Tags {"Queue" = "Geometry-1" } // earlier = hides stuff later in queue
	    Lighting Off
	    ZTest LEqual
	    ZWrite On
	    ColorMask 0
	    Pass {}
  	}
}
