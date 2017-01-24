using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarryIdleState : RollerState 
{
	public override void Enter( P_ControlState prevState )
	{
		Debug.Log( "ENTER CARRY IDLE STATE" );

		// Handle transition
		switch( prevState )
		{
		case P_ControlState.CARRYING:
			break;
		}

		// Set Idle Anim
		PlayerManager.instance.Player.AnimationController.PlayCarryIdleAnim();

	}

	public override void Exit( P_ControlState nextState )
	{
		Debug.Log( "EXITTING IDLE STATE" );

        switch( nextState )
        {
            case P_ControlState.WALKING:
                HandleDropHeldObject();
                break;
            default:
                break;
        }
	}

	public override void HandleInput( InputCollection input )
	{
		Vector3 vec = new Vector2( input.LeftStickX, input.LeftStickY );

		if( vec.magnitude > IDLE_MAXMAG )
		{
			_roller.ChangeState( P_ControlState.CARRYIDLING, P_ControlState.CARRYING );
		}

		// A BUTTON
		if (input.AButton.WasPressed)
		{
            _roller.ChangeState( P_ControlState.CARRYIDLING, P_ControlState.PLANTING );
        }

		// B BUTTON
		if (input.BButton.WasPressed & input.BButton.HasChanged)
		{
			_roller.ChangeState( P_ControlState.CARRYIDLING, P_ControlState.WALKING );
		}
	}

}
