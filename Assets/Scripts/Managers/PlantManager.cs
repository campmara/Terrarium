using System.Collections.Generic;
using UnityEngine;

public class PlantManager : SingletonBehaviour<PlantManager>
{
	[SerializeField] int _maxLargePlants = 20;
	[SerializeField] int _maxMediumPlants = 40;
	[SerializeField] int _maxSmallPlants = 200;

	List<Seed> _seeds = new List<Seed>();
	List<GroundCover> _smallPlants = new List<GroundCover>();   
	List<BasePlant> _mediumPlants = new List<BasePlant>();
	List<BasePlant> _largePlants = new List<BasePlant>();
	List<BasePlant> _mounds = new List<BasePlant>();


	public static event System.Action ExecuteGrowth;

    private void Awake()
    {
        SaveManager.PrepSave += HandleSave;
        SaveManager.CompleteLoad += HandleLoad;
    }

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
		if( _mediumPlants.Count < _maxMediumPlants && _smallPlants.Count < _maxSmallPlants )
		{
			SpawnMiniPlantEvent spawnEvent = new SpawnMiniPlantEvent( plant, timeUntil );
			TimeManager.instance.AddEvent( spawnEvent );
		}
	}

	public void RequestDropFruit( BPGrowthController plant, float timeUntil )
	{
		if( _largePlants.Count < _maxLargePlants )
		{
	    	DropFruitEvent dropGameEvent = new DropFruitEvent( plant, timeUntil );
			TimeManager.instance.AddEvent( dropGameEvent );
		}
	}
		
	public void DestroySeed( Seed oldSeed )
	{
		_seeds.Remove( oldSeed );
		Destroy( oldSeed.gameObject );
	}

	public void AddBasePlant( BasePlant BasePlant )
	{
		_largePlants.Add( BasePlant );
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

	public void DeleteLargePlant(BasePlant plant)
	{
		_largePlants.Remove(plant);
		Destroy(plant.gameObject);
	}

	public void SpawnMini( BPGrowthController plant, float waitTime )
	{
		//based on type, spawn some sort of mini
		GameObject newPlant = SpawnNonClippingPlant( plant );
		if( newPlant )
		{
			if( newPlant.GetComponent<SPGrowthController>() )
			{
				_mediumPlants.Add( newPlant.GetComponent<BasePlant>() );
				plant.SpawnedMediums = plant.SpawnedMediums + 1;
			}
			else
			{
				_smallPlants.Add( newPlant.GetComponent<GroundCover>() );
				plant.SpawnedSmalls = plant.SpawnedSmalls + 1;
			}
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

    private void OnDestroy()
    {
        SaveManager.PrepSave -= HandleSave;
        SaveManager.CompleteLoad -= HandleLoad;
    }


	GameObject SpawnNonClippingPlant( BPGrowthController parent )
	{
		GameObject newPlant = null;
		Vector3 spawnPoint;
		GameObject spawn = GetRandomSpawnable( parent );

		float checkRadius = spawn.GetComponent<BasePlant>().InnerRadius;
		checkRadius = checkRadius > 0.0f ? checkRadius : 1.0f;
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
				if( col.GetComponent<BPDeathController>() || col.GetComponent<SPGrowthController>() || col.GetComponent<BPGrowthController>() || col.GetComponent<PondTech>() || col.GetComponent<RockTag>() )
				//col.GetComponent<BasePlant>()
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
			else if( parent.spawnState == BPGrowthController.SpawningState.SmallSpawning && parent.SpawnedSmalls > 8 )
			{
				parent.ForceSpawnMediums();
			}
		}

		// this should never be executed
		return null;
	}
		
	GameObject GetRandomSpawnable( BPGrowthController plant )
	{
		GameObject spawn = null;

		if( plant.spawnState == BPGrowthController.SpawningState.SmallSpawning )
		{
			spawn = plant._smallSpawnables[ UnityEngine.Random.Range( 0, plant._smallSpawnables.Count ) ];
		}
		else if( plant.spawnState == BPGrowthController.SpawningState.MediumSpawning )
		{
			spawn = plant._mediumSpawnables[ UnityEngine.Random.Range( 0, plant._mediumSpawnables.Count ) ];
		}

		return spawn;
	}

	Vector3 GetRandomSpawnPoint( BPGrowthController plant, GameObject spawn )
	{
		Vector2 randomPoint = plant.GetRandomPoint();//GetRandomPoint( plant.InnerRadius, plant.OuterRadius );
		Vector3 pos = plant.transform.position;
		Vector3 spawnPoint = new Vector3( pos.x + randomPoint.x, .2f/*plant.SpawnHeight*/, pos.z + randomPoint.y );

		return new Vector3( spawnPoint.x, .2f, spawnPoint.z );
	}
}
