using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Interface for character control states.
public class RollerState : MonoBehaviour
{
    // ===========
    // S T A T E S
    // ===========

    public enum P_ControlState
    {
        NONE = 0,
        WALKING,
        ROLLING,
        PICKINGUP,
        CARRYING,
        IDLING
    }
    protected P_ControlState _controlState = P_ControlState.NONE;
    public P_ControlState State { get { return _controlState; } set { _controlState = value; } }

    public static WalkingState Walking = new WalkingState();   
	public static RollingState Rolling = new RollingState();
	public static WalkToRollState WalkToRoll = new WalkToRollState();
	public static RollToWalkState RollToWalk = new RollToWalkState();
	public static PickupState Pickup = new PickupState();
	public static CarryState Carrying = new CarryState();
    public static IdleState Idling = new IdleState();

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
	protected const float WALK_TURN_SPEED = 12f;
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
	protected const float PICKUP_TIME = 0.75f;

	// TRANSITIONS
	protected const float TRANSITION_TIME = 1f;
	protected const float TRANSITION_DECELERATION = 20f;

	// =================
	// V A R I A B L E S
	// =================

	protected RollerController roller;

	protected Pickupable currentHeldObject 
	{ 
		get 
		{ 
			return roller.CurrentHeldObject; 
		}
		set
		{
			roller.CurrentHeldObject = value;
		}
	}
	protected Vector3 inputVec
	{ 
		get 
		{ 
			return roller.InputVec; 
		}
		set
		{
			roller.InputVec = value;
		}
	}
	protected Vector3 lastInputVec
	{ 
		get 
		{ 
			return roller.LastInputVec; 
		}
		set
		{
			roller.LastInputVec = value;
		}
  	}
	protected float velocity
	{ 
		get
		{
			return roller.Velocity; 
		}
		set
		{
			roller.Velocity = value;
		}
	}

	protected float transitionTimer = 0f;

	// ============================
	// V I R T U A L  M E T H O D S
	// ============================

	public virtual void Enter(RollerController parent) {}
	public virtual void Exit() {}

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
}
