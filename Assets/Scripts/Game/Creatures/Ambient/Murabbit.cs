using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Murabbit : MonoBehaviour 
{
	private MurabbitData data;

	enum State
	{
		NULL,
		SPAWN,
		IDLE,
		HOP,
		ESCAPE_CHECK,
		ESCAPE_HOP,
		BURROW,
		MAX
	}
	[SerializeField, ReadOnly] private State state = State.NULL;

	private Coroutine burrowRoutine;
	private Tween hopTween;
	private Tween escapeTween;
	private Tween burrowTween;

	private const float HOP_DURATION = 0.5f;
	private const float MAX_SCARY_DISTANCE = 5f;

	private float sqrDistFromPlayer = 0f;

	private float returnTimer = 0f;
	private float returnTime = 0f;

	private float idleTimer = 0f;
	private float idleTime = 0f;

	public void Setup(MurabbitData data)
	{
		this.data = data;

		transform.localScale = new Vector3(data.scale, data.scale, data.scale);

		SetState(State.SPAWN);
	}

	private void Update()
	{
		// RETURN TIME HANDLING

		returnTimer += Time.deltaTime;
		if (returnTimer >= returnTime)
		{
			SetState(State.BURROW);
		}

		// STATE HANDLING (Timers and Stuff)

		if (state == State.IDLE)
		{
			HandleIdle();
		}
	}

	private void SetState(State next)
	{
		state = next;

		switch (next)
		{
			case State.SPAWN:
				OnEnterSpawn();
				break;
			case State.IDLE:
				OnEnterIdle();
				break;
			case State.HOP:
				OnEnterHopping();
				break;
			case State.ESCAPE_CHECK:
				OnEnterEscapeCheck();
				break;
			case State.ESCAPE_HOP:
				OnEnterEscapeHop();
				break;
			case State.BURROW:
				OnEnterBurrowing();
				break;
		}
	}

	/*
		SPAWNING
	*/
	private void OnEnterSpawn()
	{
		returnTime = Random.Range(50f, 60f);

		Vector3 jumpPos = transform.position + (Random.insideUnitSphere * 2f);
		jumpPos.y = 0f;

		hopTween = transform.DOJump(jumpPos, Random.Range(1.5f, 3f), 1, HOP_DURATION).OnComplete(() => SetState(State.HOP));
	}

	/*
		IDLING
	*/
	private void OnEnterIdle()
	{
		idleTime = Random.Range(0.5f, 3f);
		idleTimer = 0f;
	}

	private void HandleIdle()
	{
		idleTimer += Time.deltaTime;

		if (idleTimer >= idleTime)
		{
			SetState(State.HOP);
		}

		if (CheckForEscape())
		{
			SetState(State.ESCAPE_HOP);
		}
	}

	/*
		HOPPING
	*/
	private void OnEnterHopping()
	{
		Vector3 jumpPos = transform.position + (Random.insideUnitSphere * 2f);
		jumpPos.y = 0f;

		hopTween = transform.DOJump(jumpPos, Random.Range(0.5f, 1.25f), 1, HOP_DURATION)
			.OnComplete(() => SetState(State.IDLE));
	}

	/*
		ESCAPING
	*/
	private void OnEnterEscapeCheck()
	{
		if (CheckForEscape())
		{
			SetState(State.ESCAPE_HOP);
		}
		else
		{
			SetState(State.IDLE);
		}
	}

	private void OnEnterEscapeHop()
	{
		Vector3 diff = PlayerManager.instance.Player.transform.position - transform.position;
		diff.y = 0f;
		diff = -(diff.normalized);
		Vector3 jumpPos = transform.position + (diff * Random.Range(1.75f, 2.5f));
		jumpPos.y = 0f;
		transform.LookAt(jumpPos);

		escapeTween = transform.DOJump(jumpPos, Random.Range(0.5f, 1.25f), 1, HOP_DURATION)
			.OnComplete(() => SetState(State.ESCAPE_CHECK));
	}

	private bool CheckForEscape()
	{
		// Handle Escape Checking
		if (PlayerManager.instance.Player.GetComponent<RollerController>().State != P_ControlState.ROLLING &&
			PlayerManager.instance.Player.PlayerSingController.State != SingController.SingState.SINGING)
		{
			return false;
		}

		sqrDistFromPlayer = (PlayerManager.instance.Player.transform.position - transform.position).sqrMagnitude;
		if (sqrDistFromPlayer <= MAX_SCARY_DISTANCE * MAX_SCARY_DISTANCE)
		{
			return true;
		}

		return false;
	}

	/*
		BURROWING
	*/
	private void OnEnterBurrowing()
	{
		returnTime = 0f;

		burrowRoutine = StartCoroutine(BurrowRoutine());
	}

	private IEnumerator BurrowRoutine()
	{
		// Wait for tweens to finish.
		if (hopTween != null)
		{
			yield return hopTween.WaitForCompletion();
			hopTween.Kill();
			hopTween = null;
		}

		if (escapeTween != null)
		{
			yield return escapeTween.WaitForCompletion();
			escapeTween.Kill();
			escapeTween = null;
		}

		burrowTween = transform.DOJump(data.spawner.transform.position, 1f, 1, HOP_DURATION)
			.OnComplete(() => data.spawner.OnRabbitReturn(this));

		yield return burrowTween.WaitForCompletion();

		burrowTween = null;
	}
}
