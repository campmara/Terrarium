using UnityEngine;

public class CarryState : RollerState 
{
	float canDropTimer = 0.0f;

    public override void Enter( P_ControlState prevState )
	{
		Debug.Log("ENTER CARRY STATE");

		_roller.Face.BecomeEncumbered();

        _roller.Player.AnimationController.SetCarrying( true );

		switch( prevState )
		{
		    case P_ControlState.PICKINGUP:
                RollerParent.HandleEndIdle();
                break;
		}

		canDropTimer = 0.0f;
	}

	public override void Exit( P_ControlState nextState )
	{
		Debug.Log("EXIT CARRY STATE");

        _roller.Player.AnimationController.SetCarrying( false );

        switch ( nextState )
        {
            case P_ControlState.WALKING:
                HandleBothArmRelease();
                break;
        }
			
		RollerParent.Idling = false;
	}

    public override void HandleInput( InputCollection input )
    {
        if (RollerParent.CurrentHeldObject != null)
        {
            if (!RollerParent.CurrentHeldObject.Carryable)
            {
                if (RollerParent.CurrentHeldObject.GetComponent<SmallPlantPickupable>())
                {
                    if (RollerParent.InputVec.magnitude > 0.1f)
                    {
                        RollerParent.CurrentHeldObject.GetComponent<SmallPlantPickupable>().IncrementTug();
                    }
                    else
                    {
                        RollerParent.CurrentHeldObject.GetComponent<SmallPlantPickupable>().ResetTug();
                    }

                }
            }

            if (input.AButton.WasPressed)
            {
                // NOTE: Should only happen for seeds ?
                if (RollerParent.CurrentHeldObject.GetComponent<Seed>() != null)
                {
                    _roller.ChangeState( P_ControlState.PLANTING );
                }
                else
                {
                    _roller.ChangeState( P_ControlState.WALKING );
                }
            }

            if (input.BButton.WasPressed)
            {
                _roller.ChangeState( P_ControlState.WALKING );
            }
        }
    
    }

    public override void HandleFixedInput( InputCollection input )
	{
        // Makin sure ppl release button to drop the Thing they are carrying.                
        RollerParent.IKMovement( Mathf.Lerp( RollerConstants.instance.CarrySpeed, 0.0f, _roller.CurrentHeldObject.GrabberBurdenInterp ),
            RollerConstants.instance.WalkAcceleration,
            RollerConstants.instance.WalkDeceleration,
            RollerConstants.instance.CarryTurnSpeed );      
	}
}
