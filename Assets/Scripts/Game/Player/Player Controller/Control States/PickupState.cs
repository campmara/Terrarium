﻿using System.Collections;
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

		_roller.Face.BecomeInterested();

		// If Carryable the object should be parented to be moved around
		if( !_roller.CurrentHeldObject.Carryable )
		{
			_roller.CurrentHeldObject.OnPickup( this.transform );
            TransitionToCarrying();
		}

	}

	public void TransitionToCarrying()
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
        if( input.AButton.WasReleased )
        {
            _roller.ChangeState( P_ControlState.WALKING );
        }

		if( _startLiftTracking )
		{
			_roller.CarryPositionObject.transform.position = _roller.IK.ArmTipMidpoint;	
			_roller.CurrentHeldObject.transform.localPosition = Vector3.zero;
		}

    }

	public void StartLifting()
	{
		if( _roller.CurrentHeldObject.Carryable )
		{
			_roller.CurrentHeldObject.transform.parent = _roller.CarryPositionObject.transform;     

			_roller.CurrentHeldObject.transform.localPosition = Vector3.zero;

			_roller.CurrentHeldObject.OnPickup( this.transform );

			_startLiftTracking = true;
		}
	}

}
