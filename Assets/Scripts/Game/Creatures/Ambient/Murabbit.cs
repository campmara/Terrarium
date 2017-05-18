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
		HOP,
		ESCAPE,
		BURROW,
		MAX
	}
	[SerializeField, ReadOnly] private State state = State.NULL;

	private Coroutine hopRoutine;
	private Coroutine escapeRoutine;
	private Coroutine burrowRoutine;
	private Tween hopTween;
	private Tween escapeTween;
	private Tween burrowTween;

	private const float MAX_SCARY_SQR_DISTANCE = 25f;
	private float sqrDistFromPlayer = 0f;

	private float returnTimer = 0f;
	private float returnTime = 0f;

	public void Setup(MurabbitData data)
	{
		this.data = data;

		transform.localScale = new Vector3(data.scale, data.scale, data.scale);

		SetState(State.SPAWN);
	}

	private void Update()
	{
		if (state == State.HOP)
		{
			HandleHopping();
		}
	}

	private void SetState(State next)
	{
		state = next;

		if (next == State.SPAWN)
		{
			OnEnterSpawn();
		}
		else if (next == State.HOP)
		{
			OnEnterHopping();
		}
		else if (next == State.ESCAPE)
		{
			OnEnterEscaping();
		}
		else if (next == State.BURROW)
		{
			OnEnterBurrowing();
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

		hopTween = transform.DOJump(jumpPos, 2f, 1, 0.5f).OnComplete(() => SetState(State.HOP));
	}

	/*
		HOPPING
	*/
	private void OnEnterHopping()
	{
		hopTween.Complete();
		transform.SetPosY(0f);

		hopRoutine = StartCoroutine(HopRoutine());
	}

	private IEnumerator HopRoutine()
	{
		yield return new WaitForSeconds(Random.Range(0.5f, 3f));

		Vector3 jumpPos = transform.position + (Random.insideUnitSphere * 2f);
		jumpPos.y = 0f;

		hopTween = transform.DOJump(jumpPos, Random.Range(0.5f, 1.25f), 1, 0.5f);

		yield return hopTween.WaitForCompletion();

		hopRoutine = StartCoroutine(HopRoutine());

		hopTween = null;
	}

	private void HandleHopping()
	{
		returnTimer += Time.deltaTime;
		if (returnTimer >= returnTime)
		{
			SetState(State.BURROW);
		}

		// Handle Escape Checking
		if (PlayerManager.instance.Player.GetComponent<RollerController>().State != P_ControlState.ROLLING &&
			PlayerManager.instance.Player.PlayerSingController.State != SingController.SingState.SINGING)
		{
			return;
		}

		sqrDistFromPlayer = (PlayerManager.instance.Player.transform.position - transform.position).sqrMagnitude;

		if (sqrDistFromPlayer <= MAX_SCARY_SQR_DISTANCE)
		{
			// Scarem
			SetState(State.ESCAPE);
		}
	}

	/*
		ESCAPING
	*/
	private void OnEnterEscaping()
	{
		escapeRoutine = StartCoroutine(EscapeRoutine());
	}

	private IEnumerator EscapeRoutine()
	{
		Vector3 diff = PlayerManager.instance.Player.transform.position - transform.position;
		sqrDistFromPlayer = diff.sqrMagnitude;

		if (sqrDistFromPlayer < MAX_SCARY_SQR_DISTANCE)
		{
			diff.y = 0f;
			diff = -(diff.normalized);
			Vector3 jumpPos = transform.position + (diff * Random.Range(1.75f, 2.5f));
			jumpPos.y = 0f;
			transform.LookAt(jumpPos);

			escapeTween = transform.DOJump(jumpPos, Random.Range(0.5f, 1.25f), 1, 0.5f);

			yield return escapeTween.WaitForCompletion();

			escapeRoutine = StartCoroutine(EscapeRoutine());
		}
		else
		{
			// Scarem
			SetState(State.HOP);
			yield return null;
		}

		escapeTween = null;
	}

	/*
		BURROWING
	*/
	private void OnEnterBurrowing()
	{
		returnTime = 0f;
		StopAllCoroutines();
		burrowRoutine = StartCoroutine(BurrowRoutine());
	}

	private IEnumerator BurrowRoutine()
	{
		if (hopTween != null)
		{
			yield return hopTween.WaitForCompletion();
			hopTween.Kill();
			hopTween = null;
		}

		burrowTween = transform.DOJump(data.spawner.transform.position, Random.Range(0.5f, 1.25f), 1, 1f)
			.OnComplete(() => data.spawner.OnRabbitReturn(this));

		yield return burrowTween.WaitForCompletion();

		burrowTween = null;
	}
}
