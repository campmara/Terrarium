using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RootMotion.FinalIK;

public class PlayerArmIK : MonoBehaviour {
    
    PlayerIKControl _parentIKController = null;
    CCDIK _armIK = null;
    [SerializeField] SpringJoint _armSpring = null;
    [SerializeField]
    FaceManager _face = null;

    [SerializeField, Space(5)] Vector3[] _armGestureArray = { new Vector3( 7.5f, 10.0f, 0.0f) };
    [SerializeField, ReadOnly] private int _armGestureIndex = 0;

    void OnDisable()
    {
        _armIK.enabled = false;
    }

    void OnEnable()
    {
        _armIK.enabled = true;
    }

    public enum ArmType : int
    {
        NONE = -1,
		LEFT = 0,
        RIGHT,
		BOTH
    }
    [SerializeField, Space(5)] ArmType _armType = ArmType.LEFT;

    public enum ArmIKState : int
    {
        IDLE = 0,           // Base state, should follw Arm Joint
        SITTING,            // For when idling/sitting (b/c arms animate)
        GESTURING,     // Trigger reach state but no target
        AMBIENT_REACHING,   // Auto Reach State
        TARGET_REACHING,    // When triggers are pressed
        GRABBING,           // Entered through Target Reaching (when triggers are both all the way pressed down)
        IK_OFF
    }
    [SerializeField, ReadOnlyAttribute] ArmIKState _armState = ArmIKState.IDLE;
    public ArmIKState ArmState { get { return _armState; } }
    
    [SerializeField] private Transform _armTargetTransform = null;   
    public Transform ArmTargetTrans { get { return _armTargetTransform; } }    
	[SerializeField] private float _armTargetLerpSpeed = 7f;
	[SerializeField] private float _armIKLerpSpeed = 15f;    
    [SerializeField, ReadOnlyAttribute] private Vector3 _armTargetPos = Vector3.zero;
    
    /// 
    /// GRABBING VARIABLES
    /// 
    [SerializeField]
    private float _armGrabSpeed = 20f;
    private float _armGrabOffset = 0.4f;

    [SerializeField, ReadOnlyAttribute] private float _armReachInterp = 0.0f;
    public float ArmReachInterp { get { return _armReachInterp; } set { _armReachInterp = value; } }
    
    private const float ARM_IDLE_OUT = 0.5f;
    private const float ARM_IDLE_UP = 0.65f;

    // Ambient Reaching Variables
    private const float ARM_REACHDISTMAX = 8.0f;
    private const float ARM_REACHDISTMIN = 0.75f;
    private const float ARM_REACHANGLEMAX = 90.0f;

	private float _ambientReachTimer = 0.0f;
	private const float AMBIENTREACH_MINTIME = 2.0f;
	private const float AMBIENTREACH_MAXTIME = 10.0f;

	[SerializeField, Space(10)]
	Transform _armTipTransform = null;
	public Vector3 ArmTipPosition { get { return _armTipTransform.position; } }

	// Use this for initialization
	void Awake () 
	{
		_parentIKController = this.GetComponentInParent<PlayerIKControl>();
        _armIK = this.GetComponent<CCDIK>();
	}
	
    void FixedUpdate()
    {
        switch( _armState )
        {
            case ArmIKState.IDLE:
                HandleIdle();
                break;
            case ArmIKState.GESTURING:
                HandleGesturing();
                break;
            case ArmIKState.AMBIENT_REACHING:
				if( _armTargetTransform != null )
				{
					HandleAmbientReaching();	
				}
				else
				{
					ReleaseTargetTransform();	
				}
                break;
            case ArmIKState.TARGET_REACHING:
                HandleTargetReaching();
                break;            
            case ArmIKState.GRABBING:
                HandleGrabbing();
                break;
            case ArmIKState.SITTING:
                break;    
			case ArmIKState.IK_OFF:
				break;
            default:
                break;
        }
    }

	// Update is called once per frame
	public void UpdateArmIK () 
	{
        if( _armState != ArmIKState.IK_OFF )
        {
            _armIK.solver.IKPosition = Vector3.Lerp( _armIK.solver.IKPosition, _armTargetPos, _armIKLerpSpeed );
        }        
	}

