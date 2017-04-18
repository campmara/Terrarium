using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class WeatherManager : SingletonBehaviour<WeatherManager> {

    #region Wind Values

    // Determines how far inbetween each Min/Max value the wind is
    [SerializeField] float _windInterp = 0.0f;

    // World direction of wind
    [ReadOnlyAttribute, SerializeField] Vector3 _waveDir = Vector3.right;
	float _waveTime = 0.0f;

    // Force range applied to objects
    const float WINDFORCE_MIN = 0.0f;
    const float WINDFORCE_MAX = 5.0f;

    float _windForceScalar = 0.0f;
    public Vector3 WindForce { get { return _waveDir * _windForceScalar; } } 

    // Speed of wind oscillation
    const float WAVESPEED_MIN = 0.25f;
    const float WAVESPEED_MAX = 2.5f;

    // Scale of the noise in oscillation of wind
    const float WAVENOISE_MIN = 0.5f;
    const float WAVENOISE_MAX = 1.25f;

    // Scale of the oscillation of the wind
    const float WAVESCALE_MIN = 0.15f;
    const float WAVESCALE_MAX = 0.3f;

    // Amount wind effect is scaled overall
    const float WAVEAMOUNT_MIN = 0.25f;
    const float WAVEAMOUNT_MAX = 0.65f;

    // Different goal values for a wind loop
	float[] _windStateInterpValues = new float[] { 0.0f, /*0.25f, 0.5f, 0.75f, */1.0f };

    Coroutine _windWaitRoutine = null;
    Tween _windChangeTween = null;

    // How long of a wait there is between wind picking up again
    const float WINDTROUGH_WAITMIN = 2.0f;
    const float WINDTROUGH_WAITMAX = 10.0f;

    const float WINDPEAK_WAITMIN = 15.0f;
    const float WINDPEAK_WAITMAX = 20.0f;

    const float WINDRISE_MINTIME = 0.5f;
    const float WINDRISE_MAXTIME = 2.0f;

    const float WINDEND_MINTIME = 0.5f;
    const float WINDEND_MAXTIME = 2.0f;


    #endregion

    // Use this for initialization
    void Awake ()
    {
        
    }
    
    public override void Initialize()
    {
        UpdateWindShaderValues();
        UpdateWindDirection(0.0f);

        HandleWindWait( true );

        isInitialized = true;
    }

	void Update()
	{
		_waveTime = Time.deltaTime * Mathf.Lerp( WAVESPEED_MIN, WAVESPEED_MAX, _windInterp );
		Shader.SetGlobalFloat( "_WaveTime", _waveTime );
	}

    private void HandleWindEnterPeak()
    {
        StartCoroutine( WindChangeRoutine( true ) );
    }

    private void HandleWindEnterTrough()
    {
        StartCoroutine( WindChangeRoutine( false ) );
    }

    private void HandleWindWait( bool isTrough )
    {
        _windWaitRoutine = StartCoroutine( WindWaitRoutine( isTrough ) );
    }

    IEnumerator WindChangeRoutine( bool endTrough )
    {
		float timer = 0.0f;
		float duration = Random.Range( WINDRISE_MINTIME, WINDRISE_MAXTIME );
		float startInterp = _windInterp;
		float endInterp = endTrough ? Random.value/*_windStateInterpValues[Random.Range( 0, _windStateInterpValues.Length )] */: 0.0f;

		if( endTrough )
		{			
			UpdateWindDirection( duration );	
		}

		while ( timer < duration )
		{
			timer += Time.deltaTime;

			_windInterp = Mathf.Lerp( startInterp, endInterp, timer / duration );

			UpdateWindShaderValues();

			yield return 0;
		}

		_windInterp = endInterp;

		//_windInterp = _windStateInterpValues[Random.Range( 0, _windStateInterpValues.Length )];
		    
        yield return 0;
		        
        HandleWindWait( !endTrough );
    }

    IEnumerator WindWaitRoutine( bool isTrough )
    {
        // Checks if it is waiting in the trough/peak of a wind cycle
        // subsequently starts the cycle over based on which it is in
        if( isTrough )
        {
            yield return new WaitForSeconds( Random.Range( WINDTROUGH_WAITMIN, WINDTROUGH_WAITMAX ) );

            HandleWindEnterPeak();
        }
        else
        {
            yield return new WaitForSeconds( Random.Range( WINDPEAK_WAITMIN, WINDPEAK_WAITMAX ) );

            HandleWindEnterTrough();
        }

        _windWaitRoutine = null;

    }

    private void UpdateWindDirection( float duration )
    {
        StartCoroutine( DelayedUpdateWindDir( duration ) );
    }

    IEnumerator DelayedUpdateWindDir( float duration )
    {
        float timer = 0.0f;

        Vector3 startDir = _waveDir;

        Vector3 endDir = Vector3.zero;    
        endDir.x = Random.insideUnitCircle.x;        
        endDir.z = Random.insideUnitCircle.y;
        endDir.Normalize();

        while( timer < duration )
        {
            timer += Time.deltaTime;

            _waveDir = Vector3.Slerp( startDir, endDir, timer / duration );

            Shader.SetGlobalVector( "_WaveDir", _waveDir );

            yield return 0;
        }

        _waveDir = endDir;
        Shader.SetGlobalVector( "_WaveDir", _waveDir );
    }

    private void UpdateWindShaderValues()
    {
        _windForceScalar = Mathf.Lerp( WINDFORCE_MIN, WINDFORCE_MAX, _windInterp );

        Shader.SetGlobalFloat( "_WaveSpeed", Mathf.Lerp( WAVESPEED_MIN, WAVESPEED_MAX, _windInterp ) );
        Shader.SetGlobalFloat( "_WaveNoise", Mathf.Lerp( WAVENOISE_MIN, WAVENOISE_MAX, _windInterp ) );
        Shader.SetGlobalFloat( "_WaveScale", Mathf.Lerp( WAVESCALE_MIN, WAVESCALE_MAX, _windInterp ) );
        Shader.SetGlobalFloat( "_WaveAmount", Mathf.Lerp( WAVEAMOUNT_MIN, WAVEAMOUNT_MAX, _windInterp ) );
    }
}


//my hacky way of showing presets :'~( 
//a custom editor would be more elegant but it doesn't really matter
//if (presets == Presets.nopreset)
//{
//    return;
//}
//if (presets == Presets.minimal)
//{
//    _WaveSpeed = 0.5f;
//    _WaveNoise = 1f;
//    _WaveScale = 0.5f;
//    _WaveAmount = 0.25f;
//    return;
//}
//if (presets == Presets.breeze)
//{
//    _WaveSpeed = 1f;
//    _WaveNoise = 1f;
//    _WaveScale = 0.5f;
//    _WaveAmount = 0.5f;
//    return;
//}
//if (presets == Presets.windy)
//{
//    _WaveSpeed = 6f;
//    _WaveNoise = 2f;
//    _WaveScale = 0.15f;
//    _WaveAmount = 2f;
//    return;
//}
//if (presets == Presets.stormy)
//{
//    _WaveSpeed = 35f;
//    _WaveNoise = 15f;
//    _WaveScale = 0.05f;
//    _WaveAmount = 2.5f;
//    return;
//}

