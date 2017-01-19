using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class RollToWalkState : RollerState 
{
	public override void Enter(RollerController parent)
	{
		Debug.Log("ENTER ROLL TO WALK STATE");
		roller = parent;

		// START THE TRANSITION CLOCK
		//transitionTimer = 0f;
		Transition();
	}

	void Transition()
	{
		roller.ChangeState(RollToWalk, Walking);
	}

	public override void Exit()
	{
		Debug.Log("EXIT ROLL TO WALK STATE");
		CameraManager.instance.ChangeCameraState( CameraManager.CameraState.FOLLOWPLAYER_FREE );
	}

	public override void HandleInput(InputCollection input)
	{
		/*
		transitionTimer += Time.deltaTime;
		if (transitionTimer >= TRANSITION_TIME)
		{
			Transition();
		}

		if (velocity != 0f)
		{
			// Slowdown
			velocity -= Mathf.Sign(velocity) * TRANSITION_DECELERATION * Time.deltaTime;
			Vector3 slowDownPos = roller.transform.position + (roller.transform.forward * velocity * Time.deltaTime);
			roller.rigidbody.MovePosition(slowDownPos);
		}
		*/
	}
}
