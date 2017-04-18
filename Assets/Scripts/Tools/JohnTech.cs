using UnityEngine;
using System.Collections;

public static class JohnTech
{
    /// <summary>
    /// Returns the square of val.
    /// </summary>
    /// <param name="val"></param>
    /// <returns></returns>
    public static float Sqr(float val) 
    {
        return val * val;
    }

    /// <summary>
    /// True/False Coin Flip
    /// </summary>
    public static bool CoinFlip()
	{
		return UnityEngine.Random.value < 0.5f;
	}
	
	/// <summary>
	/// Checks If Num Is Within Offset
	/// </summary>
	public static bool FloatCheckZero(float num, float offset = 0.1f) // Returns true if float is within pos/neg of offset
	{
		return num < offset && num > -offset;	 
	}
	/// <summary>
	/// Returns Uniform Vector.
	/// </summary>
	/// <returns>Uniform vector.</returns>
	/// <param name="value">.</param>
	public static Vector3 UniformVec(float value)
	{
		return new Vector3(value, value, value);
	}

    public static Vector3 RandomRangeVec(float minVal, float maxVal)
    {
        return new Vector3(UnityEngine.Random.Range(minVal, maxVal), UnityEngine.Random.Range(minVal, maxVal), UnityEngine.Random.Range(minVal, maxVal));
    }

    public static float RandomSign()
    {
        return JohnTech.CoinFlip() ? -1.0f : 1.0f;
    }

	// FROM WADEUTILS lel ;/
	#region Increment Position Properties

	public static void AddPosX(this Transform t, float newX)
	{
		t.position = new Vector3(t.position.x + newX, t.position.y, t.position.z);
	}

	public static void AddPosY(this Transform t, float newY)
	{
		t.position = new Vector3(t.position.x, t.position.y + newY, t.position.z);
	}
	
	public static void AddPosZ(this Transform t, float newZ)
	{
		t.position = new Vector3(t.position.x, t.position.y, t.position.z + newZ);
	}

	public static void SetPosX(this Transform t, float newX)
	{
		t.position = new Vector3(newX, t.position.y, t.position.z);
	}

	public static void SetPosY(this Transform t, float newY)
	{
		t.position = new Vector3(t.position.x, newY, t.position.z);
	}

	public static void SetPosZ(this Transform t, float newZ)
	{
		t.position = new Vector3(t.position.x, t.position.y, newZ);
	}

	#endregion

	#region Vector Clamp Functions

	/// <summary>
	/// Clamps x,y of this between min, max values
	/// </summary>
	public static void Vector2Clamp(this Vector2 vec, float min, float max)
	{
		vec.x = Mathf.Clamp(vec.x, min, max);
		vec.y = Mathf.Clamp(vec.y, min, max);
	}

	public static void Vector2Clamp(this Vector2 vec, float xMin, float xMax, float yMin, float yMax)
	{
		vec.x = Mathf.Clamp(vec.x, xMin, xMax);
		vec.y = Mathf.Clamp(vec.y, yMin, yMax);
	}

	/// <summary>
	/// Clamps x,y,z of this between min, max values
	/// </summary>
	public static void Vector3Clamp(this Vector3 vec, float min, float max)
	{
		vec.x = Mathf.Clamp(vec.x, min, max);
		vec.y = Mathf.Clamp(vec.y, min, max);
		vec.z = Mathf.Clamp(vec.z, min, max);

	}

	/// <summary>
	/// Clamps x,y,z of this between min, max values
	/// zMin/Max defaults to 0
	/// x and y min/max must be defined
	/// </summary>
	public static void Vector3Clamp(this Vector3 vec, float xMin, float xMax, float yMin, float yMax, float zMin = 0.0f, float zMax = 0.0f)
	{
		vec.x = Mathf.Clamp(vec.x, xMin, xMax);
		vec.y = Mathf.Clamp(vec.y, yMin, yMax);
		vec.z = Mathf.Clamp(vec.z, zMin, zMax);
	}

	#endregion

	public static Vector3 Midpoint(Vector3 a, Vector3 b)
	{
		Vector3 midPoint = new Vector3((a.x + b.x) / 2.0f, (a.y + b.y) / 2.0f, (a.z + b.z) / 2.0f);
		return midPoint;
	}

	// 3 point quadratic curve lerpy thing
	public static Vector3 GetCurvePoint (Vector3 p0, Vector3 p1, Vector3 p2, float t) 
	{
		t = Mathf.Clamp01(t);
		float oneMinusT = 1f - t;
		return
			oneMinusT * oneMinusT * p0 +
			2f * oneMinusT * t * p1 +
			t * t * p2;
	}

    #region Vector Methods

    public static Vector3 SetPosX( this Vector3 vec, float value )
    {
        vec.x = value;
        return vec;
    }

    public static Vector3 SetPosY( this Vector3 vec, float value )
    {
        vec.y = value;
        return vec;
    }

    public static Vector3 SetPosZ( this Vector3 vec, float value )
    {
        vec.z = value;
        return vec;
    }

    #endregion

    #region Color Methods

    public static Color SetR( this Color col, float value )
    {
        col.r = value;
        return col;
    }

    public static Color SetG( this Color col, float value )
    {
        col.g = value;
        return col;
    }

    public static Color SetB( this Color col, float value )
    {
        col.b = value;
        return col;
    }

    public static Color SetAlpha( this Color col, float value )
    {
        col.a = value;
        return col;
    }


    #endregion

    #region CoRoutine Helpers

    public delegate void TweenEndFunction();
	public delegate void WaitFunctionDelegate();

	public static IEnumerator TweenCameraZoom(Camera cam, float endZoom, float moveTime, TweenEndFunction endFunction = null)
	{
		float timer = 0.0f;
		float startSize = cam.orthographicSize;
		while(timer < moveTime)
		{
			cam.orthographicSize = Mathf.Lerp(startSize, endZoom, timer / moveTime);
			timer += Time.deltaTime;
			yield return 0;
		}

		if(endFunction != null)
		{
			endFunction();
		}

	}

	public static IEnumerator TweenPosition(Transform focusTransform, Vector3 endPos, float moveTime, TweenEndFunction endFunction = null)
	{
		float timer = 0.0f;
		Vector3 startPos = focusTransform.position;
		while(timer < moveTime)
		{
			focusTransform.position = Vector3.Lerp(startPos, endPos, timer / moveTime);
			timer += Time.deltaTime;
			yield return 0;
		}

		if(endFunction != null)
		{
			endFunction();
		}

		focusTransform.position = endPos;
	}

	public static IEnumerator TweenLocalScale(Transform focusTransform, Vector3 startScale, Vector3 endScale, float moveTime, TweenEndFunction endFunction = null)
	{
		float timer = 0.0f;

		while(timer < moveTime)
		{
			focusTransform.localScale = Vector3.Lerp(startScale, endScale, timer / moveTime);
			timer += Time.deltaTime;
			yield return 0;
		}

		if(endFunction != null)
		{
			endFunction();
		}

		focusTransform.localScale = endScale;
	}

	public static IEnumerator WaitFunction(float waitTime, WaitFunctionDelegate endFunction)
	{
		yield return new WaitForSeconds(waitTime); 

		if(endFunction != null)
		{
			endFunction();	
		}
	}

#endregion

#region Color Tools

	/*public static void SetAlpha(this Color c, float newAlpha)
	{
		c = new Color(c.r, c.g, c.b, newAlpha);
	}*/

#endregion
}
