using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plant : Pickupable
{
	public enum PlantType
	{
		RegularTree,
		Grass,
		None
	};

	public enum GrowthStage : int
	{
		Unplanted = 0,
		Sprout = 1,
		Bush = 2 ,
		Sapling = 3,
		Final = 4
	};

	[SerializeField] List<Mesh> _plantMeshes = new List<Mesh>();
	List<float> _growthRadius = new List<float>(); // calculated from mesh you're growing into

	const float _radiusMultiplier = 2.0f;
	const float _baseGrowthRate = 1.0f;
	const float _waterMultiplier = 3.0f;
	const float _plantScaleFactor = 1.4f;
	const float _minTimeSinceDrop = .33f;
	float [] growthTime = new float[]{ 3.0f, 3.5f, 4.0f, 5.0f, 6.0f }; // USE THIS TO TWEAK DURATION OF STAGES 

	float _curGrowthRate = 0.0f;
	float _curGrowTime = 0.0f;
	float _neededRadius = 0.0f;
	bool _planted = false;

	GrowthStage _curStage = GrowthStage.Unplanted;
	public GrowthStage CurStage { get { return _curStage; } }

	protected override void Awake()
	{
		base.Awake();
		InitPlant();
	}

	void Update()
	{
		if( _planted && _curStage != GrowthStage.Final )
		{	
			GrowPlant();
		}
	}

	void InitPlant()
	{
		DetermineTreeSpaceNeeds();
		_neededRadius = _growthRadius[(int)_curStage+1] * transform.localScale.x * _plantScaleFactor;
	}

	void DetermineTreeSpaceNeeds()
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

			_growthRadius.Add( ( largestComponent / 2.0f ) * _radiusMultiplier );
		}
	}

	void GrowPlant()
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

	void ResetPlant()
	{
		_curGrowthRate = _baseGrowthRate;
		_curGrowTime = 0.0f;
		rigidbody.constraints = RigidbodyConstraints.None;
		_planted = false;
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
		
		RaycastHit[] overlappingObjects = Physics.SphereCastAll( transform.position,_neededRadius, Vector3.up );
		if( overlappingObjects.Length != 0 )
		{
			foreach( RaycastHit hitObj in overlappingObjects )
			{
				if( !hitObj.collider.isTrigger )
				{
					if( hitObj.collider.GetComponent<Plant>() && hitObj.collider.gameObject != gameObject )
					{
						return true;
					}
				}
				else
				{
					if( hitObj.transform.parent && hitObj.transform.parent.GetComponent<Plant>() )
					{
						if( hitObj.transform.parent != transform )
						{
							return true;
						}
					}
				}
			}
		}

		return false;
	}
		
	public override void DropSelf()
	{
		base.DropSelf();
		_planted = true;
		SituatePlant();
		// if the plant can grow, let it
		//if( TryTransitionStages() )
		//{
		//	SituatePlant();
		//}
	}

	public override void OnPickup()
	{
		base.OnPickup();
		ResetPlant();
	}

	public bool CanPickup()
	{
		return ( _curStage == GrowthStage.Unplanted && _curGrowTime > _minTimeSinceDrop);
	}

	public void WaterPlant()
	{
		// ups the rate if it's in a certain mode
		if( _curStage == GrowthStage.Sprout || _curStage == GrowthStage.Unplanted || _curStage == GrowthStage.Bush )
		{
			_curGrowthRate *= _waterMultiplier;
		}
	}

	void SwitchToNextStage()
	{
		transform.localScale *= _plantScaleFactor;

		if( _curStage != GrowthStage.Final )
		{
			_curStage += 1; // they are int enums so we can just increment

			if( _curStage != GrowthStage.Final )
			{
				//we always want the next mesh we'd be growing into's radius
				_neededRadius = _growthRadius[(int)_curStage + 1] * ( transform.localScale.x * _plantScaleFactor );
			}
				
			_curGrowTime = 0.0f;
			_curGrowthRate = _baseGrowthRate; // ;P resets every stage for fun
		}
			
		UpdateColliderData();
	}
		
	void SwitchToPrevStage()
	{
		if( _curStage != GrowthStage.Unplanted )
		{
			_curStage -= 1; // they are int enums so we can just increment
		}

		_curGrowTime = 0.0f;

		UpdateColliderData();
	}

	void UpdateColliderData()
	{
		GetComponentInChildren<MeshFilter>().mesh = _plantMeshes[(int)_curStage];
		BoxCollider innerCollider = GetComponent<BoxCollider>();
		innerCollider.size = GetComponentInChildren<MeshFilter>().mesh.bounds.size * transform.GetChild(0).localScale.x;
		innerCollider.center = new Vector3( 0.0f, ( innerCollider.size.y ) / 2.0f, 0.0f );
	}

	void SituatePlant()
	{
		transform.rotation = Quaternion.Euler(Vector3.zero);

		rigidbody.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ |
								 RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY |
								 RigidbodyConstraints.FreezeRotationZ;

	}

	void OnDrawGizmos() {
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(transform.position, _neededRadius);
	}
}
