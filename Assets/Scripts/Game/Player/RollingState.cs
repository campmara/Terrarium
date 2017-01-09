using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class RollingState : RollerState 
{
	const float ROLL_SPEED = 8f;
	const float TURN_SPEED = 250f;
	const float SLOWDOWN_RATE = 10f;
	const float X_INPUT_DEADZONE = 0.3f;

	RollerController roller;

	Vector3 lastInputVec = Vector3.zero;
	float velocity = 0f;

	public override void Enter(RollerController parent)
	{
		Debug.Log("ENTER ROLLING STATE");

		roller = parent;

		Vector3 posL = roller.transform.position + -roller.transform.right + (roller.transform.up * 0.5f);
		roller.leftArmBlock.transform.DOMove(posL, 1f);

		Vector3 posR = roller.transform.position + roller.transform.right + (roller.transform.up * 0.5f);
		roller.rightArmBlock.transform.DOMove(posR, 1f);

		CameraManager.instance.ChangeCameraState( CameraManager.CameraState.FOLLOWPLAYER_LOCKED );
	}

	public override void Exit()
	{
		Debug.Log("EXIT ROLLING STATE");
	}

	public override void HandleInput(InputCollection input)
	{
		if (input.AButton.WasPressed)
		{

		}

		/*
			B BUTTON
		*/
		if (input.BButton.WasPressed & input.BButton.HasChanged)
		{
			roller.ChangeState(RollerState.Rolling, RollerState.Walking);
		}

		// MOVEMENT HANDLING
		Vector3 inputVec = new Vector3
		(
			input.LeftStickX,
			0f,
			input.LeftStickY
		);

		if (Mathf.Abs(input.LeftStickY.Value) > Mathf.Epsilon)
		{
			velocity = ROLL_SPEED * inputVec.z;
			Vector3 movePos = roller.transform.position + (roller.transform.forward * velocity * Time.deltaTime);
			roller.rigidbody.MovePosition(movePos);

			lastInputVec = inputVec.normalized;
		}

		if (Mathf.Abs(input.LeftStickX.Value) > X_INPUT_DEADZONE)
		{
			Quaternion turn = Quaternion.Euler(0f, roller.transform.eulerAngles.y + (inputVec.x * TURN_SPEED * Time.deltaTime), 0f);
			roller.rigidbody.MoveRotation(turn);

			lastInputVec = inputVec.normalized;
		}

		if (velocity > 0)
		{
			// Slowdown
			velocity -= SLOWDOWN_RATE * Time.deltaTime;
			Vector3 slowDownPos = roller.transform.position + (roller.transform.forward * lastInputVec.z * velocity * Time.deltaTime);
			roller.rigidbody.MovePosition(slowDownPos);
		}
	}
}
