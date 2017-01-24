using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Growable : Plantable
{
	public enum GrowthStage : int
	{
		Unplanted = 0,
		Sprout = 1,
		Sapling = 2,
		Final = 3
	};

	[SerializeField] protected List<Mesh> _plantMeshes = new List<Mesh>();
	List<float> _growthRadius = new List<float>(); // calculated from mesh you're growing into

	const float _plantScaleFactor = 2.0f;
	const float _minTimeSinceDrop = .33f;
	float [] growthTime = new float[] { 3.0f, 3.5f, 4.0f, 5.0f, }; // USE THIS TO TWEAK DURATION OF STAGES 

	float _neededRadius = 0.0f;
	GrowthStage _curStage = GrowthStage.Unplanted;
	public GrowthStage CurStage { get { return _curStage; } }

	protected override void Awake()
	{
		base.Awake();
		InitPlant();
	}
		
	protected override void InitPlant()
	{
		DetermineTreeStagesSpaceNeeds();
		CalculateNeededRadius();
	}

	public override void WaterPlant()
	{
		// ups the rate if it's in a certain mode
		if( _curStage == GrowthStage.Sprout || _curStage == GrowthStage.Unplanted )
		{
			_curGrowthRate *= _waterMultiplier;
		}
	}
		
	public override void GrowPlant()
	{
		TimeBasedPlanting(); // once we have animations to implement,let's do that
	}
		
	void TimeBasedPlanting()
	{
		//keep moving forward at certain rate 
		if( _curGrowTime >= growthTime[ (int)_curStage ] )
		{
			TryTransitionStages();
		}
		else
		{
			_curGrowTime += _curGrowthRate * Time.deltaTime;
		}
	}

	void DetermineTreeStagesSpaceNeeds()
	{
		// calculate the radius plants absolutely need to grow
		for( int i = 0; i < _plantMeshes.Count; i++ )
		{
			Vector3 size = _plantMeshes[i].bounds.size * transform.GetChild(0).localScale.x;
			float largestComponent = size.x;

			if( size.z > size.x )
			{
				largestComponent = size.z;
			}

			_growthRadius.Add( ( largestComponent / 2.0f ) );
		}
	}

	void CalculateNeededRadius()
	{
		_neededRadius = _growthRadius[ (int)_curStage + 1 ] * transform.localScale.x * _plantScaleFactor;
		_plantMeshRadius = _neededRadius;
	}
		

	bool TryTransitionStages()
	{
		bool canTransition = false;

		if( !IsOverlappingPlants() )
		{
			canTransition = true;
			SwitchToNextStage();
		}

		return canTransition;
	}

	bool IsOverlappingPlants()
	{

		RaycastHit[] overlappingObjects = Physics.SphereCastAll( transform.position, _neededRadius, Vector3.up );
		if( overlappingObjects.Length != 0 )
		{
			foreach( RaycastHit hitObj in overlappingObjects )
			{
				if( !hitObj.collider.isTrigger )
				{
					if( hitObj.collider.GetComponent<Plantable>() && hitObj.collider.gameObject != gameObject )
					{
						return true;
					}
				}
				else
				{
					if( hitObj.collider.transform.parent && hitObj.transform.GetComponent<Plantable>() )
					{
						if( hitObj.transform.root != transform )
						{
							return true;
						}
					}
				}
			}
		}

		return false;
	}
		
	public virtual void DropFruit()
	{
		Debug.Log("DROPPING FRUIT");
	}

	void SwitchToNextStage()
	{
		if( _curStage >= GrowthStage.Sprout )
		{
			_canPickup = false;
		} 

		if( _curStage != GrowthStage.Final )
		{
			transform.localScale *= _plantScaleFactor;
			_curStage += 1; // they are int enums so we can just increment

			if( _curStage != GrowthStage.Final )
			{
				//we always want the next mesh we'd be growing into's radius
				CalculateNeededRadius();
			}
			else
			{
				PlantManager.ExecuteGrowth -= GrowPlant;
			}

			if( _curStage == GrowthStage.Sapling )
			{
				PlantManager.instance.RequestSpawnMini( this, _timeBetweenSpawns );	
			}
		}

		_curGrowTime = 0.0f;
		_curGrowthRate = _baseGrowthRate; // ;P resets every stage for fun
		UpdateMeshAndColliderData();
	}

	void SwitchToPrevStage()
	{
		if( _curStage != GrowthStage.Unplanted )
		{
			_curStage -= 1; // they are int enums so we can just increment
		}

		_curGrowTime = 0.0f;

		UpdateMeshAndColliderData();
	}

	void UpdateMeshAndColliderData()
	{
		GetComponentInChildren<MeshFilter>().mesh = _plantMeshes[(int)_curStage];
		BoxCollider innerCollider = GetComponent<BoxCollider>();
		innerCollider.size = GetComponentInChildren<MeshFilter>().mesh.bounds.size * transform.GetChild(0).localScale.x;
		innerCollider.center = new Vector3( 0.0f, ( innerCollider.size.y ) / 2.0f, 0.0f );
	}

	void OnDrawGizmos() 
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere( transform.position, _neededRadius );
	}
}

