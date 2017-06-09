using UnityEngine;

public class SittingState : RollerState 
{
    [SerializeField, ReadOnly] bool _onGround = false;
    public bool OnGround { get { return _onGround; } set { _onGround = false; } }

    [SerializeField, ReadOnly]float _sleepTimer = 0.0f;
    bool _asleep = false;

    public override void Enter (P_ControlState prevState)
	{
		Debug.Log("[RollerState] ENTER SIT STATE");

		// TRIGGER SITTING ON
		_roller.IK.DisableIK();
		_roller.Player.AnimationController.SetSitting(true);

        _asleep = false;

        CameraManager.instance.ChangeCameraState( CameraManager.CameraState.SITTING );
	}

	public override void Exit (P_ControlState nextState)
	{
		Debug.Log("[RollerState] EXIT SIT STATE");

		_roller.IK.EnableIK();

		if( _asleep )
		{
			WakeUp();
		}

        _onGround = false;

        CameraManager.instance.ChangeCameraState( CameraManager.CameraState.FOLLOWPLAYER_FREE );
	}

    public override void HandleInput( InputCollection input )
    {
        _sleepTimer += Time.deltaTime;

        if( !_asleep && _sleepTimer > RollerConstants.instance.SitSleepWaitTime )
        {
            _asleep = true;
            _roller.Face.TransitionFacePose( "Sleep" );
			_roller.Player.AnimationController.TriggerSleep();
        }

        _roller.BreathTimer += Time.deltaTime * RollerConstants.instance.BreathSpeed;
        
        _roller.Spherify = RollerConstants.instance.BreathSpherizeCurve.Evaluate( _roller.BreathTimer );

        Vector3 vec = new Vector3( input.LeftStickX, 0f, input.LeftStickY );

        if ( input.YButton.WasPressed )   // Y BUTTON
        {
			if ( _asleep )
			{
				WakeUp();
			}

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

            if ( !_onGround )
            {
                Debug.Log( "Early Transition Back to Walk" );

                AudioManager.instance.StopController( AudioManager.AudioControllerNames.PLAYER_TRANSITIONFX );

                OnStandingUpComplete();
            }
            else
            {
                if ( _asleep )
                {
					WakeUp();
                }
                else
                {
                    _roller.Face.BecomeIdle();                    
                }
            }
        }
    }

	public void OnStandingUpComplete()
	{       
        _roller.ChangeState(P_ControlState.WALKING);
	}

    public void SetOnGround( int onGround )
    {
        if (onGround == 0)
        {
            _onGround = false;
        }
        else
        {
            _onGround = true;
            _roller.Face.TransitionFacePose( "Sitting" );
        }
    }

    public void PlaySitAudio()
    {
        AudioManager.instance.PlayClipAtIndex( AudioManager.AudioControllerNames.PLAYER_TRANSITIONFX, 4 );
    }

    public void StopSitAudio()
    {
        AudioManager.instance.StopController( AudioManager.AudioControllerNames.PLAYER_TRANSITIONFX );
    }

    public void PlayStandAudio()
    {
        AudioManager.instance.PlayClipAtIndex( AudioManager.AudioControllerNames.PLAYER_TRANSITIONFX, 3 );
    }

	void WakeUp()
	{
		_asleep = false;
		_roller.Face.TransitionFacePose( "Wake Up", true, 1.0f );
		_roller.Player.AnimationController.TriggerSleep();
	}
}
