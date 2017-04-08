using System.Collections.Generic;
using UnityEngine;

public class PlantManager : SingletonBehaviour<PlantManager>
{
	[SerializeField] int _maxLargePlants = 10;
	[SerializeField] int _maxMediumPlants = 40;
	[SerializeField] int _maxSmallPlants = 120;

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

	public void RequestSpawnMini( PlantController plant, float timeUntil )
	{
		if( _mediumPlants.Count < _maxMediumPlants )
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

	public void SpawnMini( PlantController plant, float waitTime )
	{
		//based on type, spawn some sort of mini
		GameObject newPlant = SpawnNonClippingPlant( plant );
		if( newPlant )
		{
			if( newPlant.GetComponent<SPGrowthController>() )
			{
				_mediumPlants.Add( newPlant.GetComponent<BasePlant>() );
			}
			else
			{
				_smallPlants.Add( newPlant.GetComponent<GroundCover>() );
			}

			RequestSpawnMini( plant, waitTime );
		}
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


	GameObject SpawnNonClippingPlant( PlantController parent )
	{
		GameObject newPlant = null;
		Vector3 spawnPoint;
		GameObject spawn = GetRandomSpawnable( parent );

		float checkRadius = parent.GetComponent<BasePlant>().InnerRadius;
		checkRadius = checkRadius > 0.0f ? checkRadius : 1.0f;
		Collider[] hitColliders;
		bool insideObject = false;

		int allowedAttempts = 10;
		int attempts = 0;
		while( newPlant == null && allowedAttempts >= attempts  )
		{
			attempts++;
			spawnPoint = GetRandomSpawnPoint( parent.GetComponent<BasePlant>(), spawn );
			hitColliders = Physics.OverlapSphere( spawnPoint, checkRadius );

			foreach( Collider col in hitColliders )
			{
				if( col.GetComponent<BasePlant>() || col.GetComponent<PondTech>() || col.GetComponent<RockTag>() )
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
		
	GameObject GetRandomSpawnable( PlantController plant )
	{
		GameObject spawn = null;
		if( plant.GetComponent<SPGrowthController>() )
		{
			SPGrowthController spController = plant as SPGrowthController;
			spawn = spController.Spawnables[ UnityEngine.Random.Range( 0, spController.Spawnables.Count)];
		}
		else if( plant.GetComponent<BPGrowthController>() )
		{
			BPGrowthController bpController = plant as BPGrowthController;
			spawn = bpController.Spawnables[ UnityEngine.Random.Range( 0, bpController.Spawnables.Count)];
		}

		return spawn;
	}

	Vector3 GetRandomSpawnPoint( BasePlant plant, GameObject spawn )
	{
		Vector2 randomPoint = plant.GetRandomPoint();//GetRandomPoint( plant.InnerRadius, plant.OuterRadius );
		Vector3 pos = plant.transform.position;
		Vector3 spawnPoint = new Vector3( pos.x + randomPoint.x, plant.SpawnHeight, pos.z + randomPoint.y );

		return new Vector3( spawnPoint.x, .2f, spawnPoint.z );
	}
}
