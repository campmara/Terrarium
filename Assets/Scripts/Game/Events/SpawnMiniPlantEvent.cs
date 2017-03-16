public class SpawnMiniPlantEvent : GameEvent
{
	PlantController _plant = null;

	public SpawnMiniPlantEvent(){}

	public SpawnMiniPlantEvent( PlantController parent, float timeUntilSpawn ) : base( timeUntilSpawn )
	{
		_plant = parent;
	}

	public override void Execute()
	{
		if (_plant)
		{
			PlantManager.instance.SpawnMini( _plant );
		}
	}
}
