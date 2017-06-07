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
		FOLLOW_CHECK,
		FOLLOW_HOP,
		BURROW,
		MAX
	}
	[SerializeField, ReadOnly] private State state = State.NULL;

	private Coroutine burrowRoutine;
	private Tween hopTween;
	private Tween escapeTween;
	private Tween followTween;
	private Tween burrowTween;

	private const float HOP_DURATION = 0.6f;
	private const float MAX_SCARY_DISTANCE = 5f;
	private const float MAX_GESTURE_DISTANCE = 10f;

	private float sqrDistFromPlayer = 0f;

	private float returnTimer = 0f;
	private float returnTime = 0f;

	private float idleTimer = 0f;
	private float idleTime = 0f;

    AudioSource _source = null;
    [SerializeField]
    List<AudioClip> _hopClipList = new List<AudioClip>();
    [SerializeField]
    List<AudioClip> _landClipList = new List<AudioClip>();

    void Awake()
    {
        _source = this.GetComponent<AudioSource>();
    }

	public void Setup(MurabbitData data)
	{
		this.data = data;

		transform.localScale = new Vector3(data.scale, data.scale, data.scale);

		SetState(State.SPAWN);
	}

	private void Update()
	{
		if (state == State.BURROW)
		{
			return;
		}

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
        if( state == State.HOP || state == State.FOLLOW_HOP || state == State.ESCAPE_HOP )
        {
            PlayLandSound();
        }

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
			case State.FOLLOW_CHECK:
				OnEnterFollowCheck();
				break;
			case State.FOLLOW_HOP:
				OnEnterFollowHop();
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
		transform.LookAt(jumpPos);

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

		if (CheckForFollow())
		{
			SetState(State.FOLLOW_HOP);
		}
		else if (CheckForEscape())
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
		transform.LookAt(jumpPos);

		hopTween = transform.DOJump(jumpPos, Random.Range(0.5f, 1.25f), 1, HOP_DURATION)
			.OnComplete(() => SetState(State.IDLE));

        PlayHopSound();
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

        PlayHopSound();
    }

	private bool CheckForEscape()
	{
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
		FOLLOWING
	*/

	private void OnEnterFollowCheck()
	{
		if (CheckForFollow())
		{
			SetState(State.HOP);
		}
		else
		{
			SetState(State.IDLE);
		}
	}

	private void OnEnterFollowHop()
	{
		Vector3 diff = PlayerManager.instance.Player.transform.position - transform.position;
		diff.y = 0f;
		diff = diff.normalized;
		Vector3 jumpPos = transform.position + (diff * Random.Range(1.75f, 2.5f));
		jumpPos.y = 0f;
		transform.LookAt(jumpPos);

		followTween = transform.DOJump(jumpPos, Random.Range(0.5f, 1.25f), 1, HOP_DURATION)
			.OnComplete(() => SetState(State.FOLLOW_CHECK));

        PlayHopSound();
    }

	private bool CheckForFollow()
	{
		// TODO: This.
		PlayerIKControl ikRef = PlayerManager.instance.Player.GetComponent<RollerController>().IK;
		if (ikRef.LeftArm.ArmState != PlayerArmIK.ArmIKState.GESTURING &&
			ikRef.RightArm.ArmState != PlayerArmIK.ArmIKState.GESTURING)
		{
			return false;
		}

		sqrDistFromPlayer = (PlayerManager.instance.Player.transform.position - transform.position).sqrMagnitude;
		if (sqrDistFromPlayer <= MAX_GESTURE_DISTANCE * MAX_GESTURE_DISTANCE)
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
		returnTime = Mathf.Infinity;

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

		if (followTween != null)
		{
			yield return followTween.WaitForCompletion();
			followTween.Kill();
			followTween = null;
		}

		Vector3 spawnerPos = data.spawner.transform.position;
		spawnerPos.y = 0f; // just insurance
		int numJumps = Mathf.CeilToInt((spawnerPos - transform.position).magnitude / 2f);
		transform.LookAt(spawnerPos);

		burrowTween = transform.DOJump(spawnerPos, Random.Range(0.5f, 1.25f), numJumps, HOP_DURATION * (float)numJumps)
			.OnComplete(() => transform.DOScale(0f, 1f)
			.OnComplete(() => data.spawner.OnRabbitReturn(this)));

        PlayHopSound();

        yield return burrowTween.WaitForCompletion();

		burrowTween = null;
	}

    void PlayHopSound()
    {
        _source.clip = _hopClipList[Random.Range( 0, _hopClipList.Count)];
        _source.Play();
    }

    void PlayLandSound()
    {
        _source.clip = _landClipList[Random.Range( 0, _landClipList.Count )];
        _source.Play();
    }
}
