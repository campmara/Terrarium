using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnMiniPlantEvent : Event 
{
	Plantable _plant = null;

	public SpawnMiniPlantEvent(){}

	public SpawnMiniPlantEvent( Plantable parent, float timeUntilSpawn ) : base( timeUntilSpawn )
	{
		_plant = parent;
	}

	public override void Execute()
	{
		PlantManager.instance.SpawnMini( _plant );
	}
}
