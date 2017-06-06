using UnityEngine;
using System.Collections;

public enum P_ControlState
{
	NONE = 0,
	WALKING,
	ROLLING,
	PICKINGUP,
	CARRYING,
	RITUAL,
    PLANTING,
	SING,
	SIT,
	POND
}

[RequireComponent(typeof(Rigidbody))]
public class RollerController : ControllerBase 
{
	private Player _player = null;
	public Player Player { get { return _player; } }

    [ReadOnly]
    private Rigidbody _rigidbody = null;
    public Rigidbody RB { get { return _rigidbody; } }

    [ReadOnly] private PlayerIKControl _ik = null;
    public PlayerIKControl IK { get { return _ik; } }

    [ReadOnly] private FaceManager _face = null;
    public FaceManager Face { get { return _face; } }

	[SerializeField] private GameObject _mesh = null;
	public GameObject Mesh { get { return _mesh; } }

	[SerializeField] private GameObject _rig = null;
	public GameObject Rig { get { return _rig; } }

	[SerializeField] private GameObject _rollSphere = null;
	public GameObject RollSphere { get { return _rollSphere; } }

	[SerializeField] private GameObject _splatImprint = null;
	public GameObject SplatImprint { get { return _splatImprint; } }

	[ReadOnly] private ParticleSystem _explodeParticleSystem = null;
	public ParticleSystem ExplodeParticleSystem { get { return _explodeParticleSystem; } }

	// These have accessors in the RollerState
	[ReadOnly, SerializeField] Pickupable _currentHeldObject = null;
    public Pickupable CurrentHeldObject { get { return _currentHeldObject; } set { _currentHeldObject = value; } }
	[ReadOnly] Vector3 _inputVec = Vector3.zero;
    public Vector3 InputVec { get { return _inputVec; } set { _inputVec = value; } }
    [ReadOnly] Vector3 _lastInputVec = Vector3.zero;
    public Vector3 LastInputVec { get { return _lastInputVec; } set { _lastInputVec = value; } }
    [ReadOnly, SerializeField] float _velocity = 0f;
    public float Velocity { get { return _velocity; } set { _velocity = value; } }
	[ReadOnly] bool _idling = false;
	public bool Idling { get { return _idling; } set { _idling = value; } }

	[ReadOnly] Vector3 _targetIKPosition = Vector3.zero;
	public Vector3 TargetIKPosition { get { return _targetIKPosition; } set { _targetIKPosition = value; } }
	[ReadOnly] Vector3 _targetMovePosition = Vector3.zero;
	public Vector3 TargetMovePosition { get { return _targetMovePosition; } set { _targetMovePosition = value; } }
    [ReadOnly] float _headMoveSpeedInterp = 0.0f;
    public float HeadMoveInterp { get { return _headMoveSpeedInterp; } set { _headMoveSpeedInterp = value; } }
	float _currMaxVelocity = 0.0f;

	[SerializeField] GameObject _carryPositionObject = null;
	public GameObject CarryPositionObject { get { return _carryPositionObject; } }
	private Vector3 _carryPosOffset = Vector3.zero;
	public Vector3 CarryPosOffset { get { return _carryPosOffset; } set { _carryPosOffset = value; } } 

	private Quaternion _targetRotation = Quaternion.identity;
    private float _targetRotAngle = 0.0f;
	private Coroutine _idleWaitRoutine = null;

    [SerializeField]
    SkinnedMeshRenderer _bodyRenderer = null;
    public SkinnedMeshRenderer BodyRenderer { get { return _bodyRenderer; } }
    private const string SPHERIFYSCALE_SHADERPROP = "_SphereScale";
    private const string SPHERIFY_SHADERPROP = "_Spherification";
    int _spherifyPropertyHash = 0;
    int _spherifyScalePropertyHash = 0;
    public float SpherifyScale { get { return _bodyRenderer.material.GetFloat( _spherifyScalePropertyHash ); } set { _bodyRenderer.material.SetFloat( _spherifyScalePropertyHash, value ); } }
    public float Spherify { get { return _bodyRenderer.material.GetFloat( _spherifyPropertyHash ); } set { _bodyRenderer.material.SetFloat( _spherifyPropertyHash, value ); } }

    float _breathTimer = 0.0f;
    public float BreathTimer { get { return _breathTimer; } set { _breathTimer = value; } }

    WaterAccentController _waterAccentController = null;
    public WaterAccentController WaterAccent { get { return _waterAccentController; } }

