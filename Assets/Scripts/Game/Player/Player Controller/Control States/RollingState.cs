using UnityEngine;
using DG.Tweening;

public class RollingState : RollerState 
{
	private float _turnVelocity = 0f;
	private bool _grounded = false;

	private Tween _tween;

    public override void Enter( P_ControlState prevState ) 
	{
		Debug.Log("ENTER ROLLING STATE");

        // Handle Transition from PrevState
        switch ( prevState )
        {
            case P_ControlState.WALKING:
                CameraManager.instance.ChangeCameraState( CameraManager.CameraState.FOLLOWPLAYER_LOCKED );
				//PlayerManager.instance.Player.AnimationController.PlayWalkToRollAnim();
				BecomeBall();
				_grounded = false;
				_tween = _roller.RollSphere.transform.DOMoveY(0.375f, 0.5f)
					.SetEase(Ease.OutBounce)
					.OnComplete(GroundHit);

				
                break;
        }

    }

	private void GroundHit()
	{
		_grounded = true;
	}

	public override void Exit(P_ControlState nextState)
	{
		Debug.Log("EXIT ROLLING STATE");
		if (_tween != null)
	    {
	        _tween.Kill();
	        _tween = null;
	    }
	}

	public override void HandleInput(InputCollection input)
	{
		// B BUTTON
		if (!input.BButton.IsPressed)
		{
			_roller.ChangeState( P_ControlState.ROLLING, P_ControlState.WALKING );
		}

		// X BUTTON
		if (input.XButton.IsPressed)
		{
			_roller.ChangeState(P_ControlState.ROLLING, P_ControlState.RITUAL);
		}

		// MOVEMENT HANDLING
		_roller.InputVec = new Vector3(
			input.LeftStickX,
			0f,
			input.LeftStickY
		);

		HandleRolling(input);
		HandleTurning(input);

		// Update the ground paint!
		if (_grounded)
		{
			GroundManager.instance.Ground.DrawOnPosition(transform.position, 1.5f);
		}
	}

	private void HandleRolling(InputCollection input)
	{
		if (Mathf.Abs(input.LeftStickY.Value) > RollerConstants.INPUT_DEADZONE)
		{
			if (_roller.InputVec.z >= 0f)
			{
				_roller.Accelerate(RollerConstants.ROLL_MAX_SPEED, RollerConstants.ROLL_ACCELERATION, _roller.InputVec.z);
			}
			else
			{
				_roller.Accelerate(RollerConstants.REVERSE_ROLL_SPEED, RollerConstants.ROLL_ACCELERATION);
			}
		}
		else
		{
			_roller.Accelerate(RollerConstants.ROLL_SPEED, RollerConstants.ROLL_ACCELERATION);
		}

		Vector3 movePos = _roller.transform.position + (_roller.transform.forward * _roller.Velocity * Time.deltaTime);
		_roller.RB.MovePosition(movePos);

		_roller.LastInputVec = _roller.InputVec.normalized;
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

	private void HandleTurning(InputCollection input)
	{
		if (Mathf.Abs(input.LeftStickX.Value) > RollerConstants.INPUT_DEADZONE)
		{
            if (_roller.InputVec.z >= -0.2f)
			{
				AccelerateTurn(RollerConstants.TURN_SPEED, RollerConstants.TURN_ACCELERATION, _roller.InputVec.x);
			}
			else
			{
				AccelerateTurn(RollerConstants.REVERSE_TURN_SPEED, RollerConstants.TURN_ACCELERATION, _roller.InputVec.x);
			}

			Quaternion turn = Quaternion.Euler(0f, _roller.transform.eulerAngles.y + (_turnVelocity * Time.deltaTime), 0f);
			_roller.RB.MoveRotation(turn);
		}
		else if (_turnVelocity != 0f)
		{
			if( Mathf.Abs( _turnVelocity ) < RollerConstants.TURN_MINSPEED )
            {
                // Set turn vel to 0 when below certain threshold
                _turnVelocity = 0.0f;
            }
            else
            {
                // Slowdown
                _turnVelocity -= Mathf.Sign( _turnVelocity ) * RollerConstants.TURN_DECELERATION * Time.deltaTime;
            }
            
			Quaternion slowTurn = Quaternion.Euler(0f, _roller.transform.eulerAngles.y + (_turnVelocity * Time.deltaTime), 0f);
			_roller.RB.MoveRotation(slowTurn);
		}

	}

	private void AccelerateTurn(float max, float accel, float inputAffect)
	{
	    _turnVelocity += accel * inputAffect;
		if (Mathf.Abs(_turnVelocity) > max)
		{
		    _turnVelocity = Mathf.Sign(_turnVelocity) * max;
		}
	}
}
