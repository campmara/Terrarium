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

        // Handle Transition from Walking State
		if (prevState == P_ControlState.WALKING)
		{
			CameraManager.instance.ChangeCameraState( CameraManager.CameraState.FOLLOWPLAYER_LOCKED );

			_roller.BecomeBall();
			_grounded = false;

			_tween.Kill();
			_tween = _roller.RollSphere.transform.DOMoveY(PondManager.instance.Pond.GetPondY(transform.position) + 0.375f, 0.5f)
				.SetEase(Ease.InOutQuint)
				.OnComplete(GroundHit);

			Invoke("StartJiggling", 0.3f);
		}
    }

	private void GroundHit()
	{
		_grounded = true;
	}

	private void StartJiggling()
	{
		float duration = 0.4f;
		Vector3 strength = new Vector3(0.5f, -0.5f, 0f);
		int vibrato = 10;
		float randomness = 0f;
		bool fadeOut = true;

		Tween jiggle = _roller.RollSphere.transform.DOShakeScale(duration, strength, vibrato, randomness, fadeOut);
	}

	public override void Exit(P_ControlState nextState)
	{
		Debug.Log("EXIT ROLLING STATE");
		
		_tween.Kill();
		_tween = null;
	}

    public override void HandleInput( InputCollection input )
    {
		// Ensure we don't move when we roll into an object.
		if (_roller.CollidedWithObject)
		{
			return;
		}

        // B BUTTON
        if (!input.BButton.IsPressed /*&& _grounded == true*/)
        {
            _roller.ChangeState( P_ControlState.WALKING );
        }

        _roller.InputVec = new Vector3( input.LeftStickX, 0f, input.LeftStickY );
    }

    public override void HandleFixedInput( InputCollection input )
	{
		// Ensure we don't move when we roll into an object.
		if (_roller.CollidedWithObject)
		{
			return;
		}

		// MOVEMENT HANDLING
		HandleRolling(input);
		HandleTurning(input);
	}

	private void HandleRolling(InputCollection input)
	{
		if (Mathf.Abs(input.LeftStickY.Value) > RollerConstants.instance.InputDeadzone)
		{
			if (_roller.InputVec.z >= 0f)
			{
				_roller.Accelerate( Mathf.Lerp( RollerConstants.instance.RollSpeed, RollerConstants.instance.RollMaxSpeed, _roller.InputVec.magnitude ), RollerConstants.instance.RollAcceleration, _roller.InputVec.z);
			}
			else
			{
				_roller.Accelerate( Mathf.Lerp( RollerConstants.instance.RollSpeed, RollerConstants.instance.ReverseRollSpeed, _roller.InputVec.magnitude ), RollerConstants.instance.RollAcceleration);
			}
		}
		else
		{
			_roller.Accelerate(RollerConstants.instance.RollSpeed, RollerConstants.instance.RollAcceleration);
		}

		_roller.RB.MovePosition(_roller.transform.position + (_roller.transform.forward * _roller.Velocity * Time.deltaTime));

		// Roll the Roll Sphere
		_roller.RollSphere.transform.Rotate(RollerConstants.instance.RollSphereSpin * Time.deltaTime, 0f, 0f);

		_roller.LastInputVec = _roller.InputVec.normalized;

        // Update the ground paint!
        if (_grounded)
        {
			// Terrain checking.
			Vector3 spherePos = _roller.RollSphere.transform.position;
			spherePos.y = 0.375f + PondManager.instance.Pond.GetPondY(spherePos);
			_roller.RollSphere.transform.position = spherePos;

            GroundManager.instance.Ground.DrawSplatDecal( spherePos, Mathf.Lerp( RollerConstants.instance.RollPaintSize.x, RollerConstants.instance.RollPaintSize.y, Mathf.InverseLerp( RollerConstants.instance.ReverseRollSpeed, RollerConstants.instance.RollMaxSpeed, _roller.Velocity ) ) );            
        }
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
