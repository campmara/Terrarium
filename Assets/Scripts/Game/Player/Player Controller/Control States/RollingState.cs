using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class RollingState : RollerState 
{
	float turnVelocity = 0f;

    public override void Enter( P_ControlState prevState ) 
	{
		Debug.Log("ENTER ROLLING STATE");

        // Handle Transition from PrevState
        switch ( prevState )
        {
            case P_ControlState.WALKING:
			case P_ControlState.IDLING:
                CameraManager.instance.ChangeCameraState( CameraManager.CameraState.FOLLOWPLAYER_LOCKED );
				PlayerManager.instance.Player.AnimationController.PlayWalkToRollAnim();
                break;
            default:
                break;
        }

    }

	public override void Exit(P_ControlState nextState)
	{
		Debug.Log("EXIT ROLLING STATE");
	}

	public override void HandleInput(InputCollection input)
	{
		// B BUTTON
		if ((input.BButton.WasReleased & input.BButton.HasChanged) || input.BButton.Value == 0)
		{
			_roller.ChangeState( P_ControlState.ROLLING, P_ControlState.WALKING );
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

		Vector3 movePos = _roller.transform.position + (_roller.transform.forward * velocity * Time.deltaTime);
		_roller.RB.MovePosition(movePos);

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

			Quaternion turn = Quaternion.Euler(0f, _roller.transform.eulerAngles.y + (turnVelocity * Time.deltaTime), 0f);
			_roller.RB.MoveRotation(turn);
		}
		else if (turnVelocity != 0f)
		{
			// Slowdown
			turnVelocity -= Mathf.Sign(turnVelocity) * TURN_DECELERATION * Time.deltaTime;
			Quaternion slowTurn = Quaternion.Euler(0f, _roller.transform.eulerAngles.y + (turnVelocity * Time.deltaTime), 0f);
			_roller.RB.MoveRotation(slowTurn);
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
