using System.Collections;
using UnityEngine;
using RootMotion.FinalIK;

public class PlayerIKControl : MonoBehaviour
{
    [Header("Arm Properties")]
    [SerializeField] private Transform _armTarget;

    [Header("Leg Properties")]
    [SerializeField] private AnimationCurve _legYCurve;

    [SerializeField, Tooltip("The distance a leg needs to be from the player position in order to start moving.")]
    private float _legMaxDistance = 0.5f;

    [SerializeField] private float _legLiftHeight = 1f;
    [SerializeField] private float _legMoveTimeMin = 0.1f;
    [SerializeField] private float _legMoveTimeMax = 0.2f;

    [Header("Limb IK References")]
    [SerializeField] private LimbIK _leftArm;
    [SerializeField] private LimbIK _leftLeg;
    [SerializeField] private LimbIK _rightArm;
    [SerializeField] private LimbIK _rightLeg;

    public enum WalkState
    {
        IDLE,
        WALK
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

    private void LateUpdate()
    {
        HandleArms();
        HandleLegs();
    }

    // =======
    // A R M S
    // =======

    private void HandleArms()
    {
        if (_armTarget != null)
        {
            _leftArm.solver.IKPosition = _armTarget.position;
            _rightArm.solver.IKPosition = _armTarget.position;
        }
        else
        {
            _leftArm.solver.IKPosition = transform.parent.position - (transform.parent.right * 0.5f);
            _rightArm.solver.IKPosition = transform.parent.position + (transform.parent.right * 0.5f);
        }
    }

    public void SetArmTarget(Transform t)
    {
        _armTarget = t;
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

    private IEnumerator TakeStep(LimbIK leg)
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

            while (timer < _legMoveTimeMin)
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

            while (timer < _legMoveTimeMin)
            {
                timer += Time.deltaTime;

                currentPos = Vector3.Lerp(startPos, _rightDestination.transform.position, timer / _legMoveTimeMin);
                currentPos.y = _legYCurve.Evaluate(timer / legMoveTime) * _legLiftHeight;
                leg.solver.IKPosition = currentPos;

                yield return null;
            }

            _rightLegAtDest = true;
        }
    }

    private IEnumerator StepToIdle(LimbIK leg)
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

            while (timer < _legMoveTimeMin)
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

            while (timer < _legMoveTimeMin)
            {
                timer += Time.deltaTime;

                currentPos = Vector3.Lerp(startPos, _rightDestination.transform.position, timer / _legMoveTimeMin);
                currentPos.y = _legYCurve.Evaluate(timer / legMoveTime) * _legLiftHeight;
                leg.solver.IKPosition = currentPos;

                yield return null;
            }

            _rightLegAtDest = true;
        }
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

    private void ResetLegs()
    {
        _leftLeg.solver.IKPosition = transform.parent.position - (transform.parent.right * 0.25f);
        _rightLeg.solver.IKPosition = transform.parent.position + (transform.parent.right * 0.25f);

        _leftLegAtDest = true;
        _rightLegAtDest = true;
    }
}
