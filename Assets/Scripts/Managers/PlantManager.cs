using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlantManager : SingletonBehaviour<PlantManager>
{
	List<GroundCover> _groundCoverPlants = new List<GroundCover>();
	List<Plantable> _smallPlants = new List<Plantable>();
	List<Plantable> _largePlants = new List<Plantable>();

	public static event Action ExecuteGrowth;

	public override void Initialize ()
	{
		isInitialized = true;
	}

	public void RequestSpawnMini( Plantable plant, float timeUntil )
	{
		SpawnMiniPlantEvent spawnEvent = new SpawnMiniPlantEvent( plant, timeUntil );
		TimeManager.instance.AddEvent( spawnEvent );
	}

	public void RequestDropFruit( Growable plant, float timeUntil )
	{
		Event dropEvent = new DropFruitEvent( plant, timeUntil );
		TimeManager.instance.AddEvent( dropEvent );
	}

	public void DropSeed( Growable plant )
	{
		//create a new seed based on plant type
		plant.DropFruit();

		//add the new guy to our list
		//DONT FORGET TO ADD THE NEW GUY TO LIST
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
}
