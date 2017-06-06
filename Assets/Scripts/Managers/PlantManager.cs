using System.Collections.Generic;
using UnityEngine;

public class PlantManager : SingletonBehaviour<PlantManager>
{
	// ******* BIG PLANTS *********** ( per species )
	public Vector2 LrgTotalPlantsRange = new Vector2( 3, 12 );
	
	// ******** MEDIUM PLANTS *********	( per all )
	public Vector2 MedTotalPlantsRange = new Vector2( 0, 25 );
	public Vector2 MedSpawnRadRange = new Vector2( 3f, 4.5f );
	public  Vector2 MedNumPerPlant = new Vector2( 4, 6 );

	// ******** SMALL PLANTS *********	( per all )
	public Vector2 SmTotalPlantsRange = new Vector2( 0, 50 );
	public Vector2 SmSpawnRadRange = new Vector2( 1f, 3f );
	public Vector2 SmNumPerPlant = new Vector2( 8, 15 );


	// ******* UPDATE THESE FOR NEW BIG PLANTS *************
	[SerializeField] GameObject _pointSeed = null;
	List<BasePlant> _pointPlants = new List<BasePlant>();
	[SerializeField] GameObject _floweringSeed = null;
	List<BasePlant> _floweringPlants = new List<BasePlant>();
	[SerializeField] GameObject _huppetSeed = null;
	List<BasePlant> _huppetPlants = new List<BasePlant>();

	// ******   TRACKER LISTS   *************
	List<Seed> _seeds = new List<Seed>();
	List<GroundCover> _smallPlants = new List<GroundCover>();   
	List<BasePlant> _mediumPlants = new List<BasePlant>();
	List<BasePlant> _mounds = new List<BasePlant>();

	public static event System.Action ExecuteGrowth;
    public override void Initialize ()
	{
		isInitialized = true;
	}

	public int GetActiveSeedCount()
	{
		return _seeds.Count;
	}

	public void RequestSpawnMini( BPGrowthController plant, float timeUntil )
	{
		if( _mediumPlants.Count < MedTotalPlantsRange.y && _smallPlants.Count < SmTotalPlantsRange.y )
		{
			SpawnMiniPlantEvent spawnEvent = new SpawnMiniPlantEvent( plant, timeUntil );
			TimeManager.instance.AddEvent( spawnEvent );
		}
	}

	public void RequestDropFruit( BPGrowthController plant, float timeUntil )
	{
		if( GetTotalLargePlants() < LrgTotalPlantsRange.y )
		{
	    	DropFruitEvent dropGameEvent = new DropFruitEvent( plant, timeUntil );
			TimeManager.instance.AddEvent( dropGameEvent );
		}
	}
		
	public void DestroySeed( Seed oldSeed, BasePlant.PlantType seedType, bool seedPlanted )
	{
		if( seedPlanted || IsPopulationStable( seedType ) )
		{
			_seeds.Remove( oldSeed );
			Destroy( oldSeed.gameObject );
		}
	}

	public void AddBasePlant( BasePlant plant )
	{
		if( plant )
		{
			if( plant.MyPlantType == BasePlant.PlantType.FLOWERING )
			{
				_floweringPlants.Add( plant );
			}
			else if( plant.MyPlantType == BasePlant.PlantType.POINT )
			{
				_pointPlants.Add( plant );
			}
			else if( plant.MyPlantType == BasePlant.PlantType.HUPPET )
			{
				_huppetPlants.Add( plant );
			}
		}
	}

	public void AddSeed( Seed seed )
	{
		_seeds.Add( seed );
	}

	public void AddMound( BasePlant mound )
	{
		_mounds.Add( mound );
	}

	public void DeleteMound( BasePlant mound )
	{
		_mounds.Remove( mound );
		Destroy( mound.gameObject );
	}

	public void DeleteLargePlant( BasePlant plant )
	{
		if( plant )
		{
			if( plant.MyPlantType == BasePlant.PlantType.FLOWERING )
			{
				_floweringPlants.Remove(plant);

				if( !IsPopulationStable( plant ) )
				{
					DropSeed( plant.transform.position, BasePlant.PlantType.FLOWERING );
				}
			}
			else if( plant.MyPlantType == BasePlant.PlantType.POINT )
			{
				_pointPlants.Remove(plant);
				
				if( !IsPopulationStable( plant ) )
				{
					DropSeed( plant.transform.position, BasePlant.PlantType.POINT );
				}
			}
			else if( plant.MyPlantType == BasePlant.PlantType.HUPPET )
			{
				_huppetPlants.Remove(plant);
				
				if( !IsPopulationStable( plant ) )
				{
					DropSeed( plant.transform.position, BasePlant.PlantType.HUPPET );
				}
			}
		}
		
		Destroy(plant.gameObject);
	}

