using UnityEngine;

public class SittingState : RollerState 
{
    bool _onGround = false;
    public bool OnGround { get { return _onGround; } set { _onGround = false; } }

    public override void Enter (P_ControlState prevState)
	{
		Debug.Log("ENTER SIT STATE");

		// TRIGGER SITTING ON
		_roller.IK.DisableIK();
		_roller.Player.AnimationController.SetSitting(true);

		CameraManager.instance.ChangeCameraState( CameraManager.CameraState.SITTING );
	}

	public override void Exit (P_ControlState nextState)
	{
		Debug.Log("EXIT SIT STATE");

		_roller.IK.EnableIK();

        _onGround = false;

		CameraManager.instance.ChangeCameraState( CameraManager.CameraState.FOLLOWPLAYER_FREE );
	}

	public override void HandleInput (InputCollection input)
	{
		Vector3 vec = new Vector3(input.LeftStickX, 0f, input.LeftStickY);

		if ( input.ActiveDevice.AnyButtonIsPressed || vec.magnitude >= 0.75f )
		{
			// TRIGGER SITTING OFF.
			_roller.Player.AnimationController.SetSitting(false);

            if( !_onGround )
            {
                OnStandingUpComplete();
            }            
		}

	}

	public void OnStandingUpComplete()
	{
		_roller.ChangeState(P_ControlState.WALKING);
	}

    public void SetOnGround( int onGround )
    {
        if( onGround == 0 )
        {
            _onGround = false;
        }
        else
        {
            _onGround = true;
        }
        
    }
}
