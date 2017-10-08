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
	protected Coroutine _changeGrowthRateRoutine = null;
	private const float GROWTHRATE_CHANGESPEED = 0.75f;
	protected float _animEndTime = 0.0f;
	[SerializeField, ReadOnly]protected float _curPercentAnimated = 0.0f;
    public float CurPercentAnimated { get { return _curPercentAnimated; } }
	protected float _maxHeight = 0.0f;
	protected float _maxWidth = 0.0f;
	protected const float WATERED_GROWTHRETURNTIME = 10.0f;

	//****************
	// GROWTH VARIABLES
	//****************

	const float _numGrowStages = 3;
	[SerializeField] GrowthStage _curStage = GrowthStage.Seed;
	public GrowthStage CurStage { get { return _curStage; } }

	float [] _neededDistance = new float[] { 4.0f, 5.0f, 6.5f, 8.0f }; // how much room each stage need to grow, first element doesnt matter
	float [] _spawnRadii = new float[] { 3.5f, 4.5f, 5.0f, 5.5f };  
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
	[SerializeField] Transform _heighestLeaf = null;
	[SerializeField] List<GameObject> _droppingItems = null;  
	[SerializeField] GameObject _punishObject = null;
	const float _timeBetweenFruitDrops = 17.0f;
	const float _timeBetweenSummonDrops = 20.0f;
	const float _timeBetweenGoodSummonDrops = 5.0f;
	protected const float _timeBetweenSpawns = 2.0f;
	MinMax _seedDropForce;

	const int _maxSeedDrops = 2; // 2 seeds before you get rocked
	bool _droppedRock = false;
	int _seedsSummoned = 0;
	float _summonTimer = 0.0f;

	// small plants
	public GameObject _coverSpawnerPrefab = null;
	//public List<GameObject> _smallSpawnables = new List<GameObject>();
	public List<GameObject> _mediumSpawnables = new List<GameObject>();

	int _spawnedMediums = 0;
	public int SpawnedMediums { get { return _spawnedMediums; } set { _spawnedMediums = value; }}
	int _maxMediums = 0;
	
	bool _summoningSeed = false;

	public SpawningState SpawnState = SpawningState.NotSpawning;

    PlantAudioController _audioController = null;
    public PlantAudioController pAudioController { get { return _audioController; } }

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

		_maxMediums = (int)Random.Range( PlantManager.instance.MedNumPerPlant.Min, PlantManager.instance.MedNumPerPlant.Max );

        _audioController = this.GetComponentInChildren<PlantAudioController>();

		DetermineSeedDropForce();
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
		bool conflictingObject = false;
		Collider[] cols = Physics.OverlapSphere( transform.position, _neededDistance[0] );
		foreach( Collider col in cols)
		{
			if( col.GetComponent<PondTech>() || col.GetComponentInParent<RockTag>() )
			{
				conflictingObject = true;
				break;
			}
		}

		// if something is in the way just destroy it
		if( conflictingObject || IsOverlappingPlants() )
		{
			PlantManager.instance.DeleteLargePlant( GetComponent<BasePlant>() );
		}
		else
		{
			_curStage = GrowthStage.Sprout;
			_myPlant.OuterRadius = _spawnRadii[ (int)_curStage ];
           
		    if( _audioController != null )
            {
                _audioController.StartPlayingGrowSound();
            }
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
			else if( !_stemDoneGrowing ) 
			{
				BaseUpdate();
			}
			
			CustomPlantGrowth();
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
		if( _curStage < GrowthStage.Final )
		{
			//plant that are growing along well should drop items at this point
			if( _curStage == GrowthStage.Sprout )
			{
				SpawnPlant();
			}
			else if( _curStage == GrowthStage.GrowingSprout )
			{
				SpawnAmbientCreature();
			}
			else if( _curStage == GrowthStage.Sapling )
			{
				PlantManager.instance.RequestDropFruit( this, _timeBetweenFruitDrops );
			}

			_curStage += 1;
		}
		else
		{
			StopState();
			_stemDoneGrowing = true;
		}

		UpdateNewStageData();
	}
	#endregion Growth Update Functions

	public override void StopState()
	{
		// if the plant didn't get far enough growing to spawn stuff, have it spawn stuff
 		if( SpawnState == SpawningState.NotSpawning && _curStage <= GrowthStage.Sapling )
		{
			if( _spawnedMediums == 0 )
			{
				SpawnPlant();
				SpawnAmbientCreature();
			}

			PlantManager.instance.RequestDropFruit( this, _timeBetweenFruitDrops );
		}
			
		CustomStopGrowth();
	}

	protected virtual void CustomStopGrowth()
	{
		_stemDoneGrowing = true;
	}


	#region Spawn Functions
	//********************************
	// PLANT SPAWN FUNCTIONS
	//********************************
	void SpawnGroundCoverSpawner()
	{
		GameObject spawner = (GameObject)Instantiate( _coverSpawnerPrefab, transform.position, Quaternion.identity );
		spawner.GetComponent<TrunkGroundCover>().SetupSpawner( GetComponent<BasePlant>() );
	}

	public void SpawnPlant()
	{
		if( SpawnState == SpawningState.NotSpawning )
		{
			SpawnGroundCoverSpawner();

			SpawnState = SpawningState.MediumSpawning;
			PlantManager.instance.RequestSpawnMini( this, _timeBetweenSpawns );
		}
		else if( SpawnState == SpawningState.MediumSpawning )
		{
			if( _spawnedMediums >= _maxMediums )
			{
				SpawnState = SpawningState.NotSpawning;
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
		Vector3 spawnDir = Vector3.zero;

		// if we're dropping fruit ontop of the player's head as an effect for shaking the tree
		if( playerPos != Vector2.zero )
		{
			spawnPoint = new Vector3( playerPos.x, _heighestLeaf.position.y, playerPos.y );
		}
		else
		{
			// push out the randomized value by multiplying by inner radius 
			Vector2 randomPoint = Random.insideUnitCircle * _myPlant.InnerRadius;

			// get the world coordinate based on tree's pos
			spawnPoint = new Vector3( randomPoint.x, 0.0f, randomPoint.y) + _heighestLeaf.position;

			// find our dir so we can add a force to get the seed to drop farther away from tree
			spawnDir = ( spawnPoint - _heighestLeaf.position ).normalized;
		}

		if( !obj )
		{
			if(_droppingItems.Count > 1)
			{
				float seedOdds = Random.value;
				if(seedOdds > 0.25f)
				{
					obj = _droppingItems[0];
				}
				else
				{
					obj = _droppingItems[Random.Range(1, _droppingItems.Count)];
				}				
			}
			else
			{
				obj = _droppingItems[0];
			}
			
		}

		// check to see that it's not already super close to something
		
		GameObject newPlant = (GameObject)Instantiate( obj, _heighestLeaf.position, Quaternion.identity );
		Seed seed = newPlant.GetComponent<Seed>();

		if( playerPos == Vector2.zero )
		{ 
			// we use the spawndir just to get a randomized unit value, then we're snapping force range to the lerp min/max
			newPlant.GetComponent<Rigidbody>().AddForce( spawnDir * Mathf.Lerp( _seedDropForce.Min, _seedDropForce.Max, spawnDir.x ));
		}
		if( seed )
		{
			PlantManager.instance.AddSeed( seed );
		}

		return newPlant;
	}

	public void SummonSeed( Vector2 playerPos )
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
		if(_changeGrowthRateRoutine != null)
		{
			StopCoroutine(_changeGrowthRateRoutine);
			_changeGrowthRateRoutine = null;
		}

		_changeGrowthRateRoutine = StartCoroutine(ChangeGrowthRateRoutine(GROWTHRATE_CHANGESPEED, _wateredGrowthRate));
	}

	IEnumerator ChangeGrowthRateRoutine(float ChangeSpeed, float NewGrowthRate)
	{
		if(ChangeSpeed > 0.0f)
		{
			float timer = 0.0f;
			float startRate = _growthRate;
			while(timer < ChangeSpeed)
			{
				timer += Time.deltaTime;

				ChangeGrowthRate(Mathf.Lerp( startRate, NewGrowthRate, timer / ChangeSpeed));

				yield return 0;
			}
		}
		else
		{
			ChangeGrowthRate(NewGrowthRate);
		}
	}

	public override void TouchPlant(){}
	public override void GrabPlant()
	{
		 Vector3 playerPos = PlayerManager.instance.Player.transform.position;
         SummonSeed( new Vector2( playerPos.x, playerPos.z) );
	}
	public override void StompPlant(){}

	#region Helper Functions
	
	//********************************
	// HELPER FUNCTIONS
	//********************************

	void ChangeGrowthRate( float newRate )
	{
		_growthRate = newRate;
		_plantAnim.speed = _growthRate;

		foreach( Animator child in _childAnimators )
		{
			child.speed = newRate;
		}

		OnChangeGrowthRate();
	}

	protected virtual void OnChangeGrowthRate(){}
		
	void UpdateNewStageData()
	{
		_myPlant.OuterRadius = _spawnRadii[ (int)_curStage ]; // this is a greater value to manage how big things grow
		GetSetMeshRadius();

		if(_growthRate != _baseGrowthRate)
		{
			if(_changeGrowthRateRoutine != null)
			{
				StopCoroutine(_changeGrowthRateRoutine);
				_changeGrowthRateRoutine = null;
			}
			_changeGrowthRateRoutine = StartCoroutine(ChangeGrowthRateRoutine(GROWTHRATE_CHANGESPEED, _baseGrowthRate));
		}

		_plantAnim.speed = _growthRate;
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
	}
	private void DetermineSeedDropForce()
	{
		if( _myPlant.MyPlantType == BasePlant.PlantType.FLOWERING )
		{
			_seedDropForce = new MinMax( 420f, 550f );
		}
		else if( _myPlant.MyPlantType == BasePlant.PlantType.POINT )
		{
			_seedDropForce = new MinMax( 420f, 550f );
		}
		else
		{
			_seedDropForce = new MinMax( 40f, 55f );
		}
	}
	#endregion Helper Functions
}
