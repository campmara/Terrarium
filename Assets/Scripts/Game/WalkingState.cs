using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkingState : RollerState 
{
	public override void Enter(RollerController roller)
	{
		Debug.Log("ENTER WALKING STATE");
	}

	public override void Exit(RollerController roller)
	{
		Debug.Log("EXIT WALKING STATE");
	}

	public override void HandleInput(RollerController roller, InputCollection input)
	{
		Vector3 move = new Vector3(input.LeftStickX, 0f, input.LeftStickY);
		roller.rigidbody.MovePosition(roller.transform.position + (move * RollerController.WALK_SPEED));
	}
}
