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

    // Force range applied to objects
    const float WINDFORCE_MIN = 0.0f;
    const float WINDFORCE_MAX = 5.0f;

    float _windForceScalar = 0.0f;
    public Vector3 WindForce { get { return _waveDir * _windForceScalar; } } 

    // Speed of wind oscillation
    const float WAVESPEED_MIN = 0.5f;
    const float WAVESPEED_MAX = 6f;

    // Scale of the noise in oscillation of wind
    const float WAVENOISE_MIN = 1f;
    const float WAVENOISE_MAX = 2f;

    // Scale of the oscillation of the wind
    const float WAVESCALE_MIN = 0.5f;
    const float WAVESCALE_MAX = 0.15f;

    // Amount wind effect is scaled overall
    const float WAVEAMOUNT_MIN = 0.25f;
    const float WAVEAMOUNT_MAX = 2.5f;

    // Different goal values for a wind loop
    float[] _windStateInterpValues = new float[] { 0.25f, 0.5f/*, 0.75f, 1.0f */};

    Coroutine _windWaitRoutine = null;
    Tween _windChangeTween = null;

    // How long of a wait there is between wind picking up again
    const float WINDTROUGH_WAITMIN = 5.0f;
    const float WINDTROUGH_WAITMAX = 30.0f;

    const float WINDPEAK_WAITMIN = 5.0f;
    const float WINDPEAK_WAITMAX = 10.0f;

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
        UpdateWindDirection();

        //HandleWindWait( true );

        isInitialized = true;
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
        if ( endTrough )
        {
            // If ending the trough time to tween to Peak
            //_windChangeTween = DOTween.To( () => _windInterp, x => _windInterp = x, _windStateInterpValues[Random.Range( 0, _windStateInterpValues.Length )], Random.Range( WINDRISE_MINTIME, WINDRISE_MAXTIME ) );
            //_windChangeTween.SetEase( Ease.OutBack );

            _windInterp = _windStateInterpValues[Random.Range( 0, _windStateInterpValues.Length )];
        }
        else
        {
            // Else transition back to no wind
            //_windChangeTween = DOTween.To( () => _windInterp, x => _windInterp = x, 0.0f, Random.Range( WINDEND_MINTIME, WINDEND_MAXTIME ) );
            //_windChangeTween.SetEase( Ease.Linear );
            _windInterp = 0.0f;
        }

        //while ( _windChangeTween.IsPlaying() )
        //{
        //    UpdateWindShaderValues();
        //    yield return 0;
        //}

        UpdateWindShaderValues();
        UpdateWindDirection();

        yield return 0;

        //_windChangeTween.Kill();

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

    private void UpdateWindDirection()
    {
        _waveDir.x = Random.insideUnitCircle.x;
        _waveDir.y = 0.0f;  // zero out wind y for now
        _waveDir.z = Random.insideUnitCircle.y;
        _waveDir.Normalize();

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

