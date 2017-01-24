using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Animator))]
public class PlayerAnimationController : MonoBehaviour {

    Animator _animator = null;

    const string WALK_ANIM = "WalkAnim";
    const string ROLL_ANIM = "RollAnim";
    const string IDLE_ANIM = "IdleAnim";
	const string CARRYIDLE_ANIM = "IdleAnim";

	const string WALKTOROLL_ANIM = "WalkToRoll";
	const string ROLLTOWALK_ANIM = "RollToWalk";

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

	public void PlayCarryIdleAnim()
	{     
		_animator.Play( CARRYIDLE_ANIM );
	}

    public void PlayWalkAnim()
    {
        _animator.Play( WALK_ANIM );
    }

    public void PlayRollAnim()
    {
        _animator.Play( ROLL_ANIM );
    }

	// Transitions to WALK animation in the animation controller
	public void PlayRollToWalkAnim()
	{
		_animator.Play( ROLLTOWALK_ANIM );
	}

	// No transition to a roll anim yet, just a SPHERE
	public void PlayWalkToRollAnim()
	{
		_animator.Play( WALKTOROLL_ANIM );
	}
}
