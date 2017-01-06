using UnityEngine;
using System.Collections;

public class ParallaxHelpers : MonoBehaviour 
{
	// Returns the number you should be putting into the main camera's orthographic size.
	// I actually don't know what the parameters you put in are for, idk shouldn't be difficult to figure out if we need to.
	public static float GetFieldOfView(float orthoSize, float distanceFromOrigin)
	{
		// orthoSize
		float a = orthoSize;
		// distanceFromOrigin
		float b = Mathf.Abs(distanceFromOrigin);

		float fieldOfView = Mathf.Atan(a / b)  * Mathf.Rad2Deg * 2f;
		return fieldOfView;
	}
}
