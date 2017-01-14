using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RollerController : ControllerBase 
{
	[ReadOnly] public Rigidbody rigidbody;

	// These have accessors in the RollerState
	[ReadOnly] public Pickupable currentHeldObject = null;
	[ReadOnly] public Vector3 inputVec = Vector3.zero;
	[ReadOnly] public Vector3 lastInputVec = Vector3.zero;
	[ReadOnly] public float velocity = 0f;

	// STATE MACHINE
	RollerState currentState;
	public void ChangeState(RollerState fromState, RollerState toState)
	{
		fromState.Exit();
		currentState = toState;
		currentState.Enter(this);
	}

	void Awake()
	{
		//Debug.Log("Added Test Controller to Player Control Manager");
		ChangeState(new RollerState(), RollerState.Walking);

		rigidbody = GetComponent<Rigidbody>();
	}

	void OnEnable()
	{
		//Debug.Log("Enabled Test Controller on Player Control Manager");
	}

	// Also called on Destroy
	void OnDisable()
	{
		//Debug.Log("Disabled Test Controller on Player Control Manager");	
	}

	protected override void HandleInput()
	{
		// Always keep this at zero because the rigidbody's velocity is never needed and bumping into things
		// makes the character go nuts.
		rigidbody.velocity = Vector3.zero;

		currentState.HandleInput(input);
	}
}
