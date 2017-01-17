using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class WalkToRollState : RollerState 
{
	public override void Enter(RollerController parent)
	{
		Debug.Log("ENTER WALK TO ROLL STATE");
		roller = parent;

		CameraManager.instance.ChangeCameraState( CameraManager.CameraState.FOLLOWPLAYER_LOCKED );

		// START THE TRANSITION CLOCK
		//transitionTimer = 0f;
		Transition();
	}

	void Transition()
	{
		roller.ChangeState(WalkToRoll, Rolling);
	}

	public override void Exit()
	{
		Debug.Log("EXIT WALK TO ROLL STATE");
	}

	public override void HandleInput(InputCollection input)
	{
		/*
		transitionTimer += Time.deltaTime;
		if (transitionTimer >= TRANSITION_TIME)
		{
			Transition();
		}

		if (velocity > 0f)
		{
			// Slowdown
			velocity -= TRANSITION_DECELERATION * Time.deltaTime;
			Vector3 slowDownPos = roller.transform.position + (lastInputVec * velocity * Time.deltaTime);
			roller.rigidbody.MovePosition(slowDownPos);
		}
		*/
	}
}
