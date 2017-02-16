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
	const float _plantScaleFactor = 2.0f;
	const float _timeBetweenFruitDrops = 50.0f;
	float _fruitDropHeight = 8.0f;
	protected float _animEndTime = 0.0f;
	float _curTimestamp = 0.0f;

	[SerializeField] GrowthStage _curStage = GrowthStage.Sprout;
	public GrowthStage CurStage { get { return _curStage; } }

	public enum GrowthStage : int
	{
		Sprout = 0,
		GrowingSprout= 1,
		Sapling = 2,
		Final = 3
	};
			
	float [] stageRadii = new float[] { 4.0f, 7.0f, 10.0f, 12.0f }; // how much room each stage need to grow, first element doesnt matter
	float [] growthTime = new float[4]; // time splits initialized based on our animation
	float [] growthRadii = new float[] { 3.0f, 3.5f, 3.75f, 4.0f }; // how far away things need to be to even plant

    protected const float CREATURE_BASE_SPAWNODDS = 0.75f;
    protected const float CREATURE_BASE_SPAWNY = -1.0f;


    protected override void Awake()
	{
		InitPlant();
	}
		
	protected override void InitPlant()
	{
		base.InitPlant();

		_curStage = GrowthStage.Sprout;
		AnimationSetup();
		_outerSpawnRadius = stageRadii[ (int)_curStage ];
		_minDistAway = growthRadii[ 0 ];
		GetSetMeshRadius();

		StartGrowth();
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
		float duration = _animEndTime / ( _numGrowStages + 3 );
		for( int i = 0; i < _numGrowStages; i++ )
		{
			if( i  == _numGrowStages - 1)
			{
				growthTime[i] = duration * 3;
			}
			else if( i == 0)
			{
				growthTime[i] = duration;
			}
			else
			{
				growthTime[i] = duration * 2;
			}
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
	    base.StopGrowth();
		CustomStopGrowth();
	}

	protected virtual void CustomStopGrowth(){}

	public override void WaterPlant()
	{
		//start growing and start growing at a faster rate
		_curGrowthRate = _wateredGrowthRate;
	}
		
	public override void GrowPlant()
	{
		if( _curStage != GrowthStage.Final )
		{
			_curTimestamp = _plantAnim.GetCurrentAnimatorStateInfo(0).normalizedTime;

			if( _curTimestamp >= growthTime[ (int)_curStage ] )
			{
				TryTransitionStages();
			}

			CustomPlantGrowing();
		}
	}

	protected virtual void CustomPlantGrowing(){}
		
	void TryTransitionStages()
	{
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
			
		if( _curStage == GrowthStage.Sapling )
		{
			PlantManager.instance.RequestSpawnMini( this, _timeBetweenSpawns );
		}
		else if( _curStage == GrowthStage.Final )
		{
			PlantManager.instance.RequestDropFruit( this, _timeBetweenFruitDrops );

            SpawnAmbientCreature();

            StopGrowth();
		}

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
			_innerMeshRadius = size.x  * transform.localScale.x *.5f; 
		}
		else
		{
			_innerMeshRadius = size.z  * transform.localScale.x * .5f; 
		}
	}
		
	public virtual GameObject DropFruit()
	{
		GameObject newPlant = null;

		//what kind of radius do i want
		Vector2 randomPoint = Random.insideUnitCircle;
		randomPoint = new Vector2( randomPoint.x + _innerMeshRadius, randomPoint.y + _innerMeshRadius );
		Vector3 spawnPoint = new Vector3( randomPoint.x, _fruitDropHeight, randomPoint.y ) + transform.position;

		newPlant = (GameObject)Instantiate( _droppablePrefabs[Random.Range( 0, _droppablePrefabs.Count)], spawnPoint, Quaternion.identity );  

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

//		Gizmos.color = Color.red;
		//Gizmos.DrawWireSphere( transform.position, growthRadii[(int) _curStage ] );
	}
}

