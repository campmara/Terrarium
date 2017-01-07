using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InControl;

public class PlayerControlManager : MonoBehaviour 
{
	private ControllerBase _activeController = null;

	private InputDevice _playerInputDevice = null;
	public InputDevice PlayerDevice { get { return _playerInputDevice; } set { _playerInputDevice = value; } }

	void Awake ()
	{
		// Initialize Controller being used
		SetActiveController<InactiveController>();
	}

	void Update () 
	{
		// Process Input on Active Controller
		_activeController.UpdateController();
	}

	/// <summary>
	/// Pass in controller type to be set as current active controller.
	/// </summary>
	public void SetActiveController<T>() where T : ControllerBase
	{
		if( _activeController != null )
		{
			if( _activeController.GetType() != typeof(T) )
			{	
				// Disable Last Controller
				_activeController.enabled = false;

				// Check if controller already added to gameObject
				_activeController = (ControllerBase)this.GetComponent(typeof(T));

				if( _activeController != null )  // ReEnable New Controller
				{
					_activeController.enabled = true;
				}
				else   // Add new controller type to gameObject
				{
					_activeController = (ControllerBase)this.gameObject.AddComponent(typeof(T));	// Initialize new controller in awake function
				}
			}
		}
		else
		{
			_activeController = (ControllerBase)this.gameObject.AddComponent(typeof(T));
		}
	}
}
