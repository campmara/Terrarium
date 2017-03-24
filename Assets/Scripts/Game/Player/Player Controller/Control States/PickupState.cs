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

			Vector3 pickupMidPos = _roller.transform.position + ( _roller.transform.forward * RollerConstants.instance.PICKUP_FORWARDSCALAR_PART1 ) + ( _roller.transform.up * RollerConstants.instance.PICKUP_UPSCALAR_PART1 );
			pickupSequence.Append( _roller.CurrentHeldObject.transform.DOMove( pickupMidPos, RollerConstants.instance.PICKUP_TIME * 0.33f ) );

			Vector3 pickupEndPos = _roller.transform.position + (_roller.transform.forward * RollerConstants.instance.PICKUP_FORWARDSCALAR_PART2) + (_roller.transform.up * RollerConstants.instance.PICKUP_UPSCALAR_PART2 );
			pickupSequence.Append( _roller.CurrentHeldObject.transform.DOMove( pickupEndPos, RollerConstants.instance.PICKUP_TIME * 0.66f ).OnComplete( Transition ) );   
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

	public override void HandleInput( InputCollection input )
	{

	}

}
