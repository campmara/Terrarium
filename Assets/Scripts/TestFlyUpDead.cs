using UnityEngine;
using System.Collections;

public class TestFlyUpDead : MonoBehaviour 
{
	Rigidbody rb;

	const float WIND_EFFECT = 0.05f;
	const float PLUCK_FORCE = 10f;
	const float ASCEND_FORCE = 0.7f;

	const float PLUCK_MIN_TIME = 0.5f;
	const float PLUCK_MAX_TIME = 1.2f;

	enum FlyState
	{
		PLUCK,
		ASCEND
	}
	FlyState currentState;
	float pluckTime;

	void Awake()
	{
		rb = GetComponent(typeof(Rigidbody)) as Rigidbody;

		pluckTime = Random.Range(PLUCK_MIN_TIME, PLUCK_MAX_TIME);

		StartCoroutine(StateHandler());
	}

	void Update()
	{
		FlyDeath();
	}

	void FlyDeath()
	{
		// Apply a weird constant random rotation.
		rb.AddTorque(WeatherManager.instance.WindForce * WIND_EFFECT * Time.deltaTime);

		// Apply an upward force.
		Vector3 upDir = ((Vector3.up * 5f) + (WeatherManager.instance.WindForce)).normalized;
		rb.AddForce(upDir * (currentState == FlyState.PLUCK ? PLUCK_FORCE : ASCEND_FORCE) * Time.deltaTime, ForceMode.Impulse);
	}

	IEnumerator StateHandler()
	{
		currentState = FlyState.PLUCK;

		yield return new WaitForSeconds(pluckTime);

		currentState = FlyState.ASCEND;
	}
}
