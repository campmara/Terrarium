using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Animator))]
public class PlayerAnimationController : MonoBehaviour {

    Animator _animator = null;

    #region Parameter Names

    const string PLAYERSPEED_PARAM = "Velocity";
    int _velocityParamHash = 0;

    const string SITTING_PARAM = "Sitting";
    int _sittingParamHash = 0;
    public bool Sitting { get { return _animator.GetBool( _sittingParamHash ); } }

    const string CARRYING_PARAM = "Carrying";
    int _carryingParamHash = 0;
    public bool Carrying { get { return _animator.GetBool( _carryingParamHash ); } }

	const string LIFT_PARAM = "Lifting";
	int _liftParamHash = 0;
	public bool Lifting { get { return _animator.GetBool( _liftParamHash ); } }

    const string LIFTCANCEL_PARAM = "LiftCancel";
    int _liftCancelTriggerHash = 0;

    const string WALK_ANIMSTATE = "Walking Blend Tree";
    int _walkAnimStateHash = 0;

    const string CARRYWALK_ANIMSTATE = "Carrying Blend Tree";
    int _carryWalkAnimHash = 0;

    const string SIT_ANIMSTATE = "MC_Sit";
    int _sitAnimStateHash = 0;

    const string SIT_BUTTONPRESS_ANIMSTATE = "Sit_ButtonPress";
    int _sitPressAnimHash = 0;

    const string STAND_ANIMSTATE = "MC_Stand";
    int _standAnimStateHash = 0;

	const string HUGWIDTH_PARAM = "HugWidth";
	int _hugWidthPropertyHash = 0;
	const string HUGSTATE_PARAM = "HugState";
	int _hugStatePropertyHash = 0;

    #endregion

    public void Initialize()
    {
    }

	// Use this for initialization
	void Awake()
    {
        _animator = this.GetComponent<Animator>();

        _velocityParamHash = Animator.StringToHash( PLAYERSPEED_PARAM );
        _sittingParamHash = Animator.StringToHash( SITTING_PARAM );
        _carryingParamHash = Animator.StringToHash( CARRYING_PARAM );
		_liftParamHash = Animator.StringToHash( LIFT_PARAM );
        _liftCancelTriggerHash = Animator.StringToHash( LIFTCANCEL_PARAM );

		_hugStatePropertyHash = Animator.StringToHash( HUGSTATE_PARAM );
		_hugWidthPropertyHash = Animator.StringToHash( HUGWIDTH_PARAM );

        _walkAnimStateHash = Animator.StringToHash( WALK_ANIMSTATE );
        _carryWalkAnimHash = Animator.StringToHash( CARRYWALK_ANIMSTATE );

        _sitAnimStateHash = Animator.StringToHash( SIT_ANIMSTATE );
        _sitPressAnimHash = Animator.StringToHash( SIT_BUTTONPRESS_ANIMSTATE );
        _standAnimStateHash = Animator.StringToHash( STAND_ANIMSTATE );		
    }
	
    public void SetPlayerSpeed(float speed)
    {
        _animator.SetFloat( _velocityParamHash, speed );
    }

    public void SetSitting(bool isSitting)
    {
        _animator.SetBool(_sittingParamHash, isSitting);
    }

    public void SetCarrying( bool isCarrying )
    {
        _animator.SetBool( _carryingParamHash, isCarrying );
    }
		
	public void SetLifting( bool isLifting )
	{
		_animator.SetBool( _liftParamHash, isLifting );
	}

	public void SetHugWidth( float hugWidth )
	{
		_animator.SetFloat( _hugWidthPropertyHash, hugWidth );
	}
		
	public void SetHugState( float hugState )
	{
		_animator.SetFloat( _hugStatePropertyHash, hugState);
	}

    public void SitButtonPress()
    {
        _animator.Play( _sitPressAnimHash );
    }

    public void TriggerLiftCancel()
    {
        _animator.SetTrigger( _liftCancelTriggerHash );
    }

    public bool CheckCancelSitting()
    {
        if( !Sitting && ( _animator.GetCurrentAnimatorStateInfo(0).fullPathHash == _sitAnimStateHash && _animator.GetCurrentAnimatorStateInfo(0).fullPathHash != _standAnimStateHash ) )
        {
            return true;
        }

        return false;
    }
}
