using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Growable : Plantable
{
	public enum GrowthStage : int
	{
		Sprout = 0,
		GrowingSprout= 1,
		Sapling = 2,
		Final = 3
	};

	[SerializeField] GameObject _seedPrefab = null;

	float [] stageRadii = new float[] { 2.0f, 
										3.0f, 
										4.0f,
										5.0f }; // how much room each stage need to grow
	float [] growthTime = new float[4];

	const float _numGrowStages = 3;
	const float _plantScaleFactor = 2.0f;
	const float _minTimeSinceDrop = .33f;

	[SerializeField] GrowthStage _curStage = GrowthStage.Sprout;
	public GrowthStage CurStage { get { return _curStage; } }

	protected override void Awake()
	{
		base.Awake();
		InitPlant();
	}
		
	protected override void InitPlant()
	{
		_curStage = GrowthStage.Sprout;
		base.InitPlant();
		SetTransitionPoints();
		AnimationSetup();
		base.StartGrowth();
	}

	protected virtual void AnimationSetup(){}

	protected virtual void SetTransitionPoints()
	{
		for( int i = 1; i < _numGrowStages + 1; i++ )
		{
			growthTime[i-1] = _animEndTime / _numGrowStages * i;
		}
	}

	protected override void StartGrowth()
	{
		
	}

	protected override void StopGrowth()
	{
		PlantManager.ExecuteGrowth -= GrowPlant;
		_plantAnim.Stop();
		_plantAnim.StopPlayback();
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
			_curTimestamp = _plantAnim.GetCurrentAnimatorStateInfo(0).normalizedTime;// * _animEndTime;

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
		
	bool TryTransitionStages()
	{
		bool canTransition = false;

		SwitchToNextStage();

		if( !IsOverlappingPlants() )
		{
			canTransition = true;
		}
		else
		{
			StopGrowth();
		}
			
		return canTransition;
	}

	bool IsOverlappingPlants()
	{
		//only check surroundings if you are over sprout phase
		RaycastHit[] overlappingObjects = Physics.SphereCastAll( transform.position, stageRadii[ (int)_curStage ], Vector3.up );
		if( overlappingObjects.Length != 0 )
		{
			foreach( RaycastHit hitObj in overlappingObjects )
			{
				if( !hitObj.collider.isTrigger )
				{
					Growable otherPlant = hitObj.collider.GetComponent<Growable>();
					if( otherPlant && hitObj.collider.gameObject != gameObject )
					{
						//this thing needs to be one level higher than me ( we've yet to switch stages properly, so it's + 2)
						if( (int)otherPlant.CurStage == ( (int)_curStage + 1 ) )
						{
							return true;
						}
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
			_plantMeshRadius = size.x * transform.GetChild(0).localScale.x;
		}
		else
		{
			_plantMeshRadius = size.z * transform.GetChild(0).localScale.x;
		}
	}

	public virtual GameObject DropFruit()
	{   
		GameObject newPlant = null;

		//what kind of radius do i want
		Vector2 randomPoint = Random.insideUnitCircle * _spawnRadius;
		Vector3 spawnPoint = new Vector3( randomPoint.x, 2.0f, randomPoint.y ) + transform.position;
		Vector3 direction = ( spawnPoint - transform.position ).normalized * ( _plantMeshRadius );
		spawnPoint += direction;

		newPlant = (GameObject)Instantiate( _seedPrefab, spawnPoint, Quaternion.identity );

		PlantManager.instance.RequestDropFruit( this, _timeBetweenSpawns + Random.Range(.1f, .2f) );

		if( newPlant == null )
		{
			Debug.Log("dropping seed plant messed up ");
		}
		return newPlant;
	}   
		
	void SwitchToNextStage()
	{   		
		if( _curStage != GrowthStage.Final )
		{
			_curStage += 1; // they are int enums so we can just increment
		
			if( _curStage == GrowthStage.GrowingSprout )
			{
				PlantManager.instance.RequestSpawnMini( this, _timeBetweenSpawns );	
			}
			else if( _curStage == GrowthStage.Final )
			{
				PlantManager.instance.RequestDropFruit( this, _timeBetweenSpawns );
			}
		}

		if( _curStage == GrowthStage.Final )
		{
			StopGrowth();
		}

		_spawnRadius = stageRadii[ (int)_curStage ];
		_curGrowthRate = _baseGrowthRate;
		_plantAnim.speed = _curGrowthRate;
	}
		
	void UpdateMeshAndColliderData()
	{
		BoxCollider innerCollider = GetComponent<BoxCollider>();
		innerCollider.size = GetComponentInChildren<MeshFilter>().mesh.bounds.size * transform.GetChild(0).localScale.x;
		innerCollider.center = new Vector3( 0.0f, ( innerCollider.size.y ) / 2.0f, 0.0f );
	}

	void OnDrawGizmos() 
	{
		Gizmos.color = Color.yellow;

		Gizmos.DrawWireSphere( transform.position, stageRadii[ (int)_curStage ] );
	}
}