    // ===========
    // S T A T E S
    // ===========

    // STATE MACHINE
   RollerState _currentState;

    [SerializeField, ReadOnly]
    protected P_ControlState _controlState = P_ControlState.NONE;
	public P_ControlState State { get { return _controlState; } set { _controlState = value; } }

	private WalkingState _walking = null;   
	private RollingState _rolling = null;	
	private PickupState _pickup = null;
	private CarryState _carrying = null;
	private RitualState _ritual = null;	
    private PlantingState _planting = null;
	private SingState _singing = null;
	private SittingState _sitting = null;
	private PondState _ponding = null;

	void Awake()
	{
		//Debug.Log("Added Test Controller to Player Control Manager");
		_player = GetComponent(typeof(Player)) as Player;
		_rigidbody = GetComponent(typeof(Rigidbody)) as Rigidbody;
	    _ik = GetComponentInChildren(typeof(PlayerIKControl)) as PlayerIKControl;
	    _face = GetComponentInChildren(typeof(FaceManager)) as FaceManager;
		_explodeParticleSystem = GetComponentInChildren(typeof(ParticleSystem)) as ParticleSystem;
        _waterAccentController = this.GetComponentInChildren<WaterAccentController>();

        _spherifyPropertyHash = Shader.PropertyToID( SPHERIFY_SHADERPROP );
        _spherifyScalePropertyHash = Shader.PropertyToID( SPHERIFYSCALE_SHADERPROP );

        // Add State Controller, Set parent to This Script, set to inactive
        _walking = this.gameObject.AddComponent(typeof(WalkingState)) as WalkingState;
		_walking.RollerParent = this;

		_rolling = this.gameObject.AddComponent(typeof(RollingState)) as RollingState;
		_rolling.RollerParent = this;

		_pickup = this.gameObject.AddComponent(typeof(PickupState)) as PickupState;
		_pickup.RollerParent = this;

		_carrying = this.gameObject.AddComponent(typeof(CarryState)) as CarryState;
		_carrying.RollerParent = this;

		_ritual = this.gameObject.AddComponent(typeof(RitualState)) as RitualState;
		_ritual.RollerParent = this;

        _planting = this.gameObject.AddComponent(typeof(PlantingState)) as PlantingState;
        _planting.RollerParent = this;

		_singing = this.gameObject.AddComponent(typeof(SingState)) as SingState;
		_singing.RollerParent = this;

		_sitting = this.gameObject.AddComponent(typeof(SittingState)) as SittingState;
		_sitting.RollerParent = this;

		_ponding = this.gameObject.AddComponent(typeof(PondState)) as PondState;
		_ponding.RollerParent = this;

        // Set state to default (walking for now)
        ChangeState(P_ControlState.POND);
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

	public void ChangeState(P_ControlState toState)
	{
		// Exit & Deactivate current state
		if( _controlState != P_ControlState.NONE )
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
		case P_ControlState.SIT:
			_currentState = _sitting;
			break;
		case P_ControlState.POND:
			_currentState = _ponding;
			break;
        default:
			break;
		}

		//_currentState.enabled = true;

		_currentState.Enter( _controlState );

        _controlState = toState;
    }

	protected override void HandleInput()
	{
        _currentState.HandleInput( _input );
	}

    protected override void HandleFixedInput()
    {
        // Always keep this at zero because the rigidbody's velocity is never needed and bumping into things
        // makes the character go nuts.
        _rigidbody.velocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;

        _currentState.HandleFixedInput( _input );
    }

    // ======================
    // BASIC CONTROLLER STUFF
    // ======================

    /// IS NO LONGER BEING USED, PLEASE LOOK AT IK MOVEMENT METHOD ///
    public void StandardMovement(float maxMoveSpeed, float moveAcceleration, float moveDeceleration,
								 float maxTurnSpeed)
	{
		// Left Stick Movement
		Vector3 vec = new Vector3(_input.LeftStickX, 0f, _input.LeftStickY);

		if(vec.magnitude > RollerConstants.instance.IdleMaxMag)
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

		        _targetRotation = Quaternion.LookRotation(_inputVec);

		        _lastInputVec = _inputVec.normalized;
		    }

