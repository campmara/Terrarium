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

		_roller.Face.BecomeDesirous();
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
        
		_roller.ExplodeParticleSystem.Stop();
	}

	public override void HandleInput(InputCollection input)
	{
		if (!hasExploded && ritualTimer > RollerConstants.instance.RitualTime)
		{
			StartCoroutine(Explode());
		}
		else if (!hasExploded)
		{
			ritualTimer += Time.deltaTime;

			// Update how far the arms are reaching
			_roller.UpdateArmReachIK( input.LeftTrigger.Value, input.RightTrigger.Value );

			_roller.IKMovement(RollerConstants.instance.WalkSpeed, 
										RollerConstants.instance.WalkAcceleration, 
										RollerConstants.instance.WalkDeceleration, 
										RollerConstants.instance.WalkTurnSpeed);

			if (!input.XButton.IsPressed)
			{
				_roller.ChangeState(P_ControlState.WALKING);
			}
		}
	}

	private IEnumerator Explode()
	{
		// Set this for our input update above.
		hasExploded = true;

		// Handle all the object deactivation and state change we require.
		_roller.Mesh.SetActive(false);
		_roller.Face.gameObject.SetActive(false);
		_roller.IK.SetState(PlayerIKControl.WalkState.POND_RETURN);

		// ! BOOM !
		_roller.ExplodeParticleSystem.Play();

		// Wait for the boom to finish.
		while(_roller.ExplodeParticleSystem.isPlaying)
		{
			yield return null;
		}

		// Tell the pond we're comin' home!
		PondManager.instance.HandlePondReturn(); 

		// Put ourselves in the right state of mind: the pond state.
		_roller.ChangeState(P_ControlState.POND);
	}
}
