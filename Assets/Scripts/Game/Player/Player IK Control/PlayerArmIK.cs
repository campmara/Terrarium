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
        LEFT = 0,
        RIGHT
    }
    [SerializeField] ArmType _armType = ArmType.LEFT;

    public enum ArmIKState : int
    {
        IDLE = 0,           // Base state, should follw Arm Joint
        SITTING,            // For when idling/sitting (b/c arms animate)
        EMPTY_REACHING,     // Trigger reach state but no target
        AMBIENT_REACHING,   // Auto Reach State
        TARGET_REACHING,    // When triggers are pressed
        GRABBING           // Entered through Target Reaching (when triggers are both all the way pressed down)
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
    
    private const float ARM_IDLE_OUT = 0.75f;

    private const float ARM_EMPTYREACH_UP = 10.0f;
    private const float ARM_EMPTYREACH_OUT = 7.5f;

    private const float ARM_REACHDISTMAX = 8.0f;
    private const float ARM_REACHDISTMIN = 0.75f;
    private const float ARM_REACHANGLEMAX = 90.0f;

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
            case ArmIKState.EMPTY_REACHING:
                HandleEmptyReaching();
                break;
            case ArmIKState.AMBIENT_REACHING:
                HandleAmbientReaching();
                break;
            case ArmIKState.TARGET_REACHING:
                HandleTargetReaching();
                break;            
            case ArmIKState.GRABBING:
                HandleGrabbing();
                break;
            case ArmIKState.SITTING:
                break;            
            default:
                break;
        }
    }

	// Update is called once per frame
	public void UpdateArmIK () 
	{
		 _armIK.solver.IKPosition = Vector3.Lerp( _armIK.solver.IKPosition, _armTargetPos, _armIKLerpSpeed );
	}

    public void SetArmState( ArmIKState newState )
    {
        if( newState != _armState )
        {
            switch( newState )
            {
                case ArmIKState.IDLE:
                    _armSpring.GetComponent<Rigidbody>().isKinematic = false;
                    break;
                default:
                    break;
            }

            Debug.Log("Transitioning arm from " + _armState.ToString() + " to " + newState.ToString() );

            _armState = newState;
        }
    }

    private void HandleIdle()
    {
        if( _armType == ArmType.LEFT )
        {
            _armSpring.connectedAnchor = Vector3.Lerp( _armSpring.connectedAnchor, 
				_parentIKController.transform.parent.position - ( _parentIKController.transform.parent.right * ARM_IDLE_OUT ), 
				_armTargetLerpSpeed * Time.deltaTime);
        }
        else
        {
            _armSpring.connectedAnchor = Vector3.Lerp( _armSpring.connectedAnchor, 
				_parentIKController.transform.parent.position - ( -_parentIKController.transform.parent.right * ARM_IDLE_OUT ), 
				_armTargetLerpSpeed * Time.deltaTime);
        }

        _armTargetPos = Vector3.Lerp( _armTargetPos, _armSpring.transform.position, _armTargetLerpSpeed * Time.deltaTime);
    }

    private void HandleEmptyReaching()
    {
        if( _armType == ArmType.LEFT )
        {
            _armTargetPos = Vector3.Lerp( _armTargetPos, Vector3.Lerp( _armSpring.transform.position, _armSpring.transform.position + ( Vector3.up * ARM_EMPTYREACH_UP ) + ( -_parentIKController.transform.right * ARM_EMPTYREACH_OUT ), _armReachInterp ), _armTargetLerpSpeed * Time.deltaTime );
        }
        else
        {
            _armTargetPos = Vector3.Lerp( _armTargetPos, Vector3.Lerp( _armSpring.transform.position, _armSpring.transform.position + ( Vector3.up * ARM_EMPTYREACH_UP ) + ( _parentIKController.transform.right * ARM_EMPTYREACH_OUT ), _armReachInterp ), _armTargetLerpSpeed * Time.deltaTime );
        }

        if( _armReachInterp <= 0.0f )
        {
            SetArmState( ArmIKState.IDLE );
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
				_armTargetPos = Vector3.Lerp( _armTargetPos, Vector3.Lerp( _armSpring.transform.position, _armTargetTransform.position - ( _parentIKController.transform.right * _armGrabOffset ), _armReachInterp ), _armGrabSpeed * Time.deltaTime );
			}
			else
			{
				_armTargetPos = Vector3.Lerp( _armTargetPos, Vector3.Lerp( _armSpring.transform.position, _armTargetTransform.position + ( _parentIKController.transform.right * _armGrabOffset ), _armReachInterp ), _armGrabSpeed * Time.deltaTime );
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
            SetArmState( ArmIKState.EMPTY_REACHING );
        }
    }

    void CheckReachConstraints( )
    {
        Vector3 reachDir = _armTargetTransform.position - this.transform.position;
        float reachDist = reachDir.magnitude;
        float reachAngle = Vector3.Angle( _parentIKController.transform.forward, reachDir );

        //Debug.Log( reachDist );
        //Debug.Log( reachAngle );

        if ( reachDist > ARM_REACHDISTMAX || reachAngle > ARM_REACHANGLEMAX )
        {
            // TODO: Make Droopy Sad : (
            _face.BecomeSad();

            ReleaseTargetTransform();
        }
        else if ( reachDist < ARM_REACHDISTMIN )
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
}
