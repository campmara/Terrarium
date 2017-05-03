using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(AudioSource))]
public class PlantAudioController : MonoBehaviour {

    AudioSource _source = null;

    [SerializeField]
    AudioClip _plantGrowthClip = null;

    BPGrowthController _growthController = null;

    [SerializeField]
    AnimationCurve _growthSoundCurve;

	// Use this for initialization
	void Awake ()
    {
        _source = this.GetComponent<AudioSource>();
        _growthController = this.GetComponentInParent<BPGrowthController>();
	}
	
    void Update()
    {
        if( _source.isPlaying )
        {
            _source.volume = _growthSoundCurve.Evaluate( _growthController.CurPercentAnimated * 0.45f );

            if( _growthController.CurPercentAnimated >= 1.0f)
            {
                StopPlayGrowSound();
            }
        }
    }

    public void StartPlayingGrowSound()
    {
        StartCoroutine( DelayedStartPlaying() );
    }

    public void StopPlayGrowSound()
    {
        _source.Stop();
    }

    IEnumerator DelayedStartPlaying()
    {
        yield return new WaitUntil( () => _source != null );

        _source.clip = _plantGrowthClip;
        _source.Play();
    }
}
