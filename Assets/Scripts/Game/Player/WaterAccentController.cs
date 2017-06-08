using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class WaterAccentController : MonoBehaviour {

    AudioSource _source = null;
    AudioMixerGroup _mixer = null;

    const string waterAccentVolID = "waterAccent_volume";
    private FloatRange _waterAccentVolRange = new FloatRange( -80.0f, -17.0f );

	[SerializeField]AudioClip _walkingAccentClip = null;
	[SerializeField]AudioClip _rollingAccentClip = null; 

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

    public void SetWaterAccentPitch( float pitch )
    {
        _source.pitch = pitch;
    }

	public void SetWalkClip()
	{
		_source.clip = _walkingAccentClip;
		_source.Play();
	}

	public void SetRollClip()
	{
		_source.clip = _rollingAccentClip;
		_source.Play();
	}
}
