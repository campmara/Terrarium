using System.Collections;
using UnityEngine;
using RootMotion.FinalIK;

public class PlayerIKControl : MonoBehaviour
{
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
    [SerializeField] private float _legMoveTimeMin = 0.1f;
    [SerializeField] private float _legMoveTimeMax = 0.2f;

    [Header("Limb IK References")]
    [SerializeField] private CCDIK _leftArm;
    [SerializeField] private CCDIK _leftLeg;
    [SerializeField] private CCDIK _rightArm;
    [SerializeField] private CCDIK _rightLeg;
	[SerializeField] private LookAtIK _lookAt;

    public enum WalkState
    {
        IDLE,
        WALK,
		RITUAL
    };
    private WalkState _walkState = WalkState.IDLE;
    public void SetState(WalkState state)
    {
        if (_leftLegRoutine != null)
            StopCoroutine(_leftLegRoutine);
        if (_rightLegRoutine != null)
            StopCoroutine(_rightLegRoutine);

        switch (state)
        {
            case WalkState.IDLE:
                _leftLegRoutine = StartCoroutine(StepToIdle(_leftLeg));
                _rightLegRoutine = StartCoroutine(StepToIdle(_rightLeg));
                break;
            case WalkState.WALK:
                _rightLegAtDest = true;
                _leftLegAtDest = true;
				break;
			case WalkState.RITUAL:
				_rightLegAtDest = true;
				_leftLegAtDest = true;
                break;
        }

        _walkState = state;
    }

    private Vector3 _leftLegPos;
    private Vector3 _rightLegPos;

    private GameObject _leftDestination;
    private GameObject _rightDestination;

    private Coroutine _leftLegRoutine;
    private Coroutine _rightLegRoutine;
    private bool _leftLegAtDest;
    private bool _rightLegAtDest;

    private void Awake()
    {
        _leftDestination = new GameObject("Left Leg Step Destination");
        _rightDestination = new GameObject("Right Leg Step Destination");

        _leftDestination.transform.parent = transform.parent;
        _rightDestination.transform.parent = transform.parent;

        ResetLegs();
    }

    private void FixedUpdate()
    {
		HandleLookAt();
        HandleArms();
        HandleLegs();
    }

