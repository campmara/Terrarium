using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Animator))]
public class PlayerAnimationController : MonoBehaviour {

    Animator _animator = null;

    public void Initialize()
    {
    }

	// Use this for initialization
	void Awake ()
    {
        _animator = this.GetComponent<Animator>();
	}
	

}
