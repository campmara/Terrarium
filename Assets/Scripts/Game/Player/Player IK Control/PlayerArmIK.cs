using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RootMotion.FinalIK;

public class PlayerArmIK : MonoBehaviour {
    
    PlayerIKControl _parentIKController = null;
    CCDIK _armIK = null;
    [SerializeField] SpringJoint _armSpring = null;

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

    [SerializeField, ReadOnlyAttribute] private float _armReachInterp = 0.0f;
    public float ArmReachInterp { get { return _armReachInterp; } set { _armReachInterp = value; } }
    
    private const float ARM_IDLE_OUT = 0.75f;

    private const float ARM_EMPTYREACH_UP = 10.0f;
    private const float ARM_EMPTYREACH_OUT = 7.5f;

    private const float ARM_REACHDISTMAX = 8.0f;
    private const float ARM_REACHDISTMIN = 0.75f;


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
	void LateUpdate () 
	{
		 _armIK.solver.IKPosition = Vector3.Lerp( _armIK.solver.IKPosition, _armTargetPos, _armIKLerpSpeed );
	}

    public void SetArmState( ArmIKState newState )
    {
        if( newState != _armState )
        {
            switch( newState )
            {
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
        _armTargetPos = Vector3.Lerp( _armTargetPos, Vector3.Lerp( _armSpring.transform.position, _armTargetTransform.position, _armReachInterp ), _armTargetLerpSpeed * Time.deltaTime );
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
        float reachDist = ( this.transform.position - _armTargetTransform.position ).magnitude;

        //Debug.Log( reachDist );

        if ( reachDist > ARM_REACHDISTMAX )
        {
            // TODO: Make Droopy Sad : (

            ReleaseTargetTransform();
        }
        else if ( reachDist < ARM_REACHDISTMIN )
        {
            // TODO: Make Droopy Happy : )

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
        if( _armState == ArmIKState.IDLE )
        {            
            _armTargetTransform = reachTrans;

            SetArmState( ArmIKState.AMBIENT_REACHING );
        }        
    }

    public void GrabTargetTransform()
    {        
        SetArmState( ArmIKState.GRABBING );
    }
}