    public void SetArmState( ArmIKState newState )
    {
        if( newState != _armState )
        {
			if( _armState == ArmIKState.IK_OFF )
			{
				_armIK.enabled = true;
			}

			switch( newState )
            {
                case ArmIKState.IDLE:
                    _armSpring.GetComponent<Rigidbody>().isKinematic = false;
                    break;
				case ArmIKState.AMBIENT_REACHING:
					_ambientReachTimer = 0.0f;
					break;
				case ArmIKState.IK_OFF:
					_armIK.enabled = false;
					break;
				default:
                    break;
            }

            //Debug.Log("Transitioning arm from " + _armState.ToString() + " to " + newState.ToString() );

            _armState = newState;
        }
    }

    private void HandleIdle()
    {
        if( _armType == ArmType.LEFT )
        {
            _armSpring.connectedAnchor = Vector3.Lerp( _armSpring.connectedAnchor, 
				_parentIKController.transform.parent.position + ( -_parentIKController.transform.parent.right * ARM_IDLE_OUT ) + ( _parentIKController.transform.parent.up * ARM_IDLE_UP ), 
				_armTargetLerpSpeed * Time.deltaTime);
        }
        else
        {
            _armSpring.connectedAnchor = Vector3.Lerp( _armSpring.connectedAnchor, 
				_parentIKController.transform.parent.position + ( _parentIKController.transform.parent.right * ARM_IDLE_OUT ) + ( _parentIKController.transform.parent.up * ARM_IDLE_UP ), 
				_armTargetLerpSpeed * Time.deltaTime);
        }

        _armTargetPos = Vector3.Lerp( _armTargetPos, _armSpring.transform.position, _armTargetLerpSpeed * Time.deltaTime);

		if( _armReachInterp > Mathf.Epsilon )
		{
			SetArmState( ArmIKState.GESTURING );
		}
    }

    private void HandleGesturing()
    {
        if( _armType == ArmType.LEFT )
        {
            _armTargetPos = Vector3.Lerp( _armTargetPos, Vector3.Lerp( _armSpring.transform.position,
                _parentIKController.transform.position + ( -_parentIKController.transform.right * _armGestureArray[_armGestureIndex].x ) + ( Vector3.up * _armGestureArray[_armGestureIndex].y ) + ( _parentIKController.transform.forward * _armGestureArray[_armGestureIndex].z), 
                _armReachInterp ), _armTargetLerpSpeed * Time.deltaTime );
        }
        else
        {
            _armTargetPos = Vector3.Lerp( _armTargetPos, Vector3.Lerp( _armSpring.transform.position,
               _parentIKController.transform.position + ( _parentIKController.transform.right * _armGestureArray[_armGestureIndex].x ) + ( Vector3.up * _armGestureArray[_armGestureIndex].y ) + ( _parentIKController.transform.forward * _armGestureArray[_armGestureIndex].z ),
               _armReachInterp ), _armTargetLerpSpeed * Time.deltaTime );           
        }

        if( _armReachInterp <= 0.0f )
        {
            SetArmState( ArmIKState.IDLE );
        }

        SummonAtObject();
    }

    private void SummonAtObject()
    {
        RaycastHit hit; 
        if( Physics.Raycast( transform.parent.position, transform.parent.forward, out hit, 6.0f) )
        {
            if( hit.collider.GetComponent<StarterPlantGrowthController>() )
            {
                hit.collider.GetComponent<StarterPlantGrowthController>().SummonSeed( new Vector2( transform.position.x, transform.position.z ) );
            }
            else if ( hit.collider.GetComponent<PointPlantGrowthController>() )
            {
                hit.collider.GetComponent<PointPlantGrowthController>().SummonSeed( new Vector2( transform.position.x, transform.position.z ) );
            }
        }
    }

    public void IncrementGestureIndex()
    {
        if( _armGestureIndex < _armGestureArray.Length - 1 )
        {
            _armGestureIndex++;
        }
        else
        {
            _armGestureIndex = 0;
        }
    }

