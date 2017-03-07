using System.Collections;
using UnityEngine;
using RootMotion.FinalIK;

public class PlayerIKControl : MonoBehaviour
{
    RollerController _parentController = null;

    [Header( "Look At Properties" )]
    [SerializeField] private Transform _lookAtTarget;
    [SerializeField] private float _lookSpeed = 7f;
    [SerializeField]
    private AnimationCurve _headSpeedCurve;
    [SerializeField]
    private AnimationCurve _headPosCurve;
    [SerializeField] private float _headMoveInterpSpeed = 1.0f;
    [SerializeField] private float _headHeightAnimScalar = 0.025f;
    [SerializeField, Tooltip("Only counts between 0 & 1. Determines what percentage between feet and target head tries to move to")] private float _headTargetInterp = 0.25f;


[Header("Arm Properties")]
    [SerializeField] private Transform _leftArmTargetTransform = null;
    [SerializeField] private Transform _rightArmTargetTransform = null;
    public Transform LeftArmTargetTrans { get { return _leftArmTargetTransform; } }
    public Transform RightArmTargetTrans { get { return _rightArmTargetTransform; } }
    public bool ArmTargetsSet { get { return _leftArmTargetTransform != null && _rightArmTargetTransform != null; } }
	[SerializeField] private float armSpeedNoTarget = 7f;
	[SerializeField] private float armSpeedTarget = 0.015f;
    private Vector3 _leftArmTargetPos = Vector3.zero;
    private Vector3 _rightArmTargetPos = Vector3.zero;
    
    // These "base targets" are what the arms are resolving towards when not holding something / focused on something / reaching
    [SerializeField] private SpringJoint _leftArmSpringTarget = null;   
    [SerializeField] private SpringJoint _rightArmSpringTarget = null;

    private bool _armsReaching = false;
    public bool ArmsReaching { get { return _armsReaching;} set { _armsReaching = value; } }
    private float _leftArmReachInterp = 0.0f;
    private float _rightArmReachInterp = 0.0f;

    private const float ARM_REACHDISTMAX = 8.0f;
    private const float ARM_REACHDISTMIN = 0.75f;

    [Header("Leg Properties")]
    [SerializeField] private AnimationCurve _legYCurve;
    [SerializeField] private AnimationCurve _legXCurve;

    [SerializeField, Tooltip("The distance a leg needs to be from the player position in order to start moving.")]
    private float _legMaxDistance = 0.5f;

    [SerializeField] private float _legLiftHeightMin = 0.5f;
	[SerializeField] private float _legLiftHeightMax = 1f;
    [SerializeField, Tooltip("How long it takes to complete a step animation. Between min/max based on curr velocity")] private float _legMoveTimeMin = 0.1f;
    [SerializeField, Tooltip( "How long it takes to complete a step animation. Between min/max based on curr velocity" )] private float _legMoveTimeMax = 0.2f;

	[SerializeField] private float _minMoveVelocity = 0.5f;
    [SerializeField] private float _maxMoveVelocity = 4.0f;
    [SerializeField] private Vector2 _turnSpeedRange = new Vector2(1.0f, 5.0f);
	[SerializeField] private float _bodyPosMoveSpeed = 5.0f;
    [SerializeField] private float _minFootDist = 0.3f;

    [Header("Limb IK References")]
    [SerializeField] private CCDIK _leftArm;
    [SerializeField] private CCDIK _leftLeg;
    [SerializeField] private CCDIK _rightArm;
    [SerializeField] private CCDIK _rightLeg;
	[SerializeField] private LookAtIK _lookAt;

    private Vector3 _legOrigin = Vector3.zero;
	private Vector3 _leftLegPos;
	private Vector3 _rightLegPos;

	private Vector3 _targetMovePosition;
	private Vector3 _prevMovePosition;
	private Vector3 _legPosMidpoint;

	private Quaternion _targetRotation;
    private Quaternion _lastRotation;

	private Vector3 _leftDestination;
	private Vector3 _rightDestination;
	private Vector3 _headDestination;

	private float _currLegLiftHeight;
	private float _baseHeadY;
    private float _legHeightAnimEval = 0.0f;

	[SerializeField] private Coroutine _leftLegRoutine;
    [SerializeField] private Coroutine _rightLegRoutine;
	private bool _leftLegAtDest;
	private bool _rightLegAtDest;

