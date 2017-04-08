using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerBase : MonoBehaviour 
{
	protected InputCollection _input;

	public void FixedUpdateController()
	{
		_input = ControlManager.instance.getInput();
		HandleInput();
	}

	protected virtual void HandleInput() {}
}
