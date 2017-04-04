using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using InControl;

public class InputCollection : PlayerActionSet
{
	public PlayerAction AButton;
	public PlayerAction BButton;
	public PlayerAction XButton;
	public PlayerAction YButton;

	public PlayerAction LeftStickLeft;
	public PlayerAction LeftStickRight;
	public PlayerAction LeftStickDown;
	public PlayerAction LeftStickUp;

	public PlayerAction RightStickLeft;
	public PlayerAction RightStickRight;
	public PlayerAction RightStickDown;
	public PlayerAction RightStickUp;

	public PlayerOneAxisAction LeftStickX;
	public PlayerOneAxisAction LeftStickY;
	public PlayerOneAxisAction RightStickX;
	public PlayerOneAxisAction RightStickY;

	public PlayerAction LeftTrigger;
	public PlayerAction RightTrigger;

	public PlayerAction LeftStickButton;
    public PlayerAction RightStickButton;

	public InputCollection()
	{
		AButton = CreatePlayerAction("A Button");
		BButton = CreatePlayerAction("B Button");
		XButton = CreatePlayerAction("X Button");
		YButton = CreatePlayerAction("Y Button");

		LeftStickLeft = CreatePlayerAction("Left Stick Left");
		LeftStickRight = CreatePlayerAction("Left Stick Right");
		LeftStickDown = CreatePlayerAction("Left Stick Down");
		LeftStickUp = CreatePlayerAction("Left Stick Up");

		RightStickLeft = CreatePlayerAction("Right Stick Left");
		RightStickRight = CreatePlayerAction("Right Stick Right");
		RightStickDown = CreatePlayerAction("Right Stick Down");
		RightStickUp = CreatePlayerAction("Right Stick Up");

		LeftStickX = CreateOneAxisPlayerAction(LeftStickLeft, LeftStickRight);
		LeftStickY = CreateOneAxisPlayerAction(LeftStickDown, LeftStickUp);
		RightStickX = CreateOneAxisPlayerAction(RightStickLeft, RightStickRight);
		RightStickY = CreateOneAxisPlayerAction(RightStickDown, RightStickUp);

		LeftTrigger = CreatePlayerAction( "Left Trigger" );
		RightTrigger = CreatePlayerAction( "Right Trigger" );

		LeftStickButton = CreatePlayerAction("Left Stick Click");
        RightStickButton = CreatePlayerAction("Right Stick Button");

        // ADD BINDINGS
        AButton.AddDefaultBinding(InputControlType.Action1);
		BButton.AddDefaultBinding(InputControlType.Action2);
		XButton.AddDefaultBinding(InputControlType.Action3);
		YButton.AddDefaultBinding(InputControlType.Action4);

		LeftStickLeft.AddDefaultBinding(InputControlType.LeftStickLeft);
		LeftStickRight.AddDefaultBinding(InputControlType.LeftStickRight);
		LeftStickDown.AddDefaultBinding(InputControlType.LeftStickDown);
		LeftStickUp.AddDefaultBinding(InputControlType.LeftStickUp);

		RightStickLeft.AddDefaultBinding(InputControlType.RightStickLeft);
		RightStickRight.AddDefaultBinding(InputControlType.RightStickRight);
		RightStickDown.AddDefaultBinding(InputControlType.RightStickDown);
		RightStickUp.AddDefaultBinding(InputControlType.RightStickUp);

		LeftTrigger.AddDefaultBinding( InputControlType.LeftTrigger );
		RightTrigger.AddDefaultBinding( InputControlType.RightTrigger );

		LeftStickButton.AddDefaultBinding(InputControlType.LeftStickButton);
        RightStickButton.AddDefaultBinding(InputControlType.RightStickButton);

		// Keyboard Controls
		AButton.AddDefaultBinding(Key.E);
		BButton.AddDefaultBinding(Key.LeftShift);
		XButton.AddDefaultBinding(Key.Space);
		YButton.AddDefaultBinding(Key.F);

        LeftStickLeft.AddDefaultBinding(Key.A);
        LeftStickRight.AddDefaultBinding(Key.D);
        LeftStickUp.AddDefaultBinding(Key.W);
        LeftStickDown.AddDefaultBinding(Key.S);

        RightStickLeft.AddDefaultBinding(Key.LeftArrow);
        RightStickRight.AddDefaultBinding(Key.RightArrow);
        RightStickUp.AddDefaultBinding(Key.UpArrow);
        RightStickDown.AddDefaultBinding(Key.DownArrow);

		LeftTrigger.AddDefaultBinding( Key.Q );
		RightTrigger.AddDefaultBinding( Key.Q );

		LeftTrigger.Sensitivity = 0.0025f;
		RightTrigger.Sensitivity = 0.0025f;
    }
}

public class ControlManager : SingletonBehaviour<ControlManager> 
{
	HashSet<InputDevice> _devices;

	public delegate InputCollection InputCollectionDelegate();
	public InputCollectionDelegate getInput = null;

	float _screenHeight;
	public float ScreenHeight { get { return _screenHeight; } }
	float _screenWidth;
	public float ScreenWidth { get { return _screenWidth; } }
	float _screenDiagonal = 0.0f;
	public float ScreenDiag { get { return _screenDiagonal; } }

	TouchManager _touchManager = null;

	void Awake()
	{
		InputManager.OnDeviceAttached += HandleDeviceAttached;
		InputManager.OnDeviceDetached += HandleDeviceDetached;
		InputManager.OnActiveDeviceChanged += HandleActiveDeviceChanged;

		_devices = new HashSet<InputDevice>();

		_screenWidth = Camera.main.pixelWidth;
		_screenHeight = Camera.main.pixelHeight;
		_screenDiagonal = Mathf.Sqrt( ( ( _screenWidth * _screenWidth ) + ( _screenHeight * _screenHeight ) ) );

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
		//MakeMeAPersistentSingleton();

		foreach(InputDevice device in InputManager.Devices)
		{
			_devices.Add(device);
		}

		isInitialized = true;
	}

	public void Vibrate( float intensity )
	{
		InputManager.ActiveDevice.Vibrate( intensity );
	}

	public void Vibrate( float leftMotorIntensity, float rightMotorIntensity )
	{
		InputManager.ActiveDevice.Vibrate( leftMotorIntensity, rightMotorIntensity );
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
		InputCollection input = new InputCollection();
		getInput = () => input;
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
