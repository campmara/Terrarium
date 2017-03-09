using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BasePlant : MonoBehaviour
{
	protected float _deathLength = 0.0f;
	protected float _timeUntilDeath = 0.0f;

	// PLANT UPDATE 
	protected abstract void StartPlantUpdate();
	public abstract void UpdatePlant();
	protected abstract void StopPlantUpdate();

	protected abstract void StartPlantGrowth();
	protected abstract void StopPlantGrowth();

	// PLANT INTERACTION
	public abstract void WaterPlant();
	public abstract void GrabPlant();
	public abstract void TouchPlant();

	// PLANT DEATH
	protected abstract void BeginDeath();
	protected abstract void Decay();
	protected abstract void Die();

	// PLANT EVENT
	public virtual GameObject SpawnChildPlant(){ return null; }

	//helper function
	public Vector2 GetRandomPoint( float minDist, float maxDist)
	{
		Vector2 randomPoint = Random.insideUnitCircle;
		float yOffset = Random.Range( minDist, maxDist );
		float xOffset = Random.Range( minDist, maxDist );
		randomPoint = new Vector2( Mathf.Sign(randomPoint.x) * xOffset +  randomPoint.x, randomPoint.y + yOffset * Mathf.Sign(randomPoint.y) );

		return randomPoint;
	}
}
