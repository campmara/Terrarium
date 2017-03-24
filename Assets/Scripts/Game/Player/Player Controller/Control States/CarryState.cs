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
		if( RollerParent.CurrentHeldObject != null )
		{
			RollerParent.IKMovement( Mathf.Lerp( RollerConstants.instance.CARRY_SPEED, 0.0f, _roller.CurrentHeldObject.GrabberBurdenInterp ),
				RollerConstants.instance.WALK_ACCELERATION,
				RollerConstants.instance.WALK_DECELERATION,
				RollerConstants.instance.CARRY_TURN_SPEED );

			if( !RollerParent.CurrentHeldObject.Carryable )
			{
				if( RollerParent.CurrentHeldObject.GetComponent<SmallPlantPickupable>() )
				{
					if( RollerParent.InputVec.magnitude > 0.1f )
					{
						RollerParent.CurrentHeldObject.GetComponent<SmallPlantPickupable>().IncrementTug();	
					}
					else
					{
						RollerParent.CurrentHeldObject.GetComponent<SmallPlantPickupable>().ResetTug();
					}

				}
			}

			if (input.AButton.IsPressed)
			{
				// NOTE: Should only happen for seeds ?
				_roller.ChangeState( P_ControlState.PLANTING);
			}

			// Drop if you release the controller triggers...
			if ( !_roller.CurrentHeldObject.Grabbed || input.LeftTrigger.Value < 1.0f || input.RightTrigger.Value < 1.0f )
			{
				_roller.ChangeState( P_ControlState.WALKING);
			}
		}
		else
		{
			_roller.ChangeState( P_ControlState.WALKING);
		}

	}
}
