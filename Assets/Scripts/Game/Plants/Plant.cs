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
	float _neededGrowRadius = 0.0f; // calculated from mesh you're growing into

	const float _baseGrowthRate = 1.0f;
	const float _waterMultiplier = 3.0f;
	float [] growthTime = new float[]{ 3.0f, 3.5f, 4.0f, 5.0f, 6.0f }; // USE THIS TO TWEAK DURATION OF STAGES 

	float _curGrowthRate = 0.0f;
	float _curGrowTime = 0.0f;

	GrowthStage _curStage = GrowthStage.Unplanted;
	public GrowthStage CurStage { get { return _curStage; } }

	protected override void Awake()
	{
		base.Awake();
		InitPlant();
	}

	void Update()
	{
		if( _curStage != GrowthStage.Unplanted  && _curStage != GrowthStage.Final )
		{	
			GrowPlant();
		}
	}

	void InitPlant()
	{
		DetermineTreeSpaceNeeds();
	}

	void DetermineTreeSpaceNeeds()
	{
		// calculate the radius plants absolutely need to grow
		Vector3 size = GetComponentInChildren<MeshCollider>().bounds.size;
		float largestComponent = size.x;

		if( size.z > size.x )
		{
			largestComponent = size.z;
		}
		
		_neededGrowRadius = largestComponent;
	}

	void GrowPlant()
	{
		TimeBasedPlanting(); // once we have animations to implement 
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
	}

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
			SwitchToPrevStage();
		}


		return canTransition;
	}

	bool IsOverlappingPlants()
	{
		RaycastHit[] overlappingObjects = Physics.SphereCastAll( transform.position,  _neededGrowRadius, Vector3.back );
		if( overlappingObjects.Length != 0 )
		{
			foreach( RaycastHit hitObj in overlappingObjects )
			{
				// if you're close to another plant that's not yourself, you can't stay!
				if( hitObj.collider.gameObject.GetComponent<Plant>() && hitObj.collider.gameObject != gameObject )
				{
					return true;
				}
			}
		}

		return false;
	}
		
	public override void DropSelf()
	{
		base.DropSelf();
		// if the plant can grow, let it
		if( TryTransitionStages() )
		{
			SituatePlant();
		}
	}

	public override void OnPickup()
	{
		ResetPlant();
	}

	public bool CanPickup()
	{
		return ( _curStage == GrowthStage.Unplanted );
	}

	public void WaterPlant()
	{
		// ups the rate if it's in a certain mode
		if( _curStage == GrowthStage.Sprout || _curStage == GrowthStage.Bush )
		{
			_curGrowthRate *= _waterMultiplier;
			//CHANGE THE SPEED
		}
	}

	void SwitchToNextStage()
	{
		if( _curStage != GrowthStage.Final )
		{
			_curStage += 1; // they are int enums so we can just increment
			_curGrowTime = 0.0f;
		}
			
		UpdateMeshData();
	}
		
	void SwitchToPrevStage()
	{
		if( _curStage != GrowthStage.Unplanted )
		{
			_curStage -= 1; // they are int enums so we can just increment
		}

		_curGrowTime = 0.0f;

		UpdateMeshData();
	}

	void UpdateMeshData()
	{
		GetComponentInChildren<MeshFilter>().mesh = _plantMeshes[(int)_curStage];
		GetComponentInChildren<MeshCollider>().sharedMesh = _plantMeshes[(int)_curStage];

		DetermineTreeSpaceNeeds();
	}

	void SituatePlant()
	{
		_rigidbody.freezeRotation = true;
		transform.localScale = Vector3.one * 3.0f;
		transform.rotation = Quaternion.Euler(Vector3.zero);
	}
}
