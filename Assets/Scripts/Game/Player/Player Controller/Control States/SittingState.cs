using UnityEngine;

public class SittingState : RollerState 
{
	public override void Enter (P_ControlState prevState)
	{
		Debug.Log("ENTER SIT STATE");

		// TRIGGER SITTING ON
		_roller.IK.DisableIK();
		_roller.Player.AnimationController.SetSitting(true);
	}

	public override void Exit (P_ControlState nextState)
	{
		Debug.Log("EXIT SIT STATE");

		_roller.IK.EnableIK();
	}

	public override void HandleInput (InputCollection input)
	{
		Vector3 vec = new Vector3(input.LeftStickX, 0f, input.LeftStickY);

		if (input.ActiveDevice.AnyButtonIsPressed || vec.magnitude >= 0.75f)
		{
			// TRIGGER SITTING OFF.
			_roller.Player.AnimationController.SetSitting(false);
		}
	}

	public void OnStandingUpComplete()
	{
		_roller.ChangeState(P_ControlState.WALKING);
	}
}