    public enum WalkState
    {
        IDLE,
        WALK,
		RITUAL,
		POND_RETURN
    };
    private WalkState _walkState = WalkState.IDLE;
    public void SetState(WalkState state)
    {
//        if (_leftLegRoutine != null)
//		{
//			StopCoroutine(_leftLegRoutine);
//		}            
//        if (_rightLegRoutine != null)
//		{
//			StopCoroutine(_rightLegRoutine);
//		}
            

        switch (state)
        {
            case WalkState.IDLE:
                //_leftLegRoutine = StartCoroutine(StepToIdle(_leftLeg));
                //_rightLegRoutine = StartCoroutine(StepToIdle(_rightLeg));
                //_lookAtTarget = CameraManager.instance.Main.transform;
                break;
            case WalkState.WALK:
                if( _walkState == WalkState.RITUAL )
                {                    
                    ResetLegs();
                }
                //if ( _lookAtTarget == CameraManager.instance.Main.transform )
                //{
                //    _lookAtTarget = null;
                //}
                _rightLegAtDest = true;
                _leftLegAtDest = true;
				break;
			case WalkState.RITUAL:
				_rightLegAtDest = true;
				_leftLegAtDest = true;
                break;	
			case WalkState.POND_RETURN:				
				break;
        }

        _walkState = state;
    }
		
    private void Awake()
    {
		_parentController = this.transform.parent.GetComponent<RollerController>();

		_lookAt.fixTransforms = false;	 // So we can do more w/ our Big Babby's Head

		_baseHeadY = _lookAt.transform.position.y;

        ResetLegs();
    }

    private void FixedUpdate()
    {
		HandleLookAt();
        HandleArms();
        HandleLegs();

		UpdateParentController();
    }

    private void LateUpdate()
    {
        Debug.Assert( _leftLeg != null );
        _leftLeg.solver.IKPosition = Vector3.Lerp( _leftLeg.solver.IKPosition, _leftLegPos, 20.0f * Time.deltaTime );        
        _rightLeg.solver.IKPosition = Vector3.Lerp( _rightLeg.solver.IKPosition, _rightLegPos, 20.0f * Time.deltaTime );

        _rightArm.solver.IKPosition = Vector3.Lerp( _rightLeg.solver.IKPosition, _rightArmTargetPos, armSpeedTarget );
        _leftArm.solver.IKPosition = Vector3.Lerp( _leftArm.solver.IKPosition, _leftArmTargetPos, armSpeedTarget );
        //_rightArm.solver.IKPosition = _rightArmTargetPos;
        //_leftArm.solver.IKPosition = _leftArmTargetPos;
    }

    private void UpdateParentController()
	{
        Vector3 targetPos = Vector3.Lerp( _legPosMidpoint, _targetMovePosition, _headTargetInterp );
        //Vector3 targetPos = JohnTech.Midpoint(_targetMovePosition, _legPosMidpoint);  // Sets target position to be midpoint between leg midd and target pos
        //Vector3 targetPos = _legPosMidpoint;  // Sets target position to be midpoint between legs

        targetPos.y = _parentController.transform.position.y;

        _parentController.TargetMovePosition = Vector3.Lerp( _parentController.TargetMovePosition, targetPos, _bodyPosMoveSpeed * Time.deltaTime );
        //_parentController.TargetMovePosition = targetPos;

        _parentController.HeadMoveInterp = Mathf.Lerp( _parentController.HeadMoveInterp, _headSpeedCurve.Evaluate( _legHeightAnimEval ), _bodyPosMoveSpeed * Time.deltaTime );
        //_parentController.HeadMoveInterp = _headSpeedCurve.Evaluate( _legHeightAnimEval );
    }

	#region Head Methods

