using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Animator))]
public class PlayerAnimationController : MonoBehaviour {

    Animator _animator = null;

    const string ANIM_WALK = "WalkAnim";
    const string ANIM_ROLL = "RollAnim";

    public void Initialize()
    {
    }

	// Use this for initialization
	void Awake()
    {
        _animator = this.GetComponent<Animator>();
	}
	
    public void PauseAnimator()
    {
        _animator.StopPlayback();        
    }

    public void PlayWalkAnim()
    {
        _animator.Play( ANIM_WALK );
    }

    public void PlayRollAnim()
    {
        _animator.Play( ANIM_ROLL );
    }
}
