using System.Collections;
using UnityEngine;

public class BasePlant : MonoBehaviour
{	protected float _innerMeshRadius = 0.3f;
	public float InnerRadius { get { return _innerMeshRadius; } set { _innerMeshRadius = value;} }

	protected float _outerSpawnRadius = 2.5f;
	public float OuterRadius { get { return _outerSpawnRadius; } set { _outerSpawnRadius = value;} }

	public enum PlantType : int 
	{
		NONE = -1,
		POINT,
		FLOWERING,
		LEAFY,
		LIMBER,
		PBUSH,
		BUMBLE,
		HUPPET,
		JIMBO
	}
	[SerializeField] PlantType _type = PlantType.NONE;
	public PlantType MyPlantType { get { return _type; } }

    // *************
    // DEATH VARS
    // **************

    [SerializeField, ReadOnlyAttribute]
    protected float _deathDuration = 0.0f;
	public float DeathDuration { get { return _deathDuration; } set { _deathDuration = value; } }

	[SerializeField] Vector2 _DeathDurationRange = new Vector2( 0.0f, 10.0f );

    [SerializeField, ReadOnlyAttribute]
	protected float _DeathTimer = 0.0f;
	public float DeathTimer { get { return _DeathTimer; } set { _DeathTimer = Mathf.Max(value, 0.0f); } }

	[SerializeField] protected float _baseDecayRate = 0.0f;
	public float BaseDecayRate { get { return _baseDecayRate; } set { _baseDecayRate = value; } }
	[SerializeField, ReadOnlyAttribute]protected float _curDecayRate = 0.0f;
	public float CurDecayRate { get { return _curDecayRate; } set { _curDecayRate = value; } }
	protected float _wateredDecayRate = -0.1f;
	public float WateredDecayRate { get { return _wateredDecayRate; } set { _wateredDecayRate = value; } }

	Coroutine _decayReturnRoutine = null;
	public Coroutine DecayReturnRoutine { get { return _decayReturnRoutine; } set { _decayReturnRoutine = value; } }
	Coroutine _growReturnRoutine = null;
	public Coroutine GrowReturnRoutine { get { return _growReturnRoutine; } set { _growReturnRoutine = value; } }

	// *************
	// STATE CONTROLLER
	// **************

	protected PlantController _activeController = null;
    public PlantController ActiveController { get { return _activeController; } }
	PlantController[] _controllers = new PlantController[2];

	float _shaderColorSeed = 0.0f;
	public float ShaderColorSeed { get { return _shaderColorSeed; } }

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

		_shaderColorSeed = Random.Range( -5000.0000f, 5000.0000f );
		Renderer childRenderer = this.GetComponentInChildren<Renderer>();
		if (childRenderer != null && childRenderer.material.HasProperty( "_ColorSetSeed" ) )
		{
			childRenderer.material.SetFloat( "_ColorSetSeed", _shaderColorSeed );
		}

		foreach( PlantController control in _controllers )
		{
			control.Init();
			if( control.ControlType == PlantController.ControllerType.Growth )
			{
				_activeController = control;
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
		if( _activeController.ControlType == PlantController.ControllerType.Death )
		{
			foreach( PlantController controller in _controllers )
			{
				controller.GrabPlant();
			}
		}
		else
		{
			_activeController.GrabPlant();
		}
	}

	public void TouchPlant()
	{
		_controllers[0].TouchPlant();
//		_activeController.TouchPlant();
	}

	public void StompPlant()
	{
		_activeController.StompPlant();
	}

	public virtual GameObject DropFruit()
	{
		return _activeController.DropFruit();
	}

	public virtual void HandleSinging( bool playerEnter ){}

	void OnDestroy()
	{
		PlantManager.ExecuteGrowth -= UpdatePlant;

		if( _decayReturnRoutine != null )
		{
			StopCoroutine( _decayReturnRoutine );
			_decayReturnRoutine = null;
		}
	}

	public IEnumerator DelayedReturnDecayRate( float returnTime )
	{
		yield return new WaitForSeconds( returnTime );

		_curDecayRate = _baseDecayRate;
	}

	void OnTriggerStay( Collider col )
	{
		CheckReactToPlayer( col, true );
	}

	void OnTriggerExit( Collider col )
	{
		CheckReactToPlayer( col, false );
	}

	void CheckReactToPlayer( Collider col, bool entering )
	{
			if( col.GetComponent<Player>() || col.GetComponentInChildren<Player>() )
			{
				//_controllers[0].TouchPlant();
				_controllers[0].HandleSinging( entering );
			}
	}
}
