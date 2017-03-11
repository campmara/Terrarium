using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasePlant : MonoBehaviour
{
	[SerializeField] protected BasePlantAssetKey _pAssetKey = BasePlantAssetKey.NONE;
	public BasePlantAssetKey PAssetKey { get { return _pAssetKey; } set { _pAssetKey = value; } }

	// *************
	// DEATH VARS
	// **************

	protected float _deathDuration = 0.0f;
	public float DeathDuration { get { return _deathDuration; } set { _deathDuration = value; } }

	[SerializeField] Vector2 _DeathDurationRange = new Vector2( 0.0f, 10.0f );

	protected float _DeathTimer = 0.0f;
	public float DeathTimer { get { return _DeathTimer; } set { _DeathTimer = value; } }

	[SerializeField] protected float _baseDecayRate = 0.0f;
	public float BaseDecayRate { get { return _baseDecayRate; } set { _baseDecayRate = value; } }
	protected float _curDecayRate = 0.0f;
	public float CurDecayRate { get { return _curDecayRate; } set { _curDecayRate = value; } }
	protected float _wateredDecayRate = 0.0f;
	public float WateredDecayRate { get { return _wateredDecayRate; } set { _wateredDecayRate = value; } }

	// *************
	// STATE CONTROLLER
	// **************

	protected PlantController _activeController = null;
	PlantController[] _controllers = new PlantController[2];

	protected virtual void Awake()
	{
		Init();
	}

	public virtual void SwitchController( PlantController prevState )
	{
		if( _controllers[0] == prevState )
		{
			_activeController = _controllers[1];
		}
		else
		{
			_activeController = _controllers[0];
		}

		_activeController.StartState();
	}

	void Init() 
	{
		DeathTimer = 0.0f;
		CurDecayRate = BaseDecayRate;
		DeathDuration = Random.Range( _DeathDurationRange.x, _DeathDurationRange.y );

		_controllers = GetComponents<PlantController>();

		foreach( PlantController control in _controllers )
		{
			if( control.ControlType == PlantController.ControllerType.Growth )
			{
				_activeController = control;
				break;
			}
		}

		if( _activeController )
		{
			_activeController.StartState();
			PlantManager.ExecuteGrowth += UpdatePlant;
		}
		else
		{
			Debug.Log( "SOMETHING WENT WRONG WITH THE PLANT CONTROLLER. PLEASE MAKE SURE A CONTROLLER TYPE IS ASSIGNED." );
		}
	}

	public void UpdatePlant()
	{
		_activeController.UpdateState();
	}

	public virtual void WaterPlant()
	{
		_activeController.WaterPlant();
	}

	public void GrabPlant()
	{
		_activeController.GrabPlant();
	}

	public void TouchPlant()
	{
		_activeController.TouchPlant();
	}

	public void StompPlant()
	{
		_activeController.StompPlant();
	}

	public virtual GameObject SpawnChildPlant()
	{
		return _activeController.SpawnChildPlant();
	}

	public virtual GameObject DropFruit()
	{
		return _activeController.DropFruit();
	}

	void OnDestroy()
	{
		PlantManager.ExecuteGrowth -= UpdatePlant;
	}

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
