using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerBase : MonoBehaviour 
{
	protected InputCollection input;

	public void UpdateController()
	{
		input = ControlManager.instance.getInput();
		HandleInput();
	}

	virtual protected void HandleInput() {}
}
