using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum P_ControlState
{
	NONE = 0,
	WALKING,
	ROLLING,
	PICKINGUP,
	CARRYING,
	RITUAL,
	IDLING,
	CARRYIDLING,
    PLANTING
}

[RequireComponent(typeof(Rigidbody))]
public class RollerController : ControllerBase 
{
    [ReadOnly]
    Rigidbody _rigidbody = null;
    public Rigidbody RB { get { return _rigidbody; } }

	// These have accessors in the RollerState
	[ReadOnly] Pickupable _currentHeldObject = null;
    public Pickupable CurrentHeldObject { get { return _currentHeldObject; } set { _currentHeldObject = value; } }
	[ReadOnly] Vector3 _inputVec = Vector3.zero;
    public Vector3 InputVec { get { return _inputVec; } set { _inputVec = value; } }
    [ReadOnly] Vector3 _lastInputVec = Vector3.zero;
    public Vector3 LastInputVec { get { return _lastInputVec; } set { _lastInputVec = value; } }
    [ReadOnly] float _velocity = 0f;
    public float Velocity { get { return _velocity; } set { _velocity = value; } }

	// ===========
	// S T A T E S
	// ===========

	// STATE MACHINE
	RollerState _currentState;

	protected P_ControlState _controlState = P_ControlState.NONE;
	public P_ControlState State { get { return _controlState; } set { _controlState = value; } }

	private WalkingState _walking = null;   
	private RollingState _rolling = null;	
	private PickupState _pickup = null;
	private CarryState _carrying = null;
	private RitualState _ritual = null;
	private IdleState _idling = null;
	private CarryIdleState _carryIdling = null;
    private PlantingState _planting = null;

	void Awake()
	{
		//Debug.Log("Added Test Controller to Player Control Manager");
        _rigidbody = GetComponent<Rigidbody>();

		// Add State Controller, Set parent to This Script, set to inactive
		_walking = this.gameObject.AddComponent<WalkingState>();
		_walking.RollerParent = this;
		_walking.enabled = false;

		_rolling = this.gameObject.AddComponent<RollingState>();
		_rolling.RollerParent = this;
		_rolling.enabled = false;

		_pickup = this.gameObject.AddComponent<PickupState>();
		_pickup.RollerParent = this;
		_pickup.enabled = false;

		_carrying = this.gameObject.AddComponent<CarryState>();
		_carrying.RollerParent = this;
		_carrying.enabled = false;

		_ritual = this.gameObject.AddComponent<RitualState>();
		_ritual.RollerParent = this;
		_ritual.enabled = false;

		_idling = this.gameObject.AddComponent<IdleState>();
		_idling.RollerParent = this;
		_idling.enabled = false;
			
		_carryIdling = this.gameObject.AddComponent<CarryIdleState>();
		_carryIdling.RollerParent = this;
		_carryIdling.enabled = false;

        _planting = this.gameObject.AddComponent<PlantingState>();
        _planting.RollerParent = this;
        _planting.enabled = false;

        // Set state to default (walking for now)
        ChangeState( P_ControlState.NONE, P_ControlState.WALKING );
	}

/* Can use these for if we are swapping in & out controllers from Player Control Manager
	void OnEnable()
	{
		//Debug.Log("Enabled Test Controller on Player Control Manager");
	}

	// Also called on Destroy
	void OnDisable()
	{
		//Debug.Log("Disabled Test Controller on Player Control Manager");	
	}
*/

	public void ChangeState(P_ControlState fromState, P_ControlState toState)
	{
		// Exit & Deactivate current state
		if( fromState != P_ControlState.NONE )
		{
			_currentState.Exit( toState );

			_currentState.enabled = false;
		}
			
		// Enter and Activate New State
		switch( toState )
		{
		case P_ControlState.WALKING:
			_currentState = _walking;
			break;
		case P_ControlState.ROLLING:
			_currentState = _rolling;
			break;
		case P_ControlState.PICKINGUP:
			_currentState = _pickup;
			break;
		case P_ControlState.CARRYING:
			_currentState = _carrying;
			break;
		case P_ControlState.RITUAL:
			_currentState = _ritual;
			break;
		case P_ControlState.IDLING:
			_currentState = _idling;
			break;
        case P_ControlState.CARRYIDLING:
            _currentState = _carryIdling;
            break;
        case P_ControlState.PLANTING:
            _currentState = _planting;
            break;
        default:
			break;
		}

		_currentState.enabled = true;

		_currentState.Enter( fromState );
	}


	protected override void HandleInput()
	{
        // Always keep this at zero because the rigidbody's velocity is never needed and bumping into things
        // makes the character go nuts.
        _rigidbody.velocity = Vector3.zero;

		_currentState.HandleInput( _input );
	}
}
