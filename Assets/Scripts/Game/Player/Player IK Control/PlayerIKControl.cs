using System.Collections;
using UnityEngine;
using RootMotion.FinalIK;

public class PlayerIKControl : MonoBehaviour
{
	RollerController _parentController = null;

	[Header("Look At Properties")]
	[SerializeField] private Transform _lookAtTarget;
	[SerializeField] private float _lookSpeed = 7f;

    [Header("Arm Properties")]
    [SerializeField] private Transform _armTarget;
	[SerializeField] private float armSpeedNoTarget = 7f;
	[SerializeField] private float armSpeedTarget = 15f;

    [Header("Leg Properties")]
    [SerializeField] private AnimationCurve _legYCurve;

    [SerializeField, Tooltip("The distance a leg needs to be from the player position in order to start moving.")]
    private float _legMaxDistance = 0.5f;

    [SerializeField] private float _legLiftHeight = 1f;
    [SerializeField, Tooltip("How long it takes to complete a step animation. Between min/max based on curr velocity")] private float _legMoveTimeMin = 0.1f;
    [SerializeField, Tooltip( "How long it takes to complete a step animation. Between min/max based on curr velocity" )] private float _legMoveTimeMax = 0.2f;

	[SerializeField] private float _minMoveVelocity = 0.5f;
	[SerializeField] private float _minMoveDistance = 5.5f;
    [SerializeField] private float _maxMoveVelocity = 5.0f;

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

	private Vector3 _leftDestination;
	private Vector3 _rightDestination;
	private Vector3 _headDestination;

	private Coroutine _leftLegRoutine;
	private Coroutine _rightLegRoutine;
	private bool _leftLegAtDest;
	private bool _rightLegAtDest;

    public enum WalkState
    {
        IDLE,
        WALK,
		RITUAL,
		END_POP
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
                break;
            case WalkState.WALK:
                _rightLegAtDest = true;
                _leftLegAtDest = true;
				break;
			case WalkState.RITUAL:
				_rightLegAtDest = true;
				_leftLegAtDest = true;
                break;
			case WalkState.END_POP:
				_targetMovePosition = _parentController.transform.position;
				break;
			
        }

