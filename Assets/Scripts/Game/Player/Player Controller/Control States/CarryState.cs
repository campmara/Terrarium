﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarryState : RollerState 
{
	Quaternion targetRotation = Quaternion.identity;

    Coroutine _carryIdleWaitRoutine = null;

    public override void Enter( P_ControlState prevState )
	{
		Debug.Log("ENTER CARRY STATE");

		switch( prevState )
		{
		case P_ControlState.PICKINGUP:
			PlayerManager.instance.Player.AnimationController.PlayWalkAnim();
			break;
		}
	}

	public override void Exit( P_ControlState nextState )
	{
		Debug.Log("EXIT CARRY STATE");

        if( _carryIdleWaitRoutine != null )
        {
            StopCoroutine( _carryIdleWaitRoutine );
            _carryIdleWaitRoutine = null;
        }
        
        if( nextState != P_ControlState.CARRYIDLING )
        {
            HandleDropHeldObject();
        }

	}

	public override void HandleInput(InputCollection input)
	{
		if (input.AButton.WasPressed)
		{
			_roller.ChangeState( P_ControlState.CARRYING, P_ControlState.WALKING );
		}

		if (input.BButton.WasPressed & input.BButton.HasChanged)
		{
			_roller.ChangeState( P_ControlState.CARRYING, P_ControlState.ROLLING );
		}

		CarryMovement(input);
	}

	void CarryMovement(InputCollection input)
	{
		// Left Stick Movement
		Vector3 vec = new Vector3(input.LeftStickX, 0f, input.LeftStickY);

        if (vec.magnitude > IDLE_MAXMAG)
        {
            if ( _carryIdleWaitRoutine != null )
            {
                StopCoroutine( _carryIdleWaitRoutine );
                _carryIdleWaitRoutine = null;
            }

            // Accounting for camera position
            vec = CameraManager.instance.Main.transform.TransformDirection( vec );
            vec.y = 0f;
            inputVec = vec;

            if (Mathf.Abs( input.LeftStickX.Value ) > INPUT_DEADZONE || Mathf.Abs( input.LeftStickY.Value ) > INPUT_DEADZONE)
            {
                Accelerate( CARRY_SPEED, WALK_ACCELERATION );
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
            _roller.transform.rotation = Quaternion.Slerp( _roller.transform.rotation, targetRotation, CARRY_TURN_SPEED * Time.deltaTime );
        }
        else
        {
            if ( _carryIdleWaitRoutine == null )
            {
                _carryIdleWaitRoutine = StartCoroutine( JohnTech.WaitFunction( IDLE_WAITTIME, () => _roller.ChangeState( P_ControlState.CARRYING, P_ControlState.CARRYIDLING ) ) );
            }
        }

    }
		
}
