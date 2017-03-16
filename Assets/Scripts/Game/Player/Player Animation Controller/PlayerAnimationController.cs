using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Animator))]
public class PlayerAnimationController : MonoBehaviour {

    Animator _animator = null;

    const string WALK_ANIM = "WalkAnim";
    const string ROLL_ANIM = "RollAnim";
    const string IDLE_ANIM = "IdleAnim";

    #region Parameter Names
    
    const string PLAYERSPEED_PARAM = "Velocity";
    int _velocityParamHash = 0;

    const string SITTING_PARAM = "Sitting";
    int _sittingParamHash = 0;
    
    #endregion
    public void Initialize()
    {
    }

	// Use this for initialization
	void Awake()
    {
        _animator = this.GetComponent<Animator>();

        _velocityParamHash = Animator.StringToHash( PLAYERSPEED_PARAM );
        _sittingParamHash = Animator.StringToHash(SITTING_PARAM);
	}
	
    public void SetPlayerSpeed(float speed)
    {
        _animator.SetFloat( _velocityParamHash, speed );
    }

    public void SetSitting(bool isSitting)
    {
        _animator.SetBool(_sittingParamHash, isSitting);
    }
}
