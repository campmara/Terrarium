using UnityEngine;

public enum P_ControlState
{
	NONE = 0,
	WALKING,
	ROLLING,
	PICKINGUP,
	CARRYING,
	RITUAL,
    PLANTING,
	SING
}

[RequireComponent(typeof(Rigidbody))]
public class RollerController : ControllerBase 
{
    [ReadOnly]
    private Rigidbody _rigidbody = null;
    public Rigidbody RB { get { return _rigidbody; } }

    [ReadOnly] private PlayerIKControl _ik = null;
    public PlayerIKControl IK { get { return _ik; } }

    [ReadOnly] private FaceManager _face = null;
    public FaceManager Face { get { return _face; } }

	[ReadOnly] private GameObject _mesh = null;
	public GameObject Mesh { get { return _mesh; } }

	[ReadOnly] private GameObject _rig = null;
	public GameObject Rig { get { return _rig; } }

	[ReadOnly] private GameObject _rollSphere = null;
	public GameObject RollSphere { get { return _rollSphere; } }

	// These have accessors in the RollerState
	[ReadOnly] Pickupable _currentHeldObject = null;
    public Pickupable CurrentHeldObject { get { return _currentHeldObject; } set { _currentHeldObject = value; } }
	[ReadOnly] Vector3 _inputVec = Vector3.zero;
    public Vector3 InputVec { get { return _inputVec; } set { _inputVec = value; } }
    [ReadOnly] Vector3 _lastInputVec = Vector3.zero;
    public Vector3 LastInputVec { get { return _lastInputVec; } set { _lastInputVec = value; } }
    [ReadOnly] float _velocity = 0f;
    public float Velocity { get { return _velocity; } set { _velocity = value; } }
	[ReadOnly] bool _idling = false;
	public bool Idling { get { return _idling; } set { _idling = value; } }

	private Quaternion targetRotation = Quaternion.identity;
	private Coroutine _idleWaitRoutine = null;

	// ===========
	// S T A T E S
	// ===========

	// STATE MACHINE
	RollerState _currentState;

	protected P_ControlState _controlState = P_ControlState.NONE;
	public P_ControlState State { get { return _controlState; } set { _controlState = value; } }

	private WalkingState _walking = null;   
	private RollingState _rolling = null;	
	private PickupState _pickup = null;
	private CarryState _carrying = null;
	private RitualState _ritual = null;	
    private PlantingState _planting = null;
	private SingState _singing = null;

	void Awake()
	{
		//Debug.Log("Added Test Controller to Player Control Manager");
        _rigidbody = GetComponent<Rigidbody>();
	    _ik = GetComponentInChildren<PlayerIKControl>();
	    _face = GetComponentInChildren<FaceManager>();
		_mesh = GetComponentInChildren<SkinnedMeshRenderer>().gameObject;
		_rig = transform.GetChild(0).gameObject;
		_rollSphere = transform.GetChild(2).gameObject;

		// Add State Controller, Set parent to This Script, set to inactive
		_walking = this.gameObject.AddComponent<WalkingState>();
		_walking.RollerParent = this;

		_rolling = this.gameObject.AddComponent<RollingState>();
		_rolling.RollerParent = this;

		_pickup = this.gameObject.AddComponent<PickupState>();
		_pickup.RollerParent = this;

		_carrying = this.gameObject.AddComponent<CarryState>();
		_carrying.RollerParent = this;

		_ritual = this.gameObject.AddComponent<RitualState>();
		_ritual.RollerParent = this;

        _planting = this.gameObject.AddComponent<PlantingState>();
        _planting.RollerParent = this;

		_singing = this.gameObject.AddComponent<SingState>();
		_singing.RollerParent = this;

        // Set state to default (walking for now)
        ChangeState( P_ControlState.NONE, P_ControlState.WALKING );
	}

/* Can use these for if we are swapping in & out controllers from Player Control Manager
	void OnEnable()
	{
		//Debug.Log("Enabled Test Controller on Player Control Manager");
	}

	// Also called on Destroy
	void OnDisable()
	{
		//Debug.Log("Disabled Test Controller on Player Control Manager");	
	}
*/

