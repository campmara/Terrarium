using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Growable : Plantable
{
    [SerializeField] protected GrowableAssetKey _gAssetKey = GrowableAssetKey.NONE;
    public GrowableAssetKey GAssetKey { get { return _gAssetKey; } set { _gAssetKey = value; } }

    [SerializeField] GameObject _seedPrefab = null;
	protected Animator _plantAnim = null;

	const float _numGrowStages = 3;
	const float _plantScaleFactor = 2.0f;
	const float _timeBetweenFruitDrops = 30.0f;
	const float _fruitDropHeight = 8.0f;
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
			
	float [] stageRadii = new float[] { 5.0f, 6.5f, 7.0f, 7.5f }; // how much room each stage need to grow
	float [] growthTime = new float[4]; // time splits initialized based on our animation
	float [] growthRadii = new float[] { 6.0f, 10.0f, 15.0f, 20.0f };

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
		for( int i = 1; i < _numGrowStages + 1; i++ )
		{
			growthTime[i-1] = _animEndTime / _numGrowStages * i;
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
		PlantManager.ExecuteGrowth -= GrowPlant;
		_plantAnim.enabled = false;
	}

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
			else
			{
				CustomPlantGrowing();
			}
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
		_outerSpawnRadius = stageRadii[ (int)_curStage ];
		GetSetMeshRadius();

		_curGrowthRate = _baseGrowthRate;
		_plantAnim.speed = _curGrowthRate;
	}

	bool IsOverlappingPlants()
	{
		//only check surroundings if you are over sprout phase
		Collider[] overlappingObjects = Physics.OverlapSphere( transform.position, growthRadii[ (int)_curStage + 1 ] );

		if( overlappingObjects.Length != 0 )
		{
			foreach( Collider col in overlappingObjects )
			{
				Growable otherPlant = col.GetComponent<Growable>();
				if( otherPlant && col.gameObject != gameObject )
				{
					//because i just went up a level, if it's the same level as new me, growing, i should stop
					if( (int)otherPlant.CurStage > ( (int)_curStage + 1 ) || otherPlant.CurStage == GrowthStage.Final )
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
		Vector3 size = GetComponentInChildren<SkinnedMeshRenderer>().bounds.size;

		if( size.x > size.z )
		{
			_innerMeshRadius = size.x  * transform.GetChild(1).localScale.x * transform.localScale.x;
		}
		else
		{
			_innerMeshRadius = size.z * transform.GetChild(1).localScale.x * transform.localScale.x ;
		}
	}
		
	public virtual GameObject DropFruit()
	{
		GameObject newPlant = null;

		//what kind of radius do i want
		Vector2 randomPoint = Random.insideUnitCircle * _innerMeshRadius;
		Vector3 spawnPoint = new Vector3( randomPoint.x, _fruitDropHeight, randomPoint.y ) + transform.position;

		newPlant = (GameObject)Instantiate( _seedPrefab, spawnPoint, Quaternion.identity );

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

		Gizmos.DrawWireSphere( transform.position, stageRadii[ (int)_curStage ] );
	}
}

