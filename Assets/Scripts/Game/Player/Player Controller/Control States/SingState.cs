using UnityEngine;

public class SingState : RollerState 
{
	Quaternion targetRotation = Quaternion.identity;
	Coroutine _singIdleWaitRoutine = null;

	float waitToReturnTimer = 0f;

	public override void Enter (P_ControlState prevState)
	{
		Debug.Log("ENTER SING STATE");
		waitToReturnTimer = 0f;
	}

	public override void Exit (P_ControlState nextState)
	{
		Debug.Log("EXIT SING STATE");

		if( _singIdleWaitRoutine != null )
		{
			StopCoroutine( _singIdleWaitRoutine );
			_singIdleWaitRoutine = null;
		}

		_idling = false;
	}

	public override void HandleInput (InputCollection input)
	{
		SingMovement(input);

		if (input.YButton.IsPressed)
		{
			FaceManager.instance.SingFace();
		}
		else if (input.YButton.WasReleased)
		{
			FaceManager.instance.NormalFace();
			waitToReturnTimer = 0f;
		}
		else if (!input.YButton.IsPressed && !input.YButton.HasChanged)
		{
			waitToReturnTimer += Time.deltaTime;
		}

		if (waitToReturnTimer >= SINGING_RETURN_TIME)
		{
			waitToReturnTimer = 0f;
			RollerParent.ChangeState(P_ControlState.SING, P_ControlState.WALKING);
		}
	}

	// Slow movement, almost dance-like if we can get that in.
	void SingMovement(InputCollection input)
	{
		// Left Stick Movement
		Vector3 vec = new Vector3(input.LeftStickX, 0f, input.LeftStickY);

		if (vec.magnitude > IDLE_MAXMAG)
		{
			if ( _singIdleWaitRoutine != null )
			{
				StopCoroutine( _singIdleWaitRoutine );
				_singIdleWaitRoutine = null;
			}

			if( _idling )
			{
				HandleEndIdle();
			}

			// Accounting for camera position
			vec = CameraManager.instance.Main.transform.TransformDirection( vec );
			vec.y = 0f;
			inputVec = vec;

			if (Mathf.Abs( input.LeftStickX.Value ) > INPUT_DEADZONE || Mathf.Abs( input.LeftStickY.Value ) > INPUT_DEADZONE)
			{
				Accelerate( SING_WALK_SPEED, WALK_ACCELERATION );
				Vector3 movePos = _roller.transform.position + ( inputVec * velocity * Time.deltaTime );
				_roller.RB.MovePosition( movePos );

				targetRotation = Quaternion.LookRotation( inputVec );

				lastInputVec = inputVec.normalized;
			}
			else if (velocity > 0f)
			{
				// Slowdown
				velocity -= WALK_DECELERATION * Time.deltaTime;
				Vector3 slowDownPos = _roller.transform.position + ( lastInputVec * velocity * Time.deltaTime );
				_roller.RB.MovePosition( slowDownPos );
			}

			// So player continues turning even after InputUp
			_roller.transform.rotation = Quaternion.Slerp( _roller.transform.rotation, targetRotation, WALK_TURN_SPEED * Time.deltaTime );
		}
		else
		{
			if ( _singIdleWaitRoutine == null )
			{
				_singIdleWaitRoutine = StartCoroutine( JohnTech.WaitFunction( IDLE_WAITTIME, () => HandleBeginIdle() ) );
			}
		}
	}

	void HandleBeginIdle()
	{
		_idling = true;
		PlayerManager.instance.Player.AnimationController.PlayCarryIdleAnim();
	}

	void HandleEndIdle()
	{
		_idling = false;
		PlayerManager.instance.Player.AnimationController.PlayWalkAnim();
	}
}
