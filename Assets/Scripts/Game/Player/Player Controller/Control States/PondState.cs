using System.Collections;
using UnityEngine;

public class PondState : RollerState
{
	public override void Enter(P_ControlState prevState)
	{
		Debug.Log("ENTER POND STATE");

		// Zero out velocity.
		_roller.StopPlayer();
	}

	public override void Exit(P_ControlState nextState)
	{
		Debug.Log("EXIT POND STATE");

		_roller.Mesh.SetActive(true);
		_roller.Face.gameObject.SetActive(true);

        // Ensure that this is false. This is also bad :/ oh well.
        _roller.CollidedWithObject = false;
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
