using UnityEngine;

public class CarryState : RollerState 
{

	[SerializeField,ReadOnlyAttribute]float _currHugWidth = 0.0f;
	[SerializeField,ReadOnlyAttribute]float _currHugState = 0.0f;
	[SerializeField,ReadOnlyAttribute]float _heldObjWidthInterp = 0.0f;

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
		_currHugWidth = 0.0f;
     	_currHugState = 0.0f;
	}

	public override void Exit( P_ControlState nextState )
	{
		Debug.Log("EXIT CARRY STATE");

        _roller.Player.AnimationController.SetCarrying( false );
        _roller.Player.AnimationController.SetLifting( false );

        switch ( nextState )
        {
            case P_ControlState.WALKING:
                HandleBothArmRelease();
                break;
            case P_ControlState.ROLLING:
                HandleBothArmRelease();
                break;
        }
			
		_roller.Player.AnimationController.SetHugState( 0.0f );
		_roller.Player.AnimationController.SetHugWidth( 0.0f );
		RollerParent.Idling = false;
	}

    public override void HandleInput( InputCollection input )
    {
        if ( RollerParent.CurrentHeldObject != null )
        {
            if ( !RollerParent.CurrentHeldObject.Carryable )
            {
                if ( RollerParent.CurrentHeldObject.GetComponent<SmallPlantPickupable>() )
                {
                    if ( RollerParent.InputVec.magnitude > 0.1f )
                    {
                        RollerParent.CurrentHeldObject.GetComponent<SmallPlantPickupable>().IncrementTug();
                    }
                    else
                    {
                        RollerParent.CurrentHeldObject.GetComponent<SmallPlantPickupable>().ResetTug();
                    }

                }
				else if( RollerParent.CurrentHeldObject.GetComponent<BigPlantPickupable>() )
				{
                    _roller.transform.LookAt( _roller.CurrentHeldObject.transform );

                    _currHugState = Mathf.Lerp( _currHugState, 1.0f, RollerConstants.instance.HugLerpSpeed * Time.deltaTime );
					_currHugWidth = Mathf.Lerp( _currHugWidth, 1.0f, RollerConstants.instance.HugLerpSpeed * Time.deltaTime );
					_roller.Player.AnimationController.SetHugState( _currHugState );

					_heldObjWidthInterp = Mathf.InverseLerp( RollerConstants.instance.HugWidthRange.x, RollerConstants.instance.HugWidthRange.y, _roller.CurrentHeldObject.transform.localScale.x );
					_roller.Player.AnimationController.SetHugWidth( Mathf.Lerp( 0.0f, _heldObjWidthInterp, _currHugWidth ) );
				}
            }
			else
			{
				_roller.CarryPositionObject.transform.position = _roller.IK.ArmTipMidpoint;
				_roller.CurrentHeldObject.transform.localPosition = Vector3.zero;
			}

            if ( input.AButton.WasReleased || !input.AButton.IsPressed )
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

            if (input.XButton.WasPressed)
            {
                _roller.ChangeState(P_ControlState.RITUAL);
            }
        }
    }

    public override void HandleFixedInput( InputCollection input )
	{
        // Makin sure ppl release button to drop the Thing they are carrying.                
        RollerParent.IKMovement( Mathf.Lerp( RollerConstants.instance.CarrySpeed, 0.0f, _roller.CurrentHeldObject.GrabberBurdenInterp ),
            RollerConstants.instance.WalkAcceleration,
            RollerConstants.instance.WalkDeceleration,
            Mathf.Lerp( RollerConstants.instance.CarryTurnSpeed, 0.0f, _roller.CurrentHeldObject.GrabberBurdenInterp ) );      
	}
}
