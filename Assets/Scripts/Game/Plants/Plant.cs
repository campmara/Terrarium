using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plant : MonoBehaviour 
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
	float _neededGrowRadius = 0.0f;

	const float _growthRate = 0.00000166f; //  this is 1/60 as standard rate
	const float _waterMultiplier = 3.0f;
	float _curGrowthRate = 4.6f;
	float _curGrowTime = 0.0f;

	float [] growthTime = new float[]{ 1.0f, 1.5f, 3.0f, 4.0f, 5.0f };


	GrowthStage _curStage = GrowthStage.Unplanted;
	public GrowthStage CurStage { get { return _curStage; } }


	//Animations we can either drag and drop or just load in 
	Animation _curAnim =  null;

	void Awake()
	{
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
		Vector3 size = GetComponent<Collider>().bounds.size;
		float largestComponent = size.x;

		if( size.z > size.x )
		{
			largestComponent = size.z;
		}
		
		_neededGrowRadius = largestComponent;
	}

	void GrowPlant()
	{
		ScalePlant();

		//_plantObjects[0].GetComponent<Animation>().
	}

	void ScalePlant()
	{
		//keep moving forward at certain rate 
		if( _curGrowTime >= growthTime[(int)_curStage])
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
		_curGrowthRate = _growthRate;


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
		
	public bool TryDrop()
	{
		return TryTransitionStages();
	}

	public bool TryPickUp()
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
		GetComponent<MeshFilter>().mesh = _plantMeshes[(int)_curStage];
		GetComponent<MeshCollider>().sharedMesh = _plantMeshes[(int)_curStage];

		DetermineTreeSpaceNeeds();
	}
}
