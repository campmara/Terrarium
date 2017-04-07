using UnityEngine;

public class CarryState : RollerState 
{
	bool canDrop = false;

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

		canDrop = false;
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

	public override void HandleInput( InputCollection input )
	{
		// Makin sure ppl release button to drop the Thing they are carrying.
		if( !canDrop && input.AButton.WasReleased )
		{
			canDrop = true;
		}

		if( canDrop && RollerParent.CurrentHeldObject != null )
		{
			RollerParent.IKMovement( Mathf.Lerp( RollerConstants.instance.CarrySpeed, 0.0f, _roller.CurrentHeldObject.GrabberBurdenInterp ),
				RollerConstants.instance.WalkAcceleration,
				RollerConstants.instance.WalkDeceleration,
				RollerConstants.instance.CarryTurnSpeed );

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

			if ( input.AButton.WasPressed )
			{
				// NOTE: Should only happen for seeds ?
				if( RollerParent.CurrentHeldObject.GetComponent<Seed>() != null )
				{					
					_roller.ChangeState( P_ControlState.PLANTING );
				}
				else
				{
					_roller.ChangeState( P_ControlState.WALKING );	
				}
			}
		}

	}
}
