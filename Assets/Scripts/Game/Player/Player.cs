using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerControlManager)), RequireComponent(typeof(PlayerAnimationController))]
public class Player : MonoBehaviour {

	PlayerControlManager _playerControlManager = null;
	public PlayerControlManager ControlManager { get { return _playerControlManager; } }

    PlayerAnimationController _playerAnimationController = null;
    public PlayerAnimationController AnimationController { get { return _playerAnimationController; } }

    // Use this for initialization
    void Awake () 
	{
        _playerControlManager = this.GetComponent<PlayerControlManager>();
        _playerAnimationController = this.GetComponent<PlayerAnimationController>();
    }

	public void Initialize()
	{
		if ( _playerControlManager != null )
		{
            _playerControlManager.Initialize();
        }
		
        if ( _playerAnimationController != null )
        {
            _playerAnimationController.Initialize();
        }
        

	}

}
