using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InControl;

public class PlayerControlManager : MonoBehaviour 
{
	ControllerBase _activeController = null;

	InputDevice _playerInputDevice = null;
	public InputDevice PlayerDevice { get { return _playerInputDevice; } set { _playerInputDevice = value; } }

	void Awake ()
	{
	}

	void FixedUpdate () 
	{
		if (_activeController != null)
		{
			// Process Input on Active Controller
			_activeController.UpdateController();	
		}
	}

	public void Initialize()
	{      
		// Initialize Controller being used
		SetActiveController<RollerController>();
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
			if (GetComponent(typeof(T)))
			{
				_activeController = (ControllerBase)this.gameObject.GetComponent(typeof(T));
			}
			else
			{
				_activeController = (ControllerBase)this.gameObject.AddComponent(typeof(T));
			}
			
		}
	}
}
