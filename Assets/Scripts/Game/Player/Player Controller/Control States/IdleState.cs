using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : RollerState
{	
    public override void Enter( P_ControlState prevState )
    {
        Debug.Log( "ENTER IDLE STATE" );

        // Handle transition
        switch( prevState )
        {
            case P_ControlState.WALKING:
                break;
        }

        // Set Idle Anim
        PlayerManager.instance.Player.AnimationController.PlayIdleAnim();

    }

    public override void Exit( P_ControlState nextState )
    {
        Debug.Log( "EXITTING CARRY IDLE STATE" );
    }

    public override void HandleInput( InputCollection input )
    {
        Vector3 vec = new Vector2( input.LeftStickX, input.LeftStickY );

        if( vec.magnitude > IDLE_MAXMAG )
        {
			_roller.ChangeState( P_ControlState.IDLING, P_ControlState.WALKING );
        }

		// A BUTTON
		if (input.AButton.WasPressed)
		{
			HandlePickup();
		}

		// B BUTTON
		if (input.BButton.WasPressed & input.BButton.HasChanged)
		{
			_roller.ChangeState( P_ControlState.IDLING, P_ControlState.ROLLING );
		}

		// X BUTTON
		if (input.XButton.WasPressed & input.XButton.HasChanged)
		{
			_roller.ChangeState(P_ControlState.IDLING, P_ControlState.RITUAL);
		}
    }


}
