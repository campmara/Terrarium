using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class WaterAccentController : MonoBehaviour {

    AudioSource _source = null;
    AudioMixerGroup _mixer = null;

    const string waterAccentVolID = "waterAccent_volume";
    private FloatRange _waterAccentVolRange = new FloatRange( -80.0f, -17.0f );

	void Awake ()
    {
        _source = this.GetComponent<AudioSource>();
        _mixer = _source.outputAudioMixerGroup;
        
    }
	
	public void SetWaterAccentVolume( float velocityInterp )
    {
        if( _mixer != null )
        {
            _mixer.audioMixer.SetFloat( waterAccentVolID, Mathf.Lerp( _waterAccentVolRange.min, _waterAccentVolRange.max, velocityInterp ) );
        }
    }
}
