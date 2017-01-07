using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerControlManager))]
public class Player : MonoBehaviour {

	PlayerControlManager _playerControlManager = null;
	public PlayerControlManager ControlManager { get { return _playerControlManager; } }

	// Use this for initialization
	void Awake () 
	{
		
	}

	public void Initialize()
	{
		if (_playerControlManager == null)
		{
			_playerControlManager = this.GetComponent<PlayerControlManager>();
		}
		_playerControlManager.Initialize();

	}

}
