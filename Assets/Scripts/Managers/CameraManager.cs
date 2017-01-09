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
    private Vector2 _screenCenter = Vector2.zero;

	#region Player Camera Variables
	[Header("Player Cam Variables"), Space(5)]
	[SerializeField, ReadOnlyAttribute] Transform _focusTransform = null;

	Vector3 _camOffset = Vector3.zero;      // Direction from focus to Camera  
    
    Vector3 _focusCenter = Vector3.zero;    // Center of focal point following player
    Vector3 _focusOffset = Vector3.zero;    //
    float _centerDist = 0.0f;               // Current distance of focusCenter from transform
    const float CAM_FOLLOWSPEED = 6.5f;
    const float BOUNDING_RADIUS = 50.0f;         // Distance for player to move from center for cam focus to start following

    Vector2 _camInputVals = Vector2.zero;
	const float CAM_ROTSPEED = 65.0f;

	float _zoomInterp = ZOOM_RESETINTERP;
	const float ZOOM_RESETINTERP = 0.25f;
	const float ZOOM_SPEED = 0.3f;
	const float ZOOM_Y_DEADZONE = 0.1f;
	const float ZOOM_XDELTA = 1.0f;
    const float ZOOM_X_DEADZONE = 0.1f;
    Vector2 zoomXRange = new Vector2(4.0f, 10.0f);
	const float ZOOM_YDELTA = 2.0f;
    Vector2 zoomYRange = new Vector2(2.0f, 15.0f);
    const float ZOOM_DELTASPEED = 10.0f;    // Smooths out some rotation/zoom/refocus stuff

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
                if( ControlManager.instance.getInput().RightStickButton.IsPressed ) 
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
				DetermineCameraZoom();

                // Determine if player in screen bounding circle and move focus center
                UpdateFocusCenter();

                // Move towards new focus center                
                _mainCam.transform.position = Vector3.Lerp(_mainCam.transform.position, _focusCenter + _camOffset, CAM_FOLLOWSPEED * Time.fixedDeltaTime);

                // Rotate Around Camera around player if Right stick horizontal movement
                //      Done After b/c cam stutters if done before position change
                if (Mathf.Abs(_camInputVals.x) > ZOOM_X_DEADZONE)
                {
                    _mainCam.transform.RotateAround( _focusCenter, Vector3.up, CAM_ROTSPEED * _camInputVals.x * Time.fixedDeltaTime );
                    _camOffset = _mainCam.transform.position - _focusCenter;                   
                }

                _mainCam.transform.LookAt( _focusCenter );	
			}

		}
	}

    /// <summary>
    /// Based on Murray's code from here: https://raw.githubusercontent.com/MurrayIRC/frog/master/Assets/Scripts/Actors/Player/PlayerCamera.cs
    /// </summary>
    /// <returns></returns>
    private void UpdateFocusCenter()
    {
        Vector2 focusScreenPoint = _mainCam.WorldToScreenPoint(_focusTransform.position);
        Vector2 distance = focusScreenPoint - _screenCenter;

        if ( JohnTech.Sqr( distance.x ) + JohnTech.Sqr( distance.y ) > JohnTech.Sqr( BOUNDING_RADIUS ) )
        {
            // Is outside of the circle.         
            _focusCenter.x = _focusTransform.position.x - _focusOffset.x;
            _focusCenter.z = _focusTransform.position.z - _focusOffset.z;            
        }
        else
        {
            // Calculate which direction camera should move if exit bounding circle on screen
            _focusOffset.x = _focusTransform.position.x - _focusCenter.x;
            _focusOffset.y = _focusCenter.y;
            _focusOffset.z = _focusTransform.position.z - _focusCenter.z;
        }
    }

    /// <summary>
    /// Determines Position of Camera offset based on current direction from focus transform and the curr zoom value
    /// </summary>
	private void DetermineCameraZoom()
	{
		// Determine new x/z offset positions
		Vector2 lateralOffset = new Vector2( _camOffset.x, _camOffset.z ).normalized;	// Gets flat offset direction
		lateralOffset *= Mathf.Lerp( zoomXRange.x, zoomXRange.y, _zoomInterp );

		// Set new offset position
		_camOffset.x = Mathf.Lerp( _camOffset.x, lateralOffset.x, ZOOM_DELTASPEED * Time.fixedDeltaTime );
		_camOffset.y = Mathf.Lerp( _camOffset.y, Mathf.Lerp( zoomYRange.x, zoomYRange.y, _zoomInterp), ZOOM_DELTASPEED * Time.fixedDeltaTime );
		_camOffset.z = Mathf.Lerp( _camOffset.z, lateralOffset.y, ZOOM_DELTASPEED * Time.fixedDeltaTime );

    }

	private void ResetCameraOffset()
	{
		// Place camera behind player
		_mainCam.transform.position = _focusTransform.transform.position + ( -_focusTransform.forward * Mathf.Lerp(zoomXRange.x, zoomXRange.y, _zoomInterp));
        _mainCam.transform.SetPosY( Mathf.Lerp( zoomYRange.x, zoomYRange.y, _zoomInterp ) );
        _camOffset = _mainCam.transform.position - _focusCenter;

		_zoomInterp = ZOOM_RESETINTERP;
		DetermineCameraZoom();
	}

    private void OnDrawGizmos()
    {
        if (_focusTransform != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawCube(_focusCenter, Vector3.one);
            Gizmos.DrawLine(_focusCenter, _focusTransform.position);
            Gizmos.color = Color.blue;
            // Gizmos.DrawLine(_focusOffset, _focusTransform.position);
        }
    }

    private void HandeGameStateChanged(GameManager.GameState newState, GameManager.GameState prevState)
	{
	}
}
