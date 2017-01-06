using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : SingletonBehaviour<CameraManager> {

	[SerializeField, ReadOnlyAttribute] private Camera _mainCam = null;
	public Camera Main { get { return _mainCam; } }

	public float CamPixelWidth { get { return _mainCam.pixelWidth; } }
	private float CamPixelHeight { get { return _mainCam.pixelHeight; } }

	public override void Initialize ()
	{
			
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
	}
	
	// Update is called once per frame
	void FixedUpdate () 
	{
		
	}
}
