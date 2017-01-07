using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCameraController : ControllerBase {

	// Use this for initialization
	void Awake () 
	{
		
	}
	
	// Update is called once per frame
	void LateUpdate () 
	{
		UpdateController();	// Calls Handle Input
	}

	protected override void HandleInput ()
	{
		CameraManager.instance.Main.transform.LookAt(transform.position);
	}
}
