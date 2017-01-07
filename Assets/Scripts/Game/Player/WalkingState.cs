using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class WalkingState : RollerState 
{
	const float WALK_SPEED = 4f;
	const float AUTO_ROTATION_SPEED = 7f;
	const float SLOWDOWN_RATE = 15f;
	const float INPUT_DEADZONE = 0.3f;

	// PICKUP VALUES
	const float PICKUP_TIME = 0.75f;

	RollerController roller;

	Vector3 lastInputVec;
	float velocity = 0f;

	Pickupable currentHeldObject = null;
	Ray pickupRay;

	public override void Enter(RollerController parent)
	{
		Debug.Log("ENTER WALKING STATE");
		roller = parent;

		Vector3 pos = roller.transform.position + roller.transform.forward + (roller.transform.up * 0.5f);
		roller.leftArmBlock.transform.DOMove(pos, 1f);
		roller.rightArmBlock.transform.DOMove(pos, 1f);
	}

	public override void Exit()
	{
		Debug.Log("EXIT WALKING STATE");

		// Drop Current Held Object.
		DropHeldObject();
	}

	public override void HandleInput(InputCollection input)
	{
		/*
			A BUTTON
		*/
		if (input.AButton.WasPressed)
		{
			if (currentHeldObject != null)
			{
				DropHeldObject();
			}
			else
			{
				CheckForPickup();
			}
		}

		/*
			B BUTTON
		*/
		if (input.BButton.WasPressed & input.BButton.HasChanged)
		{
			roller.ChangeState(RollerState.Walking, RollerState.Rolling);
		}

		/*
			LEFT STICK MOVEMENT
		*/
		Vector3 inputVec = new Vector3
		(
			input.LeftStickX,
			0f,
			input.LeftStickY
		);

		if (Mathf.Abs(input.LeftStickX.Value) > INPUT_DEADZONE || Mathf.Abs(input.LeftStickY.Value) > INPUT_DEADZONE)
		{
			velocity = WALK_SPEED;
			Vector3 movePos = roller.transform.position + (inputVec * velocity * Time.deltaTime);
			roller.rigidbody.MovePosition(movePos);

			Quaternion qTo = Quaternion.LookRotation(inputVec);
			roller.transform.rotation = Quaternion.Slerp(roller.transform.rotation, qTo, AUTO_ROTATION_SPEED * Time.deltaTime);

			lastInputVec = inputVec.normalized;
		}
		else if (velocity > 0)
		{
			// Slowdown
			velocity -= SLOWDOWN_RATE * Time.deltaTime;
			Vector3 slowDownPos = roller.transform.position + (lastInputVec * velocity * Time.deltaTime);
			roller.rigidbody.MovePosition(slowDownPos);
		}
	}

	void CheckForPickup()
	{
		pickupRay = new Ray(roller.transform.position + (Vector3.up * 1f), roller.transform.forward);
		Debug.DrawLine(pickupRay.origin, pickupRay.origin + (pickupRay.direction * 1.5f), Color.green);

		RaycastHit hit;

		if (Physics.Raycast(pickupRay, out hit, 1.5f))
		{
			PickUpObject(hit.collider.GetComponent<PickupCollider>().GetComponentInParent<Pickupable>());
		}
	}

	void PickUpObject(Pickupable pickup)
	{
		if (pickup != null)
		{
			currentHeldObject = pickup;
			currentHeldObject.transform.parent = roller.transform;
			currentHeldObject.OnPickup();
			
			Vector3 pickupPos = roller.transform.position + (roller.transform.forward * 1f) + (roller.transform.up * 1f);
			currentHeldObject.transform.DOMove(pickupPos, PICKUP_TIME);
		}
	}

	void DropHeldObject()
	{
		if (currentHeldObject != null)
		{
			currentHeldObject.DropSelf();
			currentHeldObject = null;
		}
	}
}
