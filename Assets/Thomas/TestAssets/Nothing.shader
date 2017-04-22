﻿Shader "Custom/Nothing" {

	SubShader{
		// Render the mask after regular geometry, but before masked geometry and
		// transparent things.

		Tags{ "Queue" = "Transparent-5" }

		// Don't draw in the RGBA channels; just the depth buffer

		ColorMask 0
		ZWrite Off

		// Do nothing specific in the pass:

		Pass{}
	}
}