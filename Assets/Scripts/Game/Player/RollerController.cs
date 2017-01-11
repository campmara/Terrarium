using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RollerController : ControllerBase 
{
	[ReadOnly] public Rigidbody rigidbody;

	public GameObject leftArmBlock;
	public GameObject rightArmBlock;

	// Input Freezing
	public void FreezeInput() { isInputFrozen = true; }
	public void UnfreezeInput() { isInputFrozen = false; }
	//public bool IsInputFrozen { get { return isInputFrozen; } }
	bool isInputFrozen = false;

	// STATE STUFF
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
		UnfreezeInput();
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
		if (!isInputFrozen)
		{
			currentState.HandleInput(input);
		}
	}
}
