using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class PlantController : MonoBehaviour
{
	protected BasePlant _myPlant = null;
	protected ControllerType _controllerType = ControllerType.Unassigned;
	public ControllerType ControlType { get { return _controllerType; } }

	// ** SPAWN VARS ** // 

	public enum ControllerType
	{
		Death,
		Growth,
		Unassigned
	}

	public abstract void Init();
	public abstract void UpdateState();
	public abstract void StopState();
	public abstract void StartState();

	public virtual GameObject DropFruit(){ return null; }

	public abstract void WaterPlant();
	public abstract void TouchPlant();
	public abstract void GrabPlant();
	public abstract void StompPlant();

//	public Vector2 GetRandomPoint( )
//	{
//		Vector2 randomPoint = UnityEngine.Random.insideUnitCircle;
//		Vector2 offset = new Vector2( Random.Range( _innerMeshRadius, _outerSpawnRadius), Random.Range( _innerMeshRadius, _outerSpawnRadius ) );
//		//float yOffset = UnityEngine.Random.Range( (float)minDist, (float)maxDist );
//		//float xOffset = UnityEngine.Random.Range( minDist, maxDist );
//		//offset = new Vector2( Mathf.Sign(randomPoint.x) * offset.x +  randomPoint.x, randomPoint.y + offset.y * Mathf.Sign(randomPoint.y) );
//		//randomPoint = new Vector2( xOffset, yOffset );
//		randomPoint = new Vector2( Mathf.Sign(randomPoint.x) * offset.x +  randomPoint.x, randomPoint.y + offset.y * Mathf.Sign(randomPoint.y) );
//
//		return randomPoint;
//	}
}