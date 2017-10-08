using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenshotTech : MonoBehaviour {

	[SerializeField]
	bool _postToTwitter = false;
	Coroutine screenshotRoutine = null;

	WWW imageWWW = null;

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

	private void Start()
	{
		if(_postToTwitter)
		{
			SetupTwitter();
		}
	}

	// Update is called once per frame
	void Update () 
	{
		input = ControlManager.instance.getInput();
		if ( input.ShareButton.WasPressed )
        {
			if( _useOverlay )   // Only does overlay routine if active 
            {
                if (_overlayScreenshotRoutine == null)
                {
                    _overlayScreenshotRoutine = StartCoroutine( CaptureOverlayRoutine() );
                }
            }
			else
			{
				HandleScreenShot(4, false);
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

	#region Screenshot Handling

	void HandleScreenShot( int screenshotDetail = 4, bool hasOverlay = true )
	{
		string screenshotPath = "";

#if UNITY_STANDALONE && !UNITY_EDITOR
		if( hasOverlay )
		{
			screenshotPath = Application.dataPath + "/" + POSTCARD_SAVEFOLDERNAME + "/" + "Screenshot_" + System.DateTime.Now.ToString("MM_dd_yy_hhmmss") + ".png";
		}
		else
		{
			screenshotPath = Application.dataPath + "/" + SCREENSHOT_SAVEFOLDERNAME + "/" + "Screenshot_" + System.DateTime.Now.ToString("MM_dd_yy_hhmmss") + ".png";
		}

#else
		if( hasOverlay )
		{
			screenshotPath = Application.dataPath + "/../" + POSTCARD_SAVEFOLDERNAME + "/" + "Screenshot_" + System.DateTime.Now.ToString("MM_dd_yy_hhmmss") + ".png";
		}
		else
		{
			screenshotPath = Application.dataPath + "/../" + SCREENSHOT_SAVEFOLDERNAME + "/" + "Screenshot_" + System.DateTime.Now.ToString("MM_dd_yy_hhmmss") + ".png";
		}
#endif

		ScreenCapture.CaptureScreenshot(screenshotPath, screenshotDetail);

//#if UNITY_EDITOR
#if !UNITY_EDITOR
		if(_postToTwitter)
		{
			PostScreenshotToTwitter(screenshotPath);
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

#endregion

	void SetupTwitter()
	{
		StartCoroutine(TwitterSetupRoutine());
	}

	IEnumerator TwitterSetupRoutine()
	{
		yield return 0;
	}


	void PostScreenshotToTwitter(string screenshotPath)
	{
		if(screenshotRoutine == null)
		{
			screenshotRoutine = StartCoroutine(PostScreenshotRoutine(screenshotPath));
		}		
	}

	IEnumerator PostScreenshotRoutine(string screenshotPath)
	{
		const string CONSUMER_KEY = "8i1f3hMCs8x9lUoTrrNyKCOrS";
		const string CONSUMER_SECRET = "rdFCiJQy3hdZq1A6XwvZY3LT81bbC7MmHfxnCfwDgeEXXhY03v";
		const string ACCESS_TOKEN = "896068741221556224-gQy4VbHnoAPKh8bqomfKEKSG8wZMwa3";
		const string ACCESS_SECRET = "nv0VhAFiQN9Pb7w7O3Qi15B4pSOEk3n4Ktsei787dbSIC";
		const string tweetText = "Greetings From #ThatBloomingFeeling!";

		//UnityEngine.Debug.Log("Screenshot location=" + Application.persistentDataPath + "/Screenshot.png");
		Texture2D screenshotImage = new Texture2D(5120, 2880);
		string imageUri = "";
#if UNITY_STANDALONE_WIN
		imageUri = "file:///" + screenshotPath;
#elif UNITY_STANDALONE_OSX
		imageUri = "file://" + screenshotPath;
#endif

		yield return new WaitUntil(() => File.Exists(screenshotPath));

		imageWWW = new WWW(imageUri);

		yield return imageWWW;
		yield return new WaitUntil(() => imageWWW.isDone);

		yield return StartCoroutine(LoadImageIntoTextureRoutine(screenshotImage));
		
		yield return StartCoroutine(Twitter.API.PostTweet(screenshotImage.EncodeToPNG(), tweetText, CONSUMER_KEY, CONSUMER_SECRET, ACCESS_TOKEN, ACCESS_SECRET, ScreenshotSuccess));

		screenshotRoutine = null;
	}

	IEnumerator LoadImageIntoTextureRoutine(Texture2D image)
	{
		imageWWW.LoadImageIntoTexture(image);
		
		yield return 0;
	}

	void ScreenshotSuccess(bool success)
	{
		Debug.Log(success ? "Screenshot Succeeded" : "Screenshot Failed");
	}
}
