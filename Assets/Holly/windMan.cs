using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class windMan : MonoBehaviour
{
    private enum Presets { nopreset, minimal, breeze, windy, stormy };
    [SerializeField]
    private Presets presets;

    [SerializeField]
    private float lerpSpeed = 1f;

    [SerializeField, ReadOnlyAttribute]
    private float _WaveTime;

    [SerializeField, ReadOnlyAttribute]
    private Vector3 _WaveDir = new Vector3(1, 0, 0);
    [SerializeField]
    private Vector3 idealWaveDir = new Vector3(1, 0, 0);

    //the direction of the wind
    [SerializeField, ReadOnlyAttribute]
    private float _WaveSpeed = 1f;
    [SerializeField]
    private float idealWaveSpeed = 1f;

    //the speed of oscillation in the wind
    [SerializeField, ReadOnlyAttribute]
    private float _WaveNoise = 1f;
    [SerializeField]
    private float idealWaveNoise = 1f;

    //the scale of the noise in the oscillation of the wind
    [SerializeField, ReadOnlyAttribute]
    private float _WaveScale = 0.5f;
    [SerializeField]
    private float idealWaveScale = 0.5f;

    //the scale of the oscillation part of the wind
    [SerializeField, ReadOnlyAttribute]
    private float _WaveAmount = 0.5f;
    [SerializeField]
    private float idealWaveAmount = 0.5f;

    //how much the wind effect overall is applied

    void Update()
    {
        _WaveTime += Time.deltaTime * _WaveSpeed;

        float interpolation = Time.deltaTime * lerpSpeed;
        _WaveDir = Vector3.Lerp(_WaveDir, idealWaveDir, interpolation);
        _WaveSpeed = Mathf.Lerp(_WaveSpeed, idealWaveSpeed, interpolation);
        _WaveNoise = Mathf.Lerp(_WaveNoise, idealWaveNoise, interpolation);
        _WaveScale = Mathf.Lerp(_WaveScale, idealWaveScale, interpolation);
        _WaveAmount = Mathf.Lerp(_WaveAmount, idealWaveAmount, interpolation);


        //this is the meat of this script
        //''''''''''''''''''''''''''''''''
        Shader.SetGlobalFloat("_WaveTime", _WaveTime);
        Shader.SetGlobalVector("_WaveDir", _WaveDir);
        Shader.SetGlobalFloat("_WaveSpeed", _WaveSpeed);
        Shader.SetGlobalFloat("_WaveNoise", _WaveNoise);
        Shader.SetGlobalFloat("_WaveScale", _WaveScale);
        Shader.SetGlobalFloat("_WaveAmount", _WaveAmount);
        //................................

        //my hacky way of showing presets :'~( 
        //a custom editor would be more elegant but it doesn't really matter
        if (presets == Presets.nopreset)
        {
            return;
        }
        if (presets == Presets.minimal)
        {
            SetWind(0.5f, 1f, 0.5f, 0.25f);
            return;
        }
        if (presets == Presets.breeze)
        {
            SetWind(1f, 1f, 0.5f, 0.5f);
            return;
        }
        if (presets == Presets.windy)
        {
            SetWind(6f, 2f, 0.15f, 2f);
            return;
        }
        if (presets == Presets.stormy)
        {
            SetWind(35f, 15f, 0.05f, 2.5f);
            return;
        }
    }

    void SetWind(float speed, float noise, float scale, float amount)
    {
        idealWaveSpeed = speed;
        idealWaveNoise = noise;
        idealWaveScale = scale;
        idealWaveAmount = amount;
    }
}
