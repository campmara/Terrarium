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
    
    const string PLAYERSPEED_PARAM = "player_velocity";
    
    #endregion
    public void Initialize()
    {
    }

	// Use this for initialization
	void Awake()
    {
        _animator = this.GetComponent<Animator>();
	}
	
    public void SetPlayerSpeed(float speed)
    {
        _animator.SetFloat(PLAYERSPEED_PARAM, speed);
    }
}
