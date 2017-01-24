using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class RitualState : RollerState
{
    private Tween _tween;

	public override void Enter(P_ControlState prevState)
	{
		Debug.Log("ENTER RITUAL STATE");
	    _tween = transform.DOScaleY(0.1f, RITUAL_TIME).OnComplete(OnCompleteRitual);

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

	    if (!isComplete && (input.XButton.WasReleased && input.XButton.HasChanged))
		{
		    _roller.ChangeState(P_ControlState.RITUAL, P_ControlState.IDLING);
		}
	}

    private void OnCompleteRitual()
    {
        transform.localScale = Vector3.one;
        PondManager.instance.ReturnPlayerToPond();
    }
}
