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
	private Tween jumpTween;

	private const float MAX_SCARY_SQR_DISTANCE = 25f;
	private float sqrDistFromPlayer = 0f;

	public void Setup(MurabbitData data)
	{
		this.data = data;

		transform.localScale = new Vector3(data.scale, data.scale, data.scale);

		SetState(State.SPAWN);
	}

	private void Update()
	{
		if (state == State.SPAWN)
		{

		}
		else if (state == State.HOP)
		{

		}
		else if (state == State.ESCAPE)
		{

		}
		else if (state == State.BURROW)
		{

		}
	}

	private void SetState(State next)
	{
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

		state = next;
	}

	/*
		SPAWNING
	*/
	private void OnEnterSpawn()
	{
		Vector3 jumpPos = transform.position + (Random.insideUnitSphere * 2f);
		jumpPos.y = 0f;

		jumpTween = transform.DOJump(jumpPos, 2f, 1, 0.5f).OnComplete(() => SetState(State.HOP));
	}

	/*
		HOPPING
	*/
	private void OnEnterHopping()
	{
		//StopCoroutine(escapeRoutine);
		//jumpTween.Complete();

		hopRoutine = StartCoroutine(HopRoutine());
	}

	private IEnumerator HopRoutine()
	{
		yield return new WaitForSeconds(Random.Range(0.5f, 3f));

		Vector3 jumpPos = transform.position + (Random.insideUnitSphere * 2f);
		jumpPos.y = 0f;

		jumpTween = transform.DOJump(jumpPos, Random.Range(0.5f, 1.25f), 1, 0.5f);

		yield return jumpTween.WaitForCompletion();

		hopRoutine = StartCoroutine(HopRoutine());
	}

	private void HandleHopping()
	{
		sqrDistFromPlayer = (PlayerManager.instance.Player.transform.position - transform.position).sqrMagnitude;

		if (sqrDistFromPlayer < MAX_SCARY_SQR_DISTANCE)
		{
			// Scarem
			// SetState(State.ESCAPE);
		}
	}

	/*
		ESCAPING
	*/
	private void OnEnterEscaping()
	{
		StopCoroutine(hopRoutine);
		jumpTween.Complete();

		escapeRoutine = StartCoroutine(EscapeRoutine());
	}

	private IEnumerator EscapeRoutine()
	{
		yield return null;
	}

	/*
		BURROWING
	*/
	private void OnEnterBurrowing()
	{

	}
}