	public void SpawnMini( BPGrowthController plant, float waitTime )
	{
		//based on type, spawn some sort of mini
		GameObject newPlant = SpawnNonClippingPlant( plant );
		if( newPlant )
		{
			AudioManager.instance.PlantAdded( GetTotalLargePlants() + _mediumPlants.Count );
			_mediumPlants.Add( newPlant.GetComponent<BasePlant>() );
			plant.SpawnedMediums = plant.SpawnedMediums + 1;
		}
		
		plant.SpawnPlant();
	}

	public void GrowPlants()
	{
		if( ExecuteGrowth != null )
		{
			ExecuteGrowth.Invoke();
		}
	}

    void HandleSave(){}

    void HandleLoad(){}

	int GetTotalLargePlants()
	{
		int largePlants = 0;
		largePlants += _pointPlants.Count;
		largePlants += _floweringPlants.Count;
		largePlants += _huppetPlants.Count;
		return largePlants;
	}
	void DropSeed( Vector3 spawnPoint, BasePlant.PlantType plantType )
	{
		GameObject seed = null;
		if( plantType == BasePlant.PlantType.FLOWERING )
		{
			seed = Instantiate( _floweringSeed, spawnPoint, Quaternion.identity );
		}
		else if( plantType == BasePlant.PlantType.POINT )
		{
			seed = Instantiate( _pointSeed, spawnPoint, Quaternion.identity );
		}
		else if( plantType == BasePlant.PlantType.HUPPET )
		{
			seed = Instantiate( _huppetSeed, spawnPoint, Quaternion.identity );
		}

		_seeds.Add( seed.GetComponent<Seed>() );
	}

	GameObject SpawnNonClippingPlant( BPGrowthController parent )
	{
		GameObject newPlant = null;
		Vector3 spawnPoint;
		GameObject spawn = 	parent._mediumSpawnables[ UnityEngine.Random.Range( 0, parent._mediumSpawnables.Count ) ];

		float checkRadius = spawn.GetComponent<BasePlant>().InnerRadius;
		checkRadius = checkRadius > 1.5f ? checkRadius : 1.5f;
		Collider[] hitColliders;
		bool insideObject = false;

		int allowedAttempts = 25;
		int attempts = 0;

		while( newPlant == null && allowedAttempts >= attempts  )
		{
			attempts++;
			spawnPoint = GetRandomSpawnPoint( parent, spawn );
			hitColliders = Physics.OverlapSphere( spawnPoint, checkRadius );

			foreach( Collider col in hitColliders )
			{
				if( col.GetComponent<BPDeathController>() 
				|| col.GetComponent<SPGrowthController>() 
				|| col.GetComponent<GroundPlantGrowthController>()
				|| col.GetComponent<BumbleGrowthController>()
				|| col.GetComponent<BPGrowthController>() 
				|| col.GetComponent<PondTech>() 
				|| col.GetComponentInParent<RockTag>() )
				{
					insideObject = true;
					break;
				}
			}
				
			if( !insideObject )
			{
				newPlant = (GameObject)Instantiate( spawn, spawnPoint, Quaternion.identity );
				return newPlant;
			}
		}

		// this should never be executed
		return null;
	}
	
	Vector3 GetRandomSpawnPoint( BPGrowthController plant, GameObject spawn )
	{
		Vector2 randomPoint = plant.GetRandomPoint();//GetRandomPoint( plant.InnerRadius, plant.OuterRadius );
		Vector3 pos = plant.transform.position;
		Vector3 spawnPoint = new Vector3( pos.x + randomPoint.x, .2f/*plant.SpawnHeight*/, pos.z + randomPoint.y );

		return new Vector3( spawnPoint.x, .05f, spawnPoint.z );
	}

	public bool IsPopulationStable( BasePlant plant )
	{
		if( plant )
		{
			return IsPopulationStable( plant.MyPlantType );
		}
		else
		{
			return false;
		}
	}

	public bool IsPopulationStable( BasePlant.PlantType _plantType )
	{
		bool result = false;
		if( _plantType == BasePlant.PlantType.FLOWERING )
		{
			 result = _floweringPlants.Count >= LrgTotalPlantsRange.x;

		}
		else if( _plantType == BasePlant.PlantType.POINT )
		{
			result = _pointPlants.Count >= LrgTotalPlantsRange.x;			
		}
		else if( _plantType == BasePlant.PlantType.HUPPET )
		{
			result = _huppetPlants.Count >= LrgTotalPlantsRange.x;			
		}

		return result;
	}
}
