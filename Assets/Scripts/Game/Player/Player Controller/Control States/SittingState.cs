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

    public override void HandleInput( InputCollection input )
    {
        _roller.BreathTimer += Time.deltaTime * RollerConstants.instance.BreathSpeed;
        _roller.Spherify = 1.0f - Mathf.Pow(Mathf.Abs(Mathf.Sin(Mathf.PI * _roller.BreathTimer / 2.0f)), 0.5f)* RollerConstants.instance.BreathSpherize;
        //_roller.Spherify = Mathf.PingPong( _roller.BreathTimer, RollerConstants.instance.BreathSpherize );

        Vector3 vec = new Vector3( input.LeftStickX, 0f, input.LeftStickY );

        if ( input.YButton.WasPressed )   // Y BUTTON
        {
            _roller.Player.PlayerSingController.BeginSinging();
            //_roller.ChangeState( P_ControlState.SING);
        }
        else if ( input.YButton.WasReleased )
        {
            _roller.Player.PlayerSingController.StopSinging();
        }
        else if ( !input.YButton.IsPressed && ( input.ActiveDevice.AnyButtonIsPressed || vec.magnitude >= 0.75f ) )
        {
            // TRIGGER SITTING OFF.
            _roller.Player.AnimationController.SetSitting( false );

            if (!_onGround)
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
