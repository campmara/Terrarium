using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using InControl;

public class ControlManager : SingletonBehaviour<ControlManager> 
{
	private HashSet<InputDevice> _devices;

	public delegate Vector3 InputPositionDelegate();
	public InputPositionDelegate _getInputPos = null;
	public InputPositionDelegate _getInputWorldPos = null;

	public delegate bool InputCheckDelegate();
	public InputCheckDelegate _getInputDown = null;
	public InputCheckDelegate _getInputUp = null;
	public InputCheckDelegate _getInput = null;

	private float _screenHeight;
	public float ScreenHeight { get { return _screenHeight; } }
	private float _screenWidth;
	public float ScreenWidth { get { return _screenWidth; } }
	private float _screenDiagonal = 0.0f;
	public float ScreenDiag { get { return _screenDiagonal; } }

	private TouchManager _touchManager = null;

	void Awake()
	{
		InputManager.OnDeviceAttached += HandleDeviceAttached;
		InputManager.OnDeviceDetached += HandleDeviceDetached;
		InputManager.OnActiveDeviceChanged += HandleActiveDeviceChanged;

		_devices = new HashSet<InputDevice>();

		_screenWidth = Camera.main.pixelWidth;
		_screenHeight = Camera.main.pixelHeight;
		_screenDiagonal = Mathf.Sqrt( ( ( _screenWidth * _screenWidth ) + ( _screenHeight + _screenHeight ) ) );

		this.gameObject.AddComponent<InControlManager>();

		#if (UNITY_EDITOR || UNITY_STANDALONE)
		SetupControlsStandalone();
		#elif !(UNITY_EDITOR || UNITY_STANDALONE) && ((UNITY_IOS || UNITY_ANDROID))
		SetupControlsMobile();
		#endif

		AssignInputDelegates();
	}

	public override void Initialize()
	{
		MakeMeAPersistentSingleton();

		foreach(InputDevice device in InputManager.Devices)
		{
			_devices.Add(device);
		}

		isInitialized = true;
	}

	// Update is called once per frame
	void Update () 
	{
		
	}

	/// <summary>
	/// Setups the controls for standalone.
	/// Runs in Awake of ControlManager.
	/// </summary>
	private void SetupControlsStandalone()
	{
	}

	/// <summary>
	/// Setups the controls for mobile.
	/// Runs in Awake of ControlManager.
	/// </summary>
	private void SetupControlsMobile()
	{
		_touchManager = this.gameObject.AddComponent<TouchManager>();

		_touchManager.touchCamera = Camera.main;
	}

	/// <summary>
	/// Assigns the input delegates based on compile environment.
	/// Runs in Awake of ControlManager.
	/// </summary>
	private void AssignInputDelegates()
	{
		#if (UNITY_EDITOR || UNITY_STANDALONE)
		Debug.Log("Initialized Input Delegates for EDITOR/STANDALONE");
		_getInputPos = () => Input.mousePosition;
		_getInputDown = () => Input.GetMouseButtonDown(0);
		_getInputUp = () => Input.GetMouseButtonUp(0);
		_getInput = () => Input.GetMouseButton(0);
		#elif !(UNITY_EDITOR || UNITY_STANDALONE) && ((UNITY_IOS || UNITY_ANDROID))
		Debug.Log("Initialized Input Delegates for IOS");
		_getInputPos = () => TouchManager.GetTouch(0).position;        
		_getInputDown = () => TouchManager.TouchCount > 0 && TouchManager.GetTouch(0).phase == TouchPhase.Began;
		_getInputUp = () => TouchManager.TouchCount > 0 && TouchManager.GetTouch(0).phase == TouchPhase.Ended;
		_getInput = () => TouchManager.TouchCount > 0;
		#endif
		_getInputWorldPos = () => Camera.main.ScreenToWorldPoint(this._getInputPos());
	}

	public bool PingActiveDevicesForSpecificInput(InputControlType action)
	{
		foreach (InputDevice device in _devices)
		{
			if (device.GetControl(action))
			{
				return true;
			}
		}

		return false;
	}

	#region Delegate Functions

	void HandleDeviceAttached(InputDevice device)
	{
		_devices.Add(device);

		Debug.Log( "Attached: " + device.Name );
	}

	void HandleDeviceDetached(InputDevice device)
	{
		_devices.Remove(device);

		Debug.Log( "Detached: " + device.Name );
	}

	void HandleActiveDeviceChanged(InputDevice device)
	{
		Debug.Log( "Active device changed to: " + device.Name );
	}

	void OnDestroy()
	{
		InputManager.OnDeviceAttached -= HandleDeviceAttached;
		InputManager.OnDeviceDetached -= HandleDeviceDetached;
		InputManager.OnActiveDeviceChanged -= HandleActiveDeviceChanged;
	}

	#endregion
}
