using System.Collections;
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
	[SerializeField] protected float _wateredGrowthRate = 0.0f;
	protected float _growthRate = 0.0f;
	protected float _animEndTime = 0.0f;
	protected float _curPercentAnimated = 0.0f;
	protected const float WATERED_GROWTHRETURNTIME = 10.0f;

	//****************
	// GROWTH VARIABLES
	//****************

	const float _numGrowStages = 3;
	float _innerMeshRadius = 0.0f;
	protected float _outerSpawnRadius = 2.5f;

	[SerializeField] GrowthStage _curStage = GrowthStage.Seed;
	public GrowthStage CurStage { get { return _curStage; } }

	float [] _neededDistance = new float[] { 4.0f, 8.0f, 10.0f, 12.0f }; // how much room each stage need to grow, first element doesnt matter
	float [] _spawnRadii = new float[] { 4.0f, 6.0f, 8.0f, 11.0f };  
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
	float _fruitDropHeight = 8.0f;
	const float _timeBetweenFruitDrops = 30.0f;
	protected const float _timeBetweenSpawns = 15.0f;

	// small plants
	[SerializeField] List<GameObject> _spawnables = new List<GameObject>();
	protected const int _maxMinisSpawned = 5;
	protected int _minisSpawned = 0;

	// ambient creatures
	protected const float CREATURE_BASE_SPAWNODDS = 0.5f;	// Higher number is LESS LIKELY ( checks if random val is greater than )
	protected const float CREATURE_BASE_SPAWNY = 1.0f;

	public override void Init()
	{
		_myPlant = GetComponent<BasePlant>();
		_controllerType = ControllerType.Growth;
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
			_outerSpawnRadius = _spawnRadii[ (int)_curStage ];
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
			}
		}
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
			PlantManager.instance.RequestSpawnMini( this, _timeBetweenSpawns );
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

	public override GameObject DropFruit()
	{
		Vector2 randomPoint = _myPlant.GetRandomPoint( _innerMeshRadius, _outerSpawnRadius );
		Vector3 spawnPoint = new Vector3( randomPoint.x, _fruitDropHeight, randomPoint.y ) + transform.position;

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

	public override GameObject SpawnChildPlant()
	{
		GameObject newPlant = null;

		if ( _myPlant != null )
		{
			if( _spawnables.Count != 0 )
			{
				Vector2 randomPoint = _myPlant.GetRandomPoint( _innerMeshRadius, _outerSpawnRadius );
				Vector3 spawnPoint = new Vector3( randomPoint.x, .1f, randomPoint.y ) + transform.position;
				spawnPoint = new Vector3( spawnPoint.x, .05f, spawnPoint.z );

				newPlant = (GameObject)Instantiate( _spawnables[Random.Range( 0, _spawnables.Count)], spawnPoint, Quaternion.identity );
			}

			_minisSpawned++;

			if( _minisSpawned < _maxMinisSpawned )
			{
				PlantManager.instance.RequestSpawnMini( this, _timeBetweenSpawns );
			}

			if( newPlant == null )
			{
				Debug.Log("spawning minis plant messed up ");
			}
		}
		else
		{
			Debug.Log("Tryna Spawn Mini WHile Destroyed");
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
		_outerSpawnRadius = _spawnRadii[ (int)_curStage ]; // this is a greater value to manage how big things grow
		GetSetMeshRadius();

		_growthRate = _baseGrowthRate;
		_plantAnim.speed = _growthRate;
		_fruitDropHeight = _myPlant.transform.GetChild(1).GetComponent<SkinnedMeshRenderer>().bounds.size.y * ( _numGrowStages + 1 );
	}

	protected void GetSetMeshRadius()
	{
		// get our stem
		Vector3 size = _myPlant.transform.GetChild(1).GetComponent<SkinnedMeshRenderer>().bounds.size;

		// take biggest component as radius size 
		_innerMeshRadius = size.x > size.z ? size.x : size.z;
		_innerMeshRadius *= .5f;
	}

	void OnDrawGizmos() 
	{
		Gizmos.color = Color.yellow;
		if( (int) _curStage > -1 )
		{
//			Gizmos.DrawWireSphere( _myPlant.transform.position, _neededDistance[(int) _curStage ] );
		}
	}
	#endregion Helper Functions
}
