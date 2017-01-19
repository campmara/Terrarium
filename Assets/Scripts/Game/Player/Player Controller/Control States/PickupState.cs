using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PickupState : RollerState 
{
    void Awake()
    {
        _controlState = P_ControlState.PICKINGUP;
    }

    public override void Enter(RollerController parent, P_ControlState prevState)
	{
		Debug.Log("ENTER PICKUP STATE");
		roller = parent;

		currentHeldObject.transform.parent = roller.transform;
		currentHeldObject.OnPickup();

		Vector3 pickupPos = roller.transform.position + (roller.transform.forward * 1f) + (roller.transform.up * 1f);
		currentHeldObject.transform.DOMove(pickupPos, PICKUP_TIME).OnComplete(Transition);
	}

	void Transition()
	{
		roller.ChangeState(Pickup, Carrying);
	}

	public override void Exit(P_ControlState nextState)
	{
		Debug.Log("EXIT PICKUP STATE");
	}

	public override void HandleInput(InputCollection input)
	{

	}
}
