public class SpawnMiniPlantEvent : GameEvent
{
	BasePlant _plant = null;

	public SpawnMiniPlantEvent(){}

	public SpawnMiniPlantEvent( BasePlant parent, float timeUntilSpawn ) : base( timeUntilSpawn )
	{
		_plant = parent;
	}

	public override void Execute()
	{
		PlantManager.instance.SpawnMini( _plant );
	}
}
