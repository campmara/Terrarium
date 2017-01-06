using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RollerController : ControllerBase 
{
	public const float WALK_SPEED = 0.075f;
	public const float ROLL_SPEED = 1f;

	[ReadOnly] public Rigidbody rigidbody;

	RollerState currentState;
	public void ChangeState(RollerState newState)
	{
		if (currentState != null)
		{
			currentState.Exit(this);
		}

		currentState = newState;
		currentState.Enter(this);
	}

	void Awake()
	{
		Debug.Log("Added Test Controller to Player Control Manager");

		ChangeState(RollerState.Walking);

		rigidbody = GetComponent<Rigidbody>();
	}

	void OnEnable()
	{
		Debug.Log("Enabled Test Controller on Player Control Manager");
	}

	// Also called on Destroy
	void OnDisable()
	{
		Debug.Log("Disabled Test Controller on Player Control Manager");	
	}

	protected override void HandleInput()
	{
		currentState.HandleInput(this, input);
	}
}
