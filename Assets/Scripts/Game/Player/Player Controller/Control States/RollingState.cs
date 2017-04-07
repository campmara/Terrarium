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
				_roller.BecomeBall();
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
			_roller.ChangeState( P_ControlState.WALKING);
		}

		/*
		// X BUTTON
		if (input.XButton.IsPressed)
		{
			_roller.ChangeState(P_ControlState.ROLLING, P_ControlState.RITUAL);
		}
		*/

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
			GroundManager.instance.Ground.DrawSplatDecal(transform.position, 1f);
			//GroundManager.instance.Ground.DrawOnPosition(transform.position, 4f);
		}
	}

	private void HandleRolling(InputCollection input)
	{
		if (Mathf.Abs(input.LeftStickY.Value) > RollerConstants.instance.InputDeadzone)
		{
			if (_roller.InputVec.z >= 0f)
			{
				_roller.Accelerate(RollerConstants.instance.RollMaxSpeed, RollerConstants.instance.RollAcceleration, _roller.InputVec.z);
			}
			else
			{
				_roller.Accelerate(RollerConstants.instance.ReverseRollSpeed, RollerConstants.instance.RollAcceleration);
			}
		}
		else
		{
			_roller.Accelerate(RollerConstants.instance.RollSpeed, RollerConstants.instance.RollAcceleration);
		}

		_roller.RB.MovePosition(_roller.transform.position + (_roller.transform.forward * _roller.Velocity * Time.deltaTime));

		// Roll the Roll Sphere
		_roller.RollSphere.transform.Rotate(RollerConstants.instance.RollSphereSpin * Time.deltaTime, 0f, 0f);

		// Set the sphere y based on distance to pond.
		Vector3 spherePos = _roller.RollSphere.transform.position;
		spherePos.y = 0.375f + PondManager.instance.Pond.GetPondY(spherePos);
		_roller.RollSphere.transform.position = spherePos;

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
		if (Mathf.Abs(input.LeftStickX.Value) > RollerConstants.instance.InputDeadzone)
		{
            if (_roller.InputVec.z >= -0.2f)
			{
				AccelerateTurn(RollerConstants.instance.TurnSpeed, RollerConstants.instance.TurnAcceleration, _roller.InputVec.x);
			}
			else
			{
				AccelerateTurn(RollerConstants.instance.ReverseTurnSpeed, RollerConstants.instance.TurnAcceleration, _roller.InputVec.x);
			}

			Quaternion turn = Quaternion.Euler(0f, _roller.transform.eulerAngles.y + (_turnVelocity * Time.deltaTime), 0f);
			_roller.RB.MoveRotation(turn);
		}
		else if (_turnVelocity != 0f)
		{
			if( Mathf.Abs( _turnVelocity ) < RollerConstants.instance.TurnMinSpeed )
            {
                // Set turn vel to 0 when below certain threshold
                _turnVelocity = 0.0f;
            }
            else
            {
                // Slowdown
                _turnVelocity -= Mathf.Sign( _turnVelocity ) * RollerConstants.instance.TurnDeceleration * Time.deltaTime;
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
