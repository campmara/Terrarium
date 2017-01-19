using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerBase : MonoBehaviour 
{
	protected InputCollection _input;

	public void UpdateController()
	{
		_input = ControlManager.instance.getInput();
		HandleInput();
	}

	virtual protected void HandleInput() {}
}