	private void HandleLookAt()
	{
        if (_walkState == WalkState.IDLE)
        {
            _lookAt.transform.localPosition.SetPosY( _baseHeadY + ( 0.024f * Mathf.Sin( 7.0f * ( Time.time ) ) ) );
        }
        else if ( _walkState == WalkState.WALK )
        {
            _lookAt.transform.localPosition.SetPosY( Mathf.Lerp( _lookAt.transform.localPosition.y, _baseHeadY + ( _headPosCurve.Evaluate( _legHeightAnimEval ) * _headHeightAnimScalar ), _headMoveInterpSpeed * Time.deltaTime) );
        }
        else if ( _walkState == WalkState.POND_RETURN )
        {
            _lookAt.transform.localPosition.SetPosY( 0.2f );
        }

        // Setting where IK should resolve to look at
		if ( _lookAtTarget != null )
		{
			_lookAt.solver.IKPosition = Vector3.Lerp( _lookAt.solver.IKPosition, _lookAtTarget.position, _lookSpeed * Time.deltaTime );
		}
		else
		{
			//Vector3 lookPos =  new Vector3( _targetMovePosition.x, _lookAt.transform.position.y, _targetMovePosition.z );
			_lookAt.solver.IKPosition = Vector3.Lerp( _lookAt.solver.IKPosition, _lookAt.transform.position + this.transform.parent.forward, _lookSpeed * Time.deltaTime );
		}
	}

	#endregion

    #region Arm Methods

    // =======
    // A R M S
    // =======

    private void HandleArms()
    {
        // TODO: lerp speed should be unique for each type of arm reaching/movement
        // TODO: Adjust hand spring anchors

        // Move arm target pos, updates IK in Late Update
        if ( _armsReaching )
        {            
            if( _leftArmTargetTransform == null && _rightArmTargetTransform == null  )  // Lift arms up just cuzz (reaching not grabbing)
            {
                _leftArmTargetPos = Vector3.Lerp( _leftArmTargetPos, Vector3.Lerp( _leftArmSpringTarget.transform.position, _leftArmSpringTarget.transform.position + ( Vector3.up * 50.0f ), _leftArmReachInterp ), armSpeedTarget * Time.deltaTime );
                _rightArmTargetPos = Vector3.Lerp( _rightArmTargetPos, Vector3.Lerp( _rightArmSpringTarget.transform.position, _rightArmSpringTarget.transform.position + ( Vector3.up * 50.0f ), _rightArmReachInterp ), armSpeedTarget * Time.deltaTime );
            }
            else if( ArmTargetsSet) // Reach both arms to position
            {
                _leftArmTargetPos = Vector3.Lerp( _leftArmTargetPos, Vector3.Lerp( _leftArmSpringTarget.transform.position, _leftArmTargetTransform.position, _leftArmReachInterp ), armSpeedTarget * Time.deltaTime );
                _rightArmTargetPos = Vector3.Lerp( _rightArmTargetPos, Vector3.Lerp( _rightArmSpringTarget.transform.position, _rightArmTargetTransform.position, _rightArmReachInterp ), armSpeedTarget * Time.deltaTime );
            }
            else    // Figure out which arm is reaching otherwise
            {
                if( _leftArmTargetTransform != null )
                {
                    // Check if reaching is still viable (too far away from object)
                    CheckReachConstraints( _leftArmTargetTransform );
                    if( _armsReaching )
                    {
                        _leftArmTargetPos = Vector3.Lerp( _leftArmTargetPos, _leftArmTargetTransform.position, armSpeedTarget * Time.deltaTime );

                        _rightArmSpringTarget.connectedAnchor = Vector3.Lerp( _rightArmSpringTarget.connectedAnchor, transform.parent.position + ( transform.parent.right * 0.5f ), armSpeedNoTarget * Time.deltaTime );
                        _rightArmTargetPos = Vector3.Lerp( _rightArmTargetPos, _rightArmSpringTarget.transform.position, armSpeedNoTarget * Time.deltaTime );
                    }
                }
                else if( _rightArmTargetTransform != null )
                {
                    CheckReachConstraints( _rightArmTargetTransform );
                    if( _armsReaching )
                    {
                        _rightArmTargetPos = Vector3.Lerp( _rightArmTargetPos, Vector3.Lerp( _rightArmSpringTarget.transform.position, _rightArmTargetTransform.position, _rightArmReachInterp ), armSpeedTarget * Time.deltaTime );

                        _leftArmSpringTarget.connectedAnchor = Vector3.Lerp( _leftArmSpringTarget.connectedAnchor, transform.parent.position - ( transform.parent.right * 0.5f ), armSpeedNoTarget * Time.deltaTime );
                        _leftArmTargetPos = Vector3.Lerp( _leftArmTargetPos, _leftArmSpringTarget.transform.position, armSpeedNoTarget * Time.deltaTime );
                    }
                }
            }

        }        
		else if (_walkState == WalkState.RITUAL)    // Lift arms up to spin~~
        {
			_leftArmTargetPos = Vector3.Lerp(_leftArmTargetPos, 
				transform.position + (transform.parent.up * 5f) - (transform.parent.right * 0.5f), 
				armSpeedNoTarget * Time.deltaTime);
			_rightArmTargetPos = Vector3.Lerp(_rightArmTargetPos, 
				transform.position + (transform.parent.up * 5f) + (transform.parent.right * 0.5f), 
				armSpeedNoTarget * Time.deltaTime);
        }
		else   // Position arms at sides, update position of springs
		{            
            _leftArmSpringTarget.connectedAnchor = Vector3.Lerp( _leftArmSpringTarget.connectedAnchor, 
				transform.parent.position - (transform.parent.right * 0.5f), 
				armSpeedNoTarget * Time.deltaTime);
			_rightArmSpringTarget.connectedAnchor = Vector3.Lerp( _rightArmSpringTarget.connectedAnchor, 
                transform.parent.position + (transform.parent.right * 0.5f), 
				armSpeedNoTarget * Time.deltaTime);

            _leftArmTargetPos = Vector3.Lerp(_leftArmTargetPos, _leftArmSpringTarget.transform.position, armSpeedNoTarget * Time.deltaTime);
			_rightArmTargetPos = Vector3.Lerp(_rightArmTargetPos, _rightArmSpringTarget.transform.position, armSpeedNoTarget * Time.deltaTime);            
		}
    }

