using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class WalkingState : RollerState
{
	Quaternion targetRotation = Quaternion.identity;

    Coroutine _idleWaitRoutine = null;

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
        default:
                break;
        }

        PlayerManager.instance.Player.AnimationController.PlayWalkAnim();
	}

	public override void Exit(P_ControlState nextState)
	{
		Debug.Log("EXIT WALKING STATE");

        if (_idleWaitRoutine != null)
        {
            StopCoroutine( _idleWaitRoutine );
            _idleWaitRoutine = null;
        }

        _idling = false;
    }

	public override void HandleInput(InputCollection input)
	{   
        // A BUTTON
        if (input.AButton.WasPressed)
        {
            HandlePickup();
        }

        WalkMovement( input );

        // B BUTTON
        if (input.BButton.WasPressed)
        {
            _roller.ChangeState( P_ControlState.WALKING, P_ControlState.ROLLING );
        }

        // X BUTTON
        if (input.XButton.WasPressed)
        {
            _roller.ChangeState( P_ControlState.WALKING, P_ControlState.RITUAL );
        }

		// Y BUTTON
		if (input.YButton.WasPressed)
		{
			_roller.ChangeState( P_ControlState.WALKING, P_ControlState.SING );
		}
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
            if( _idleWaitRoutine != null )
            {
                StopCoroutine( _idleWaitRoutine );
                _idleWaitRoutine = null;
            }

            if (_idling)
            {
                HandleEndIdle();
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
            if( _idleWaitRoutine == null )
            {
                _idleWaitRoutine = StartCoroutine( JohnTech.WaitFunction( IDLE_WAITTIME, () => HandleBeginIdle() ) );
            }
        }

	}

    void HandleBeginIdle()
    {
        _idling = true;

        PlayerManager.instance.Player.AnimationController.PlayIdleAnim();
    }

    void HandleEndIdle()
    {
        _idling = false;

        PlayerManager.instance.Player.AnimationController.PlayWalkAnim();
    }
}