    private void HandleAmbientReaching()
    {
        _armTargetPos = Vector3.Lerp( _armTargetPos,  _armTargetTransform.position, _armTargetLerpSpeed * Time.deltaTime );

        _armSpring.connectedAnchor = Vector3.Lerp( _armSpring.connectedAnchor, _parentIKController.transform.parent.position - ( _parentIKController.transform.parent.right * 0.5f ), _armTargetLerpSpeed * Time.deltaTime );
        _armTargetPos = Vector3.Lerp( _armTargetPos, _armSpring.transform.position, _armTargetLerpSpeed * Time.deltaTime ); 
        
        CheckReachConstraints();
    }

    private void HandleTargetReaching()
    {
        _armTargetPos = Vector3.Lerp( _armTargetPos, Vector3.Lerp( _armSpring.transform.position, _armTargetTransform.position, _armReachInterp ), _armTargetLerpSpeed * Time.deltaTime );
    }

    private void HandleGrabbing()
    {
		if( _armTargetTransform != null )
		{
			// Each arm offseted differently. should be done in animation idk
			if( _armType == ArmType.LEFT )
			{
				_armTargetPos = Vector3.Lerp( _armTargetPos, _armTargetTransform.position - ( _parentIKController.transform.right * _armGrabOffset ), _armGrabSpeed * Time.deltaTime );
			}
			else
			{
				_armTargetPos = Vector3.Lerp( _armTargetPos,_armTargetTransform.position + ( _parentIKController.transform.right * _armGrabOffset ), _armGrabSpeed * Time.deltaTime );
			}
		}                
    }

    public void SetArmTargetTransform( Transform target )
    {
        _armTargetTransform = target;

        _armSpring.GetComponent<Rigidbody>().isKinematic = true;

        if ( target != null )
        {
            SetArmState( ArmIKState.TARGET_REACHING );
        }
        else
        {
            SetArmState( ArmIKState.GESTURING );
        }
    }

    void CheckReachConstraints( )
    {
        Vector3 reachDir = _armTargetTransform.position - this.transform.position;
        float reachDist = reachDir.magnitude;
        float reachAngle = Vector3.Angle( _parentIKController.transform.forward, reachDir );

		_ambientReachTimer += Time.deltaTime;

        //Debug.Log( reachDist );
        //Debug.Log( reachAngle );

		if ( reachDist > ARM_REACHDISTMAX || reachAngle > ARM_REACHANGLEMAX || _ambientReachTimer > AMBIENTREACH_MAXTIME )
        {

			if( _ambientReachTimer < AMBIENTREACH_MINTIME )
			{
				// TODO: Make Droopy Sad : (
				//_face.BecomeSad();	
			}            

            ReleaseTargetTransform();
        }
        else if ( reachDist <= ARM_REACHDISTMIN )
        {
            // TODO: Make Droopy Happy : )
            _face.BecomeHappy();

            ReleaseTargetTransform();
        }
    }

    public void ReleaseTargetTransform()
    {
        _armTargetTransform = null;

        _armSpring.GetComponent<Rigidbody>().isKinematic = false;

        SetArmState( ArmIKState.IDLE );
    }

    public void SetAmbientReachTransform( Transform reachTrans )
    {
        Vector3 reachDir = reachTrans.position - this.transform.position;
        float reachAngle = Vector3.Angle( _parentIKController.transform.forward, reachDir.normalized );

        if ( _armState == ArmIKState.IDLE || reachAngle < ARM_REACHANGLEMAX )
        {
            // Pick a random reach point
            if (JohnTech.CoinFlip())
            {
                _face.BecomeInterested();
            }
            else
            {
                _face.BecomeDesirous();
            }

            _armTargetTransform = reachTrans;

            SetArmState( ArmIKState.AMBIENT_REACHING );
        }        
    }

    public void GrabTargetTransform()
    {        
        SetArmState( ArmIKState.GRABBING );
    }

	public void SetIKOff()
	{
		SetArmState( ArmIKState.IK_OFF );
	}

	public void SetIKOn()
	{
		SetArmState( ArmIKState.IDLE );
	}

}