		    // So player continues turning even after InputUp
			transform.rotation = Quaternion.Slerp(transform.rotation, _targetRotation, maxTurnSpeed * Time.deltaTime);
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
				_idleWaitRoutine = StartCoroutine( JohnTech.WaitFunction(RollerConstants.instance.IdleWaitTime, () => HandleBeginIdle() ) );
			}
		}
	}


	public void IKMovement(float maxMoveSpeed, float moveAcceleration, float moveDeceleration,
		float maxTurnSpeed, float bodyMoveSpeed = 7.5f )
	{
		// Left Stick Movement
		Vector3 vec = new Vector3(_input.LeftStickX, 0f, _input.LeftStickY);

		if( vec.magnitude > RollerConstants.instance.IdleMaxMag )
		{
			if( _idleWaitRoutine != null )
			{
				StopCoroutine(_idleWaitRoutine);
				_idleWaitRoutine = null;
			}

			if( _idling )
			{
				HandleEndIdle();
			}

			// Forces a max velocity based on how magnitude of controller stick
			_currMaxVelocity = Mathf.Lerp( 0.0f, maxMoveSpeed, vec.magnitude );

			// Accounting for camera position
			vec = CameraManager.instance.Main.transform.TransformDirection(vec);
			vec.y = 0f;
			_inputVec = vec.normalized;

			// MOVEMENT
			
			Accelerate( _currMaxVelocity, moveAcceleration );
			
			_targetRotation = Quaternion.LookRotation( _inputVec );
            _targetRotAngle = Quaternion.Angle(_targetRotation, transform.rotation);            

			_lastInputVec = _inputVec;
			
			// So player continues turning even after InputUp

			_rigidbody.MoveRotation( Quaternion.Slerp( transform.rotation, _targetRotation, maxTurnSpeed * Time.deltaTime ) );
		}
		else if ( _velocity > 0f )
		{
			// Slowdown
			_velocity -= moveDeceleration * Time.deltaTime;

			if( _velocity < 0.0f )
			{
				_velocity = 0.0f;
			}
		}
		else
		{
			if( _idleWaitRoutine == null )
			{
				_idleWaitRoutine = StartCoroutine( JohnTech.WaitFunction(RollerConstants.instance.IdleWaitTime, () => HandleBeginIdle() ) );
			}
		}
		_ik.UpdateMovementData( _velocity, transform.position + (transform.forward * _inputVec.magnitude * _velocity * RollerConstants.instance.IKTargetWorldScalar * Time.deltaTime), transform.rotation );

		// Hmm
		// yeah hmmm
		this._player.AnimationController.SetPlayerSpeed( _velocity / maxMoveSpeed );
        _waterAccentController.SetWaterAccentVolume( _velocity / maxMoveSpeed );

        if ( CanPlayerMove() )
        {
            _rigidbody.MovePosition( transform.position + ( transform.forward * _inputVec.magnitude * _velocity * Time.deltaTime) );
            _rigidbody.position = new Vector3( _rigidbody.position.x,
                                                PondManager.instance.Pond.GetPondY( _rigidbody.position ),
                                                _rigidbody.position.z );
        }	

		//_rigidbody.MovePosition( Vector3.Lerp(transform.position, _targetMovePosition, Mathf.Lerp( RollerConstants.instance.BODY_MINMOVESPEED, bodyMoveSpeed, _headMoveSpeedInterp ) * Time.fixedDeltaTime ) );
	}

	public void UpdateArmReachIK( float leftArmValue, float rightArmValue )
	{
		_ik.UpdateArmInterpValues( leftArmValue, rightArmValue );
	}

	public void Accelerate( float max, float accel, float inputAffect = 1.0f )
	{
		_velocity += accel * inputAffect;
		if ( Mathf.Abs( _velocity ) > max )
		{
			_velocity = Mathf.Sign(_velocity) * max;
		}

        _velocity -= RollerConstants.instance.WalkTurnDampening * Mathf.InverseLerp( RollerConstants.instance.WalkTurnAngleMin, RollerConstants.instance.WalkTurnAngleMax, _targetRotAngle );

		if( _velocity < 0.0f )
		{
			_velocity = 0.0f;
		}
    }

    public bool CanPlayerMove()
    {
        if( PlayerManager.instance.DistanceFromPond >= 1.0f )
        {
            if( Vector3.Dot( PlayerManager.instance.DirectionFromPond, this.transform.forward ) > 0.0f )
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Zeros out Velocity
    /// </summary>
    public void ZeroVelocity()
    {
        _velocity = 0.0f;
        _waterAccentController.SetWaterAccentVolume( _velocity );
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

		//_face.gameObject.SetActive(false);
		_mesh.SetActive(false);
		_rig.SetActive(false);
		_rollSphere.SetActive(true);

        _waterAccentController.SetWaterAccentPitch( 1.5f );

		AudioManager.instance.PlayClipAtIndex( AudioManager.AudioControllerNames.PLAYER_TRANSITIONFX, 0 );
	}

	public void BecomeWalker()
	{
		//_face.gameObject.SetActive(true);
		_mesh.SetActive(true);
		_rig.SetActive(true);

		_rollSphere.transform.localPosition = Vector3.up * 1.5f;
		_rollSphere.SetActive(false);

        //Spherify = 0.0f;

        _targetMovePosition = this.transform.position;        
        this._player.AnimationController.SetPlayerSpeed( 0.0f );

		if (InputVec.magnitude > RollerConstants.instance.IdleMaxMag)
		{
			// dooooo nothing?
		}
		else
		{
			_velocity = 0f;
		}
        _waterAccentController.SetWaterAccentVolume( _velocity );
        _waterAccentController.SetWaterAccentPitch( 1.0f );

        _ik.ResetLegs();

        _ik.SetState( PlayerIKControl.WalkState.WALK );        

        AudioManager.instance.PlayClipAtIndex( AudioManager.AudioControllerNames.PLAYER_TRANSITIONFX, 1 );
	}

	public void HandleOutOfBounds()
	{
		// Put ourselves in the right state of mind: the pond state.
		BecomeWalker();

        if (_currentHeldObject != null)
        {
            _currentState.HandleBothArmRelease();
        }

        ChangeState(P_ControlState.POND);

		// Tell the pond we're comin' home!
		PondManager.instance.HandlePondReturn();
	}

	public void HandlePondReturn()
	{
		Coroutine returnRoutine = StartCoroutine(PondReturnRoutine());
	}

	private IEnumerator PondReturnRoutine()
	{
		// Handle all the object deactivation and state change we require.
		_mesh.SetActive(false);
        _face.gameObject.SetActive(false);
		_rollSphere.SetActive(false);
		_ik.SetState(PlayerIKControl.WalkState.POND_RETURN);

		// ! BOOM !
		AudioManager.instance.PlayClipAtIndex(AudioManager.AudioControllerNames.PLAYER_ACTIONFX, 3);
		_explodeParticleSystem.Play();

		// Wait for the boom to finish.
		while( _explodeParticleSystem.isPlaying )
		{
			yield return null;
		}

		// Put ourselves in the right state of mind: the pond state.
		ChangeState(P_ControlState.POND);

        // Tell the pond we're comin' home!
        //PondManager.instance.HandlePondReturn(); 
        PondManager.instance.HandlePondWait();
    }

	public void MakeDroopyExplode()
	{
		_collidedWithObject = true;
		BecomeWalker();
		HandlePondReturn();
	}

    public float GetArmInterpTotal()
    {
        return _ik.RightArm.ArmReachInterp + _ik.LeftArm.ArmReachInterp;
    }


    public bool CollidedWithObject { get { return _collidedWithObject; } set { _collidedWithObject = value; } }
	private bool _collidedWithObject = false;
	private void OnCollisionEnter(Collision other)
	{
		if (!_collidedWithObject && _currentState == _rolling && _input.LeftStickY >= 0.0f )
		{
			// if we collide with something, die.
			if (other.gameObject.layer != LayerMask.NameToLayer("Ground") 
                && other.gameObject.layer != LayerMask.NameToLayer( "Player" ) 
                && other.gameObject.layer != LayerMask.NameToLayer( "PlayerBodyParts" ) 
                && other.gameObject.layer != LayerMask.NameToLayer( "PlayerHand" ))
			{
                //CameraManager.instance.ScreenShake(0.25f, 0.25f, 10);                
                MakeDroopyExplode();
			}
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if ( _controlState == P_ControlState.ROLLING )
		{
			SeedSlug slug = null;
			slug = other.GetComponent(typeof(SeedSlug)) as SeedSlug;
			if (slug != null)
			{
				slug.OnHitWithRoll();
				slug = null;
			}
		}
		else 
		{
			if( other.GetComponent<FlyingSquirrel>() )
			{
				_face.TransitionFacePose( "Squirrel", true );
			}
			else if( other.GetComponent<SmallPlantPickupable>() )
			{
				AudioManager.instance.PlayRandomAudioClip( AudioManager.AudioControllerNames.PLANT_FX );
			}

		}

	}
}
