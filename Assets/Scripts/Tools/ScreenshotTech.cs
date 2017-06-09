using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenshotTech : MonoBehaviour {

	Coroutine _screenshotRoutine = null;
	const float SCREENSHOT_TIMER = 30.0f;
	const string SCREENSHOT_INDEXSAVEKEY = "ScreenshotIndex";
	const string SCREENSHOT_SAVEFOLDERNAME = "Screenshots";
	const string POSTCARD_SAVEFOLDERNAME = "PostcardScreenshots";

    [SerializeField]
    bool _useOverlay = true;
    Coroutine _overlayScreenshotRoutine = null;
    [SerializeField] float _overlayDisableDelay = 0.0f;
    [SerializeField]
    float _overlayFadeTime = 0.1f;
    [SerializeField]
    float _overlayMaxAlphaValue = 0.75f;

	AudioSource _source = null;
	[SerializeField, Space(5)]
	AudioClip _screenshotSound = null;

	InputCollection input;

    // Use this for initialization
    void Awake () 
	{		
		_source = this.GetComponent<AudioSource>();
		_source.clip = _screenshotSound;

		#if !UNITY_EDITOR
		if( !Directory.Exists( Application.dataPath + "/" + SCREENSHOT_SAVEFOLDERNAME ) )
		{
			Directory.CreateDirectory( Application.dataPath + "/" + SCREENSHOT_SAVEFOLDERNAME );
		}
		if( !Directory.Exists( Application.dataPath + "/" + POSTCARD_SAVEFOLDERNAME ) )
		{
		Directory.CreateDirectory( Application.dataPath + "/" + POSTCARD_SAVEFOLDERNAME );
		}

        // Uncomment to start the game w/ Screenshots Enabled
		//_screenshotRoutine = StartCoroutine( DelayedCaptureScreenshot() );
		#else
		if( !Directory.Exists( Application.dataPath + "/../" + SCREENSHOT_SAVEFOLDERNAME ) )
		{
			Directory.CreateDirectory( Application.dataPath + "/../" + SCREENSHOT_SAVEFOLDERNAME );
		}
		if( !Directory.Exists( Application.dataPath + "/../" + POSTCARD_SAVEFOLDERNAME ) )
		{
			Directory.CreateDirectory( Application.dataPath + "/../" + POSTCARD_SAVEFOLDERNAME );
		}
		#endif
	}
	
	// Update is called once per frame
	void Update () 
	{
		//input = ControlManager.instance.getInput();
		if ( /*input.ShareButton.WasPressed*/ Input.GetKeyDown( KeyCode.P ) )
        {
			HandleScreenShot( 4, false );

			if( _useOverlay )   // Only does overlay routine if active 
            {
                if (_overlayScreenshotRoutine == null)
                {
                    _overlayScreenshotRoutine = StartCoroutine( CaptureOverlayRoutine() );
                }
            }
				            
        }
        else if ( Input.GetKeyDown( KeyCode.Alpha9 ) )
        {
            if (_screenshotRoutine != null)
            {
                StopCoroutine( _screenshotRoutine );
                _screenshotRoutine = null;
            }
            else
            {
                _screenshotRoutine = StartCoroutine( DelayedCaptureScreenshot() );
            }
        }
	}

	void HandleScreenShot( int screenshotDetail = 4, bool hasOverlay = true )
	{	
		#if UNITY_STANDALONE && !UNITY_EDITOR	
		if( hasOverlay )
		{
			Application.CaptureScreenshot( Application.dataPath + "/" + POSTCARD_SAVEFOLDERNAME + "/" + "Screenshot_" + System.DateTime.Now.ToString("MM_dd_yy_hhmm") + ".png", screenshotDetail );
		}
		else
		{
			Application.CaptureScreenshot( Application.dataPath + "/" + SCREENSHOT_SAVEFOLDERNAME + "/" + "Screenshot_" + System.DateTime.Now.ToString("MM_dd_yy_hhmm") + ".png", screenshotDetail );
		}

		#else
		if( hasOverlay )
		{
			Application.CaptureScreenshot( Application.dataPath + "/../" + POSTCARD_SAVEFOLDERNAME + "/" + "Screenshot_" + System.DateTime.Now.ToString("MM_dd_yy_hhmm") + ".png", screenshotDetail );
		}
		else
		{
			Application.CaptureScreenshot( Application.dataPath + "/../" + SCREENSHOT_SAVEFOLDERNAME + "/" + "Screenshot_" + System.DateTime.Now.ToString("MM_dd_yy_hhmm") + ".png", screenshotDetail );
		}
		#endif
	}

	IEnumerator DelayedCaptureScreenshot()
	{
		yield return new WaitForSeconds( SCREENSHOT_TIMER );

		//StartCoroutine( CaptureOverlayRoutine() );
		HandleScreenShot( 4, false );

		_screenshotRoutine = StartCoroutine( DelayedCaptureScreenshot() );
	}

	IEnumerator CaptureOverlayRoutine( int screenshotDetail = 4 )
	{
        float timer = 0.0f;
        Color overlayColor = new Color( 1.0f, 1.0f, 1.0f, 0.0f );

		UIManager.GetPanelOfType<PanelOverlay>().RandomizeScreenshotOverlay();

        while ( timer < _overlayFadeTime)
        {
            timer += Time.deltaTime;

            overlayColor.a = Mathf.Lerp( 0.0f, _overlayMaxAlphaValue, timer / _overlayFadeTime );
            UIManager.GetPanelOfType<PanelOverlay>().ScreenshotOverlay.color = overlayColor;

            yield return 0;
        }
        overlayColor.a = _overlayMaxAlphaValue;
        UIManager.GetPanelOfType<PanelOverlay>().ScreenshotOverlay.color = overlayColor;

		PlayScreenshotSound();

        yield return new WaitForEndOfFrame();

		HandleScreenShot( screenshotDetail );

        if( _overlayDisableDelay > 0.0f )
        {
            yield return new WaitForSeconds( _overlayDisableDelay );
        }
        else
        {
            yield return new WaitForEndOfFrame();
        }

        timer = 0.0f;

        while ( timer < _overlayFadeTime )
        {
            timer += Time.deltaTime;

            overlayColor.a = Mathf.Lerp( _overlayMaxAlphaValue, 0.0f, timer / _overlayFadeTime );
            UIManager.GetPanelOfType<PanelOverlay>().ScreenshotOverlay.color = overlayColor;

            yield return 0;
        }
        UIManager.GetPanelOfType<PanelOverlay>().ScreenshotOverlay.color = new Color( 1.0f, 1.0f, 1.0f, 0.0f );

        _overlayScreenshotRoutine = null;
	}

	void PlayScreenshotSound()
	{
		_source.Play();
	}
}
