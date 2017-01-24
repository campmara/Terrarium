using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PickupState : RollerState 
{

    public override void Enter( P_ControlState prevState )
	{
		Debug.Log("ENTER PICKUP STATE");

		currentHeldObject.transform.parent = _roller.transform;
		currentHeldObject.OnPickup();

		Vector3 pickupPos = _roller.transform.position + (_roller.transform.forward * PICKUP_FORWARDSCALAR) + (_roller.transform.up * PICKUP_UPSCALAR);
		currentHeldObject.transform.DOMove(pickupPos, PICKUP_TIME).OnComplete( Transition );
	}

	void Transition()
	{
		_roller.ChangeState( P_ControlState.PICKINGUP, P_ControlState.CARRYING );
	}

	public override void Exit( P_ControlState nextState )
	{
		Debug.Log("EXIT PICKUP STATE");
	}

	public override void HandleInput( InputCollection input )
	{

	}
}
