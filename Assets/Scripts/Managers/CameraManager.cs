using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : SingletonBehaviour<CameraManager> {

	public enum CameraState
	{
		NONE = 0,
		FOLLOWPLAYER_FREE,
		FOLLOWPLAYER_LOCKED
	}
	CameraState _state = CameraState.FOLLOWPLAYER_FREE;

	[SerializeField, ReadOnlyAttribute] Camera _mainCam = null;
	public Camera Main { get { return _mainCam; } }

	public float CamPixelWidth { get { return _mainCam.pixelWidth; } }
	public float CamPixelHeight { get { return _mainCam.pixelHeight; } }

	#region Player Camera Variables
	[Header("Player Cam Variables"), Space(5)]
	[SerializeField, ReadOnlyAttribute] Transform _focusTransform = null;

	Vector3 _camOffset = Vector3.zero;
	const float CAM_FOLLOWSPEED = 15.0f;

	const float CAM_ROTSPEED = 45.0f;

	float _zoomInterp = ZOOM_RESETINTERP;
	const float ZOOM_RESETINTERP = 0.25f;
	const float ZOOM_SPEED = 0.25f;
	const float ZOOM_DEADZONE = 0.1f;
	const float ZOOM_XDELTA = 1.0f;
	Vector2 zoomXRange = new Vector2(3.0f, 20.0f);
	const float ZOOM_YDELTA = 2.0f;
	Vector2 zoomYRange = new Vector2(2.0f, 15.0f);

	const float CAM_FOV = 90;

	#endregion

	public override void Initialize ()
	{
		if( PlayerManager.instance.Player != null )
		{
			_focusTransform = PlayerManager.instance.Player.transform;	
		}
		else
		{
			Debug.LogError("Camera needs a focus transform");
		}

		ResetCameraOffset();

		_mainCam.fieldOfView = CAM_FOV;

		isInitialized = true;
	}

	// Use this for initialization
	void Awake () 
	{
		if( _mainCam == null )
		{
			_mainCam = FindObjectOfType<Camera>();

			if( _mainCam == null)
			{
				_mainCam = new GameObject().AddComponent<Camera>();
			}
		}	

		GameManager.GameStateChanged += HandeGameStateChanged;
	}

	// Update is called once per frame
	void LateUpdate () 
	{
		switch( _state ) 
		{
		case CameraState.FOLLOWPLAYER_FREE:
			HandleFreePlayerCamera();
			break;
		default:
			break;
		}
	}

	private void HandleFreePlayerCamera()
	{
		if( _mainCam != null )
		{
			// Moves Camera
			if ( _focusTransform != null )
			{
				// Move Camera based on current Offset if player Moved
				//_mainCam.transform.position = Vector3.Lerp (_mainCam.transform.position, _focusTransform.transform.position + _camOffset, CAM_FOLLOWSPEED * Time.deltaTime);

				// Rotate Around Camera around player if Right stick horizontal movement
				if( ControlManager.instance.getInput().RightStickX != 0.0f )
				{
					_mainCam.transform.RotateAround( _focusTransform.position, Vector3.up, CAM_ROTSPEED * ControlManager.instance.getInput().RightStickX * Time.deltaTime );
					_camOffset = _mainCam.transform.position - _focusTransform.position;
				}

				// Determine New Zoom Interp level if right stick input
				if( ControlManager.instance.getInput().RightStickY > ZOOM_DEADZONE || ControlManager.instance.getInput().RightStickY < -ZOOM_DEADZONE )
				{
					float rightStickInputVal = ControlManager.instance.getInput().RightStickY;

					_zoomInterp = Mathf.Clamp01( _zoomInterp + ( ZOOM_SPEED * -rightStickInputVal * Time.deltaTime ) );
				}

				// Calculate Camera Positioning
				DetermineCameraZoom();

				_mainCam.transform.position = Vector3.Lerp( _mainCam.transform.position, _focusTransform.position + _camOffset, CAM_FOLLOWSPEED * Time.deltaTime );

				_mainCam.transform.LookAt( _focusTransform );	
			}

		}
	}

	private void DetermineCameraZoom()
	{
		// Determine new x/z offset positions
		Vector2 xOffset = new Vector2( _camOffset.x, _camOffset.z ).normalized;	// Gets flat offset direction
		xOffset *= Mathf.Lerp( zoomXRange.x, zoomXRange.y, _zoomInterp );

		// Set new offset position
		_camOffset.x = xOffset.x;
		_camOffset.y = Mathf.Lerp( zoomYRange.x, zoomYRange.y, _zoomInterp );
		_camOffset.z = xOffset.y;

	}

	private void ResetCameraOffset()
	{
		// Place camera behind player
		_mainCam.transform.position = _focusTransform.transform.position + -_focusTransform.forward;
		_camOffset = _mainCam.transform.position - _focusTransform.position;

		_zoomInterp = ZOOM_RESETINTERP;
		DetermineCameraZoom();

	}

	private void HandeGameStateChanged(GameManager.GameState newState, GameManager.GameState prevState)
	{
	}
}
