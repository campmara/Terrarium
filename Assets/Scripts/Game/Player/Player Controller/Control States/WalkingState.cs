﻿using UnityEngine;
using System.Collections;
using DG.Tweening;

public class WalkingState : RollerState
{
    private Tween _tween;

    Coroutine _reachCoroutine = null;

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
        // A BUTTON
        if (input.LeftTrigger.WasPressed || input.RightTrigger.WasPressed)
        {
            // End coroutine waiting to see if the player should auto reach if the player inputs for arms  
            if (_reachCoroutine != null)
            {
                StopCoroutine( _reachCoroutine );
                _reachCoroutine = null;
            }

            HandlePickup();
        }
        //else if ( !_roller.IK.ArmsReaching && ( input.LeftTrigger.Value <= 0.0f || input.RightTrigger.Value <= 0.0f ) )
        //{
        //    if ( _reachCoroutine == null )
        //    {
        //        _reachCoroutine = StartCoroutine( ReachWaitRoutine() );
        //    }
        //}
        else if( _roller.IK.ArmsReaching )   // If Arms Reaching
        {
            // If triggers are released
            if( input.LeftTrigger.Value <= 0.0f && input.RightTrigger.Value <= 0.0f )
            {
                HandleDropHeldObject();
            }
            else    // Else if triggers are held down... 
            {
                // Update how far the arms are reaching
                _roller.UpdateArmReachIK( input.LeftTrigger.Value, input.RightTrigger.Value );
                
                // If both triggers pulled down all the way
                if ( _roller.IK.ArmTargetsSet && ( input.LeftTrigger.Value >= 1.0f && input.RightTrigger.Value >= 1.0f ))
                {
                    HandleGrabObject();
                }
            }            
        }

		_roller.IKMovement(RollerConstants.WALK_SPEED, 
									  RollerConstants.WALK_ACCELERATION, 
									  RollerConstants.WALK_DECELERATION, 
									  RollerConstants.WALK_TURN_SPEED);

		if (_tween != null && _tween.IsPlaying())
		{
			return;
		}

        // B BUTTON
		if (input.BButton.IsPressed)
        {
            if (GameManager.Instance.State == GameManager.GameState.MAIN)
            {
                _roller.ChangeState( P_ControlState.ROLLING);
            }
        }        
        else if (input.XButton.IsPressed)   // X BUTTON
        {
            _roller.ChangeState( P_ControlState.RITUAL);
        }

		// Y BUTTON
		if (input.YButton.IsPressed)
		{
			_roller.ChangeState( P_ControlState.SING);
		}
    }

    IEnumerator ReachWaitRoutine()
    {
        Debug.Log( "Starting Reach Timer" );

        yield return new WaitForSeconds( Random.Range( RollerConstants.IK_REACH_WAITMIN, RollerConstants.IK_REACH_WAITMAX ) );

        Debug.Log( "Prepping Reach" );

        // Flip to decide where arm is reaching
        CheckForReachable( JohnTech.CoinFlip() );

        _reachCoroutine = null;
    }
}
