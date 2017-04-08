using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenshotTech : MonoBehaviour {

	int _screenshotIndex = 0;

	Coroutine _screenshotRoutine = null;
	const float SCREENSHOT_TIMER = 30.0f;
	const string SCREENSHOT_INDEXSAVEKEY = "ScreenshotIndex";
	const string SCREENSHOT_SAVEFOLDERNAME = "Screenshots";

	// Use this for initialization
	void Awake () 
	{
		_screenshotIndex = PlayerPrefs.GetInt( SCREENSHOT_INDEXSAVEKEY );


		#if !UNITY_EDITOR
		if( !Directory.Exists( Application.dataPath + "/" + SCREENSHOT_SAVEFOLDERNAME ) )
		{
			Directory.CreateDirectory( Application.dataPath + "/" + SCREENSHOT_SAVEFOLDERNAME );
		}

		_screenshotRoutine = StartCoroutine( DelayedCaptureScreenshot() );
		#else
		if( !Directory.Exists( Application.dataPath + "/../" + SCREENSHOT_SAVEFOLDERNAME ) )
		{
			Directory.CreateDirectory( Application.dataPath + "/../" + SCREENSHOT_SAVEFOLDERNAME );
		}
		#endif
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (Input.GetKeyDown(KeyCode.P))
		{
			if( Input.GetKey( KeyCode.RightShift ) )
			{
				if( _screenshotRoutine != null )
				{
					StopCoroutine( _screenshotRoutine );
					_screenshotRoutine = null;
				}	
				else
				{
					_screenshotRoutine = StartCoroutine( DelayedCaptureScreenshot() );
				}
			}
			else
			{
				HandleScreenShot();	
			}
		}
	}

	void HandleScreenShot()
	{	
		#if UNITY_STANDALONE && !UNITY_EDITOR	
		Application.CaptureScreenshot(Application.dataPath + "/" + SCREENSHOT_SAVEFOLDERNAME + "/" + "Screenshot_" + _screenshotIndex.ToString() + ".png", 4);
		#else
		Application.CaptureScreenshot( Application.dataPath + "/../" + SCREENSHOT_SAVEFOLDERNAME + "/" + "Screenshot_" + _screenshotIndex.ToString() + ".png", 4);
		#endif
	}

	IEnumerator DelayedCaptureScreenshot()
	{
		yield return new WaitForSeconds( SCREENSHOT_TIMER );

		HandleScreenShot();

		_screenshotIndex++;
		PlayerPrefs.SetInt( SCREENSHOT_INDEXSAVEKEY, _screenshotIndex );

		_screenshotRoutine = StartCoroutine( DelayedCaptureScreenshot() );
	}
}
