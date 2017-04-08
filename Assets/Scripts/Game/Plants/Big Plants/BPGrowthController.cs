﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BPGrowthController : PlantController
{
	//****************
	// ANIMATION VARIABLES
	//****************

	protected Animator _plantAnim = null;
	protected List<Animator> _childAnimators = new List<Animator>();
	float [] growthTime = new float[4]; // time splits initialized based on our animation

	[SerializeField] protected float _baseGrowthRate = 0.0f;
	[SerializeField] protected Vector2 _scaleMultiplierRange = new Vector2( 3.0f, 18.0f );
	[SerializeField] protected List<Vector2> _scaleRatios = new List<Vector2>();
	[SerializeField] protected float _wateredGrowthRate = 0.0f;

	protected float _growthRate = 0.0f;
	protected float _animEndTime = 0.0f;
	protected float _curPercentAnimated = 0.0f;
	protected float _maxHeight = 0.0f;
	protected float _maxWidth = 0.0f;
	protected const float WATERED_GROWTHRETURNTIME = 10.0f;

	//****************
	// GROWTH VARIABLES
	//****************

	const float _numGrowStages = 3;
	[SerializeField] GrowthStage _curStage = GrowthStage.Seed;
	public GrowthStage CurStage { get { return _curStage; } }

	float [] _neededDistance = new float[] { 4.0f, 5.0f, 8.5f, 15.0f }; // how much room each stage need to grow, first element doesnt matter
	float [] _spawnRadii = new float[] { 3.5f, 4.0f, 4.5f, 5.0f };  
	bool _hardStopGrowth = false;

	public enum GrowthStage : int
	{
		Seed = -1,
		Sprout = 0,
		GrowingSprout= 1,
		Sapling = 2,
		Final = 3
	};

	//****************
	// SPAWN VARIABLES
	//****************

	//fruits
	[SerializeField] List<GameObject> _droppingItems = null;  

	const float _timeBetweenFruitDrops = 30.0f;
	protected const float _timeBetweenSpawns = 5.0f;

	// small plants
	public List<GameObject> _smallSpawnables = new List<GameObject>();
	public List<GameObject> _mediumSpawnables = new List<GameObject>();

	[SerializeField] Vector2 _smallSpawnRadRange = new Vector2( 1f, 3f );
	[SerializeField] Vector2 _numSmallPlants = new Vector2( 8, 15 );
	[SerializeField] Vector2 _mediumSpawnRadRange = new Vector2( 3f, 4.5f );
	[SerializeField] Vector2 _numMedPlants = new Vector2( 4, 6 );

	int _spawnedMediums = 0;
	public int SpawnedMediums { get { return _spawnedMediums; } set { _spawnedMediums = value; }}
	int _spawnedSmalls = 0;
	public int SpawnedSmalls { get { return _spawnedSmalls; } set { _spawnedSmalls = value; }}
	int _maxSmalls = 0;
	int _maxMediums = 0;

	public SpawningState spawnState = SpawningState.NotSpawning;

	public enum SpawningState
	{
		NotSpawning,
		SmallSpawning,
		MediumSpawning
	};

	// ambient creatures
	protected const float CREATURE_BASE_SPAWNODDS = 0.5f;	// Higher number is LESS LIKELY ( checks if random val is greater than )
	protected const float CREATURE_BASE_SPAWNY = 1.0f;

	public override void Init()
	{
		_controllerType = ControllerType.Growth;

		Vector2 ratio = _scaleRatios[ Random.Range( 0, _scaleRatios.Count - 1 ) ];
		float multiplier = Random.Range( _scaleMultiplierRange.x, _scaleMultiplierRange.y);
		_maxHeight = ratio.y * multiplier;
		_maxWidth = ratio.x * multiplier;

		_maxSmalls = (int)Random.Range( _numSmallPlants.x, _numSmallPlants.y );
		_maxMediums = (int)Random.Range( _numMedPlants.x, _numMedPlants.y );
	}

	public override void StartState()
	{
		_myPlant = GetComponent<BasePlant>();
		_growthRate = _baseGrowthRate;
		_curStage = GrowthStage.Seed;

		GetSetMeshRadius();
		InitPlant();
		DetermineGrowth();	
	}

	protected virtual void InitPlant()
	{
		_plantAnim = _myPlant.GetComponent<Animator>();
		_plantAnim.enabled = true;
		_animEndTime = _plantAnim.GetCurrentAnimatorStateInfo(0).length;
		_plantAnim.speed = _baseGrowthRate;

		SetGrowthTransitionPoints();
	}
		
	void DetermineGrowth()
	{
		if( !IsOverlappingPlants() )
		{
			_curStage = GrowthStage.Sprout;
			_myPlant.OuterRadius = _spawnRadii[ (int)_curStage ];
		}
		else
		{
			_hardStopGrowth = true;
			StopState();
		}
	}

	protected virtual void SetGrowthTransitionPoints()
	{
		float _duration = 1.0f / ( _numGrowStages + 1 );
		for( int i = 0; i < _numGrowStages + 1; i++ )
		{
			growthTime[i] = _duration * (i+1);
		}
	}

	#region Growth Update Functions
	//********************************
	// PLANT GROWTH UPDATE FUNCTIONS
	//********************************
	public override void UpdateState()
	{
		if( _curStage != GrowthStage.Final )
		{
			_curPercentAnimated = _plantAnim.GetCurrentAnimatorStateInfo(0).normalizedTime / _animEndTime;
			if( _curPercentAnimated >= growthTime[ (int)_curStage ] )
			{
				TryTransitionStages();
			}
			else
			{
				CustomPlantGrowth();
				UpdateScale();
			}
		}
	}

	void UpdateScale()
	{
		float width = Mathf.Lerp( 1.0f, _maxWidth, _curPercentAnimated );
		transform.localScale = new Vector3( width, Mathf.Lerp( 1.0f, _maxHeight, _curPercentAnimated ), width );
	}

	protected virtual void CustomPlantGrowth(){}

	void TryTransitionStages()
	{
		if( IsOverlappingPlants() )
		{
			StopState();
		}
		else
		{
			SwitchToNextStage();
		}
	}

	bool IsOverlappingPlants()
	{
		Collider[] overlappingObjects = Physics.OverlapSphere( _myPlant.transform.position, _neededDistance[ (int)_curStage + 1 ] ); 

		if( overlappingObjects.Length != 0 )
		{
			foreach( Collider col in overlappingObjects )
			{
				BPGrowthController otherPlant = col.GetComponent<BPGrowthController>();
				if( otherPlant && col.gameObject != _myPlant.gameObject )
				{
					if( FillsOverlapCondition( otherPlant ) )
					{
						return true;
					}
				}
			}
		}

		return false;
	}  

	bool FillsOverlapCondition( BPGrowthController other )
	{
		if( (int)other.CurStage > ( (int)_curStage ) || other.CurStage == GrowthStage.Final )
		{
			return true;
		} 

		return false;
	}

	void SwitchToNextStage()
	{   		
		if( _curStage != GrowthStage.Final )
		{
			_curStage += 1;
		}

		if( _curStage == GrowthStage.Sapling )
		{
			SpawnPlant();
		}

		if( _curStage == GrowthStage.Final )
		{
			PlantManager.instance.RequestDropFruit( this, _timeBetweenFruitDrops );
			SpawnAmbientCreature();
			StopState();
		}

		ChangeGrowthRate( _baseGrowthRate );
		UpdateNewStageData();
	}
	#endregion Growth Update Functions

	public override void StopState()
	{
		if( _hardStopGrowth )
		{
			HardStopAnim();
		}
		else
		{
			CustomStopGrowth();
		}
	}

	protected virtual void CustomStopGrowth()
	{
		HardStopAnim();
	}

	protected void HardStopAnim()
	{
		if( _plantAnim )
		{
			_plantAnim.enabled = false;
		}

		foreach( Animator child in _childAnimators )
		{
			child.enabled = false;
		}

		_myPlant.SwitchController( this );
	}

	#region Spawn Functions
	//********************************
	// PLANT SPAWN FUNCTIONS
	//********************************

	public void SpawnPlant()
	{
		if( spawnState == SpawningState.NotSpawning && _spawnedSmalls <= 0 )
		{
			spawnState = SpawningState.SmallSpawning;
			PlantManager.instance.RequestSpawnMini( this, _timeBetweenSpawns );
		}
		else if( spawnState == SpawningState.SmallSpawning )
		{
			if( _spawnedSmalls >= _maxSmalls )
			{
				spawnState = SpawningState.MediumSpawning;
			}
			
			PlantManager.instance.RequestSpawnMini( this, _timeBetweenSpawns );
		}
		else if( spawnState == SpawningState.MediumSpawning )
		{
			if( _spawnedMediums >= _maxMediums )
			{
				spawnState = SpawningState.NotSpawning;
			}
			else
			{
				PlantManager.instance.RequestSpawnMini( this, _timeBetweenSpawns );
			}
		}
	}


	public override GameObject DropFruit()
	{
		Vector2 randomPoint = GetRandomPoint(true);
		Vector3 spawnPoint = new Vector3( randomPoint.x, _myPlant.SpawnHeight, randomPoint.y ) + transform.position;
		GameObject newPlant = (GameObject)Instantiate( _droppingItems[Random.Range( 0, _droppingItems.Count)], spawnPoint, Quaternion.identity );  

		Seed seed = newPlant.GetComponent<Seed>();
		if( seed )
		{
			PlantManager.instance.AddSeed( seed );
		}

		PlantManager.instance.RequestDropFruit( this, _timeBetweenFruitDrops );

		if( newPlant == null )
		{
			Debug.Log("dropping seed plant messed up ");
		}

		return newPlant;
	}

	public virtual void SpawnAmbientCreature()
	{
		if (Random.value > CREATURE_BASE_SPAWNODDS)
		{
			CreatureManager.instance.SpawnRandomCreature( this.transform.position + ( Vector3.up * ( CREATURE_BASE_SPAWNY + Random.Range( -2.0f, 2.0f ) ) ) );
		}
	}

	#endregion Spawn Functions

	public override void WaterPlant()
	{
		ChangeGrowthRate( _wateredGrowthRate );

		if( _myPlant.GrowReturnRoutine != null )
		{
			StopCoroutine( _myPlant.GrowReturnRoutine );
		}

		_myPlant.GrowReturnRoutine = StartCoroutine( DelayedReturnGrowthRate( WATERED_GROWTHRETURNTIME ) );
	}

	public override void TouchPlant(){}
	public override void GrabPlant(){}
	public override void StompPlant(){}

	#region Helper Functions
	//********************************
	// HELPER FUNCTIONS
	//********************************

	IEnumerator DelayedReturnGrowthRate( float returnTime )
	{
		yield return new WaitForSeconds( returnTime );

		ChangeGrowthRate( _baseGrowthRate );
	}

	void ChangeGrowthRate( float newRate )
	{
		_plantAnim.speed = newRate;

		foreach( Animator child in _childAnimators )
		{
			child.speed = newRate;
		}
	}
		
	void UpdateNewStageData()
	{
		_myPlant.OuterRadius = _spawnRadii[ (int)_curStage ]; // this is a greater value to manage how big things grow
		GetSetMeshRadius();

		_growthRate = _baseGrowthRate;
		_plantAnim.speed = _growthRate;
		_myPlant.SpawnHeight = _myPlant.transform.GetChild(1).GetComponent<SkinnedMeshRenderer>().bounds.size.y * ( _numGrowStages + 1 );
	}

	protected void GetSetMeshRadius()
	{
		// get our stem
		Vector3 size = _myPlant.transform.GetChild(1).GetComponent<SkinnedMeshRenderer>().bounds.size;

		// take biggest component as radius size 
		_myPlant.InnerRadius = size.x > size.z ? size.x : size.z;
		_myPlant.InnerRadius *= .5f * _maxWidth;
	}

	void OnDrawGizmos() 
	{
		Gizmos.color = Color.yellow;
		if( (int) _curStage > -1 )
		{
//			Gizmos.DrawWireSphere( _myPlant.transform.position, _neededDistance[(int) _curStage ] );
		}
	}
	public Vector2 GetRandomPoint( bool fruitDrop = false )
	{
		Vector2 randomPoint = Random.insideUnitCircle;
		float inner = 1.0f;
		float outer = 2.0f;

		if( fruitDrop )
		{
			inner = _myPlant.InnerRadius;
			outer = _myPlant.OuterRadius;
		}
		else if( spawnState == SpawningState.SmallSpawning )
		{
			inner = _smallSpawnRadRange.x;
			outer = _smallSpawnRadRange.y;
		}
		else if( spawnState == SpawningState.MediumSpawning )
		{
			inner = _mediumSpawnRadRange.x;
			outer = _mediumSpawnRadRange.y;
		}
		
		float xRand = Random.Range( inner, outer );
		float yRand = Random.Range( inner, outer );
		randomPoint = new Vector2( Mathf.Sign( randomPoint.x ) * xRand +  randomPoint.x, randomPoint.y + yRand * Mathf.Sign( randomPoint.y ) );
	
		return randomPoint;
	}

	public void ForceSpawnMediums()
	{
		_spawnedSmalls = _maxSmalls;
	}

	#endregion Helper Functions
}
