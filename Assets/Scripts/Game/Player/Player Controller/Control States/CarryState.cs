using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarryState : RollerState 
{
	Quaternion targetRotation = Quaternion.identity;

	public override void Enter(RollerController parent)
	{
		Debug.Log("ENTER CARRY STATE");
		roller = parent;
	}

	public override void Exit()
	{
		Debug.Log("EXIT CARRY STATE");
		DropHeldObject();
	}

	public override void HandleInput(InputCollection input)
	{
		if (input.AButton.WasPressed)
		{
			roller.ChangeState(Carrying, Walking);
		}

		if (input.BButton.WasPressed & input.BButton.HasChanged)
		{
			roller.ChangeState(Carrying, WalkToRoll);
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
			Vector3 movePos = roller.transform.position + (inputVec * velocity * Time.deltaTime);
			roller.RB.MovePosition(movePos);

			targetRotation = Quaternion.LookRotation(inputVec);

			lastInputVec = inputVec.normalized;
		}
		else if (velocity > 0f)
		{
			// Slowdown
			velocity -= WALK_DECELERATION * Time.deltaTime;
			Vector3 slowDownPos = roller.transform.position + (lastInputVec * velocity * Time.deltaTime);
			roller.RB.MovePosition(slowDownPos);
		}

		// So player continues turning even after InputUp
		roller.transform.rotation = Quaternion.Slerp(roller.transform.rotation, targetRotation, CARRY_TURN_SPEED * Time.deltaTime);
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
