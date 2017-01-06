using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerBase : MonoBehaviour 
{
	protected Vector3 _inputStartPos = Vector3.zero;
	protected Vector3 _currInputPos = Vector3.zero;
	protected Vector3 _currInputWorldPos = Vector3.zero;

	public void UpdateController()
	{
		if( ControlManager.instance._getInput() )
		{
			_currInputPos = ControlManager.instance._getInputPos();
			_currInputWorldPos = ControlManager.instance._getInputWorldPos();

			HandleGetInput();
		}

		if( ControlManager.instance._getInputDown() )
		{
			_inputStartPos = ControlManager.instance._getInputPos();
			_currInputPos = _inputStartPos;
			_currInputWorldPos = ControlManager.instance._getInputWorldPos();

			HandleGetInputDown();
		}

		if( ControlManager.instance._getInputUp() )
		{
			HandleGetInputUp();
		}
	}

	virtual protected void HandleGetInput() {}

	virtual protected void HandleGetInputDown() {}

	virtual protected void HandleGetInputUp() {}

}
