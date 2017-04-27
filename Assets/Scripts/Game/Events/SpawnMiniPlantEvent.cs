public class SpawnMiniPlantEvent : GameEvent
{
	BPGrowthController _plant = null;

	public SpawnMiniPlantEvent(){}

	public SpawnMiniPlantEvent( BPGrowthController parent, float timeUntilSpawn ) : base( timeUntilSpawn )
	{
		_plant = parent;
	}

	public override void Execute()
	{
		if (_plant)
		{
			PlantManager.instance.SpawnMini( _plant, TimeUntilExecution );
		}
	}
}
