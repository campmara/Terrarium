using UnityEngine;

public static class RollerConstants 
{
	// INPUT
	public const float INPUT_DEADZONE = 0.3f;

	// WALK
	public const float WALK_SPEED = 4f;
	public const float CARRY_SPEED = 3f;
	public const float SING_WALK_SPEED = 2f;
	public const float WALK_ACCELERATION = 0.25f;
	public const float WALK_DECELERATION = 15f;

	// WALK TURNING
	public const float WALK_TURN_SPEED = 5f;
	public const float CARRY_TURN_SPEED = 7f;

	// ROLL
	public const float ROLL_SPEED = 10f;
	public const float ROLL_MAX_SPEED = 13f;
	public const float REVERSE_ROLL_SPEED = 6f;
	public const float ROLL_ACCELERATION = 1f;
	public const float ROLL_DECELERATION = 10f;

	// ROLL TURNING
	public const float TURN_SPEED = 125f;
	public const float REVERSE_TURN_SPEED = 100f;
	public const float TURN_ACCELERATION = 15f;
	public const float TURN_DECELERATION = 700f;

	// PICKUP
	public const float PICKUP_CHECKHEIGHT = 0.5f;
	public const float PICKUP_CHECKRADIUS = 1.0f;
	public const float PICKUP_FORWARDSCALAR = 1.0f;
	public const float PICKUP_UPSCALAR = 1.0f;
	public const float PICKUP_TIME = 0.75f;

	// IDLE
	public const float IDLE_MAXMAG = 0.01f;
	public const float IDLE_WAITTIME = 0.1f;

	// TRANSITIONS
	public const float TRANSITION_TIME = 1f;
	public const float TRANSITION_DECELERATION = 20f;

	// RITUAL DANCE
	public const float RITUAL_TIME = 1.5f;

	// PLANTING
	public const float PLANTING_TIME = 0.75f;
	public const float PLANTING_ENDY = 0f;

	// SINGING
	public const float SINGING_RETURN_TIME = 0.6f;
}
