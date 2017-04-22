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

	private Vector3 euler;

	private void Awake()
	{
		euler = Vector3.zero;

		StartCoroutine(FlyRoutine());
	}

	private void Update()
	{
		euler.z = Mathf.Sin(1f * Time.time) * 10f;

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
		float totalTime = 100f;

		while (timer < totalTime)
		{
			timer += Time.deltaTime;

			transform.position = Vector3.Lerp(startPos, endPos, timer / totalTime);
			yield return null;
		}

		yield return new WaitForSeconds(Random.Range(600f, 900f));

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
