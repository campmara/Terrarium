using UnityEngine;

public class SingState : RollerState 
{
	private float _waitToReturnTimer;

    private float _singPitch;

	public override void Enter (P_ControlState prevState)
	{
		Debug.Log("ENTER SING STATE");
	    _waitToReturnTimer = 0f;
	    _singPitch = 1f;
	}

	public override void Exit (P_ControlState nextState)
	{
		Debug.Log("EXIT SING STATE");

		RollerParent.Idling = false;
	}

	public override void HandleInput (InputCollection input)
	{
		RollerParent.IKMovement(RollerConstants.SING_WALK_SPEED, 
									  RollerConstants.WALK_ACCELERATION, 
									  RollerConstants.WALK_DECELERATION, 
									  RollerConstants.WALK_TURN_SPEED);

	    // B BUTTON
	    if (input.BButton.IsPressed)
	    {
	        _roller.ChangeState( P_ControlState.ROLLING);
	    }

	    // X BUTTON
	    if (input.XButton.IsPressed)
	    {
	        _roller.ChangeState( P_ControlState.RITUAL);
	    }

	    //float desiredPitch = 1.0f + _roller.InputVec.magnitude;
	    float desiredPitch = AudioManager.instance.GetCurrentMusicPitch();
	    _singPitch = Mathf.Lerp(_singPitch, desiredPitch, RollerConstants.PITCH_LERP_SPEED * Time.deltaTime);

	    // Y BUTTON
	    if (input.YButton.IsPressed)
	    {
	        AudioManager.instance.PlaySing(_singPitch);
	        _roller.Face.SingFace();
	        _waitToReturnTimer = 0f;
	    }
		else
		{
		    _roller.Face.NormalFace();
		    _waitToReturnTimer += Time.deltaTime;
		}

	    if (_waitToReturnTimer < RollerConstants.SINGING_RETURN_TIME)
	        return;

        _waitToReturnTimer = 0f;
        RollerParent.ChangeState(P_ControlState.WALKING);
	}
}