    public void SetArmTarget( Transform t )
    {
        if( t != null )
        {
            _leftArmTargetTransform = t;
            _rightArmTargetTransform = t;
            _lookAtTarget = t;

            _leftArmSpringTarget.GetComponent<Rigidbody>().isKinematic = true;
            _rightArmSpringTarget.GetComponent<Rigidbody>().isKinematic = true;
        }
        _armsReaching = true;
    }

    public void SetReachTarget( Transform reachable, bool rightArm )
    {
        if( rightArm )
        {
            _rightArmTargetTransform = reachable;
            _rightArmSpringTarget.GetComponent<Rigidbody>().isKinematic = true;
        }
        else
        {
            _leftArmTargetTransform = reachable;
            _leftArmSpringTarget.GetComponent<Rigidbody>().isKinematic = true;            
        }

        if( JohnTech.CoinFlip() )
        {
            _lookAtTarget = reachable;
        }

        _armsReaching = true;
    }


    public void LetGo()
	{
		_leftArmTargetTransform = null;
        _rightArmTargetTransform = null;
        _lookAtTarget = null;

        _leftArmSpringTarget.GetComponent<Rigidbody>().isKinematic = false;
        _rightArmSpringTarget.GetComponent<Rigidbody>().isKinematic = false;

        _armsReaching = false;
	}

    public void UpdateArmInterpValues( float leftValue, float rightValue )
    {
        _leftArmReachInterp = leftValue;
        _rightArmReachInterp = rightValue;
    }

    void CheckReachConstraints( Transform reachTransform )
    {
        float reachDist = ( _parentController.transform.position - reachTransform.position ).magnitude;

        if ( reachDist > ARM_REACHDISTMAX )
        {
            // TODO: Make Droopy Sad : (

            LetGo();
        }
        else if ( reachDist < ARM_REACHDISTMIN )
        {
            // TODO: Make Droopy Happy : )

            LetGo();
        }
    }

    #endregion

    #region Legs Methods

    // =======
    // L E G S
    // =======

    // Occurs in fixed update through Roller Controller IK Movement
    public void UpdateMovementData( float velocity, Vector3 targetOffset, Quaternion targetRotation )
	{
        if( velocity > _minMoveVelocity || ( _legPosMidpoint - _targetMovePosition ).magnitude > _legMaxDistance )
        {        
            _prevMovePosition = _targetMovePosition;

            //_targetMovePosition += targetOffset * 0.5f;
            _targetMovePosition = targetOffset;

            _lastRotation = _targetRotation;
            _targetRotation = targetRotation;
        }

        _targetMovePosition.y = PondManager.instance.Pond.GetPondY(_targetMovePosition);
	}

