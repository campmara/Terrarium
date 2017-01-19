using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RollerController : ControllerBase 
{
    [ReadOnly]
    Rigidbody _rigidbody = null;
    public Rigidbody RB { get { return _rigidbody; } }

	// These have accessors in the RollerState
	[ReadOnly] Pickupable _currentHeldObject = null;
    public Pickupable CurrentHeldObject { get { return _currentHeldObject; } set { _currentHeldObject = value; } }
	[ReadOnly] Vector3 _inputVec = Vector3.zero;
    public Vector3 InputVec { get { return _inputVec; } set { _inputVec = value; } }
    [ReadOnly] Vector3 _lastInputVec = Vector3.zero;
    public Vector3 LastInputVec { get { return _lastInputVec; } set { _lastInputVec = value; } }
    [ReadOnly] float _velocity = 0f;
    public float Velocity { get { return _velocity; } set { _velocity = value; } }

    // STATE MACHINE
    RollerState currentState;

	public void ChangeState(RollerState fromState, RollerState toState)
	{
		fromState.Exit( toState.State );
		currentState = toState;
		currentState.Enter( this, fromState.State );
	}

	void Awake()
	{
		//Debug.Log("Added Test Controller to Player Control Manager");
		ChangeState(new RollerState(), RollerState.Walking);

        _rigidbody = GetComponent<Rigidbody>();
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
        _rigidbody.velocity = Vector3.zero;

		currentState.HandleInput(_input);
	}
}
