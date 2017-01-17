﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class RollingState : RollerState 
{
	float turnVelocity = 0f;

	public override void Enter(RollerController parent)
	{
		Debug.Log("ENTER ROLLING STATE");

		roller = parent;
	}

	public override void Exit()
	{
		Debug.Log("EXIT ROLLING STATE");
	}

	public override void HandleInput(InputCollection input)
	{
		// B BUTTON
		if ((input.BButton.WasReleased & input.BButton.HasChanged) || input.BButton.Value == 0)
		{
			roller.ChangeState(Rolling, RollToWalk);
		}

		// MOVEMENT HANDLING
		inputVec = new Vector3(
			input.LeftStickX,
			0f,
			input.LeftStickY
		);

		HandleRolling(input);
		HandleTurning(input);
	}

	void HandleRolling(InputCollection input)
	{
		if (Mathf.Abs(input.LeftStickY.Value) > INPUT_DEADZONE)
		{
			if (inputVec.z >= 0f)
			{
				Accelerate(ROLL_MAX_SPEED, ROLL_ACCELERATION, inputVec.z);
			}
			else
			{
				Accelerate(REVERSE_ROLL_SPEED, ROLL_ACCELERATION);
			}
		}
		else
		{
			Accelerate(ROLL_SPEED, ROLL_ACCELERATION);
		}

		Vector3 movePos = roller.transform.position + (roller.transform.forward * velocity * Time.deltaTime);
		roller.rigidbody.MovePosition(movePos);

		lastInputVec = inputVec.normalized;
		/*
		else if (velocity != 0f)
		{
			// Slowdown
			velocity -= Mathf.Sign(velocity) * ROLL_DECELERATION * Time.deltaTime;
			Vector3 slowDownPos = roller.transform.position + (roller.transform.forward * velocity * Time.deltaTime);
			roller.rigidbody.MovePosition(slowDownPos);
		}
		*/
	}

	void HandleTurning(InputCollection input)
	{
		if (Mathf.Abs(input.LeftStickX.Value) > INPUT_DEADZONE)
		{
			if (inputVec.z >= -0.2f)
			{
				AccelerateTurn(TURN_SPEED, TURN_ACCELERATION, inputVec.x);
			}
			else
			{
				AccelerateTurn(REVERSE_TURN_SPEED, TURN_ACCELERATION, inputVec.x);
			}

			Quaternion turn = Quaternion.Euler(0f, roller.transform.eulerAngles.y + (turnVelocity * Time.deltaTime), 0f);
			roller.rigidbody.MoveRotation(turn);
		}
		else if (turnVelocity != 0f)
		{
			// Slowdown
			turnVelocity -= Mathf.Sign(turnVelocity) * TURN_DECELERATION * Time.deltaTime;
			Quaternion slowTurn = Quaternion.Euler(0f, roller.transform.eulerAngles.y + (turnVelocity * Time.deltaTime), 0f);
			roller.rigidbody.MoveRotation(slowTurn);
		}
	}

	void AccelerateTurn(float max, float accel, float inputAffect)
	{
		turnVelocity += accel * inputAffect;
		if (Mathf.Abs(turnVelocity) > max)
		{
			turnVelocity = Mathf.Sign(turnVelocity) * max;
		}
	}
}