    private void HandleLegs()
    {
        _leftLegPos = _leftLeg.solver.IKPosition;
        _rightLegPos = _rightLeg.solver.IKPosition;
		_legPosMidpoint = JohnTech.Midpoint( _leftLegPos, _rightLegPos );

        if (_walkState == WalkState.IDLE)
        {
            // Nothing atm.
        }
        if ( _walkState == WalkState.WALK  || _walkState == WalkState.IDLE )
        {
			// Try to move leg farthest from target (?)
			if( ( _leftLegPos - _targetMovePosition ).magnitude < ( _rightLegPos - _targetMovePosition ).magnitude )
			{
				if ( _rightLegRoutine == null && CheckForLegStep(_rightLegPos) && _leftLegAtDest)
				{
                    _rightLegRoutine = StartCoroutine( TakeStep( _rightLeg ) );
				}
			}
			else
			{
				// Check if other leg moving and if should be moving based on distance from parent
				if ( _leftLegRoutine == null && CheckForLegStep(_leftLegPos) && _rightLegAtDest )
				{
                    _leftLegRoutine = StartCoroutine( TakeStep( _leftLeg ) );
				}
			}
        }
		else if (_walkState == WalkState.RITUAL)
		{
			if ( _leftLegRoutine == null && _leftLegAtDest )
			{
                _leftLegRoutine = StartCoroutine( RitualStep( _leftLeg ) );
			}

			if (_rightLegRoutine == null && _rightLegAtDest )
			{
                _rightLegRoutine = StartCoroutine( RitualStep( _rightLeg ) );
			}
		}
    }

    private bool CheckForLegStep(Vector3 legPos)
    {
        // can step if: target position is farther away from leg than the body OR leg to target is mag is greater than a minimum distance it is trying to resolve to
        return (legPos - _targetMovePosition).magnitude > (legPos - transform.position).magnitude || ( legPos - _targetMovePosition ).magnitude > _minFootDist;
    }

    private IEnumerator TakeStep( CCDIK leg )
    {
		// Move The Leg
        float timer = 0f;

        Vector3 startPos = leg.solver.IKPosition;
        Vector3 currentPos = startPos;
        Vector3 destination = Vector3.down;

        // Randomize the footfall distances to make it look a little more natural.
        float rayDistZ = Random.Range(0.25f, 0.75f);
        float rayDistX = Random.Range(0.15f, 0.25f);
        float legMoveTime = Mathf.Lerp( _legMoveTimeMin, _legMoveTimeMax, Mathf.InverseLerp( _minMoveVelocity, _maxMoveVelocity, _parentController.Velocity ) );

        if (leg == _leftLeg)
        {
            // Toggle leg moving
			_leftLegAtDest = false;

            // Check distance to target from foot
            if ( (_leftLegPos - _targetMovePosition).magnitude > _legMaxDistance )
            {
                // If target too far away, step as far as possible               
                _legOrigin = transform.position + ( transform.parent.forward * _legMaxDistance  * Mathf.InverseLerp( _turnSpeedRange.y, _turnSpeedRange.x, Quaternion.Angle( _lastRotation, _targetRotation ) ) ) + ( -transform.right * rayDistX );
            }
            else
            {
                // else step to target
                _legOrigin = _targetMovePosition + ( Vector3.up * 0.25f ) + ( -transform.right * rayDistX );
            }

			// Set destination position based on raycast down
			_leftDestination = ShootRayDown( _legOrigin, destination);

			_currLegLiftHeight = Mathf.Lerp( _legLiftHeightMin, _legLiftHeightMax, Mathf.InverseLerp( _minMoveVelocity, _maxMoveVelocity, _parentController.Velocity ));
			// Move leg IK position 
			while (timer < legMoveTime)
            {
                timer += Time.deltaTime;

                LegIKUpdate( ref _leftLegPos, startPos, _leftDestination, currentPos, timer, legMoveTime );

                yield return null;
            }

			leg.solver.IKPosition = _leftDestination;

			// Toggle leg moving done
            _leftLegAtDest = true;
			_leftLegRoutine = null;
        }
        else
        {
            _rightLegAtDest = false;

            // Check distance to target from foot
            if ( ( _rightLegPos - _targetMovePosition ).magnitude > _legMaxDistance )
            {
                // If target too far away, step as far as possible
                _legOrigin = transform.position + ( transform.parent.forward * _legMaxDistance * Mathf.InverseLerp( _turnSpeedRange.y, _turnSpeedRange.x, Quaternion.Angle( _lastRotation, _targetRotation ) ) ) + ( transform.parent.right * rayDistX );
            }
            else
            {
                // else step to target
                _legOrigin = _targetMovePosition + ( Vector3.up * 0.25f ) + ( transform.parent.right * rayDistX );
            }
            _rightDestination = ShootRayDown( _legOrigin, destination);

			_currLegLiftHeight = Mathf.Lerp( _legLiftHeightMin, _legLiftHeightMax, Mathf.InverseLerp( _minMoveVelocity, _maxMoveVelocity, _parentController.Velocity ));
			while (timer < legMoveTime)
            {
                timer += Time.deltaTime;

                LegIKUpdate( ref _rightLegPos, startPos, _rightDestination, currentPos, timer, legMoveTime );

                yield return null;
            }

			leg.solver.IKPosition = _rightDestination;

            _rightLegAtDest = true;
			_rightLegRoutine = null;
        }
        GroundManager.instance.Ground.DrawSplatDecal(currentPos, 0.25f);
		//GroundManager.instance.Ground.DrawOnPosition(currentPos, 1f);
		AudioManager.instance.PlayRandomAudioClip( AudioManager.AudioControllerNames.PLAYER_FOOTSTEPS );
    }