	private void HandleLookAt()
	{
		if (_lookAtTarget != null)
		{
			_lookAt.solver.IKPosition = Vector3.Lerp(_lookAt.solver.IKPosition, _lookAtTarget.position, _lookSpeed * Time.deltaTime);
		}
		else
		{
			_lookAt.solver.IKPosition = Vector3.Lerp(_lookAt.solver.IKPosition, transform.forward, _lookSpeed * Time.deltaTime);
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

    private void HandleLegs()
    {
        _leftLegPos = _leftLeg.solver.IKPosition;
        _rightLegPos = _rightLeg.solver.IKPosition;

        if (_walkState == WalkState.IDLE)
        {
            // Nothing atm.
        }
        if (_walkState == WalkState.WALK)
        {
            if (CheckForLegStep(_leftLegPos) && _rightLegAtDest)
            {
                if (_leftLegRoutine != null)
                    StopCoroutine(_leftLegRoutine);

                _leftLegRoutine = StartCoroutine(TakeStep(_leftLeg));
            }

            if (CheckForLegStep(_rightLegPos) && _leftLegAtDest)
            {
                if (_rightLegRoutine != null)
                    StopCoroutine(_rightLegRoutine);

                _rightLegRoutine = StartCoroutine(TakeStep(_rightLeg));
            }
        }
		else if (_walkState == WalkState.RITUAL)
		{
			if (_leftLegAtDest)
			{
				if (_leftLegRoutine != null)
					StopCoroutine(_leftLegRoutine);

				_leftLegRoutine = StartCoroutine(RitualStep(_leftLeg));
			}

			if (_rightLegAtDest)
			{
				if (_rightLegRoutine != null)
					StopCoroutine(_rightLegRoutine);

				_rightLegRoutine = StartCoroutine(RitualStep(_rightLeg));
			}
		}
    }

    private bool CheckForLegStep(Vector3 legPos)
    {
        Vector3 diff = legPos - transform.parent.position;
        float dist = diff.magnitude;

        // is the leg far enough away from the player position?
        bool test1 = dist > _legMaxDistance;

        // is the leg behind the player?
        bool test2 = Vector3.Dot(diff, transform.parent.forward) < 0f;

        return test1 && test2;
    }

    private IEnumerator TakeStep(CCDIK leg)
    {
        // Move The Leg
        float timer = 0f;

        Vector3 startPos = leg.solver.IKPosition;
        Vector3 currentPos = startPos;
        Vector3 d = Vector3.down;

        // Randomize the footfall distances to make it look a little more natural.
        float rayDistZ = Random.Range(0.75f, 0.9f);
        float rayDistX = Random.Range(0.15f, 0.3f);
        float legMoveTime = Random.Range(_legMoveTimeMin, _legMoveTimeMax);

        if (leg == _leftLeg)
        {
            _leftLegAtDest = false;

            Vector3 o = transform.position + (transform.forward * rayDistZ) + (transform.right * -rayDistX);
            _leftDestination.transform.position = ShootRayDown(o, d);

			while (timer < legMoveTime)
            {
                timer += Time.deltaTime;

                currentPos = Vector3.Lerp(startPos, _leftDestination.transform.position, timer / _legMoveTimeMin);
                currentPos.y = _legYCurve.Evaluate(timer / legMoveTime) * _legLiftHeight;
                leg.solver.IKPosition = currentPos;

                yield return null;
            }

            _leftLegAtDest = true;
        }
        else
        {
            _rightLegAtDest = false;

            Vector3 o = transform.position + (transform.forward * rayDistZ) + (transform.right * rayDistX);
            _rightDestination.transform.position = ShootRayDown(o, d);

			while (timer < legMoveTime)
            {
                timer += Time.deltaTime;

                currentPos = Vector3.Lerp(startPos, _rightDestination.transform.position, timer / _legMoveTimeMin);
                currentPos.y = _legYCurve.Evaluate(timer / legMoveTime) * _legLiftHeight;
                leg.solver.IKPosition = currentPos;

                yield return null;
            }

            _rightLegAtDest = true;
        }
		GroundManager.instance.Ground.DrawOnPosition(currentPos, 1f);
    }

    private IEnumerator StepToIdle(CCDIK leg)
    {
        yield return new WaitForSeconds(Random.Range(0f, 0.5f));

        // Move The Leg
        float timer = 0f;

        Vector3 startPos = leg.solver.IKPosition;
        Vector3 currentPos = startPos;
        Vector3 d = Vector3.down;

        // Randomize the footfall distances to make it look a little more natural.
        float rayDistZ = Random.Range(0f, 0.1f);
        float rayDistX = Random.Range(0.2f, 0.3f);
        float legMoveTime = Random.Range(0.1f, 0.3f);

        if (leg == _leftLeg)
        {
            _leftLegAtDest = false;

            Vector3 o = transform.position + (transform.forward * rayDistZ) + (transform.right * -rayDistX);
            _leftDestination.transform.position = ShootRayDown(o, d);

			while (timer < legMoveTime)
            {
                timer += Time.deltaTime;

                currentPos = Vector3.Lerp(startPos, _leftDestination.transform.position, timer / _legMoveTimeMin);
                currentPos.y = _legYCurve.Evaluate(timer / legMoveTime) * _legLiftHeight;
                leg.solver.IKPosition = currentPos;

                yield return null;
            }

            _leftLegAtDest = true;
        }
        else
        {
            _rightLegAtDest = false;

            Vector3 o = transform.position + (transform.forward * rayDistZ) + (transform.right * rayDistX);
            _rightDestination.transform.position = ShootRayDown(o, d);

			while (timer < legMoveTime)
            {
                timer += Time.deltaTime;

                currentPos = Vector3.Lerp(startPos, _rightDestination.transform.position, timer / _legMoveTimeMin);
                currentPos.y = _legYCurve.Evaluate(timer / legMoveTime) * _legLiftHeight;
                leg.solver.IKPosition = currentPos;

                yield return null;
            }

            _rightLegAtDest = true;
        }
		GroundManager.instance.Ground.DrawOnPosition(currentPos, 1f);
    }

	private IEnumerator RitualStep(CCDIK leg)
	{
		// Move The Leg
		float timer = 0f;

		Vector3 startPos = leg.solver.IKPosition;
		Vector3 currentPos = startPos;
		Vector3 d = Vector3.down;

		// Randomize the footfall distances to make it look a little more natural.
		float rayDistX = Random.Range(0.15f, 0.3f);
		float legMoveTime = Random.Range(0.25f, 0.35f);

		if (leg == _leftLeg)
		{
			_leftLegAtDest = false;

			Vector3 o = transform.position + (transform.right * -rayDistX);
			_leftDestination.transform.position = new Vector3(o.x, 0f, o.z);

			while (timer < legMoveTime)
			{
				timer += Time.deltaTime;

				currentPos = Vector3.Lerp(startPos, _leftDestination.transform.position, timer / _legMoveTimeMin);
				currentPos.y = _legYCurve.Evaluate(timer / legMoveTime) * _legLiftHeight;
				leg.solver.IKPosition = currentPos;

				yield return null;
			}
			_leftLegAtDest = true;
		}
		else
		{
			_rightLegAtDest = false;

			Vector3 o = transform.position + (transform.right * rayDistX);
			_rightDestination.transform.position = new Vector3(o.x, 0f, o.z);

			while (timer < legMoveTime)
			{
				timer += Time.deltaTime;

				currentPos = Vector3.Lerp(startPos, _rightDestination.transform.position, timer / _legMoveTimeMin);
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
        _leftLeg.solver.IKPosition = transform.parent.position - (transform.parent.right * 0.25f);
        _rightLeg.solver.IKPosition = transform.parent.position + (transform.parent.right * 0.25f);

        _leftLegAtDest = true;
        _rightLegAtDest = true;
    }
}
