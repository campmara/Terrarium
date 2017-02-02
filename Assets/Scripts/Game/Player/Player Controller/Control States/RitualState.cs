﻿using System.Collections;
using DG.Tweening;
using UnityEngine;

public class RitualState : RollerState
{
    private Tween _tween;

	public override void Enter(P_ControlState prevState)
	{
		Debug.Log("ENTER RITUAL STATE");
        _tween = transform.DOScaleY( 0.1f, RollerConstants.RITUAL_TIME ).OnComplete( OnCompleteRitual );

	    PlayerManager.instance.Player.AnimationController.PlayIdleAnim();
	}

	public override void Exit(P_ControlState nextState)
	{
		Debug.Log("EXIT RITUAL STATE");
	    if (_tween != null)
	    {
	        _tween.Kill();
	        _tween = null;
	    }
	    transform.localScale = Vector3.one;
	}

	public override void HandleInput(InputCollection input)
	{
	    bool isComplete = _tween.IsComplete();

	    if (!isComplete && !input.XButton.IsPressed)
		{
		    _roller.ChangeState(P_ControlState.RITUAL, P_ControlState.WALKING);
		}
	}

    private void OnCompleteRitual()
    {
        GameManager.Instance.ChangeGameState( GameManager.GameState.POND_RETURN );

        transform.DOMoveY( -5.0f, 0.5f );

        StartCoroutine( DelayedCompleteRitual() );        
    }

    IEnumerator DelayedCompleteRitual()
    {       
        yield return new WaitForSeconds( RollerConstants.RITUAL_COMPLETEWAIT );

        // TODO: implement plant watering here

        transform.localScale = Vector3.one;
        PondManager.instance.HandlePondReturn();
    }
}
