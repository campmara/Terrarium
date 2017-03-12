using UnityEngine;

public class CarryState : RollerState 
{
    public override void Enter( P_ControlState prevState )
	{
		Debug.Log("ENTER CARRY STATE");

		_roller.Face.BecomeEncumbered();

		switch( prevState )
		{
		    case P_ControlState.PICKINGUP:
                RollerParent.HandleEndIdle();
                break;
		}
	}

	public override void Exit( P_ControlState nextState )
	{
		Debug.Log("EXIT CARRY STATE");
        
        switch( nextState )
        {
            case P_ControlState.WALKING:
                HandleBothArmRelease();
                break;
        }

		RollerParent.Idling = false;
	}

	public override void HandleInput(InputCollection input)
	{
		RollerParent.IKMovement(RollerConstants.CARRY_SPEED,
									  RollerConstants.WALK_ACCELERATION,
									  RollerConstants.WALK_DECELERATION,
									  RollerConstants.CARRY_TURN_SPEED);

        if (input.AButton.IsPressed)
		{
            // NOTE: Should only happen for seeds ?
			_roller.ChangeState( P_ControlState.PLANTING);
		}

		// Drop if you release the controller triggers...
		if ( input.LeftTrigger.Value < 1.0f || input.RightTrigger.Value < 1.0f )
		{
			_roller.ChangeState( P_ControlState.WALKING);
		}		
	}
}
