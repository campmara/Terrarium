using System.Collections;
using UnityEngine;
using DG.Tweening;

public class PickupState : RollerState 
{
    Sequence _pickupSequence = null;

    public override void Enter(P_ControlState prevState)
	{
		Debug.Log("ENTER PICKUP STATE");

		_roller.Face.BecomeInterested();

		// If Carryable the object should be parented to be moved around
		if( _roller.CurrentHeldObject.Carryable )
		{
			_roller.CurrentHeldObject.transform.parent = _roller.transform;     

			_roller.CurrentHeldObject.OnPickup( this.transform );

            _pickupSequence = DOTween.Sequence();

			Vector3 pickupMidPos = _roller.transform.position + ( _roller.transform.forward * RollerConstants.instance.PickupForwardScalarPart1 ) + ( _roller.transform.up * RollerConstants.instance.PickupUpScalarPart1 );
            _pickupSequence.Append( _roller.CurrentHeldObject.transform.DOMove( pickupMidPos, RollerConstants.instance.PickupTime * 0.33f ) );

            Vector3 pickupEndPos = _roller.transform.position + ( _roller.transform.forward * RollerConstants.instance.PickupForwardScalarPart2 ) + ( _roller.transform.up * RollerConstants.instance.PickupUpScalarPart2 );
            _pickupSequence.Append( _roller.CurrentHeldObject.transform.DOMove( pickupEndPos, RollerConstants.instance.PickupTime * 0.66f ).OnComplete( TransitionToCarrying ) );   
		}
		else
		{
			_roller.CurrentHeldObject.OnPickup( this.transform );
            TransitionToCarrying();
		}

	}

	void TransitionToCarrying()
	{
		_roller.ChangeState(P_ControlState.CARRYING);
	}

	public override void Exit( P_ControlState nextState )
	{
		Debug.Log("EXIT PICKUP STATE");

        switch (nextState)
        {
            case P_ControlState.WALKING:
                HandleBothArmRelease();
                _pickupSequence.Kill();
                _pickupSequence = null;
                break;
        }        
	}

    public override void HandleInput( InputCollection input )
    {
        if( input.AButton.WasReleased )
        {
            _roller.ChangeState( P_ControlState.WALKING );
        }
    }

}
