using System.Collections;
using DG.Tweening;
using UnityEngine;

public class RitualState : RollerState
{
    //private Tween _tween;
	//private float ritualTimer = 0f;
	//float currentTurnSpeed = 0f;

	public override void Enter(P_ControlState prevState)
	{
		Debug.Log("ENTER RITUAL STATE");
        //_tween = transform.DOScaleY( 0.1f, RollerConstants.instance.RITUAL_TIME ).OnComplete( OnCompleteRitual );
        _roller.IK.SetState( PlayerIKControl.WalkState.RITUAL );
		//ritualTimer = 0f;
	    //PlayerManager.instance.Player.AnimationController.PlayIdleAnim();

		//AudioManager.instance.PlayClipAtIndex( AudioManager.AudioControllerNames.PLAYER_ACTIONFX, 2 );

		//_roller.Face.BecomeDesirous();

		_roller.Mesh.SetActive(false);
		_roller.Face.gameObject.SetActive(false);
		_roller.ExplodeParticleSystem.Play();
		OnCompleteRitual();
	}

	public override void Exit(P_ControlState nextState)
	{
		Debug.Log("EXIT RITUAL STATE");

		_roller.Mesh.SetActive(true);
		_roller.Face.gameObject.SetActive(true);

        _roller.IK.SetState( PlayerIKControl.WalkState.WALK );
		_roller.Face.BecomeIdle();
		AudioManager.instance.StopController( AudioManager.AudioControllerNames.PLAYER_ACTIONFX );

		_roller.ExplodeParticleSystem.Stop();
	}

	public override void HandleInput(InputCollection input)
	{
	    //bool isComplete = _tween.IsComplete();
		/*
		ritualTimer += Time.deltaTime;
		if (ritualTimer > RollerConstants.instance.RITUAL_TIME)
		{
			OnCompleteRitual();
		}

		currentTurnSpeed = Mathf.Lerp(RollerConstants.instance.RITUAL_TURN_SPEED * 0.1f, RollerConstants.instance.RITUAL_TURN_SPEED, ritualTimer / RollerConstants.instance.RITUAL_TIME);
		transform.Rotate(0f, currentTurnSpeed * Time.deltaTime, 0f);

	    if (!input.XButton.IsPressed)
		{
		    _roller.ChangeState(P_ControlState.WALKING);
		}
		*/
	}
		
    private void OnCompleteRitual()
    {
        GameManager.Instance.ChangeGameState( GameManager.GameState.POND_RETURN );

        Vector3 pos = transform.position;
        //transform.DOMoveY( -50.0f, 1.0f );

		_roller.IK.SetState( PlayerIKControl.WalkState.POND_RETURN );

        StartCoroutine( DelayedCompleteRitual( pos ) );        
    }

    IEnumerator DelayedCompleteRitual(Vector3 pos)
    {
		/*
		float currentPaintSize = 0f;
		float maxPaintSize = 10f;

		// Tell the plant manager to pop up all planted seeds in the vicinity and some grass / bushes.
				
		Tween paint = DOTween.To(()=> currentPaintSize, x=> currentPaintSize = x, maxPaintSize, RollerConstants.instance.RITUAL_COMPLETEWAIT);
		while(paint.IsPlaying())
		{
			GroundManager.instance.Ground.DrawSplatDecal(pos, currentPaintSize);
			//GroundManager.instance.Ground.DrawOnPosition(pos, currentPaintSize);
			yield return null;
		}

		yield return paint.WaitForCompletion();

        // TODO: implement plant watering here
        transform.localScale = Vector3.one;
		WaterPlantsCloseBy( currentPaintSize, pos );      
		*/

		while (_roller.ExplodeParticleSystem.isPlaying)
		{
			yield return null;
		}

		PondManager.instance.HandlePondReturn();
        _roller.StopPlayer();
		_roller.ChangeState(P_ControlState.POND);
    }

	void WaterPlantsCloseBy( float searchRadius, Vector3 pos )
	{
		Collider[] cols = Physics.OverlapSphere( pos, searchRadius );
		BasePlant plant = null;
		if( cols.Length > 0 )
		{
			foreach( Collider col in cols )
			{
				plant = col.GetComponent<BasePlant>();
				if( plant != null )
				{
					plant.WaterPlant();

					plant = null;
				}
			}
		}
	}
}
