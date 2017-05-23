using System.Collections;
using DG.Tweening;
using UnityEngine;

public class RitualState : RollerState
{
	private float ritualTimer = 0f;
	private bool hasExploded = false;

	public override void Enter(P_ControlState prevState)
	{
		Debug.Log("ENTER RITUAL STATE");
        _roller.IK.SetState( PlayerIKControl.WalkState.RITUAL );
		ritualTimer = 0f;
		hasExploded = false;

        _roller.Spherify = 0.0f;
        _roller.SpherifyScale = RollerConstants.instance.RitualSphereizeScale;

		_roller.Face.TransitionFacePose( "Pop" );
	}

	public override void Exit(P_ControlState nextState)
	{
		Debug.Log("EXIT RITUAL STATE");

		if (nextState == P_ControlState.WALKING)
		{
			_roller.IK.SetState( PlayerIKControl.WalkState.WALK );
			_roller.Face.BecomeIdle();
			AudioManager.instance.StopController( AudioManager.AudioControllerNames.PLAYER_ACTIONFX );
		}

        // TODO: make this lerp back!

        _roller.ExplodeParticleSystem.Stop();
	}

	public override void HandleFixedInput(InputCollection input)
	{
		if ( !hasExploded && ritualTimer > RollerConstants.instance.RitualTime )
		{
			hasExploded = true;
			_roller.HandlePondReturn();

            CameraManager.instance.ScreenShake( 0.25f, 0.75f, 10 );

            if (_roller.CurrentHeldObject != null)
			{
				Seed seed = _roller.CurrentHeldObject.GetComponent<Seed>();
				if( seed != null )
				{
					seed.DropOnRitual();
				}

				_roller.IK.LetGoBothArms();
				_roller.CurrentHeldObject = null;
			}
		}
		else if ( !hasExploded )
		{
			ritualTimer += Time.deltaTime;

			// Update how far the arms are reaching
			_roller.UpdateArmReachIK( input.LeftTrigger.Value, input.RightTrigger.Value );

			_roller.IKMovement(RollerConstants.instance.WalkSpeed, 
										RollerConstants.instance.WalkAcceleration, 
										RollerConstants.instance.WalkDeceleration, 
										RollerConstants.instance.WalkTurnSpeed);

            _roller.Spherify = Mathf.Lerp( 0.0f, RollerConstants.instance.RitualMaxSpherize, RollerConstants.instance.RitualPopCurve.Evaluate( ritualTimer / RollerConstants.instance.RitualTime ) );

            if (!input.XButton.IsPressed)
			{
				_roller.ChangeState(P_ControlState.WALKING);
			}
		}
	}
}
