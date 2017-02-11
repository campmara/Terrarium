using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PickupState : RollerState 
{
    public override void Enter(P_ControlState prevState)
	{
		Debug.Log("ENTER PICKUP STATE");

		_roller.Face.BecomeInterested();

		_roller.CurrentHeldObject.transform.parent = _roller.transform;
		_roller.CurrentHeldObject.OnPickup();

		Vector3 pickupPos = _roller.transform.position + (_roller.transform.forward * RollerConstants.PICKUP_FORWARDSCALAR) + (_roller.transform.up * RollerConstants.PICKUP_UPSCALAR);
		_roller.CurrentHeldObject.transform.DOMove(pickupPos, RollerConstants.PICKUP_TIME).OnComplete( Transition );
	}

	void Transition()
	{
		_roller.ChangeState(P_ControlState.PICKINGUP, P_ControlState.CARRYING);
	}

	public override void Exit( P_ControlState nextState )
	{
		Debug.Log("EXIT PICKUP STATE");
	}

	public override void HandleInput( InputCollection input )
	{

	}
}
