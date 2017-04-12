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

			Sequence pickupSequence = DOTween.Sequence();

			Vector3 pickupMidPos = _roller.transform.position + ( _roller.transform.forward * RollerConstants.instance.PickupForwardScalarPart1 ) + ( _roller.transform.up * RollerConstants.instance.PickupUpScalarPart1 );
			pickupSequence.Append( _roller.CurrentHeldObject.transform.DOMove( pickupMidPos, RollerConstants.instance.PickupTime * 0.33f ) );

			Vector3 pickupEndPos = _roller.transform.position + (_roller.transform.forward * RollerConstants.instance.PickupForwardScalarPart2) + (_roller.transform.up * RollerConstants.instance.PickupUpScalarPart2 );
			pickupSequence.Append( _roller.CurrentHeldObject.transform.DOMove( pickupEndPos, RollerConstants.instance.PickupTime * 0.66f ).OnComplete( Transition ) );   
		}
		else
		{
			_roller.CurrentHeldObject.OnPickup( this.transform );
            Transition();
		}

	}

	void Transition()
	{
		_roller.ChangeState(P_ControlState.CARRYING);
	}

	public override void Exit( P_ControlState nextState )
	{
		Debug.Log("EXIT PICKUP STATE");
	}

}
