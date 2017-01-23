using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class WalkingState : RollerState
{
	Quaternion targetRotation = Quaternion.identity;

    Tween _idleWaitTween = null;
    float _idleTimer = 0.0f;

    public override void Enter( P_ControlState prevState )
	{
		Debug.Log("ENTER WALKING STATE");

        // Handle Transition
        switch ( prevState )
        {
        case P_ControlState.ROLLING:
			CameraManager.instance.ChangeCameraState( CameraManager.CameraState.FOLLOWPLAYER_FREE );
			PlayerManager.instance.Player.AnimationController.PlayRollToWalkAnim();
            break;
		case P_ControlState.IDLING:
			PlayerManager.instance.Player.AnimationController.PlayWalkAnim();
			break;
        default:
                break;
        }

        //PlayerManager.instance.Player.AnimationController.PlayWalkAnim();
	}

	public override void Exit(P_ControlState nextState)
	{
		Debug.Log("EXIT WALKING STATE");
	}

	public override void HandleInput(InputCollection input)
	{
		// A BUTTON
		if (input.AButton.WasPressed)
		{
			HandlePickup();
		}

		// B BUTTON
		if (input.BButton.WasPressed & input.BButton.HasChanged)
		{
			_roller.ChangeState( P_ControlState.WALKING, P_ControlState.ROLLING );
		}

		// X BUTTON
		if (input.XButton.WasPressed & input.XButton.HasChanged)
		{
			_roller.ChangeState(P_ControlState.WALKING, P_ControlState.RITUAL);
		}

		WalkMovement(input);
	}

	// ===============
	// M O V E M E N T
	// ===============

	void WalkMovement(InputCollection input)
	{
		// Left Stick Movement
		Vector3 vec = new Vector3(input.LeftStickX, 0f, input.LeftStickY);

        if( vec.magnitude > IDLE_MAXMAG )
        {
            if( _idleWaitTween != null )
            {
                _idleWaitTween.Kill();
                _idleWaitTween = null;
            }

            // Accounting for camera position
            vec = CameraManager.instance.Main.transform.TransformDirection( vec );
            vec.y = 0f;
            inputVec = vec;

            if (Mathf.Abs( input.LeftStickX.Value ) > INPUT_DEADZONE || Mathf.Abs( input.LeftStickY.Value ) > INPUT_DEADZONE)
            {
                Accelerate( WALK_SPEED, WALK_ACCELERATION );
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
            if(_idleWaitTween == null )
            {
                _idleTimer = 0.0f;

                _idleWaitTween = DOTween.To( () => _idleTimer, x => _idleTimer = x, 1.0f, IDLE_WAITTIME ).OnComplete( () => _roller.ChangeState( P_ControlState.WALKING, P_ControlState.IDLING ) );
            }
        }

	}

}
