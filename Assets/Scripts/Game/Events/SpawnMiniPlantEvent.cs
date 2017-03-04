public class SpawnMiniPlantEvent : GameEvent
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