    private IEnumerator StepToIdle(CCDIK leg)
    {
        yield return new WaitForSeconds(Random.Range(0f, 0.5f));

        // Move The Leg
        float timer = 0f;

        Vector3 startPos = leg.solver.IKPosition;
        Vector3 currentPos = startPos;
		Vector3 destination = Vector3.down;

        // Randomize the footfall distances to make it look a little more natural.
        float rayDistZ = Random.Range(0f, 0.1f);
        float rayDistX = Random.Range(0.2f, 0.3f);
        float legMoveTime = Mathf.Lerp( _legMoveTimeMin, _legMoveTimeMax, Mathf.InverseLerp( _minMoveVelocity, _maxMoveVelocity, _parentController.Velocity ) );

        if (leg == _leftLeg)
        {
            _leftLegAtDest = false;

			_legOrigin = _targetMovePosition + (Vector3.up * 0.25f) + (transform.parent.right * -rayDistX);
            _leftDestination = ShootRayDown( _legOrigin, destination);

			_currLegLiftHeight = Mathf.Lerp( _legLiftHeightMin, _legLiftHeightMax, Mathf.InverseLerp( _minMoveVelocity, _maxMoveVelocity, _parentController.Velocity ));
			while (timer < legMoveTime)
            {
                timer += Time.deltaTime;

                LegIKUpdate( ref _leftLegPos, startPos, _leftDestination, currentPos, timer, legMoveTime );

                yield return null;
            }

            leg.solver.IKPosition = _leftDestination;

            _leftLegAtDest = true;
			_leftLegRoutine = null;
        }
        else
        {
            _rightLegAtDest = false;

            _legOrigin = _targetMovePosition + (Vector3.up * 0.25f) + (transform.parent.right * rayDistX);
            _rightDestination = ShootRayDown( _legOrigin, destination);

			_currLegLiftHeight = Mathf.Lerp( _legLiftHeightMin, _legLiftHeightMax, Mathf.InverseLerp( _minMoveVelocity, _maxMoveVelocity, _parentController.Velocity ));
			while (timer < legMoveTime)
            {
                timer += Time.deltaTime;

                LegIKUpdate( ref _rightLegPos, startPos, _rightDestination, currentPos, timer, legMoveTime );

                yield return null;
            }

            leg.solver.IKPosition = _rightDestination;

            _rightLegAtDest = true;
			_rightLegRoutine = null;
        }
        GroundManager.instance.Ground.DrawSplatDecal(currentPos, 0.25f);
		//GroundManager.instance.Ground.DrawOnPosition(currentPos, 1f);
    }