        _walkState = state;
    }
		
    private void Awake()
    {
		_parentController = this.transform.parent.GetComponent<RollerController>();

		_lookAt.fixTransforms = false;	 // So we can do more w/ our Big Babby's Head

        ResetLegs();
    }

    private void FixedUpdate()
    {
		HandleLookAt();
        HandleArms();
        HandleLegs();

		UpdateParentController();
    }

	private void UpdateParentController()
	{
		Vector3 targetPos = _legPosMidpoint;
		targetPos.y = _parentController.transform.position.y;
        //_parentController.TargetMovePosition = Vector3.Lerp( _parentController.TargetMovePosition, targetPos, _bodyPosMoveSpeed * Time.deltaTime );
        _parentController.TargetMovePosition = targetPos;

    }

	private void HandleLookAt()
	{
		//_headDestination = new Vector3( _legPosMidpoint.x, _lookAt.transform.position.y, _legPosMidpoint.z );
		//_lookAt.transform.position = Vector3.Lerp(_lookAt.transform.position, _headDestination, _bodyPosMoveSpeed * Time.deltaTime);
		if ( _lookAtTarget != null )
		{
			_lookAt.solver.IKPosition = Vector3.Lerp( _lookAt.solver.IKPosition, _lookAtTarget.position, _lookSpeed * Time.deltaTime );
		}
		else
		{
			Vector3 lookPos =  new Vector3( _targetMovePosition.x, _lookAt.transform.position.y, _targetMovePosition.z );
			_lookAt.solver.IKPosition = Vector3.Lerp( _lookAt.solver.IKPosition, lookPos + transform.forward, _lookSpeed * Time.deltaTime );
		}
	}

    // =======
    // A R M S
    // =======

    private void HandleArms()
    {
        if (_armTarget != null)
        {
			_leftArm.solver.IKPosition = Vector3.Lerp(_leftArm.solver.IKPosition, _armTarget.position, armSpeedTarget * Time.deltaTime);
			_rightArm.solver.IKPosition = Vector3.Lerp(_rightArm.solver.IKPosition, _armTarget.position, armSpeedTarget * Time.deltaTime);
        }
		else if (_walkState == WalkState.RITUAL)
        {
			_leftArm.solver.IKPosition = Vector3.Lerp(_leftArm.solver.IKPosition, 
				transform.position + (transform.parent.up * 5f) - (transform.parent.right * 0.5f), 
				armSpeedNoTarget * Time.deltaTime);
			_rightArm.solver.IKPosition = Vector3.Lerp(_rightArm.solver.IKPosition, 
				transform.position + (transform.parent.up * 5f) + (transform.parent.right * 0.5f), 
				armSpeedNoTarget * Time.deltaTime);
        }
		else
		{
			_leftArm.solver.IKPosition = Vector3.Lerp(_leftArm.solver.IKPosition, 
				transform.parent.position - (transform.parent.right * 0.5f), 
				armSpeedNoTarget * Time.deltaTime);
			_rightArm.solver.IKPosition = Vector3.Lerp(_rightArm.solver.IKPosition,
                transform.parent.position + (transform.parent.right * 0.5f), 
				armSpeedNoTarget * Time.deltaTime);
		}
    }

    public void SetArmTarget(Transform t)
    {
        _armTarget = t;
		_lookAtTarget = t;
    }

	public void LetGo()
	{
		_armTarget = null;
		_lookAtTarget = null;
	}

    // =======
    // L E G S
    // =======

	// Occurs in fixed update through Roller Controller IK Movement
	public void UpdateMovementData( float velocity, Vector3 targetOffset, Quaternion targetRotation )
	{
        if( velocity > _minMoveVelocity )
        {        
            _prevMovePosition = _targetMovePosition;

            //_targetMovePosition += targetOffset * 0.5f;
            _targetMovePosition = targetOffset;

            _targetRotation = targetRotation;
        }
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
        if ( _walkState == WalkState.WALK )
        {
			// Try to move leg farthest from target (?)
			if( ( _leftLegPos - _targetMovePosition ).magnitude < ( _rightLegPos - _targetMovePosition ).magnitude )
			{
				if ( _rightLegRoutine == null /*&& CheckForLegStep(_rightLegPos) */&& _leftLegAtDest)
				{
					_rightLegRoutine = StartCoroutine(TakeStep(_rightLeg));
				}
			}
			else
			{
				// Check if other leg moving and if should be moving based on distance from parent
				if ( _leftLegRoutine == null /*&& CheckForLegStep(_leftLegPos)*/ && _rightLegAtDest )
				{
					_leftLegRoutine = StartCoroutine(TakeStep(_leftLeg));
				}
			}
        }
		else if (_walkState == WalkState.RITUAL)
		{
			if ( _leftLegRoutine == null && _leftLegAtDest )
			{
				_leftLegRoutine = StartCoroutine(RitualStep(_leftLeg));
			}

			if (_rightLegRoutine == null && _rightLegAtDest )
			{
				_rightLegRoutine = StartCoroutine(RitualStep(_rightLeg));
			}
		}
    }

    private bool CheckForLegStep(Vector3 legPos)
    {
		return (legPos - _targetMovePosition).magnitude > _minMoveDistance;
    }

    private IEnumerator TakeStep( CCDIK leg )
    {
		// Move The Leg
        float timer = 0f;

        Vector3 startPos = leg.solver.IKPosition;
        Vector3 currentPos = startPos;
        Vector3 destination = Vector3.down;

        // Randomize the footfall distances to make it look a little more natural.
        float rayDistZ = Random.Range(0.75f, 0.9f);
        float rayDistX = Random.Range(0.15f, 0.3f);
        float legMoveTime = Mathf.Lerp( _legMoveTimeMin, _legMoveTimeMax, Mathf.InverseLerp( _minMoveVelocity, _maxMoveVelocity, _parentController.Velocity ) );

        if (leg == _leftLeg)
        {
            // Toggle leg moving
			_leftLegAtDest = false;

            // Check distance to target from foot
            if ( (_leftLegPos - _targetMovePosition).magnitude > _legMaxDistance )
            {
                // If target too far away, step as far as possible
                _legOrigin = transform.position + ( transform.parent.forward * _legMaxDistance ) + ( -transform.right * rayDistX );
            }
            else
            {
                // else step to target
                _legOrigin = _targetMovePosition + ( Vector3.up * 0.25f ) + ( -transform.right * rayDistX );
            }

			// Set destination position based on raycast down
			_leftDestination = ShootRayDown( _legOrigin, destination);

			// Move leg IK position 
			while (timer < legMoveTime)
            {
                timer += Time.deltaTime;

                currentPos = Vector3.Lerp(startPos, _leftDestination, timer / _legMoveTimeMin);
                currentPos.y = _legYCurve.Evaluate(timer / legMoveTime) * _legLiftHeight;
                leg.solver.IKPosition = currentPos;

                yield return null;
            }

			// Toggle leg moving done
            _leftLegAtDest = true;
			_leftLegRoutine = null;
        }
        else
        {
            _rightLegAtDest = false;

            // Check distance to target from foot
            if ( ( _leftLegPos - _targetMovePosition ).magnitude > _legMaxDistance )
            {
                // If target too far away, step as far as possible
                _legOrigin = transform.position + ( transform.parent.forward * _legMaxDistance ) + ( transform.parent.right * rayDistX );
            }
            else
            {
                // else step to target
                _legOrigin = _targetMovePosition + ( Vector3.up * 0.25f ) + ( transform.parent.right * rayDistX );
            }
            _rightDestination = ShootRayDown( _legOrigin, destination);

			while (timer < legMoveTime)
            {
                timer += Time.deltaTime;

                currentPos = Vector3.Lerp(startPos, _rightDestination, timer / _legMoveTimeMin);
                currentPos.y = _legYCurve.Evaluate(timer / legMoveTime) * _legLiftHeight;
                leg.solver.IKPosition = currentPos;

                yield return null;
            }

            _rightLegAtDest = true;
			_rightLegRoutine = null;
        }
		GroundManager.instance.Ground.DrawOnPosition(currentPos, 1f);
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

			while (timer < legMoveTime)
            {
                timer += Time.deltaTime;

                currentPos = Vector3.Lerp(startPos, _leftDestination, timer / _legMoveTimeMin);
                currentPos.y = _legYCurve.Evaluate(timer / legMoveTime) * _legLiftHeight;
                leg.solver.IKPosition = currentPos;

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

			while (timer < legMoveTime)
            {
                timer += Time.deltaTime;

                currentPos = Vector3.Lerp(startPos, _rightDestination, timer / _legMoveTimeMin);
                currentPos.y = _legYCurve.Evaluate(timer / legMoveTime) * _legLiftHeight;
                leg.solver.IKPosition = currentPos;

                yield return null;
            }

            leg.solver.IKPosition = _rightDestination;

            _rightLegAtDest = true;
			_rightLegRoutine = null;
        }
		GroundManager.instance.Ground.DrawOnPosition(currentPos, 1f);
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

			while (timer < legMoveTime)
			{
				timer += Time.deltaTime;

				currentPos = Vector3.Lerp(startPos, _leftDestination, timer / _legMoveTimeMin);
				currentPos.y = _legYCurve.Evaluate(timer / legMoveTime) * _legLiftHeight;
				leg.solver.IKPosition = currentPos;

				yield return null;
			}
			_leftLegAtDest = true;
		}
		else
		{
			_rightLegAtDest = false;

            _legOrigin = transform.position + (transform.right * rayDistX);
			_rightDestination = new Vector3( _legOrigin.x, 0f, _legOrigin.z);

			while (timer < legMoveTime)
			{
				timer += Time.deltaTime;

				currentPos = Vector3.Lerp(startPos, _rightDestination, timer / _legMoveTimeMin);
				currentPos.y = _legYCurve.Evaluate(timer / legMoveTime) * _legLiftHeight;
				leg.solver.IKPosition = currentPos;

				yield return null;
			}
			_rightLegAtDest = true;
		}
		GroundManager.instance.Ground.DrawOnPosition(currentPos, 1f);
	}

    private Vector3 ShootRayDown(Vector3 origin, Vector3 direction)
    {
        Ray ray = new Ray(origin, direction.normalized);
        RaycastHit hit;

        //Debug.DrawRay(ray.origin, ray.direction, Color.white, 10f);

        if (Physics.Raycast(ray, out hit, 30f))
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
        _leftLeg.solver.IKPosition = _targetMovePosition - (transform.parent.right * 0.25f);
		_rightLeg.solver.IKPosition = _targetMovePosition + (transform.parent.right * 0.25f);

        _leftLegAtDest = true;
        _rightLegAtDest = true;
    }

	void OnDrawGizmos()
	{
		Gizmos.color = Color.blue;
		Gizmos.DrawSphere( _leftDestination, 0.25f );
		Gizmos.DrawSphere( _rightDestination, 0.25f );

		Gizmos.color = Color.red;
		Gizmos.DrawLine( _leftDestination, _rightDestination );
		Gizmos.DrawCube( _legPosMidpoint, JohnTech.UniformVec(0.25f) );

		Gizmos.color = Color.green; 
		Gizmos.DrawCube( _targetMovePosition, JohnTech.UniformVec(0.5f) );
	}
}
