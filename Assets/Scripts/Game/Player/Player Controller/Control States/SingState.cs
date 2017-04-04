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
		_roller.Face.BecomeIdle();
		RollerParent.Idling = false;
	}

	public override void HandleInput (InputCollection input)
	{
		RollerParent.IKMovement(RollerConstants.instance.SING_WALK_SPEED, 
									  RollerConstants.instance.WALK_ACCELERATION, 
									  RollerConstants.instance.WALK_DECELERATION, 
									  RollerConstants.instance.WALK_TURN_SPEED);

	    // B BUTTON
//	    if (input.BButton.IsPressed)
//	    {
//	        _roller.ChangeState( P_ControlState.ROLLING);
//			return;
//	    }

	    // X BUTTON
	    if (input.XButton.IsPressed)
	    {
	        _roller.ChangeState( P_ControlState.RITUAL);
	    }

	    //float desiredPitch = 1.0f + _roller.InputVec.magnitude;
	    float desiredPitch = AudioManager.instance.GetCurrentMusicPitch();
	    _singPitch = Mathf.Lerp(_singPitch, desiredPitch, RollerConstants.instance.PITCH_LERP_SPEED * Time.deltaTime);

	    // Y BUTTON
	    if (input.YButton.IsPressed)
	    {
	        AudioManager.instance.PlaySing(_singPitch);
			_roller.Face.Sing();
	        _waitToReturnTimer = 0f;
	    }
		else
		{
		    _roller.Face.BecomeIdle();
		    _waitToReturnTimer += Time.deltaTime;
		}

	    if (_waitToReturnTimer < RollerConstants.instance.SINGING_RETURN_TIME)
	        return;

        _waitToReturnTimer = 0f;
        RollerParent.ChangeState(P_ControlState.WALKING);
	}
}
