using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SPGrowthController : PlantController 
{
	// spawn information
	[SerializeField] private List<GameObject> _spawnables = new List<GameObject>();
	protected List<Animator> _childAnimators = new List<Animator>();
	protected const int _maxMinisSpawned = 5;
	protected int _minisSpawned = 0;

	protected float _spawnRadius = 2.5f;
	protected const float _timeBetweenSpawns = 35.0f;

	protected float _innerMeshRadius = 2.0f;
	protected float _outerSpawnRadius = 2.5f;
	[SerializeField] const float _defaultInnerRadiusSize = 1.0f; // use for items you can't easily calculate the mesh size

	// plant n' growth vars
	[SerializeField] protected float _growthRate = 0.0f;
	[SerializeField] protected float _baseGrowthRate = .005f;
	[SerializeField] Vector2 _scaleRange = new Vector2( 8.0f, 12.0f ); // we want to let these vary per plant

	protected Rigidbody _rigidbody = null;

	void Awake()
	{
		_myPlant = GetComponent<BasePlant>();
		_controllerType = ControllerType.Growth;
	}

	public override void StartState()
	{
		InitPlant();
	}
		
	protected virtual void InitPlant()
	{
		_myPlant = GetComponent<BasePlant>();
		// get mesh radius
		GetSetMeshRadius();

		_growthRate = _baseGrowthRate;
		_rigidbody = GetComponent<Rigidbody>();

		AssignRandomSize();
		CalculateObjectRadius();

		float randoScale = Random.Range( _scaleRange.x, _scaleRange.y);
		transform.localScale = new Vector3( randoScale, randoScale, randoScale );

		PlantManager.ExecuteGrowth += UpdateState;
	}

	//********************************
	// UPDATE GROWTH FUNCTIONS
	//********************************

	public override void UpdateState()
	{
		CustomPlantGrowth();
	}

	protected virtual void CustomPlantGrowth(){}

	//********************************
	// STOP GROWTH FUNCTIONS
	//********************************

	public override void StopState()
	{
		CustomStopGrowth();
	}

	protected virtual void CustomStopGrowth()
	{
		foreach( Animator child in _childAnimators )
		{
			child.enabled = false;
		}

		PlantManager.instance.RequestSpawnMini( _myPlant, _timeBetweenSpawns );
	}

	//********************************
	// PLANT SPAWN FUNCTIONS
	//********************************

	public override GameObject SpawnChildPlant()
	{
		GameObject newPlant = null;

		if( _spawnables.Count != 0 )
		{
			//what kind of radius do i want
			Vector2 randomPoint = _myPlant.GetRandomPoint( _innerMeshRadius, _outerSpawnRadius );
			Vector3 spawnPoint = new Vector3( randomPoint.x, .1f, randomPoint.y ) + transform.position;
			Vector3 direction = ( spawnPoint - transform.position ).normalized * ( _innerMeshRadius );
			spawnPoint += direction;
			spawnPoint = new Vector3( spawnPoint.x, .1f, spawnPoint.z );

			newPlant = (GameObject)Instantiate( _spawnables[Random.Range( 0, _spawnables.Count)], spawnPoint, Quaternion.identity );
		}

		_minisSpawned++;

		if( _minisSpawned < _maxMinisSpawned )
		{
			PlantManager.instance.RequestSpawnMini( _myPlant, _timeBetweenSpawns );
		}

		if( newPlant == null )
		{
			Debug.Log("spawning minis plant messed up ");
		}

		return newPlant;
	}
				
	//********************************
	// INTERACTIONS
	//********************************

	public override void WaterPlant(){}
	public override void TouchPlant(){}
	public override void GrabPlant(){}
	public override void StompPlant(){}

	//********************************
	// HELPER FUNCTIONS
	//********************************
	void CalculateObjectRadius()
	{
		// PLEASE FILL 
	}

	void AssignRandomSize()
	{
		float randoScale = Random.Range( _scaleRange.x, _scaleRange.y);
		transform.localScale = new Vector3( randoScale, randoScale, randoScale );
	}

	protected virtual void GetSetMeshRadius()
	{
		MeshFilter meshFilter = GetComponentInChildren<MeshFilter>();

		if( meshFilter )
		{
			Mesh plantMesh = meshFilter.mesh;
			Vector3 size = plantMesh.bounds.size;

			if( size.x > size.z )
			{
				_innerMeshRadius = size.x * transform.GetChild(0).localScale.x;
			}
			else
			{
				_innerMeshRadius = size.z * transform.GetChild(0).localScale.x;
			}
		}
		else
		{
			_innerMeshRadius = _defaultInnerRadiusSize;
		}
	}
}
