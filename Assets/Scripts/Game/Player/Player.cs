using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerControlManager))]
[RequireComponent(typeof(PlayerCameraController))]
public class Player : MonoBehaviour {

	private PlayerControlManager _playerControlManager = null;
	public PlayerControlManager ControlManager { get { return _playerControlManager; } }

	// Use this for initialization
	void Awake () 
	{
		_playerControlManager = this.GetComponent<PlayerControlManager>();
	}
	
}
