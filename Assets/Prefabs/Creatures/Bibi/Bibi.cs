using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Bibi : Singable
{
	[SerializeField] private GameObject head;
	[SerializeField] private GameObject ringA;
	[SerializeField] private GameObject ringB;
	[SerializeField] private GameObject ringC;
	[SerializeField] private ParticleSystem sleepBubbles;

	[HeaderAttribute("Faces"), SerializeField] private Sprite regFace;
	[SerializeField] private Sprite sleepFace;
	[SerializeField] private Sprite madFace;

	private const float MOVE_SPEED = 5f;
	private const float CLOSE_ENOUGH = 0.5f;
	private const float AWAKE_TIME = 40f;
	private const float SLEEP_TIME = 50f;

	private Vector3 desiredLocation;
	private float sleepTimer = 0f;

	private AudioSource bibiAudioSource;
	private SpriteRenderer face;
	private Rigidbody _rigidbody;

    private enum BibiState
	{
		BURROWING,
		UNDERGROUND,
		DISRUPTED,
		ESCAPING,
		SLEEPING
	};
	private BibiState state;

	private void ChangeState(BibiState nextState)
	{
		switch(state)
		{
			case BibiState.BURROWING:
				OnExitBurrowing();
				break;
			case BibiState.UNDERGROUND:
				OnExitUnderground();
				break;
			case BibiState.DISRUPTED:
				OnExitDisrupted();
				break;
			case BibiState.ESCAPING:
				OnExitEscaping();
				break;
			case BibiState.SLEEPING:
				OnExitSleeping();
				break;
		}

		state = nextState;

		switch(nextState)
		{
			case BibiState.BURROWING:
				OnEnterBurrowing();
				break;
			case BibiState.UNDERGROUND:
				OnEnterUnderground();
				break;
			case BibiState.DISRUPTED:
				OnEnterDisrupted();
				break;
			case BibiState.ESCAPING:
				OnEnterEscaping();
				break;
			case BibiState.SLEEPING:
				OnEnterSleeping();
				break;
		}
	}

	/*
	public void OnPickup()
	{
		// AhhH!!!! BIBIBIBIBIBIBIBIBIBIBIBIBI.
		if (state == BibiState.UNDERGROUND || state == BibiState.SLEEPING)
		{
			ChangeState(BibiState.DISRUPTED);
		}
	}
	*/

	public override void OnAffectedBySinging()
	{
		// AhhH!!!! BIBIBIBIBIBIBIBIBIBIBIBIBI.
		if (state == BibiState.UNDERGROUND || state == BibiState.SLEEPING)
		{
			ChangeState(BibiState.DISRUPTED);
		}
	}

	private void Awake()
	{
		bibiAudioSource = GetComponent(typeof(AudioSource)) as AudioSource;
		face = GetComponentInChildren(typeof(SpriteRenderer)) as SpriteRenderer;
		_rigidbody = GetComponent(typeof(Rigidbody)) as Rigidbody;

		ChangeState(BibiState.BURROWING);
	}

	private void FixedUpdate()
	{
		if (state == BibiState.BURROWING)
		{
			HandleBurrowing();
		}
		else if (state == BibiState.UNDERGROUND)
		{
			HandleUnderground();
		}
		else if (state == BibiState.DISRUPTED)
		{
			HandleDisrupted();
		}
		else if (state == BibiState.ESCAPING)
		{
			HandleEscaping();
		}
		else if (state == BibiState.SLEEPING)
		{
			HandleSleeping();
		}
	}

	// State Enter / Exits
	private void OnEnterBurrowing()
	{
		float ringY = -0.02f;
		float headY = 0f;
		float faceY = 0.05f;

		float duration = Random.Range(0.5f, 1);

		head.transform.DOMoveY(headY, duration)
			.SetEase(Ease.OutQuint);
		face.transform.DOMoveY(faceY, duration)
			.SetEase(Ease.OutQuint);

		ringA.transform.DOMoveY(ringY, duration)
			.SetEase(Ease.OutQuint);
		ringB.transform.DOMoveY(ringY, duration)
			.SetEase(Ease.OutQuint);
		ringC.transform.DOMoveY(ringY, duration)
			.SetEase(Ease.OutQuint)
			.OnComplete(BurrowTweenComplete);

		face.sprite = regFace;
	}
	private void BurrowTweenComplete()
	{
		ChangeState(BibiState.UNDERGROUND);
	}
	private void OnExitBurrowing()
	{

	}

	private void OnEnterUnderground()
	{
		sleepTimer = 0f;
	}
	private void OnExitUnderground()
	{

	}

	private void OnEnterDisrupted()
	{
		float yA = 0.75f;
		float yB = 0.50f;
		float yC = 0.25f;
		float headY = 1f;
		float faceY = 1.05f;

		float duration = Random.Range(0.5f, 1);

		head.transform.DOMoveY(headY, duration)
			.SetEase(Ease.OutQuint);
		face.transform.DOMoveY(faceY, duration)
			.SetEase(Ease.OutQuint);

		ringA.transform.DOMoveY(yA, duration)
			.SetEase(Ease.OutQuint);
		ringB.transform.DOMoveY(yB, duration)
			.SetEase(Ease.OutQuint);
		ringC.transform.DOMoveY(yC, duration)
			.SetEase(Ease.OutQuint)
			.OnComplete(DisruptTweenComplete);

		face.sprite = regFace;
	}
	private void DisruptTweenComplete()
	{
		ChangeState(BibiState.ESCAPING);
	}
	private void OnExitDisrupted()
	{

	}

	private void OnEnterEscaping()
	{
		desiredLocation = new Vector3(Random.Range(-28f, 28f), 0f, Random.Range(-28f, 28f));
		//desiredLocation = Random.insideUnitCircle * 28f;

		while (desiredLocation == Vector3.zero)
		{
			desiredLocation = new Vector3(Random.Range(-28f, 28f), 0f, Random.Range(-28f, 28f));
			//desiredLocation = Random.insideUnitCircle * 28f;
		}

		while ((desiredLocation - transform.position).magnitude < 15f)
		{
			desiredLocation = new Vector3(Random.Range(-28f, 28f), 0f, Random.Range(-28f, 28f));
			//desiredLocation = Random.insideUnitCircle * 28f;
			//desiredLocation.y = 0f;
		}

		//desiredLocation.y = 0f;

		// GRRRRR
		face.sprite = madFace;

		// Start the bibi sound
		bibiAudioSource.Play();
	}
	private void OnExitEscaping()
	{
		desiredLocation = Vector3.zero;

		// Stop the bibi sound.
		bibiAudioSource.Stop();
	}

	private void OnEnterSleeping()
	{
		sleepTimer = 0f;

		face.sprite = sleepFace;
		sleepBubbles.Play();
	}
	private void OnExitSleeping()
	{
		sleepBubbles.Stop();
	}

	// State Handlers
	private void HandleBurrowing()
	{

	}

	private void HandleUnderground()
	{
		sleepTimer += Time.deltaTime;

		if (sleepTimer >= AWAKE_TIME)
		{
			ChangeState(BibiState.SLEEPING);
			sleepTimer = 0f;
		}
	}

	private void HandleDisrupted()
	{
		
	}

	private void HandleEscaping()
	{
		if (desiredLocation != Vector3.zero)
		{
			Vector3 diff = desiredLocation - transform.position;
			Vector3 moveDirection = diff.normalized;
			//moveDirection.x += (Mathf.Sin(Time.deltaTime) * 1.5f);

			_rigidbody.MovePosition(transform.position + (moveDirection * MOVE_SPEED * Time.deltaTime));

			Vector3 targetLook = transform.position + moveDirection;
			transform.LookAt(targetLook);

			if (diff.sqrMagnitude < (CLOSE_ENOUGH * CLOSE_ENOUGH))
			{
				// We made it!
				ChangeState(BibiState.BURROWING);
			}
		}
	}

	private void HandleSleeping()
	{
		sleepTimer += Time.deltaTime;

		if (sleepTimer >= SLEEP_TIME)
		{
			ChangeState(BibiState.UNDERGROUND);
			sleepTimer = 0f;
		}
	}
}