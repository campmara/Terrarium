using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CameraManager : SingletonBehaviour<CameraManager> 
{
	public enum CameraState
	{
		NONE = 0,
		FOLLOWPLAYER_FREE,
		FOLLOWPLAYER_LOCKED,
		TRANSITION,
        POND_RETURNPAN        
	}
	CameraState _state = CameraState.FOLLOWPLAYER_FREE;

	[SerializeField, ReadOnlyAttribute] Camera _mainCam = null;
	public Camera Main { get { return _mainCam; } }

	public float CamPixelWidth { get { return _mainCam.pixelWidth; } }
	public float CamPixelHeight { get { return _mainCam.pixelHeight; } }
    private Vector2 _screenCenter = Vector2.zero;

	#region Player Camera Variables
	[Header("Player Cam Variables"), Space(5)]
	[SerializeField, ReadOnlyAttribute] Transform _focusTransform = null;

	Vector3 _camOffset = Vector3.zero;      // Direction from focus to Camera  
	Vector3 _camTargetPos = Vector3.zero;

    Vector3 _focusPoint = Vector3.zero;    // Center of focal point following player
    Vector3 _focusOffset = Vector3.zero;    //
    float _centerDist = 0.0f;               // Current distance of focusCenter from transform
    const float CAM_FOLLOWSPEED = 3f;
	const float CAM_CENTER_UPDATE_SPEED = 15f;
	const float CAM_OFFSETSCALAR = 1.0f;
    const float BOUNDING_RADIUS = 1.7f;         // Distance for player to move from center for cam focus to start following

	/*
	 * ROTATION VARIABLES
	*/
    Vector2 _camInputVals = Vector2.zero;
	const float CAM_ROTSPEED = 100.0f;

	/*
	 * ZOOM VARIABLES
	 */ 
	float _zoomInterp = ZOOM_RESETINTERP;
	const float ZOOM_RESETINTERP = 0.25f;	// Zoom Interp value for initialization & reset of camera (right stick click)
	const float ZOOM_SPEED = 0.6f;			// How much a frame of "zooming" input increments zoom interp
	const float ZOOM_Y_DEADZONE = 0.1f;
    const float ZOOM_X_DEADZONE = 0.1f;
	Vector2 zoomXRange = new Vector2( 4.25f, 10.0f );	// Min & Max zoom x value (x is min)
	Vector2 zoomYRange = new Vector2( 2.5f, 15.0f );	// Min & Max zoom y values (x is min)
    const float ZOOM_DELTASPEED = 50.0f;    // How quickly camOffset moves towards new zoom values

	const float LOCKED_ZOOMINTERP = 0.15f;

	Vector3 _transStartPos = Vector3.zero;
	Vector3 _tranEndPos = Vector3.zero;

    const float CAM_FOV = 60f;

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

        _screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);

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
	void FixedUpdate () 
	{

/*#if UNITY_EDITOR
        if(Input.GetKeyDown(KeyCode.Space))
        {
            if (_state == CameraState.NONE)
            {
                _state = CameraState.FOLLOWPLAYER_FREE;
            }
            else
            {
                _state = CameraState.NONE;
            }
        }

#endif*/
        switch( _state ) 
		{
		case CameraState.FOLLOWPLAYER_FREE:
			HandleFreePlayerCamera();
			break;
		case CameraState.FOLLOWPLAYER_LOCKED:
			HandleLockedPlayerCamera();
			break;
		case CameraState.TRANSITION:
			HandleTransitionMovement();
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
				// Is currently On Press it refocuses b/c Holding Down led to gross stuff
				if( ControlManager.instance.getInput().RightStickButton.IsPressed && ControlManager.instance.getInput().RightStickButton.LastValue == 0.0f ) 
                {
                    ResetCameraOffset();
                }

                // Storing Right Stick Input Data
                _camInputVals.x = ControlManager.instance.getInput().RightStickX;
                _camInputVals.y = ControlManager.instance.getInput().RightStickY;

				// Determine New Zoom Interp level if right stick input
				if ( Mathf.Abs(_camInputVals.y) > ZOOM_Y_DEADZONE )
				{			
					_zoomInterp = Mathf.Clamp01( _zoomInterp + ( ZOOM_SPEED * -_camInputVals.y * Time.fixedDeltaTime) );
				}

				// Calculate Camera Positioning
				DetermineCameraZoom( _zoomInterp );

                // Determine if player in screen bounding circle and move focus center
                UpdateFocusPoint();

                // Move towards new focus center                
				_mainCam.transform.position = Vector3.Lerp(_mainCam.transform.position, _focusPoint + _camOffset, CAM_FOLLOWSPEED * Time.fixedDeltaTime);

                // Rotate Around Camera around player if Right stick horizontal movement
                //      Done After b/c cam stutters if done before position change
                if ( Mathf.Abs(_camInputVals.x) > ZOOM_X_DEADZONE )
                {
                    _mainCam.transform.RotateAround( _focusPoint, Vector3.up, CAM_ROTSPEED * _camInputVals.x * Time.fixedDeltaTime );
                    _camOffset = _mainCam.transform.position - _focusPoint;                   
                }
					
                _mainCam.transform.LookAt( _focusPoint );	
			}

		}
	}


	private void HandleLockedPlayerCamera()
	{
		if( _mainCam != null )
		{
			// Moves Camera
			if ( _focusTransform != null )
			{              
				LockCamFocus();

				_camInputVals.y = ControlManager.instance.getInput().RightStickY;              

				// Determine New Zoom Interp level if right stick input
				if ( Mathf.Abs(_camInputVals.y) > ZOOM_Y_DEADZONE )
				{			
					_zoomInterp = Mathf.Clamp01( _zoomInterp + ( ZOOM_SPEED * -_camInputVals.y * Time.fixedDeltaTime) );
				}
					
				// Calculate Camera Positioning
				DetermineCameraZoom( _zoomInterp );

				// Move towards new focus center                
				_mainCam.transform.position = Vector3.Lerp(_mainCam.transform.position, _focusPoint + _camOffset, CAM_FOLLOWSPEED * Time.fixedDeltaTime);

				_mainCam.transform.LookAt( _focusPoint );	
			}

		}
	}

	/// <summary>
	/// Sets Cam Offset behind focus transform.
	/// </summary>
	private void LockCamFocus()
	{
		_focusPoint = _focusTransform.position;	

		PositionCameraBehindFocus();
	}

	/// <summary>
	/// For when rotating camera behind player or transitioning camera to any position ideally
	/// </summary>
	void HandleTransitionMovement()
	{
	}

	/// <summary>
	/// Handles the pond return pan.
	/// </summary>
	IEnumerator HandlePondReturnPan()
	{
		Tween tween = _mainCam.transform.DOMove(_camOffset, 2f);

		yield return tween.WaitForCompletion();

		ChangeCameraState(CameraState.FOLLOWPLAYER_FREE);
	}

    /// <summary>
    /// Based on Murray's code from here: https://raw.githubusercontent.com/MurrayIRC/frog/master/Assets/Scripts/Actors/Player/PlayerCamera.cs
    /// </summary>
    /// <returns></returns>
    private void UpdateFocusPoint()
    {
		// Center is focusTransform.position
		float distance = (_focusTransform.position - _focusPoint).sqrMagnitude; 

        if ( distance > JohnTech.Sqr( BOUNDING_RADIUS ) )
        {
            // Is outside of the circle.   
			Vector3 desiredPos = new Vector3(_focusTransform.position.x - _focusOffset.x, _focusPoint.y, _focusTransform.position.z - _focusOffset.z);
			_focusPoint = Vector3.Lerp(_focusPoint, desiredPos, CAM_CENTER_UPDATE_SPEED * Time.fixedDeltaTime);         
        }
        else
        {
            // Calculate which direction camera should move if exit bounding circle on screen
            _focusOffset.x = _focusTransform.position.x - _focusPoint.x;
            _focusOffset.y = _focusPoint.y;
            _focusOffset.z = _focusTransform.position.z - _focusPoint.z;
        }
    }

	/// <summary>
	/// Determines Position of Camera offset based on current direction from focus transform and the curr zoom value
	/// </summary>
	private void DetermineCameraZoom( float zoomInterp )
	{
		// Determine new x/z offset positions
		Vector2 lateralOffset = new Vector2( _camOffset.x, _camOffset.z ).normalized;	// Gets flat offset direction
		lateralOffset *= Mathf.Lerp( zoomXRange.x, zoomXRange.y, zoomInterp );

		if(  Mathf.Abs(_camInputVals.x) > ZOOM_X_DEADZONE )
		{
			// If rotating then it sets the offset instead of lerping
			// Hacky but a solution for jumpy zoom while rotating atm
			_camOffset.x = lateralOffset.x;
			_camOffset.y = Mathf.Lerp( zoomYRange.x, zoomYRange.y, zoomInterp);
			_camOffset.z = lateralOffset.y;
		}
		else
		{
			// Set new offset position
			_camOffset.x = Mathf.Lerp( _camOffset.x, lateralOffset.x, ZOOM_DELTASPEED * Time.fixedDeltaTime );
			_camOffset.y = Mathf.Lerp( _camOffset.y, Mathf.Lerp( zoomYRange.x, zoomYRange.y, zoomInterp), ZOOM_DELTASPEED * Time.fixedDeltaTime );
			_camOffset.z = Mathf.Lerp( _camOffset.z, lateralOffset.y, ZOOM_DELTASPEED * Time.fixedDeltaTime );
		}

	}

	/// <summary>
	/// Sets Cam Offset behind focus transform.
	/// </summary>
	private void PositionCameraBehindFocus()
	{
		_camOffset = ( -_focusTransform.forward * Mathf.Lerp(zoomXRange.x, zoomXRange.y, _zoomInterp) ) + ( Vector3.up * Mathf.Lerp( zoomYRange.x, zoomYRange.y, _zoomInterp ) );
	}

	private void ResetCameraOffset()
	{
		PositionCameraBehindFocus();

		_zoomInterp = ZOOM_RESETINTERP;
		DetermineCameraZoom( _zoomInterp );
	}

	public void ChangeCameraState(CameraState newState)
	{
		if( newState != _state )
		{
			// On Disable Old State
			switch( _state )
			{
			case CameraState.FOLLOWPLAYER_FREE:
				if( newState == CameraState.TRANSITION )
				{
					
				}
				break;
			case CameraState.FOLLOWPLAYER_LOCKED:
				break;
			default:
				break;
			}

			_state = newState;

			// On Enable New State
			switch( _state )
			{
			case CameraState.FOLLOWPLAYER_FREE:
				break;
			case CameraState.FOLLOWPLAYER_LOCKED:
				_camInputVals.x = 0.0f;	// So _camOffset lerps in zooming quack
				break;
			case CameraState.POND_RETURNPAN:
				StartCoroutine(HandlePondReturnPan());
				break;
			default:
				break;
			}
		}

	}

    private void OnDrawGizmos()
    {
        if (_focusTransform != null)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(_focusPoint, BOUNDING_RADIUS);
            Gizmos.DrawLine(_focusPoint, _focusTransform.position);
            Gizmos.color = Color.blue;
            // Gizmos.DrawLine(_focusOffset, _focusTransform.position);
        }
    }

    private void HandeGameStateChanged(GameManager.GameState newState, GameManager.GameState prevState)
	{
	}
}
