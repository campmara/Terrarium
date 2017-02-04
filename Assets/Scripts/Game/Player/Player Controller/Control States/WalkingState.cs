using UnityEngine;
using DG.Tweening;

public class WalkingState : RollerState
{
    private Tween _tween;

    public override void Enter( P_ControlState prevState )
	{
		Debug.Log("ENTER WALKING STATE");

        // Handle Transition
        switch ( prevState )
        {
        case P_ControlState.ROLLING:            
            CameraManager.instance.ChangeCameraState( CameraManager.CameraState.FOLLOWPLAYER_FREE );
            //PlayerManager.instance.Player.AnimationController.PlayRollToWalkAnim();
            _tween = _roller.RollSphere.transform.DOMoveY(1.5f, 0.5f)
                .SetEase(Ease.OutQuint)
                .OnComplete(BecomeWalker);


            
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

		RollerParent.Idling = false;
    }

	public override void HandleInput(InputCollection input)
	{   
        // A BUTTON
        if (input.AButton.WasPressed)
        {
            HandlePickup();
        }

		RollerParent.StandardMovement(RollerConstants.WALK_SPEED, 
									  RollerConstants.WALK_ACCELERATION, 
									  RollerConstants.WALK_DECELERATION, 
									  RollerConstants.WALK_TURN_SPEED);

        // B BUTTON
        if (input.BButton.IsPressed)
        {
            if ( GameManager.Instance.State == GameManager.GameState.MAIN )
            {
                _roller.ChangeState( P_ControlState.WALKING, P_ControlState.ROLLING );
            }
        }

        // X BUTTON
        if (input.XButton.IsPressed)
        {
            _roller.ChangeState( P_ControlState.WALKING, P_ControlState.RITUAL );
        }

		// Y BUTTON
		if (input.YButton.IsPressed)
		{
			_roller.ChangeState( P_ControlState.WALKING, P_ControlState.SING );
		}
    }
}
