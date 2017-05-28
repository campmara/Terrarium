using System.Collections;
using UnityEngine;

public class PondState : RollerState
{
	bool _crashPondReturn = false;

	public override void Enter(P_ControlState prevState)
	{
		Debug.Log("[RollerState] ENTER POND STATE");

		// Zero out velocity.
		_roller.StopPlayer();
		if( prevState == P_ControlState.ROLLING )
		{
			_roller.BecomeWalker();	
			_crashPondReturn = true;
		}
		else
		{
			_crashPondReturn = false;
		}
	}

	public override void Exit(P_ControlState nextState)
	{
		Debug.Log("[RollerState] EXIT POND STATE");

		_roller.Mesh.SetActive(true);
		_roller.Face.gameObject.SetActive(true);

        _roller.StopPlayer();

        // Ensure that this is false. This is also bad :/ oh well.
        _roller.CollidedWithObject = false;

		if( _crashPondReturn )
		{
			_roller.Face.TransitionFacePose( "Crash Rebirth", true, 1.5f );
		}
		else
		{
			_roller.Face.TransitionFacePose( "Rebirth", true, 1.0f );
		}
    }

    public override void HandleInput( InputCollection input )
    {
        if ( input.AButton.IsPressed ||
               input.BButton.IsPressed ||
               input.XButton.IsPressed ||
               input.YButton.IsPressed )
        {
            if (CameraManager.instance.CamState == CameraManager.CameraState.POND_WAIT)
            {
                PondManager.instance.HandlePondReturn();
            }
            else
            {
                PondManager.instance.PopPlayerFromPond();
            }

            _roller.ChangeState( P_ControlState.WALKING );
        }

        if ( input.LeftStickX.Value > RollerConstants.instance.IdleMaxMag ||
            input.LeftStickY.Value > RollerConstants.instance.IdleMaxMag )
        {
            if( CameraManager.instance.CamState == CameraManager.CameraState.POND_WAIT )
            {
                PondManager.instance.HandlePondReturn();
            }
            else
            {
                PondManager.instance.PopPlayerFromPond();
            }

            _roller.ChangeState( P_ControlState.WALKING );
        }

    }

}
