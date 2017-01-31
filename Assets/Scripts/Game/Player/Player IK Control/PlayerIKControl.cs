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
    [SerializeField] private float _legMoveTime = 0.1f;

    [Header("Limb IK References")]
    [SerializeField] private LimbIK _leftArm;
    [SerializeField] private LimbIK _leftLeg;
    [SerializeField] private LimbIK _rightArm;
    [SerializeField] private LimbIK _rightLeg;

    private Vector3 _leftLegPos;
    private Vector3 _rightLegPos;

    private GameObject _leftDestination;
    private GameObject _rightDestination;

    private Coroutine _leftLegRoutine;
    private Coroutine _rightLegRoutine;

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

        if (CheckForLegStep(_leftLegPos))
        {
            if (_leftLegRoutine != null)
                StopCoroutine(_leftLegRoutine);

            _leftLegRoutine = StartCoroutine(TakeStep(_leftLeg));
        }

        if (CheckForLegStep(_rightLegPos))
        {
            if (_rightLegRoutine != null)
                StopCoroutine(_rightLegRoutine);

            _rightLegRoutine = StartCoroutine(TakeStep(_rightLeg));
        }
    }

    private bool CheckForLegStep(Vector3 legPos)
    {
        Vector3 diff = legPos - transform.parent.position;
        float dist = diff.magnitude;
        Debug.Log("dist = " + dist);

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

        if (leg == _leftLeg)
        {
            Vector3 o = transform.position + (transform.forward) + (transform.right * -0.25f);
            _leftDestination.transform.position = ShootRayDown(o, d);

            while (timer < _legMoveTime)
            {
                timer += Time.deltaTime;

                currentPos = Vector3.Lerp(startPos, _leftDestination.transform.position, timer / _legMoveTime);
                currentPos.y = _legYCurve.Evaluate(timer / _legMoveTime) * _legLiftHeight;
                leg.solver.IKPosition = currentPos;

                yield return null;
            }
        }
        else
        {
            Vector3 o = transform.position + (transform.forward) + (transform.right * 0.25f);
            _rightDestination.transform.position = ShootRayDown(o, d);

            while (timer < _legMoveTime)
            {
                timer += Time.deltaTime;

                currentPos = Vector3.Lerp(startPos, _rightDestination.transform.position, timer / _legMoveTime);
                currentPos.y = _legYCurve.Evaluate(timer / _legMoveTime) * _legLiftHeight;
                leg.solver.IKPosition = currentPos;

                yield return null;
            }
        }
    }

    /*
    private IEnumerator MoveLeg(LimbIK leg)
    {
        float timer = 0f;
        float totalTime = 0.5f;

        while (timer < totalTime)
        {
            timer += Time.deltaTime;

            // Move the current leg towards the destination position.
            //_currentLegPos = Vector3.Lerp(_currentLegPos, _destLegPos, timer / totalTime);
            //_currentLeg.solver.IKPosition = _currentLegPos;

            yield return null;
        }

        // Otherwise, we know the leg is done moving and can switch legs.
        int mod = (_currentLeg == _leftLeg) ? -1 : 1;
        if (mod == -1)
        {
            //_currentLeg = _rightLeg;
            _currentLegPos = _rightLeg.solver.IKPosition;
        }
        else
        {
            //_currentLeg = _leftLeg;
            _currentLegPos = _leftLeg.solver.IKPosition;
        }

        Vector3 o = transform.position + (transform.forward) + (transform.right * -mod * 0.25f);
        Vector3 d = Vector3.down;
        //_destLegPos = ShootRayDown(o, d);

        StartCoroutine(MoveLeg());
    }
*/

    private Vector3 ShootRayDown(Vector3 origin, Vector3 direction)
    {
        Ray ray = new Ray(origin, direction.normalized);
        RaycastHit hit;

        //Debug.DrawRay(ray.origin, ray.direction, Color.white, 10f);

        if (Physics.Raycast(ray, out hit, 10f))
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
    }
}
