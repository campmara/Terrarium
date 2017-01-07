using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollingState : RollerState 
{
	public override void Enter(RollerController roller)
	{
		Debug.Log("ENTER ROLLING STATE");
	}

	public override void Exit(RollerController roller)
	{
		Debug.Log("EXIT ROLLING STATE");
	}

	public override void HandleInput(RollerController roller, InputCollection input)
	{

	}
}
