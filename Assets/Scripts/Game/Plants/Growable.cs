using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Growable : Plantable
{
    [SerializeField] protected GrowableAssetKey _gAssetKey = GrowableAssetKey.NONE;
    public GrowableAssetKey GAssetKey { get { return _gAssetKey; } set { _gAssetKey = value; } }

	[SerializeField] List<GameObject> _droppablePrefabs = null;  
	protected Animator _plantAnim = null;

	const float _numGrowStages = 3;
	const float _timeBetweenFruitDrops = 50.0f;
	float _fruitDropHeight = 8.0f;
	protected float _animEndTime = 0.0f;
	float _curTimestamp = 0.0f;
	protected float _curPercentAnimated = 0.0f;

	[SerializeField] GrowthStage _curStage = GrowthStage.Seed;
	public GrowthStage CurStage { get { return _curStage; } }

	public enum GrowthStage : int
	{
		Seed = -1,
		Sprout = 0,
		GrowingSprout= 1,
		Sapling = 2,
		Final = 3
	};
			
	float [] stageRadii = new float[] { 6.0f, 8.0f, 10.0f, 12.0f }; // how much room each stage need to grow, first element doesnt matter
	float [] growthTime = new float[4]; // time splits initialized based on our animation
	float [] growthRadii = new float[] { 4.0f, 5.5f, 5.75f, 6.0f }; // how far away things need to be to even plant

    protected const float CREATURE_BASE_SPAWNODDS = 0.75f;
    protected const float CREATURE_BASE_SPAWNY = -1.0f;


    protected override void Awake()
	{
		InitPlant();
	}
		
	protected override void InitPlant()
	{
		base.InitPlant();
		_curStage = GrowthStage.Seed;
		AnimationSetup();

		if( !IsOverlappingPlants() )
		{
			_curStage = GrowthStage.Sprout;
			_outerSpawnRadius = stageRadii[ (int)_curStage ];
			_minDistAway = growthRadii[ 0 ];
			GetSetMeshRadius();
			StartGrowth();
		}
		else
		{
			if( _plantAnim )
			{
				_plantAnim.enabled = false;
			}
			CustomStopGrowth();
		}
	}

	protected virtual void AnimationSetup()
	{
		_plantAnim = GetComponent<Animator>();
		_animEndTime = _plantAnim.GetCurrentAnimatorStateInfo(0).length;
		_plantAnim.speed = _baseGrowthRate;

		SetGrowthTransitionPoints();
	}

	protected virtual void SetGrowthTransitionPoints()
	{
		float _duration = 1.0f / ( _numGrowStages + 1 );
		for( int i = 0; i < _numGrowStages + 1; i++ )
		{
//			if( i != 0 )
//			{
//				growthTime[i] = _duration * (i+1);
//			}
//			else
//			{
//				growthTime[i] = .15f;
//			}

			growthTime[i] = _duration * (i+1);
		}
	}

	protected override void StartGrowth()
	{
	    base.StartGrowth();
		PlantManager.ExecuteGrowth += GrowPlant;
		_plantAnim.enabled = true;
	}

	protected override void StopGrowth()
	{
		PlantManager.ExecuteGrowth -= GrowPlant;
		CustomStopGrowth();
	}

	protected virtual void CustomStopGrowth()
	{
		base.StopGrowth();
		if( _plantAnim )
		{
			_plantAnim.enabled = false;
		}
	}

	public override void WaterPlant()
	{
		ChangeGrowthRate( _baseGrowthRate * _wateredGrowthMultiplier );
	}

	void ChangeGrowthRate(float newRate )
	{
		_plantAnim.speed = newRate;

		foreach( Animator child in _childAnimators )
		{
			child.speed = newRate;
		}
	}
		
	public override void GrowPlant()
	{
		if( _curStage != GrowthStage.Final )
		{
			_curPercentAnimated = _plantAnim.GetCurrentAnimatorStateInfo(0).normalizedTime / _animEndTime; // Mathf.Lerp(0.0f, _animEndTime, _plantAnim.GetCurrentAnimatorStateInfo(0).normalizedTime ); // i am x percent of the way through anim

			if( _curPercentAnimated >= growthTime[ (int)_curStage ] )
			{
 				TryTransitionStages();
			}

			CustomPlantGrowing();
		}
	}

	protected virtual void CustomPlantGrowing(){}
		
	void TryTransitionStages()
	{
		if( _curStage == GrowthStage.Sprout )
		{
			Debug.Log("HELLO");
		}
		if( IsOverlappingPlants() )
		{
			StopGrowth();
		}
		else
		{
			SwitchToNextStage();
		}
	}

	void SwitchToNextStage()
	{   		
		if( _curStage != GrowthStage.Final )
		{
            _curStage += 1; // they are int enums so we can just increment
		}
			
		if( _curStage == GrowthStage.Sapling ) //if i'm now growing into final
		{
			PlantManager.instance.RequestSpawnMini( this, _timeBetweenSpawns );
		}
		else if( _curStage == GrowthStage.Final )
		{
			PlantManager.instance.RequestDropFruit( this, _timeBetweenFruitDrops );

            SpawnAmbientCreature();

            StopGrowth();
		}

		ChangeGrowthRate( _baseGrowthRate );
		UpdateNewStageData();
	}
		
	void UpdateNewStageData()
	{
		_outerSpawnRadius = stageRadii[ (int)_curStage ]; // this is a greater value to manage how big things grow
		_minDistAway = growthRadii[(int)_curStage]; // this is a smaller value to keep planted things spaced
		GetSetMeshRadius();

		_curGrowthRate = _baseGrowthRate;
		_plantAnim.speed = _curGrowthRate;
		_fruitDropHeight = transform.GetChild(1).GetComponent<SkinnedMeshRenderer>().bounds.size.y * ( _numGrowStages + 1 );
	}

	bool IsOverlappingPlants()
	{
		//only check surroundings if you are over sprout phase
		Collider[] overlappingObjects = Physics.OverlapSphere( transform.position, stageRadii[ (int)_curStage + 1 ] ); 

		if( overlappingObjects.Length != 0 )
		{
			foreach( Collider col in overlappingObjects )
			{
				Growable otherPlant = col.GetComponent<Growable>();
				if( otherPlant && col.gameObject != gameObject )
				{
					//because i just went up a level, if it's the same level as new me, growing, i should stop
					if( (int)otherPlant.CurStage > ( (int)_curStage ) || otherPlant.CurStage == GrowthStage.Final )
					{
						return true;
					} 
				}
			}
		}
		
		return false;
	}   
		
	protected override void GetSetMeshRadius()
	{
		Vector3 size = transform.GetChild(1).GetComponent<SkinnedMeshRenderer>().bounds.size;

		if( size.x > size.z )
		{
			_innerMeshRadius = size.x  * .5f; 
		}
		else
		{
			_innerMeshRadius = size.z  * .5f; 
		}
	}
		
	public virtual GameObject DropFruit()
	{
		GameObject newPlant = null;

		//what kind of radius do i want
		Vector2 randomPoint = Random.insideUnitCircle;
		randomPoint = new Vector2( randomPoint.x + Mathf.Sign(randomPoint.x) * _innerMeshRadius, randomPoint.y + Mathf.Sign(randomPoint.y) + _innerMeshRadius );
		Vector3 spawnPoint = new Vector3( randomPoint.x, _fruitDropHeight, randomPoint.y ) + transform.position;

		newPlant = (GameObject)Instantiate( _droppablePrefabs[Random.Range( 0, _droppablePrefabs.Count)], spawnPoint, Quaternion.identity );  
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
            CreatureManager.instance.SpawnRandomCreature( this.transform.position + ( Vector3.up * CREATURE_BASE_SPAWNY ) );
        }
    }
    	
	void OnDrawGizmos() 
	{
		Gizmos.color = Color.yellow;
		if( _curStage != GrowthStage.Final )
		{
			Gizmos.DrawWireSphere( transform.position, _innerMeshRadius );//stageRadii[ (int)_curStage + 1 ] );
		}

		Gizmos.color = Color.red;
		if( (int) _curStage > -1 )
		{
		Gizmos.DrawWireSphere( transform.position, stageRadii[(int) _curStage ] );
		}
	}
}

