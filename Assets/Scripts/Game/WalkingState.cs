using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class WalkingState : RollerState 
{
	const float WALK_SPEED = 4f;
	const float AUTO_ROTATION_SPEED = 7f;
	const float SLOWDOWN_RATE = 15f;
	const float INPUT_DEADZONE = 0.3f;

	Vector3 lastInputVec;
	float velocity = 0f;

	public override void Enter(RollerController roller)
	{
		Debug.Log("ENTER WALKING STATE");

		Vector3 pos = roller.transform.position + roller.transform.forward + (roller.transform.up * 0.5f);
		roller.leftArmBlock.transform.DOMove(pos, 1f);
		roller.rightArmBlock.transform.DOMove(pos, 1f);
	}

	public override void Exit(RollerController roller)
	{
		Debug.Log("EXIT WALKING STATE");
	}

	public override void HandleInput(RollerController roller, InputCollection input)
	{
		if (input.AButton.WasPressed)
		{

		}

		if (input.BButton.WasPressed)
		{
			roller.ChangeState(RollerState.Rolling);
		}

		// Movement Input!
		Vector3 inputVec = new Vector3
		(
			input.LeftStickX,
			0f,
			input.LeftStickY
		);

		if (Mathf.Abs(input.LeftStickX.Value) > INPUT_DEADZONE || Mathf.Abs(input.LeftStickY.Value) > INPUT_DEADZONE)
		{
			velocity = WALK_SPEED;
			Vector3 movePos = roller.transform.position + (inputVec * velocity * Time.deltaTime);
			roller.rigidbody.MovePosition(movePos);

			Quaternion qTo = Quaternion.LookRotation(inputVec);
			roller.transform.rotation = Quaternion.Slerp(roller.transform.rotation, qTo, AUTO_ROTATION_SPEED * Time.deltaTime);

			lastInputVec = inputVec.normalized;
		}
		else if (velocity > 0)
		{
			// Slowdown
			velocity -= SLOWDOWN_RATE * Time.deltaTime;
			Vector3 slowDownPos = roller.transform.position + (lastInputVec * velocity * Time.deltaTime);
			roller.rigidbody.MovePosition(slowDownPos);
		}
	}
}
