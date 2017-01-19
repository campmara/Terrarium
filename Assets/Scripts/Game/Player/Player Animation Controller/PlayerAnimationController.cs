using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Animator))]
public class PlayerAnimationController : MonoBehaviour {

    Animator _animator = null;

    const string WALK_ANIM = "WalkAnim";
    const string ROLL_ANIM = "RollAnim";
    const string IDLE_ANIM = "IdleAnim";

    public void Initialize()
    {
    }

	// Use this for initialization
	void Awake()
    {
        _animator = this.GetComponent<Animator>();
	}
	
    public void PlayIdleAnim()
    {     
        _animator.Play( IDLE_ANIM );
    }

    public void PlayWalkAnim()
    {
        _animator.Play( WALK_ANIM );
    }

    public void PlayRollAnim()
    {
        _animator.Play( ROLL_ANIM );
    }
}
