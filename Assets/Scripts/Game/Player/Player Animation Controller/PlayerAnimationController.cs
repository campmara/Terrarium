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

    const string WALK_ANIMSTATE = "Walking Blend Tree";
    int _walkAnimStateHash = 0;

    const string SIT_ANIMSTATE = "MC_Sit";
    int _sitAnimStateHash = 0;

    const string SIT_BUTTONPRESS_ANIMSTATE = "Sit_ButtonPress";
    int _sitPressAnimHash = 0;

    const string STAND_ANIMSTATE = "MC_Stand";
    int _standAnimStateHash = 0;

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
        _walkAnimStateHash = Animator.StringToHash( WALK_ANIMSTATE );
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

    public void SitButtonPress()
    {
        _animator.Play( _sitPressAnimHash );
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
