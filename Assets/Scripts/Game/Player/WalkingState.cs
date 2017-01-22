using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkingState : RollerState 
{
	Quaternion targetRotation = Quaternion.identity;
	Ray pickupRay;

	public override void Enter(RollerController parent)
	{
		Debug.Log("ENTER WALKING STATE");

		roller = parent;
	}

	public override void Exit()
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
			roller.ChangeState(Walking, WalkToRoll);
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

		// Accounting for camera position
		vec = CameraManager.instance.Main.transform.TransformDirection(vec);
		vec.y = 0f;
		inputVec = vec;

		if (Mathf.Abs(input.LeftStickX.Value) > INPUT_DEADZONE || Mathf.Abs(input.LeftStickY.Value) > INPUT_DEADZONE)
		{
			Accelerate(WALK_SPEED, WALK_ACCELERATION);
			Vector3 movePos = roller.transform.position + (inputVec * velocity * Time.deltaTime);
			roller.rigidbody.MovePosition(movePos);

			targetRotation = Quaternion.LookRotation(inputVec);

			lastInputVec = inputVec.normalized;
		}
		else if (velocity > 0f)
		{
			// Slowdown
			velocity -= WALK_DECELERATION * Time.deltaTime;
			Vector3 slowDownPos = roller.transform.position + (lastInputVec * velocity * Time.deltaTime);
			roller.rigidbody.MovePosition(slowDownPos);
		}

		// So player continues turning even after InputUp
		roller.transform.rotation = Quaternion.Slerp(roller.transform.rotation, targetRotation, WALK_TURN_SPEED * Time.deltaTime);
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

			if( collider )
			{ 
				Plantable plantComponent = collider.GetComponentInParent<Plantable>();
				if( plantComponent && plantComponent.CanPickup )
				{
					PickUpObject( collider.GetComponentInParent<Pickupable>() );
				}
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
