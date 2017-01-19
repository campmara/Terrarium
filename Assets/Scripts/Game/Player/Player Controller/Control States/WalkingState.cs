using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class WalkingState : RollerState 
{
	Quaternion targetRotation = Quaternion.identity;
	Ray pickupRay;

    Tween _idleWaitTween = null;
    float _idleTimer = 0.0f;

    void Awake()
    {
        _controlState = P_ControlState.WALKING;
    }

    public override void Enter(RollerController parent, P_ControlState prevState)
	{
		Debug.Log("ENTER WALKING STATE");
        
        // Handle Transition
        switch (prevState)
        {
            case P_ControlState.WALKING:
                CameraManager.instance.ChangeCameraState( CameraManager.CameraState.FOLLOWPLAYER_LOCKED );                
                break;
            default:
                break;
        }

        roller = parent;

        PlayerManager.instance.Player.AnimationController.PlayWalkAnim();
	}

	public override void Exit(P_ControlState nextState)
	{
		Debug.Log("EXIT WALKING STATE");
	}

	public override void HandleInput(InputCollection input)
	{
		// A BUTTON
		if (input.AButton.WasPressed)
		{
			CheckForPickup();
		}

		// B BUTTON
		if (input.BButton.WasPressed & input.BButton.HasChanged)
		{
			roller.ChangeState(Walking, Rolling);
		}

		WalkMovement(input);
	}

	// ===============
	// M O V E M E N T
	// ===============

	void WalkMovement(InputCollection input)
	{
		// Left Stick Movement
		Vector3 vec = new Vector3(input.LeftStickX, 0f, input.LeftStickY);

        if( vec.magnitude > IDLE_MAXMAG )
        {
            if( _idleWaitTween != null )
            {
                _idleWaitTween.Kill();                
                _idleWaitTween = null;
            }

            // Accounting for camera position
            vec = CameraManager.instance.Main.transform.TransformDirection( vec );
            vec.y = 0f;
            inputVec = vec;

            if (Mathf.Abs( input.LeftStickX.Value ) > INPUT_DEADZONE || Mathf.Abs( input.LeftStickY.Value ) > INPUT_DEADZONE)
            {
                Accelerate( WALK_SPEED, WALK_ACCELERATION );
                Vector3 movePos = roller.transform.position + ( inputVec * velocity * Time.deltaTime );
                roller.RB.MovePosition( movePos );

                targetRotation = Quaternion.LookRotation( inputVec );

                lastInputVec = inputVec.normalized;
            }
            else if (velocity > 0f)
            {
                // Slowdown
                velocity -= WALK_DECELERATION * Time.deltaTime;
                Vector3 slowDownPos = roller.transform.position + ( lastInputVec * velocity * Time.deltaTime );
                roller.RB.MovePosition( slowDownPos );
            }

            // So player continues turning even after InputUp
            roller.transform.rotation = Quaternion.Slerp( roller.transform.rotation, targetRotation, WALK_TURN_SPEED * Time.deltaTime );
        }
        else
        {
            if(_idleWaitTween == null )
            {
                _idleTimer = 0.0f;
                _idleWaitTween = DOTween.To( () => _idleTimer, x => _idleTimer = x, 1.0f, IDLE_WAITTIME ).OnComplete( () => roller.ChangeState( Walking, Idling ) );              
            }
        }

	}

	// =============
	// P I C K U P S
	// =============

	void CheckForPickup()
	{
		pickupRay = new Ray(roller.transform.position + (Vector3.up * 1f), roller.transform.forward);
		Debug.DrawLine(pickupRay.origin, pickupRay.origin + (pickupRay.direction * 1.5f), Color.green);

		RaycastHit hit;

		if (Physics.Raycast(pickupRay, out hit, 1.5f))
		{
			//if the pickupable is a plant, we can only pick it up if it's still in seed stage
			PickupCollider collider = hit.collider.GetComponent<PickupCollider>();
			if( collider && ( collider.GetComponentInParent<Plant>() == null || collider.GetComponentInParent<Plant>().CurStage == Plant.GrowthStage.Unplanted ) )
			{ 
				PickUpObject( collider.GetComponentInParent<Pickupable>() );
			}
		}
	}

	void PickUpObject(Pickupable pickup)
	{
		if (pickup != null)
		{
			currentHeldObject = pickup;
			roller.ChangeState(Walking, Pickup);
		}
	}

}
