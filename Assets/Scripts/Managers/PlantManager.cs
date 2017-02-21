using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlantManager : SingletonBehaviour<PlantManager>
{
	List<Seed> _seeds = new List<Seed>();
	List<GroundCover> _groundCoverPlants = new List<GroundCover>();   
	List<Plantable> _smallPlants = new List<Plantable>();
	List<Growable> _largePlants = new List<Growable>();

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

	public void RequestSpawnMini( Plantable plant, float timeUntil )
	{
		SpawnMiniPlantEvent spawnEvent = new SpawnMiniPlantEvent( plant, timeUntil );
		TimeManager.instance.AddEvent( spawnEvent );
	}

	public void RequestDropFruit( Growable plant, float timeUntil )
	{
	    DropFruitEvent dropGameEvent = new DropFruitEvent( plant, timeUntil );
		TimeManager.instance.AddEvent( dropGameEvent );
	}
		
	public void DestroySeed( Seed oldSeed )
	{
		_seeds.Remove( oldSeed );
		Destroy( oldSeed.gameObject );
	}

	public void AddBigPlant( Growable bigPlant )
	{
		_largePlants.Add( bigPlant );
	}

	public void AddSeed( Seed seed )
	{
		_seeds.Add( seed );
	}

	public void SpawnMini( Plantable plant )
	{
		//based on type, spawn some sort of mini
		GameObject newPlant = plant.SpawnMiniPlant();
		if( newPlant )
		{
			if( newPlant.GetComponent<Plantable>() )
			{
				_smallPlants.Add( newPlant.GetComponent<Plantable>() );
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

    void HandleSave()
    {

    }

    void HandleLoad()
    {

    }

    private void OnDestroy()
    {
        SaveManager.PrepSave -= HandleSave;
        SaveManager.CompleteLoad -= HandleLoad;
    }
}
