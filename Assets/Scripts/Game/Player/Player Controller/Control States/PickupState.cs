using System.Collections;
using UnityEngine;
using DG.Tweening;

public class PickupState : RollerState 
{
    Sequence _pickupSequence = null;

	bool _startLiftTracking = false;

    public override void Enter(P_ControlState prevState)
	{
		Debug.Log("ENTER PICKUP STATE");

		_startLiftTracking = false;

		_roller.Face.TransitionFacePose( "Pickup" );

		// If Carryable the object should be parented to be moved around
		if( !_roller.CurrentHeldObject.Carryable )
		{
			_roller.CurrentHeldObject.OnPickup( this.transform );

            TransitionToCarrying();
		}
        else
        {
            Vector3 heldObjPos = _roller.CurrentHeldObject.transform.position;
            heldObjPos.y = _roller.transform.position.y;
            _roller.transform.LookAt( heldObjPos );
        }

	}

	public void TransitionToCarrying()
	{
        _roller.Player.AnimationController.SetCarrying( true );

        _roller.ChangeState(P_ControlState.CARRYING);
	}

	public override void Exit( P_ControlState nextState )
	{
		Debug.Log("EXIT PICKUP STATE");

        switch (nextState)
        {
            case P_ControlState.WALKING:
                _roller.Player.AnimationController.TriggerLiftCancel();
                HandleBothArmRelease(); 
				_roller.Face.BecomeIdle();
                break;
		case P_ControlState.CARRYING:
			if( _startLiftTracking )
			{
				_roller.CarryPosOffset =  _roller.CarryPositionObject.transform.position - _roller.transform.position;
			}
			break;
        }  


	}

    public override void HandleInput( InputCollection input )
    {
        if( input.AButton.WasReleased || _roller.CurrentHeldObject == null )
        {
            _roller.ChangeState( P_ControlState.WALKING );
        }

		if( _startLiftTracking && _roller.CurrentHeldObject )
		{
			_roller.CarryPositionObject.transform.position = _roller.IK.ArmTipMidpoint;	
			_roller.CurrentHeldObject.transform.localPosition = Vector3.zero;
		}
    }

	public void StartLifting()
	{
		if( _roller.CurrentHeldObject != null && _roller.CurrentHeldObject.Carryable )
		{
			_roller.CurrentHeldObject.transform.parent = _roller.CarryPositionObject.transform;     

			_roller.CurrentHeldObject.transform.localPosition = Vector3.zero;

			_roller.CurrentHeldObject.OnPickup( this.transform );

			_startLiftTracking = true;
		}
	}

}
