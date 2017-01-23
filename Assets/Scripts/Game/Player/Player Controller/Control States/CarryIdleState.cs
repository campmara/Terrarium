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
		PlayerManager.instance.Player.AnimationController.PlayIdleAnim();

	}

	public override void Exit( P_ControlState nextState )
	{
		Debug.Log( "EXITTING IDLE STATE" );

		if( nextState != P_ControlState.CARRYING )
		{
			HandleDropHeldObject();
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
			HandlePickup();
		}

		// B BUTTON
		if (input.BButton.WasPressed & input.BButton.HasChanged)
		{
			_roller.ChangeState( P_ControlState.CARRYIDLING, P_ControlState.ROLLING );
		}

		// X BUTTON
		if (input.XButton.WasPressed & input.XButton.HasChanged)
		{
			_roller.ChangeState(P_ControlState.CARRYIDLING, P_ControlState.RITUAL);
		}
	}

}
