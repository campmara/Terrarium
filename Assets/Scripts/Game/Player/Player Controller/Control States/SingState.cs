using UnityEngine;

public class SingState : RollerState 
{
	private float _waitToReturnTimer;

	public override void Enter (P_ControlState prevState)
	{
		Debug.Log("ENTER SING STATE");
	    _waitToReturnTimer = 0f;
	}

	public override void Exit (P_ControlState nextState)
	{
		Debug.Log("EXIT SING STATE");

		RollerParent.Idling = false;
	}

	public override void HandleInput (InputCollection input)
	{
		RollerParent.StandardMovement(RollerConstants.SING_WALK_SPEED, 
									  RollerConstants.WALK_ACCELERATION, 
									  RollerConstants.WALK_DECELERATION, 
									  RollerConstants.WALK_TURN_SPEED);

	    // B BUTTON
	    if (input.BButton.IsPressed)
	    {
	        _roller.ChangeState( P_ControlState.WALKING, P_ControlState.ROLLING );
	    }

	    // X BUTTON
	    if (input.XButton.IsPressed)
	    {
	        _roller.ChangeState( P_ControlState.WALKING, P_ControlState.RITUAL );
	    }

	    float singPitch = 1.0f + _roller.InputVec.magnitude;

	    // Y BUTTON
	    if (input.YButton.IsPressed)
	    {
	        AudioManager.instance.PlaySing(singPitch);
	        FaceManager.instance.SingFace();
	        _waitToReturnTimer = 0f;
	    }
		else
		{
		    FaceManager.instance.NormalFace();
		    _waitToReturnTimer += Time.deltaTime;
		}

	    if (_waitToReturnTimer < RollerConstants.SINGING_RETURN_TIME)
	        return;

        _waitToReturnTimer = 0f;
        RollerParent.ChangeState(P_ControlState.SING, P_ControlState.WALKING);
	}
}
