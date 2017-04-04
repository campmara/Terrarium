using UnityEngine;
using System.Collections;
using DG.Tweening;

public class WalkingState : RollerState
{
    private Tween _tween;
    private float _idleTimer = 0f;

    Coroutine _reachCoroutine = null;

	bool canPickup = false;

    public override void Enter( P_ControlState prevState )
	{
		Debug.Log("ENTER WALKING STATE");

        // Handle Transition
        switch ( prevState )
        {
        case P_ControlState.ROLLING:            
            CameraManager.instance.ChangeCameraState( CameraManager.CameraState.FOLLOWPLAYER_FREE );
                //PlayerManager.instance.Player.AnimationController.PlayRollToWalkAnim();
                _roller.BecomeWalker();
                _tween = _roller.RollSphere.transform.DOMoveY( 1.5f, 0.5f ).SetEase( Ease.OutQuint );           
            break;
        }

        _idleTimer = 0f;
		canPickup = false;
        //PlayerManager.instance.Player.AnimationController.PlayWalkAnim();
	}

	public override void Exit(P_ControlState nextState)
	{
		Debug.Log("EXIT WALKING STATE");

        if (_tween != null)
	    {
	        _tween.Kill();
	        _tween = null;
	    }

        if( _reachCoroutine != null )
        {            
            StopCoroutine( _reachCoroutine );
            _reachCoroutine = null;
        }

		RollerParent.Idling = false;
    }

	public override void HandleInput(InputCollection input)
	{
        // Check for sitting after idling for a while.
        IdleTimer(input);

		// A BUTTON 
		// THIS MAKES FIRST TIME YOU PRESS A BUTTON NOT WORK
		// have to figure out a way to deal Incontrol detecting double button presserino between frames
		if( !canPickup && input.AButton.WasReleased )
		{
			canPickup = true;	
		}
          
		if ( canPickup && input.AButton.WasPressed )
        {
            // End coroutine waiting to see if the player should auto reach if the player inputs for arms  
            if ( _reachCoroutine != null )
            {
                StopCoroutine( _reachCoroutine );
                _reachCoroutine = null;
            }
/*
			if( !_roller.IK.ArmsIdle )
			{
				HandleBothArmRelease();
			}
*/
			HandlePickup( PlayerArmIK.ArmType.BOTH );
			if( _roller.IK.ArmFocus != null )
			{
				HandleGrabObject();	
			}
        }

		if( _roller.IK.ArmsIdle )
        {
            if ( _reachCoroutine == null )
            {
                _reachCoroutine = StartCoroutine( ReachWaitRoutine() );
            }
        }

        // Update how far the arms are reaching
        _roller.UpdateArmReachIK( input.LeftTrigger.Value, input.RightTrigger.Value );

		_roller.IKMovement(RollerConstants.instance.WALK_SPEED, 
									  RollerConstants.instance.WALK_ACCELERATION, 
									  RollerConstants.instance.WALK_DECELERATION, 
									  RollerConstants.instance.WALK_TURN_SPEED);

		if ( _tween != null && _tween.IsPlaying() )
		{
			return;
		}

        // B BUTTON
		if (input.BButton.IsPressed)
        {
            if ( GameManager.Instance.State == GameManager.GameState.MAIN )
            {
                _roller.ChangeState( P_ControlState.ROLLING);
            }
        }        
        else if (input.XButton.IsPressed)   // X BUTTON
        {
            _roller.ChangeState( P_ControlState.RITUAL);
        }			
		else if (input.YButton.IsPressed)	// Y BUTTON
		{
			_roller.ChangeState( P_ControlState.SING);
		}
    }

    void IdleTimer(InputCollection input)
    {
        // handle idle timing
        _idleTimer += Time.deltaTime;

        Vector3 vec = new Vector3(input.LeftStickX, 0f, input.LeftStickY);
        if (input.ActiveDevice.AnyButtonIsPressed || vec.magnitude >= 0.25f)
        {
            _idleTimer = 0f;
        }
        else
        {
            // Left Stick Button
            if (input.LeftStickButton.IsPressed)
            {
                _roller.ChangeState(P_ControlState.SIT);
            }
        }

        if (_idleTimer >= RollerConstants.instance.IDLE_SITTING_TIMER)
        {
            // go to sitting State
            _roller.ChangeState(P_ControlState.SIT);
        }
    }

    IEnumerator ReachWaitRoutine()
    {
        //Debug.Log( "Starting Reach Timer" );

        yield return new WaitForSeconds( Random.Range( RollerConstants.instance.IK_REACH_WAITMIN, RollerConstants.instance.IK_REACH_WAITMAX ) );

        //Debug.Log( "Prepping Reach" );

        // Flip to decide where arm is reaching
        CheckForReachable( JohnTech.CoinFlip() ? PlayerArmIK.ArmType.LEFT : PlayerArmIK.ArmType.RIGHT );

        _reachCoroutine = null;
    }
}
