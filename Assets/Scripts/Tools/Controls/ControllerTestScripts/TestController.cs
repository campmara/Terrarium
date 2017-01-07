using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestController : ControllerBase {

	void Awake()
	{
		Debug.Log("Added Test Controller to Player Control Manager");
	}

	void OnEnable()
	{
		Debug.Log("Enabled Test Controller on Player Control Manager");
	}

	// Also called on Destroy
	void OnDisable()
	{
		Debug.Log("Disabled Test Controller on Player Control Manager");	
	}

	/*
	protected override void HandleGetInput ()
	{
	}

	protected override void HandleGetInputDown ()
	{
	}

	protected override void HandleGetInputUp ()
	{
	}
	*/
}
