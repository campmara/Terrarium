using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(AudioSource))]
public class PlantAudioController : MonoBehaviour {

    public enum PlantAudioState
    {
        NONE,
        GROWING,
        SHAKING
    }
    PlantAudioState _plantAudioState = PlantAudioState.NONE;

    AudioSource _source = null;

    [SerializeField]
    AudioClip _plantGrowthClip = null;

    BPGrowthController _growthController = null;
    BasePlant _basePlant = null;

    [SerializeField]
    AnimationCurve _growthSoundCurve;

    [SerializeField] AudioClip[] _shakeAudioClipArray;
    const float SHAKE_AUDIOSCALAR = 1.0f;

    // Use this for initialization
    void Awake ()
    {
        _source = this.GetComponent<AudioSource>();
        _basePlant = this.GetComponentInParent<BasePlant>();
        _growthController = this.GetComponentInParent<BPGrowthController>();

    }
	
    void Update()
    {
        if( _source.isPlaying )
        {
            if (_plantAudioState == PlantAudioState.GROWING )
            {
                _source.volume = _growthSoundCurve.Evaluate( _growthController.CurPercentAnimated * 0.45f );

                if ( _basePlant.ActiveController.ControlType != PlantController.ControllerType.Growth || _growthController.CurPercentAnimated >= 1.0f)
                {
                    StopPlayGrowSound();
                }
            }
            
        }
    }

    public void StartPlayingGrowSound()
    {
        StartCoroutine( DelayedStartPlantGrowth() );
    }

    public void StopPlayGrowSound()
    {
        _source.Stop();
        _plantAudioState = PlantAudioState.NONE;
    }

    IEnumerator DelayedStartPlantGrowth()
    {
        yield return new WaitUntil( () => _source != null );

        _source.clip = _plantGrowthClip;
        _source.Play();

        _plantAudioState = PlantAudioState.GROWING;
    }

    public void PlayPlantShakeSound()
    {
        _source.PlayOneShot( _shakeAudioClipArray[Random.Range( 0, _shakeAudioClipArray.Length - 1)], SHAKE_AUDIOSCALAR );
    }
}
