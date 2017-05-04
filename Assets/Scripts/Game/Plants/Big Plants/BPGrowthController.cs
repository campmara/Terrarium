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
	[SerializeField] protected Vector2 _scaleMultiplierRange = new Vector2( 3.0f, 18.0f );
	[SerializeField] protected List<Vector2> _scaleRatios = new List<Vector2>(){ Vector2.one };
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
	bool _stemDoneGrowing = false;
	float _origScale = 1.0f;

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
	[SerializeField] GameObject _punishObject = null;
	const float _timeBetweenFruitDrops = 30.0f;
	const float _timeBetweenSummonDrops = 20.0f;
	const float _timeBetweenGoodSummonDrops = 5.0f;
	protected const float _timeBetweenSpawns = 3.0f;

	const int _maxSeedDrops = 3; // 3 seeds before you get rocks 
	bool _droppedRock = false;
	int _seedsSummoned = 0;
	float _summonTimer = 0.0f;

	// small plants
	public List<GameObject> _smallSpawnables = new List<GameObject>();
	public List<GameObject> _mediumSpawnables = new List<GameObject>();

	int _spawnedMediums = 0;
	public int SpawnedMediums { get { return _spawnedMediums; } set { _spawnedMediums = value; }}
	int _spawnedSmalls = 0;
	public int SpawnedSmalls { get { return _spawnedSmalls; } set { _spawnedSmalls = value; }}
	int _maxSmalls = 0;
	int _maxMediums = 0;
	
	bool _summoningSeed = false;

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

		Vector2 ratio = _scaleRatios[ Random.Range( 0, _scaleRatios.Count ) ];
		float multiplier = Random.Range( _scaleMultiplierRange.x, _scaleMultiplierRange.y);
		_maxHeight = ratio.y * multiplier;
		_maxWidth = ratio.x * multiplier;

		_maxSmalls = (int)Random.Range( PlantManager.instance.SmNumPerPlant.x, PlantManager.instance.SmNumPerPlant.y );
		_maxMediums = (int)Random.Range( PlantManager.instance.MedNumPerPlant.x, PlantManager.instance.MedNumPerPlant.y );

		Collider[] cols = Physics.OverlapSphere( transform.position, 1.0f );
		foreach( Collider col in cols)
		{
			if( col.GetComponent<PondTech>() || col.GetComponent<Rock>() )
			{
				PlantManager.instance.DeleteLargePlant( GetComponent<BasePlant>() );
				break;
			}
		}

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
	
	public void UpdateToMoundScale( float moundScale )
	{
		_origScale = moundScale;
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
			if( _plantAnim.GetCurrentAnimatorStateInfo(0).normalizedTime >= growthTime[ (int)_curStage ] && !_stemDoneGrowing )
			{
				TryTransitionStages();
			}
			else
			{
				CustomPlantGrowth();
				BaseUpdate();
			}
	}

	void UpdateScale()
	{
		float width = Mathf.Lerp( _origScale, _maxWidth, _curPercentAnimated );
		transform.localScale = new Vector3( width, Mathf.Lerp( _origScale, _maxHeight, _curPercentAnimated ), width );
	}

	void UpdateSummonsData()
	{
		if( _maxSeedDrops <= _seedsSummoned )
		{
			if( _summonTimer >= _timeBetweenGoodSummonDrops )
			{
				_seedsSummoned = 0;
				_summonTimer = 0.0f;
				_droppedRock = false;
			}
			else
			{
				_summonTimer += Time.deltaTime;
			}
		} 
	}
	void BaseUpdate()
	{
		_curPercentAnimated = _plantAnim.GetCurrentAnimatorStateInfo(0).normalizedTime;
		UpdateScale();
		UpdateSummonsData();
	}

	protected virtual void CustomPlantGrowth(){}

	void TryTransitionStages()
	{
		if( _curStage != GrowthStage.Final && IsOverlappingPlants() )
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
					if( FillsOverlapCondition( otherPlant ) || col.GetComponent<PondTech>() )
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
			_stemDoneGrowing = true;
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
		GameObject newPlant = SpawnFruit( Vector2.zero, null );

		PlantManager.instance.RequestDropFruit( this, _timeBetweenFruitDrops );

		return newPlant;
	}

	GameObject SpawnFruit( Vector2 playerPos, GameObject obj )
	{
		Vector3 spawnPoint;
		if( playerPos != Vector2.zero )
		{
			spawnPoint = new Vector3( playerPos.x, _myPlant.SpawnHeight, playerPos.y );
		}
		else
		{
			Vector2 randomPoint = GetRandomPoint(true);
			spawnPoint = new Vector3( randomPoint.x, _myPlant.SpawnHeight - 1.0f, randomPoint.y ) + transform.position;
		}

		if( !obj )
		{
			obj = _droppingItems[Random.Range( 0, _droppingItems.Count)];
		}
		
		GameObject newPlant = (GameObject)Instantiate( obj, spawnPoint, Quaternion.identity );  

		Seed seed = newPlant.GetComponent<Seed>();

		if( seed )
		{
			PlantManager.instance.AddSeed( seed );
		}

		if( newPlant == null )
		{
			Debug.Log("dropping seed plant messed up ");
		}

		return newPlant;

	}

	public void SummonSeed( Vector2 playerPos)
	{
		if( !_summoningSeed && _curStage == GrowthStage.Final )
		{
			// if you've used too many summons, summon a rock
			if( _seedsSummoned >= _maxSeedDrops )
			{
				if( !_droppedRock )
				{
					StartCoroutine( SeedSummonCooldown( playerPos, _punishObject ) );
					_droppedRock = true;
				}

				_summonTimer = 0.0f;

			}
			else
			{
				StartCoroutine( SeedSummonCooldown( playerPos, null ) );
				_seedsSummoned++;
			}
		}
	}

	IEnumerator SeedSummonCooldown( Vector2 playerPos, GameObject summonObj )
	{
		_summoningSeed = true;
		SpawnFruit( playerPos, summonObj );
		yield return new WaitForSeconds( _timeBetweenSpawns );
		_summoningSeed = false;
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
		_myPlant.SpawnHeight = _myPlant.SpawnHeight > 20.0f ? 20.0f : _myPlant.SpawnHeight;
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
			inner = PlantManager.instance.SmSpawnRadRange.x;
			outer = PlantManager.instance.SmSpawnRadRange.y;
		}
		else if( spawnState == SpawningState.MediumSpawning )
		{
			inner = PlantManager.instance.MedSpawnRadRange.x;
			outer = PlantManager.instance.MedSpawnRadRange.y;
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