	public void ChangeState(P_ControlState fromState, P_ControlState toState)
	{
		// Exit & Deactivate current state
		if( fromState != P_ControlState.NONE )
		{
			_currentState.Exit( toState );

			//_currentState.enabled = false;

			if (_idleWaitRoutine != null)
			{
				StopCoroutine( _idleWaitRoutine );
				_idleWaitRoutine = null;
			}


		}
			
		// Enter and Activate New State
		switch( toState )
		{
		case P_ControlState.WALKING:
			_currentState = _walking;
			break;
		case P_ControlState.ROLLING:
			_currentState = _rolling;
			break;
		case P_ControlState.PICKINGUP:
			_currentState = _pickup;
			break;
		case P_ControlState.CARRYING:
			_currentState = _carrying;
			break;
		case P_ControlState.RITUAL:
			_currentState = _ritual;
			break;
        case P_ControlState.PLANTING:
            _currentState = _planting;
            break;
		case P_ControlState.SING:
			_currentState = _singing;
			break;
        default:
			break;
		}

		//_currentState.enabled = true;

		_currentState.Enter( fromState );
	}


	protected override void HandleInput()
	{
        // Always keep this at zero because the rigidbody's velocity is never needed and bumping into things
        // makes the character go nuts.
        _rigidbody.velocity = Vector3.zero;
		_rigidbody.angularVelocity = Vector3.zero;

		_currentState.HandleInput(_input);
	}

	// ======================
	// BASIC CONTROLLER STUFF
	// ======================

	public void StandardMovement(float maxMoveSpeed, float moveAcceleration, float moveDeceleration,
								 float maxTurnSpeed)
	{
		// Left Stick Movement
		Vector3 vec = new Vector3(_input.LeftStickX, 0f, _input.LeftStickY);

		if(vec.magnitude > RollerConstants.IDLE_MAXMAG)
		{
			if(_idleWaitRoutine != null)
			{
				StopCoroutine(_idleWaitRoutine);
				_idleWaitRoutine = null;
			}

			if (_idling)
			{
				HandleEndIdle();
			}

			// Accounting for camera position
			vec = CameraManager.instance.Main.transform.TransformDirection(vec);
			vec.y = 0f;
			_inputVec = vec;

		    // MOVEMENT
		    {
		        Accelerate(maxMoveSpeed, moveAcceleration);
		        Vector3 movePos = transform.position + (_inputVec * _velocity * Time.deltaTime);
		        _rigidbody.MovePosition(movePos);

		        targetRotation = Quaternion.LookRotation(_inputVec);

		        _lastInputVec = _inputVec.normalized;
		    }

		    // So player continues turning even after InputUp
			transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, maxTurnSpeed * Time.deltaTime);
		}
	    else if (_velocity > 0f)
		{
		    // Slowdown
		    _velocity -= moveDeceleration * Time.deltaTime;
		    Vector3 slowDownPos = transform.position + (_lastInputVec * _velocity * Time.deltaTime );
		    _rigidbody.MovePosition( slowDownPos );
		}
		else
		{
		    if( _idleWaitRoutine == null )
			{
				_idleWaitRoutine = StartCoroutine( JohnTech.WaitFunction(RollerConstants.IDLE_WAITTIME, () => HandleBeginIdle() ) );
			}
		}
	}

	public void Accelerate(float max, float accel, float inputAffect = 1.0f)
	{
		_velocity += accel * inputAffect;
		if (Mathf.Abs(_velocity) > max)
		{
			_velocity = Mathf.Sign(_velocity) * max;
		}
	}

	public void HandleBeginIdle()
	{
		_idling = true;

		//PlayerManager.instance.Player.AnimationController.PlayIdleAnim();

	    // SET THE IK STATE (REPLACES ABOVE)
	    IK.SetState(PlayerIKControl.WalkState.IDLE);
	}

	public void HandleEndIdle()
	{
		_idling = false;

		//PlayerManager.instance.Player.AnimationController.PlayWalkAnim();

	    // SET THE IK STATE (REPLACES ABOVE)
	    IK.SetState(PlayerIKControl.WalkState.WALK);
	}

	public void BecomeBall()
	{
		_ik.SetState(PlayerIKControl.WalkState.IDLE);

		_face.gameObject.SetActive(false);
		_mesh.SetActive(false);
		_rig.SetActive(false);
		_rollSphere.SetActive(true);

		AudioManager.instance.PlayClipAtIndex( AudioManager.AudioControllerNames.PLAYER_TRANSITIONFX, 0 );
	}

	public void BecomeWalker()
	{
		_face.gameObject.SetActive(true);
		_mesh.SetActive(true);
		_rig.SetActive(true);
		_rollSphere.SetActive(false);

		_ik.ResetLegs();
		_ik.SetState(PlayerIKControl.WalkState.WALK);

		AudioManager.instance.PlayClipAtIndex( AudioManager.AudioControllerNames.PLAYER_TRANSITIONFX, 1 );
	}
}
