using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RitualState : RollerState 
{
	float ritualTimer = 0f;

	public override void Enter(P_ControlState prevState) 
	{
		Debug.Log("ENTER RITUAL STATE");
		ritualTimer = 0f;
	}

	public override void Exit(P_ControlState nextState)
	{
		Debug.Log("EXIT RITUAL STATE");
	}

	public override void HandleInput(InputCollection input)
	{
		if ((input.XButton.WasReleased & input.XButton.HasChanged) || input.XButton.Value == 0)
		{
			_roller.ChangeState(P_ControlState.RITUAL, P_ControlState.IDLING);
		}

		ritualTimer += Time.deltaTime;

		if (ritualTimer >= RITUAL_TIME)
		{
			PondManager.instance.ReturnPlayerToPond();
		}
	}
}
