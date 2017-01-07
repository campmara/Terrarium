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

	[SerializeField, ReadOnlyAttribute] private Camera _mainCam = null;
	public Camera Main { get { return _mainCam; } }

	public float CamPixelWidth { get { return _mainCam.pixelWidth; } }
	private float CamPixelHeight { get { return _mainCam.pixelHeight; } }

	#region Player Camera Variables
	[Header("Player Cam Variables"), Space(5)]
	[SerializeField, ReadOnlyAttribute]private Transform _focusTransform = null;

	private Vector3 _camOffsetPosition = Vector3.zero;
	private const float CAM_FOLLOWSPEED = 15.0f;

	private const float CAM_FOV = 90;

	#endregion

	public override void Initialize ()
	{
		_focusTransform = PlayerManager.instance.Player.transform;

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
		if(_mainCam != null)
		{
			// Moves Camera
			if (_focusTransform)
			{
				_mainCam.transform.position = Vector3.Lerp (_mainCam.transform.position, _focusTransform.transform.position + _camOffsetPosition, CAM_FOLLOWSPEED * Time.fixedDeltaTime);

				_mainCam.transform.LookAt(_focusTransform);
			}
				
		}
	}

	private void HandeGameStateChanged(GameManager.GameState newState, GameManager.GameState prevState)
	{
		
	}
}
