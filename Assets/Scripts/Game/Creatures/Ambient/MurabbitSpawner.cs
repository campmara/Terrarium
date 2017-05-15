using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MurabbitData
{
	public MurabbitSpawner spawner;
	public float scale;
	public float lifetime;

	public MurabbitData(MurabbitSpawner spawner,
						float scale = 1f, 
						float lifetime = 20f)
	{
		this.spawner = spawner;
		this.scale = scale;
		this.lifetime = lifetime;
	}
}

public class MurabbitSpawner : MonoBehaviour 
{
	[SerializeField] private GameObject holePrefab;
	[SerializeField] private GameObject rabbitPrefab;

	// Rabbits all leave the hole. Once they are all done and burrowed in the ground, we set the state to countdown and wait to spawn them again.
	enum SpawnState
	{
		COUNT = 0,
		SPAWN,
		MAX
	}
	[SerializeField, ReadOnly] private SpawnState state;

	private List<Murabbit> rabbits;

	[SerializeField, ReadOnly] private float countdownTime = 0f;
	[SerializeField, ReadOnly] private float spawnTimer = 0f;

	private void Awake()
	{
		rabbits = new List<Murabbit>();

		SetState(SpawnState.COUNT);
	}

	private void Update()
	{
		if (state == SpawnState.COUNT)
		{
			HandleCountdown();
		}
		else if (state == SpawnState.SPAWN)
		{
			HandleSpawning();
		}
	}

	private void SetState(SpawnState next)
	{
		switch(next)
		{
			case SpawnState.COUNT:
				OnEnterCountdown();
				break;
			case SpawnState.SPAWN:
				OnEnterSpawning();
				break;
			default:
				break;
		}

		state = next;
	}

	/*
		COUNTDOWN
	*/
	private void OnEnterCountdown()
	{
		countdownTime = Random.Range(10f, 50f);
		spawnTimer = 0f;
	}

	private void HandleCountdown()
	{
		spawnTimer += Time.deltaTime;

		if (spawnTimer >= countdownTime)
		{
			SetState(SpawnState.SPAWN);
			spawnTimer = 0f;
		}
	}

	/*
		SPAWNING
	*/
	private void OnEnterSpawning()
	{
		Vector3 pos = Random.insideUnitSphere * 32f;
		pos.y = 0f;
		transform.position = pos;

		int rabbitsToSpawn = Random.Range(2, 6);

		StartCoroutine(SpawnRoutine(rabbitsToSpawn));
	}

	private IEnumerator SpawnRoutine(int rabbitsToSpawn)
	{
		// Spawn the hole
		GameObject hole = Instantiate(holePrefab, transform.position, Quaternion.identity) as GameObject;
		hole.transform.parent = transform.parent;
		hole.transform.localScale = Vector3.zero;
		Tween holeTween = hole.transform.DOScale(Vector3.one, 0.75f).SetEase(Ease.Linear);

		yield return holeTween.WaitForCompletion();

		// Spawn the rabbits
		for (int i = 0; i < rabbitsToSpawn; i++)
		{
			// MAKE BABY
			GameObject rabObj = Instantiate(rabbitPrefab, transform.position, Quaternion.identity) as GameObject;
			rabObj.transform.parent = transform.parent;
			Murabbit rab = rabObj.GetComponent(typeof(Murabbit)) as Murabbit;
			
			// TELL BABY HOW TO LIVE
			MurabbitData data = new MurabbitData(this, Random.Range(0.1f, 1f), Random.Range(40f, 80f));
			rab.Setup(data);

			// KEEP BABY IN YOUR CAREFUL WATCH
			rabbits.Add(rab);

			// WAIT
			// WE WAS JUST HANGIN
			yield return new WaitForSeconds(Random.Range(0.25f, 2f));
		}

		// Get rid of the hole
		holeTween = hole.transform.DOScale(Vector3.zero, 0.75f).SetEase(Ease.Linear).OnComplete(() => Destroy(hole));
	}

	private void HandleSpawning()
	{
		if (rabbits.Count <= 0)
		{
			// Time to return to the countdown.
			SetState(SpawnState.COUNT);
		}
	}

	public void OnRabbitReturn(Murabbit rab)
	{
		rabbits.Remove(rab);

		Destroy(rab.gameObject);
	}
}
