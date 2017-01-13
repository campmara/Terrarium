using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class WalkingState : RollerState 
{
	const float WALK_SPEED = 4f;
	const float CARRYING_SPEED = 3f;
	const float ROTATION_SPEED = 9f;
	const float CARRYING_ROTATION_SPEED = 7f;
	const float SLOWDOWN_RATE = 15f;
	const float INPUT_DEADZONE = 0.3f;

	// PICKUP VALUES
	const float PICKUP_TIME = 0.75f;

	RollerController _roller;

	float _currentWalkSpeed;
	float _currentRotationSpeed;
	Vector3 _currInputVec = Vector3.zero;
	Vector3 _lastInputVec = Vector3.zero;
	float _velocity = 0f;

	Quaternion _targetRotation = Quaternion.identity;

	Pickupable _currentHeldObject = null;
	Ray _pickupRay;

	public override void Enter(RollerController parent)
	{
		Debug.Log("ENTER WALKING STATE");

		_roller = parent;
		_currentWalkSpeed = WALK_SPEED;
		_currentRotationSpeed = ROTATION_SPEED;

		// MOVE THE HANDS, THIS WILL BE REPLACED BY ANIMATIONS
		Vector3 pos = _roller.transform.position + _roller.transform.forward + (_roller.transform.up * 0.5f);
		_roller.FreezeInput();
		_roller.leftArmBlock.transform.DOMove(pos, 0.75f);
		_roller.rightArmBlock.transform.DOMove(pos, 0.75f).OnComplete(_roller.UnfreezeInput);
		// END

		CameraManager.instance.ChangeCameraState( CameraManager.CameraState.FOLLOWPLAYER_FREE );
	}

	public override void Exit()
	{
		Debug.Log("EXIT WALKING STATE");

		// Drop Current Held Object.
		DropHeldObject();
	}

	public override void HandleInput(InputCollection input)
	{
		// Always keep this at zero because the rigidbody's velocity is never needed and bumping into things
		// makes the character go nuts.
		_roller.rigidbody.velocity = Vector3.zero;

		/*
			A BUTTON
		*/
		if (input.AButton.WasPressed)
		{
			if (_currentHeldObject != null)
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
			_roller.ChangeState(RollerState.Walking, RollerState.Rolling);
		}

		/*
			LEFT STICK MOVEMENT
            Only Moving on X & Z axis
		*/
		_currInputVec.x = input.LeftStickX;
		_currInputVec.z = input.LeftStickY;

        // Accounting for camera position
		_currInputVec = CameraManager.instance.Main.transform.TransformDirection(_currInputVec);
		_currInputVec.y = 0.0f;
        
		if (Mathf.Abs(input.LeftStickX.Value) > INPUT_DEADZONE || Mathf.Abs(input.LeftStickY.Value) > INPUT_DEADZONE)
		{
			_velocity = _currentWalkSpeed;
			Vector3 movePos = _roller.transform.position + (_currInputVec * _velocity * Time.deltaTime);
			_roller.rigidbody.MovePosition(movePos);

			_targetRotation = Quaternion.LookRotation(_currInputVec);

			_lastInputVec = _currInputVec.normalized;
		}
		else if (_velocity > 0f)
		{
			// Slowdown
			_velocity -= SLOWDOWN_RATE * Time.deltaTime;
			Vector3 slowDownPos = _roller.transform.position + (_lastInputVec * _velocity * Time.deltaTime);
			_roller.rigidbody.MovePosition(slowDownPos);
		}

		// So player continues turning even after InputUp
		_roller.transform.rotation = Quaternion.Slerp(_roller.transform.rotation, _targetRotation, _currentRotationSpeed * Time.deltaTime);
	}

	void CheckForPickup()
	{
		_pickupRay = new Ray(_roller.transform.position + (Vector3.up * 1f), _roller.transform.forward);
		Debug.DrawLine(_pickupRay.origin, _pickupRay.origin + (_pickupRay.direction * 1.5f), Color.green);

		RaycastHit hit;

		if (Physics.Raycast(_pickupRay, out hit, 1.5f))
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
			_currentHeldObject = pickup;
			_currentHeldObject.transform.parent = _roller.transform;
			_currentHeldObject.OnPickup();
			
			Vector3 pickupPos = _roller.transform.position + (_roller.transform.forward * 1f) + (_roller.transform.up * 1f);
			_currentHeldObject.transform.DOMove(pickupPos, PICKUP_TIME).OnComplete(_roller.UnfreezeInput);
			_roller.FreezeInput();

			_currentWalkSpeed = CARRYING_SPEED;
			_currentRotationSpeed = CARRYING_ROTATION_SPEED;
		}
	}

	void DropHeldObject()
	{
		if (_currentHeldObject != null)
		{
			_currentHeldObject.DropSelf();
			_currentHeldObject = null;

			_currentWalkSpeed = WALK_SPEED;
			_currentRotationSpeed = ROTATION_SPEED;
		}
	}
}
