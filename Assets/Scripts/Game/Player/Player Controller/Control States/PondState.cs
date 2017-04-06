﻿using System.Collections;
using UnityEngine;

public class PondState : RollerState
{
	public override void Enter(P_ControlState prevState)
	{
		Debug.Log("ENTER POND STATE");

		// Zero out velocity.
		_roller.StopPlayer();
	}

	public override void Exit(P_ControlState nextState)
	{
		Debug.Log("EXIT POND STATE");

		_roller.Mesh.SetActive(true);
		_roller.Face.gameObject.SetActive(true);
	}

	public override void HandleInput(InputCollection input)
	{
		if (input.AButton.IsPressed || input.BButton.IsPressed || input.XButton.IsPressed || input.YButton.IsPressed)
		{
			PondManager.instance.PopPlayerFromPond();
		}

		if (input.AButton.WasReleased || input.BButton.WasReleased || input.XButton.WasReleased || input.YButton.WasReleased)
		{
			_roller.ChangeState(P_ControlState.WALKING);
		}
	}
}
