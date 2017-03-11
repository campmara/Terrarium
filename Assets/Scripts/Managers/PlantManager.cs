using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlantManager : SingletonBehaviour<PlantManager>
{
	List<Seed> _seeds = new List<Seed>();
	List<GroundCover> _groundCoverPlants = new List<GroundCover>();   
	List<BasePlant> _smallPlants = new List<BasePlant>();
	List<BasePlant> _largePlants = new List<BasePlant>();
	List<BasePlant> _mounds = new List<BasePlant>();


	public static event Action ExecuteGrowth;

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
		SpawnMiniPlantEvent spawnEvent = new SpawnMiniPlantEvent( plant, timeUntil );
		TimeManager.instance.AddEvent( spawnEvent );
	}

	public void RequestDropFruit( BPGrowthController plant, float timeUntil )
	{
	    DropFruitEvent dropGameEvent = new DropFruitEvent( plant, timeUntil );
		TimeManager.instance.AddEvent( dropGameEvent );
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

	public void SpawnMini( PlantController plant )
	{
		//based on type, spawn some sort of mini
		GameObject newPlant = plant.SpawnChildPlant();
		if( newPlant )
		{
			if( newPlant.GetComponent<SPGrowthController>() )
			{
				_smallPlants.Add( newPlant.GetComponent<BasePlant>() );
			}
			else
			{
				_groundCoverPlants.Add( newPlant.GetComponent<GroundCover>() );
			}
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
}
