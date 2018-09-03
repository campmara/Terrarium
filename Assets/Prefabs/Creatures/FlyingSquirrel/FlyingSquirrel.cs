using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingSquirrel : MonoBehaviour 
{
	/*
	TODO:
	- Make this.
	- Subtle z rotation shifts on a sine wave, to make it look like it's very slightly lilting.
	- starts at some point far away from the playspace, turns to face the center.
	- moves forward very very slowly
	- once it reaches the opposite end of where it started in terms of distance we can disable it.
	- only appears once every 30 minutes??? something like that.
	*/

	private const float START_DISTANCE = 300f;
    private const float FLY_TIME = 200f;

    [SerializeField] private Vector2 _flyWaitRange = new Vector2( 240f, 480f );

	private Vector3 euler;

	private void Awake()
	{
		euler = Vector3.zero;

		StartCoroutine(FlyRoutine());
	}

	private void Update()
	{
		euler.z = Mathf.Sin(1f * Time.time) * 4;

		transform.eulerAngles = euler;
	}

	private IEnumerator FlyRoutine()
	{
		Vector3 startPos = CalculateStartPos();
		transform.position = startPos;

		Vector3 endPos = -startPos;
		
		Vector3 eRot = Quaternion.LookRotation((endPos - startPos).normalized, Vector3.up).eulerAngles;
		euler = eRot;
		transform.eulerAngles = euler;

		float timer = 0f;
		

		while (timer < FLY_TIME)
		{
			timer += Time.unscaledDeltaTime;

			transform.position = Vector3.Lerp(startPos, endPos, timer / FLY_TIME );
			yield return null;
		}

		yield return new WaitForSecondsRealtime(Random.Range( _flyWaitRange.x, _flyWaitRange.y ) );

		StartCoroutine(FlyRoutine());
	}

	private Vector3 CalculateStartPos()
	{
		Vector3 rand = Random.insideUnitCircle;
		rand.y = 0f;
		rand.Normalize();

		rand *= START_DISTANCE;

		return rand;
	}
}
