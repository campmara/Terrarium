using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class windMan : MonoBehaviour
{
    private enum Presets { nopreset, minimal, breeze, windy, stormy };
    [SerializeField]
    private Presets presets;

    [SerializeField]
    private Vector3 _WaveDir = new Vector3(1, 0, 0);
    //the direction of the wind
    [SerializeField]
    private float _WaveSpeed = 1f;
    //the speed of oscillation in the wind
    [SerializeField]
    private float _WaveNoise = 1f;
    //the scale of the noise in the oscillation of the wind
    [SerializeField]
    private float _WaveScale = 0.5f;
    //the scale of the oscillation part of the wind
    [SerializeField]
    private float _WaveAmount = 0.5f;
    //how much the wind effect overall is applied

    void Update()
    {
        //this is the meat of this script
        //''''''''''''''''''''''''''''''''
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
            _WaveSpeed = 0.5f;
            _WaveNoise = 1f;
            _WaveScale = 0.5f;
            _WaveAmount = 0.25f;
            return;
        }
        if (presets == Presets.breeze)
        {
            _WaveSpeed = 1f;
            _WaveNoise = 1f;
            _WaveScale = 0.5f;
            _WaveAmount = 0.5f;
            return;
        }
        if (presets == Presets.windy)
        {
            _WaveSpeed = 6f;
            _WaveNoise = 2f;
            _WaveScale = 0.15f;
            _WaveAmount = 2f;
            return;
        }
        if (presets == Presets.stormy)
        {
            _WaveSpeed = 35f;
            _WaveNoise = 15f;
            _WaveScale = 0.05f;
            _WaveAmount = 2.5f;
            return;
        }
    }
}