	private IEnumerator RitualStep(CCDIK leg)
	{
		// Move The Leg
		float timer = 0f;

		Vector3 startPos = leg.solver.IKPosition;
		Vector3 currentPos = startPos;
		Vector3 destination = Vector3.down;

		// Randomize the footfall distances to make it look a little more natural.
		float rayDistX = Random.Range(0.15f, 0.3f);
		float legMoveTime = Random.Range(0.25f, 0.35f);

		if (leg == _leftLeg)
		{
			_leftLegAtDest = false;

            _legOrigin = transform.position + (transform.right * -rayDistX);
			_leftDestination = new Vector3( _legOrigin.x, 0f, _legOrigin.z);

            _currLegLiftHeight = Mathf.Lerp( _legLiftHeightMin, _legLiftHeightMax, Mathf.InverseLerp( _minMoveVelocity, _maxMoveVelocity, _parentController.Velocity ) );
			while (timer < legMoveTime)
			{
				timer += Time.deltaTime;

                LegIKUpdate( ref _leftLegPos, startPos, _leftDestination, currentPos, timer, legMoveTime );

                yield return null;
			}
			_leftLegAtDest = true;
		}
		else
		{
			_rightLegAtDest = false;

            _legOrigin = transform.position + ( transform.right * rayDistX );
            _rightDestination = new Vector3( _legOrigin.x, 0f, _legOrigin.z );

            _currLegLiftHeight = Mathf.Lerp( _legLiftHeightMin, _legLiftHeightMax, Mathf.InverseLerp( _minMoveVelocity, _maxMoveVelocity, _parentController.Velocity ) );
			while (timer < legMoveTime)
			{
				timer += Time.deltaTime;
                
                LegIKUpdate( ref _rightLegPos, startPos, _rightDestination, currentPos, timer, legMoveTime );

                yield return null;
			}
			_rightLegAtDest = true;
		}
        GroundManager.instance.Ground.DrawSplatDecal(currentPos, 0.25f);
		//GroundManager.instance.Ground.DrawOnPosition(currentPos, 1f);
	}

    /// <summary>
    /// Calculates and sets new leg ik position
    /// </summary>
    /// <param name="leg"></param>
    /// <param name="startPos"></param>
    /// <param name="destination"></param>
    /// <param name="currentPos"></param>
    /// <param name="timer"></param>
    /// <param name="legMoveTime"></param>
    private void LegIKUpdate( /*CCDIK leg,*/ ref Vector3 legPosition, Vector3 startPos, Vector3 destination, Vector3 currentPos, float timer, float legMoveTime )
    {
        currentPos = Vector3.Lerp( startPos, destination, timer / legMoveTime );
        _legHeightAnimEval = _legYCurve.Evaluate( timer / legMoveTime );    // Used elsewhere to determine head movement, arm movement, etc.
        currentPos.y = _legHeightAnimEval * _currLegLiftHeight;

        //leg.solver.IKPosition = currentPos;
        legPosition = currentPos;
    }

    private Vector3 ShootRayDown(Vector3 origin, Vector3 direction)
    {
        Ray ray = new Ray(origin, direction.normalized);
        RaycastHit hit;

        //Debug.DrawRay(ray.origin, ray.direction, Color.white, 10f);

        if (Physics.Raycast(ray, out hit, 30f, ~(1 << LayerMask.NameToLayer("Player"))))
        {
            return hit.point;
        }
        else
        {
            return Vector3.zero;
        }

    }

    public void ResetLegs()
    {
        _targetMovePosition = _parentController.transform.position;

        _leftLeg.solver.IKPosition = transform.parent.position - (transform.parent.right * 0.25f);
		_rightLeg.solver.IKPosition = transform.parent.position + (transform.parent.right * 0.25f);

        _leftLegPos = transform.parent.position - ( transform.parent.right * 0.25f );
        _rightLegPos = transform.parent.position + ( transform.parent.right * 0.25f );
        _legPosMidpoint = JohnTech.Midpoint( _leftLegPos, _rightLegPos );

        _legHeightAnimEval = 0.0f;

        _leftLegRoutine = null;
        _rightLegRoutine = null;

        _leftLegAtDest = true;
        _rightLegAtDest = true;
    }

    #endregion

    void OnDrawGizmos()
	{
		Gizmos.color = Color.blue;
		Gizmos.DrawSphere( _leftDestination, 0.25f );
		Gizmos.DrawSphere( _rightDestination, 0.25f );

        Gizmos.DrawSphere( _leftLegPos, 0.05f );
        Gizmos.DrawSphere( _rightLegPos, 0.05f );

        Gizmos.color = Color.red;
		Gizmos.DrawLine( _leftDestination, _rightDestination );
		Gizmos.DrawCube( _legPosMidpoint, JohnTech.UniformVec(0.25f) );

		Gizmos.color = Color.green; 
		Gizmos.DrawCube( _targetMovePosition, JohnTech.UniformVec(0.5f) );
	}
}
