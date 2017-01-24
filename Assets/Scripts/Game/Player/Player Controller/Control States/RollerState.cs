using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Interface for character control states.
public class RollerState : MonoBehaviour
{
	// =================
	// C O N S T A N T S
	// =================

	// INPUT
	protected const float INPUT_DEADZONE = 0.3f;

	// WALK
	protected const float WALK_SPEED = 4f;
	protected const float CARRY_SPEED = 3f;
	protected const float WALK_ACCELERATION = 0.25f;
	protected const float WALK_DECELERATION = 15f;

	// WALK TURNING
	protected const float WALK_TURN_SPEED = 5f;
	protected const float CARRY_TURN_SPEED = 7f;

	// ROLL
	protected const float ROLL_SPEED = 10f;
	protected const float ROLL_MAX_SPEED = 13f;
	protected const float REVERSE_ROLL_SPEED = 6f;
	protected const float ROLL_ACCELERATION = 1f;
	protected const float ROLL_DECELERATION = 10f;

	// ROLL TURNING
	protected const float TURN_SPEED = 125f;
	protected const float REVERSE_TURN_SPEED = 100f;
	protected const float TURN_ACCELERATION = 15f;
	protected const float TURN_DECELERATION = 700f;

	// PICKUP
	protected const float PICKUP_CHECKHEIGHT = 0.5f;
	protected const float PICKUP_CHECKRADIUS = 1.0f;
    protected const float PICKUP_FORWARDSCALAR = 1.0f;
    protected const float PICKUP_UPSCALAR = 1.0f;
	protected const float PICKUP_TIME = 0.75f;

    // IDLE
    protected const float IDLE_MAXMAG = 0.01f;
    protected const float IDLE_WAITTIME = 0.1f;

    // TRANSITIONS
    protected const float TRANSITION_TIME = 1f;
	protected const float TRANSITION_DECELERATION = 20f;

	// RITUAL DANCE
	protected const float RITUAL_TIME = 1.5f;

    // PLANTING
    protected const float PLANTING_TIME = 0.75f;
    protected const float PLANTING_ENDY = 0f;

	// =================
	// V A R I A B L E S
	// =================

	protected RollerController _roller;
	public RollerController RollerParent { get { return _roller; } set { _roller = value; } }

	protected Pickupable currentHeldObject 
	{ 
		get 
		{ 
			return _roller.CurrentHeldObject; 
		}
		set
		{
			_roller.CurrentHeldObject = value;
		}
	}
	protected Vector3 inputVec
	{ 
		get 
		{ 
			return _roller.InputVec; 
		}
		set
		{
			_roller.InputVec = value;
		}
	}
	protected Vector3 lastInputVec
	{ 
		get 
		{ 
			return _roller.LastInputVec; 
		}
		set
		{
			_roller.LastInputVec = value;
		}
  	}
	protected float velocity
	{ 
		get
		{
			return _roller.Velocity; 
		}
		set
		{
			_roller.Velocity = value;
		}
	}

	protected float transitionTimer = 0f;

    protected bool _idling = false;

	// ============================
	// V I R T U A L  M E T H O D S
	// ============================

	public virtual void Enter( P_ControlState prevState ) {}
	public virtual void Exit( P_ControlState nextState ) {}

	public virtual void HandleInput(InputCollection input) {}
	public virtual void ProcessMovement() {}

	// ==========================
	// H E L P E R  M E T H O D S
	// ==========================

	protected void Accelerate(float max, float accel, float inputAffect = 1.0f)
	{
		velocity += accel * inputAffect;
		if (Mathf.Abs(velocity) > max)
		{
			velocity = Mathf.Sign(velocity) * max;
		}
	}

	protected void HandlePickup()
	{
		CheckForPickup();
	}
		
	private void CheckForPickup()
	{
		Vector3 _pickupCenter = _roller.transform.position + ( Vector3.up * PICKUP_CHECKHEIGHT ) + _roller.transform.forward;

		Collider[] overlapArray = Physics.OverlapSphere( _pickupCenter, PICKUP_CHECKRADIUS );

		if ( overlapArray.Length > 0 )
		{
			//if the pickupable is a plant, we can only pick it up if it's still in seed stage
			foreach( Collider col in overlapArray )
			{
				Pickupable pickup = col.gameObject.GetComponent<Pickupable>();
				if( pickup )
				{
					PickUpObject( pickup );
					break;
				}
			}

		}
	}

	private void PickUpObject( Pickupable pickup )
	{
		currentHeldObject = pickup;
		_roller.ChangeState( _roller.State, P_ControlState.PICKINGUP );
	}

	protected void HandleDropHeldObject()
	{
		DropHeldObject();
	}

	void DropHeldObject()
	{
		if (currentHeldObject != null)
		{
			currentHeldObject.DropSelf();
			currentHeldObject = null;
		}
	}
}
