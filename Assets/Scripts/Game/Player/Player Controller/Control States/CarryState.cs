using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarryState : RollerState 
{
	Quaternion targetRotation = Quaternion.identity;

    public override void Enter( P_ControlState prevState )
	{
		Debug.Log("ENTER CARRY STATE");
	}

	public override void Exit( P_ControlState nextState )
	{
		Debug.Log("EXIT CARRY STATE");
		DropHeldObject();
	}

	public override void HandleInput(InputCollection input)
	{
		if (input.AButton.WasPressed)
		{
			_roller.ChangeState( P_ControlState.CARRYING, P_ControlState.WALKING );
		}

		if (input.BButton.WasPressed & input.BButton.HasChanged)
		{
			_roller.ChangeState( P_ControlState.CARRYING, P_ControlState.ROLLING );
		}

		CarryMovement(input);
	}

	void CarryMovement(InputCollection input)
	{
		// Left Stick Movement
		Vector3 vec = new Vector3(input.LeftStickX, 0f, input.LeftStickY);

		// Accounting for camera position
		vec = CameraManager.instance.Main.transform.TransformDirection(vec);
		vec.y = 0f;
		inputVec = vec;

		if (Mathf.Abs(input.LeftStickX.Value) > INPUT_DEADZONE || Mathf.Abs(input.LeftStickY.Value) > INPUT_DEADZONE)
		{
			Accelerate(CARRY_SPEED, WALK_ACCELERATION);
			Vector3 movePos = _roller.transform.position + (inputVec * velocity * Time.deltaTime);
			_roller.RB.MovePosition(movePos);

			targetRotation = Quaternion.LookRotation(inputVec);

			lastInputVec = inputVec.normalized;
		}
		else if (velocity > 0f)
		{
			// Slowdown
			velocity -= WALK_DECELERATION * Time.deltaTime;
			Vector3 slowDownPos = _roller.transform.position + (lastInputVec * velocity * Time.deltaTime);
			_roller.RB.MovePosition(slowDownPos);
		}

		// So player continues turning even after InputUp
		_roller.transform.rotation = Quaternion.Slerp(_roller.transform.rotation, targetRotation, CARRY_TURN_SPEED * Time.deltaTime);
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
