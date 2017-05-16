using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CameraManager : SingletonBehaviour<CameraManager>
{
	public enum CameraState
	{
		NONE = 0,
        INTRO,
		FOLLOWPLAYER_FREE,
		FOLLOWPLAYER_LOCKED,
		TRANSITION,
        POND_WAIT,
        POND_RETURNPAN,
		SITTING
	}
	CameraState _state = CameraState.NONE;
    public CameraState CamState { get { return _state; } }

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

	public Vector3 FocusPoint { get { return _focusPoint; } }
    Vector3 _focusPoint = Vector3.zero;    	// Center of focal point following player
    Vector3 _focusOffset = Vector3.zero;    
    float _centerDist = 0.0f;               // Current distance of focusCenter from transform
    const float CAM_FOLLOWSPEED = 15f;
	const float CAM_CENTER_UPDATE_SPEED = 15f;
    float _lookVertOffset;
    const float CAMLOOK_VERTICALOFFSET_MIN = 1.0f;	// How far up/down the cam looks relative to the actually cam focus point
    const float CAMLOOK_VERTICALOFFSET_MAX = 2.0f;  // CHANGES IN REVERSE: gets closer to max as you zoom in closer
    [SerializeField] AnimationCurve _lookVertOffsetCurve = null;

    const float BOUNDING_VERTICALOFFSET = 0.25f;	// How much the bounding radius for camera movement is offset vertically from camera
	const float BOUNDING_LATERALOFFSET = 1.7f;		// How much the bounding radius for camera movement is offset horizontally from camera
	//Vector3 _inputCamOffset = Vector3.zero; 				// Offsets camera based on bounding offsets for when player is moving player
	const float BOUNDING_RADIUS = 1.7f;         // Distance for player to move from center for cam focus to start following   
	const float BOUNDING_RADIUS_MIN = 0.25f;   
	const float BOUNDING_RADIUS_MAX = 1.7f;  
	// Distance for player to move from center for cam focus to start following

    #endregion

    #region Camera Rotation & Zoom Values

    /*
	 * ROTATION VARIABLES
	*/
    Vector2 _camInputVals = Vector2.zero;
	const float CAM_ROTSPEED = 100.0f;

    /*
	 * ZOOM VARIABLES
	 */
    [SerializeField] AnimationCurve _zoomInterpCurve = null;
	float _zoomInterp = ZOOM_RESETINTERP;
	const float ZOOM_RESETINTERP = 0.35f;	// Zoom Interp value for initialization & reset of camera (right stick click)
	const float ZOOM_SPEED = 0.6f;			// How much a frame of "zooming" input increments zoom interp
	const float ZOOM_Y_DEADZONE = 0.1f;		// Zoom Input Deadzone for Y 
	const float ZOOM_X_DEADZONE = 0.1f;		// Zoom Input Deadzone for X 
	Vector2 zoomXRange = new Vector2( 2.25f, 8.0f );	// Min & Max zoom x value (x is min)
	Vector2 zoomYRange = new Vector2( 1.0f, 10.0f );	// Min & Max zoom y values (x is min)
    const float ZOOM_DELTASPEED = 15.0f;    // How quickly camOffset moves towards new zoom values
    const float CAMERA_MINY = 0.25f;

    const float LOCKED_ZOOMINTERP = 0.15f;

    #endregion

    #region Pond Transition Variables

	const float INTRO_PAN_PREWAIT_TIME = 2f;
	const float INTRO_PAN_TIME = 2f;

    const float PONDRETURN_FORWARD = 4.5f;
    const float PONDRETURN_UP = 2.0f;    
    const float PONDRETURN_TRANSITIONTIME = 2f;

    //const float PLAYERPOP_FORWARDPOS = 5.0f;
	const float PLAYERPOP_FORWARDPOS = 0f;

    #endregion

	float _fov; 
	const float CAM_FOV_MIN = 60.0f;    // CHANGES IN REVERSE: the lower closer it is zoomed in the closer the fov is to its max val 
	const float CAM_FOV_MAX = 90.0f; 
	[SerializeField] AnimationCurve _fovCurve = null; 
    const float CAM_FOV = 60f;

	const float SITTING_ROTATESPEED = 2.0f;

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

		//_mainCam.fieldOfView = Mathf.Lerp( CAM_FOV_MIN, CAM_FOV_MAX, ZOOM_RESETINTERP );
		_mainCam.fieldOfView = 82.5f;

        _lookVertOffset = Mathf.Lerp( CAMLOOK_VERTICALOFFSET_MIN, CAMLOOK_VERTICALOFFSET_MAX, ZOOM_RESETINTERP );

        _screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);

        isInitialized = true;
	}

	// Use this for initialization
	void Awake ()
	{
		if( _mainCam == null )
		{
            _mainCam = Camera.main;
            //_mainCam = FindObjectOfType<Camera>();

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
		case CameraState.SITTING:
			SittingCameraRotate();
			HandleFreePlayerCamera();
			break;
        case CameraState.POND_WAIT:
            SittingCameraRotate();
            //HandleFreePlayerCamera();
            break;
	    default:
			break;
		}
	}

    public void ScreenShake( float duration, float strength, int vibrato, float randomness = 90, bool fadeout = true )
	{
		Main.DOShakePosition( duration, strength, vibrato, randomness, fadeout ).SetUpdate(UpdateType.Late);
    }

	public void ChangeCameraState(CameraState newState)
	{
		if( newState != _state )
		{
			CameraState prevState = _state;

			// On Disable Old State
			switch( _state )
			{
			case CameraState.FOLLOWPLAYER_FREE:				
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
				if( prevState != CameraState.SITTING)
				{
					_zoomInterp = ZOOM_RESETINTERP;	
				}			
				break;
			case CameraState.FOLLOWPLAYER_LOCKED:
				_camInputVals.x = 0.0f;	// So _camOffset lerps in zooming quack
				break;
			case CameraState.POND_RETURNPAN:
                StartCoroutine( PondReturnPan() );
				break;
			case CameraState.INTRO:
				StartCoroutine( PondIntroPan() );
				break;
			default:
				break;
			}

			Debug.Log("[CameraManager]: " + prevState.ToString() + " to " + newState.ToString());
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
				if(ControlManager.instance.getInput().RightStickButton.IsPressed && ControlManager.instance.getInput().RightStickButton.LastValue == 0.0f)
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
				DetermineCameraZoom( _zoomInterpCurve.Evaluate( _zoomInterp ) );

                // Determine if player in screen bounding circle and move focus center
                UpdateFocusPoint();

                // Move towards new focus center
				_mainCam.transform.position = Vector3.Lerp( _mainCam.transform.position, _focusPoint + _camOffset, CAM_FOLLOWSPEED * Time.fixedDeltaTime );
                if ( _mainCam.transform.position.y < CAMERA_MINY )
                {
                    _mainCam.transform.SetPosY( CAMERA_MINY );
                }

                // Rotate Around Camera around player if Right stick horizontal movement
                //      Done After b/c cam stutters if done before position change
                if ( Mathf.Abs(_camInputVals.x) > ZOOM_X_DEADZONE )
                {
                    _mainCam.transform.RotateAround( _focusPoint, Vector3.up, CAM_ROTSPEED * _camInputVals.x * Time.fixedDeltaTime );
                    _camOffset = _mainCam.transform.position - _focusPoint;
                }

				CameraLookAtFocusPoint();
			}

		}
	}

	void SittingCameraRotate()
	{
		_mainCam.transform.RotateAround( _focusPoint, Vector3.up, SITTING_ROTATESPEED * Time.fixedDeltaTime );
		_camOffset = _mainCam.transform.position - _focusPoint;
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
				DetermineCameraZoom( _zoomInterpCurve.Evaluate(_zoomInterp) );

				// Move towards new focus center
				_mainCam.transform.position = Vector3.Lerp(_mainCam.transform.position, _focusPoint + _camOffset, CAM_FOLLOWSPEED * Time.fixedDeltaTime);

				CameraLookAtFocusPoint();
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
	private void HandleTransitionMovement()
	{
	}

	/// <summary>
	/// Handles the pond return pan.
	/// </summary>
	private IEnumerator PondReturnPan()
	{
		PlayerManager.instance.Player.ControlManager.SetActiveController<InactiveController>();

        yield return new WaitForSeconds( RollerConstants.instance.RitualCompleteWait );

        Transform pondTransform = PondManager.instance.Pond.transform;
	    Vector3 desiredPos = ( pondTransform.forward * PONDRETURN_FORWARD ) + ( pondTransform.up * PONDRETURN_UP );

        Vector3 forward = ( pondTransform.position - ( desiredPos ) ).normalized;
        Quaternion desiredRot = Quaternion.LookRotation( forward, Vector3.up );

        Tween posTween = _mainCam.transform.DOMove( desiredPos, PONDRETURN_TRANSITIONTIME );
        Tween rotTween = _mainCam.transform.DORotateQuaternion( desiredRot, PONDRETURN_TRANSITIONTIME );

        yield return rotTween.WaitForCompletion();

        PositionCameraInFrontOfFocus();

        _zoomInterp = ZOOM_RESETINTERP;
        DetermineCameraZoom( _zoomInterp );

        _focusPoint = pondTransform.position;
        _focusOffset = _focusPoint;

		PlayerManager.instance.Player.ControlManager.SetActiveController<RollerController>();

        PondManager.instance.PopPlayerFromPond();
	}

	private IEnumerator PondIntroPan()
	{
		PlayerManager.instance.Player.ControlManager.SetActiveController<InactiveController>();

		yield return new WaitForSeconds(INTRO_PAN_PREWAIT_TIME);

		Transform pondTransform = PondManager.instance.Pond.transform;
		Vector3 desiredPos = ( pondTransform.forward * PONDRETURN_FORWARD ) + ( pondTransform.up * PONDRETURN_UP );

		Vector3 forward = ( pondTransform.position - ( desiredPos ) ).normalized;
        Quaternion desiredRot = Quaternion.LookRotation( forward, Vector3.up );

		Tween posTween = _mainCam.transform.DOMove( desiredPos, INTRO_PAN_TIME );
        Tween rotTween = _mainCam.transform.DORotateQuaternion( desiredRot, INTRO_PAN_TIME );

		yield return rotTween.WaitForCompletion();

		PositionCameraInFrontOfFocus();

        //_zoomInterp = ZOOM_RESETINTERP;
        //DetermineCameraZoom( _zoomInterp );

        _focusPoint = pondTransform.position;
        _focusOffset = _focusPoint;

		PlayerManager.instance.Player.ControlManager.SetActiveController<RollerController>();
	}

    /// <summary>
    /// Based on Murray's code from here: https://raw.githubusercontent.com/MurrayIRC/frog/master/Assets/Scripts/Actors/Player/PlayerCamera.cs
    /// </summary>
    /// <returns></returns>
    private void UpdateFocusPoint()
    {
		// Center is focusTransform.position
		Vector3 focusDir = _focusTransform.position - _focusPoint;
		float distance = focusDir.sqrMagnitude;

		if ( distance > JohnTech.Sqr( Mathf.Lerp( BOUNDING_RADIUS_MIN, BOUNDING_RADIUS_MAX, _zoomInterp ) ) /*|| Mathf.Abs( focusDir.x ) > BOUNDING_LATERALOFFSET || Mathf.Abs( focusDir.z ) > BOUNDING_VERTICALOFFSET */)
        {
			// Is outside of the circle. 
			Vector3 desiredPos = new Vector3( _focusTransform.position.x - _focusOffset.x, _focusPoint.y, _focusTransform.position.z - _focusOffset.z );
			_focusPoint = Vector3.Lerp(_focusPoint, desiredPos, CAM_CENTER_UPDATE_SPEED * Time.fixedDeltaTime);
        }
        else
        {
            // Calculate which direction camera should move if exit bounding circle on screen
			CalculateFocusOffset();
        }
    }

	private void CalculateFocusOffset()
	{
		_focusOffset.x = _focusTransform.position.x - _focusPoint.x;
		_focusOffset.y = _focusPoint.y;
		_focusOffset.z = _focusTransform.position.z - _focusPoint.z;

		_focusOffset.Normalize();

		_focusOffset *= Mathf.Lerp( BOUNDING_RADIUS_MIN, BOUNDING_RADIUS_MAX, _zoomInterp );
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

        // NOT CURRENTLY: Evaluating animation curve for FOV/Look height changes
        _fov = Mathf.Lerp( _fov, Mathf.Lerp( CAM_FOV_MAX, CAM_FOV_MIN, _fovCurve.Evaluate( zoomInterp ) ), ZOOM_DELTASPEED * Time.fixedDeltaTime );
        _mainCam.fieldOfView = _fov;

        _lookVertOffset = Mathf.Lerp( _lookVertOffset, Mathf.Lerp( CAMLOOK_VERTICALOFFSET_MAX, CAMLOOK_VERTICALOFFSET_MIN, _lookVertOffsetCurve.Evaluate( zoomInterp ) ), ZOOM_DELTASPEED * Time.fixedDeltaTime ); ;

		CalculateFocusOffset();
    }

	/// <summary>
	/// Sets Cam Offset behind focus transform.
	/// </summary>
	private void PositionCameraBehindFocus()
	{
		_camOffset = ( -_focusTransform.forward * Mathf.Lerp(zoomXRange.x, zoomXRange.y, _zoomInterp) ) + ( Vector3.up * Mathf.Lerp( zoomYRange.x, zoomYRange.y, _zoomInterp ) );
	}

    private void PositionCameraInFrontOfFocus()
    {
        _camOffset = ( _focusTransform.forward * Mathf.Lerp( zoomXRange.x, zoomXRange.y, _zoomInterp ) ) + ( Vector3.up * Mathf.Lerp( zoomYRange.x, zoomYRange.y, _zoomInterp ) );
    }

	/// <summary>
	/// Rotates Camera to look at the focus point.
	/// Takes into account any offsets to the focus point for the look direction
	/// </summary>
	private void CameraLookAtFocusPoint()
	{
		_mainCam.transform.LookAt( _focusPoint + ( Vector3.up * _lookVertOffset ) );
	}

	private void ResetCameraOffset()
	{
		PositionCameraBehindFocus();

		_zoomInterp = ZOOM_RESETINTERP;
		DetermineCameraZoom( _zoomInterp );
	}

    private void OnDrawGizmos()
    {
        if (_focusTransform != null)
        {
            Gizmos.color = Color.white;
			Gizmos.DrawWireSphere(_focusPoint, Mathf.Lerp( BOUNDING_RADIUS_MIN, BOUNDING_RADIUS_MAX, _zoomInterp ) );
            Gizmos.DrawLine(_focusPoint, _focusTransform.position);
            Gizmos.color = Color.blue;
            // Gizmos.DrawLine(_focusOffset, _focusTransform.position);
        }
    }

    private void HandeGameStateChanged(GameManager.GameState newState, GameManager.GameState prevState)
	{
	}
}

