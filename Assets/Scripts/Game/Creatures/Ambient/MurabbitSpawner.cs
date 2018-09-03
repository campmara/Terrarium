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

	private GameObject hole;

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
		spawnTimer += Time.unscaledDeltaTime;

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
		this.transform.position = JohnTech.GenerateRandomXZDirection() * Random.Range( 10.0f, 32.0f );

		int rabbitsToSpawn = Random.Range(3, 6);

		StartCoroutine(SpawnRoutine(rabbitsToSpawn));
	}

	private IEnumerator SpawnRoutine(int rabbitsToSpawn)
	{
		// Spawn the hole
		hole = Instantiate(holePrefab, transform.position, Quaternion.identity) as GameObject;
		hole.transform.parent = transform.parent;
		hole.transform.eulerAngles = new Vector3(90f, 0f, 0f);
		hole.transform.localScale = Vector3.zero;
		Tween holeTween = hole.transform.DOScale(Vector3.one, 0.75f).SetEase(Ease.Linear);

		yield return holeTween.WaitForCompletion();

		// Spawn the rabbits
		for (int i = 0; i < rabbitsToSpawn; i++)
		{
			// MAKE BABY
			GameObject rabObj = Instantiate(rabbitPrefab, transform.position, Quaternion.identity) as GameObject;
			rabObj.transform.parent = transform.parent;
			rabObj.name = "Rabbit " + i;
			Murabbit rab = rabObj.GetComponent(typeof(Murabbit)) as Murabbit;
			
			// TELL BABY HOW TO LIVE
			MurabbitData data = new MurabbitData(this, Random.Range(0.1f, 1f), Random.Range(40f, 80f));
			rab.Setup(data);

			// KEEP BABY IN YOUR CAREFUL WATCH
			rabbits.Add(rab);

			// WAIT
			// WE WAS JUST HANGIN
			yield return new WaitForSecondsRealtime(Random.Range(0.25f, 2f));
		}
	}

	public void OnRabbitReturn(Murabbit rab)
	{
		rabbits.Remove(rab);

		Destroy(rab.gameObject);

		if (rabbits.Count == 0)
		{
			// Get rid of the hole
			Tween holeTween = hole.transform.DOScale(Vector3.zero, 0.75f).SetEase(Ease.Linear).OnComplete(() => OnAllRabbitsReturned());
		}
	}

	private void OnAllRabbitsReturned()
	{
		Destroy(hole);
		hole = null;

		SetState(SpawnState.COUNT);
	}
}
