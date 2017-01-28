using UnityEngine;

public class CarryState : RollerState 
{
    public override void Enter( P_ControlState prevState )
	{
		Debug.Log("ENTER CARRY STATE");

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
                HandleDropHeldObject();
                break;
        }

		RollerParent.Idling = false;
	}

	public override void HandleInput(InputCollection input)
	{
		RollerParent.StandardMovement(RollerConstants.CARRY_SPEED,
									  RollerConstants.WALK_ACCELERATION,
									  RollerConstants.WALK_DECELERATION,
									  RollerConstants.CARRY_TURN_SPEED);

        if (input.AButton.IsPressed)
		{
            // NOTE: Should only happen for seeds ?
			_roller.ChangeState( P_ControlState.CARRYING, P_ControlState.PLANTING );
		}

		if (input.BButton.IsPressed)
		{
			_roller.ChangeState( P_ControlState.CARRYING, P_ControlState.WALKING );
		}		
	}
}
