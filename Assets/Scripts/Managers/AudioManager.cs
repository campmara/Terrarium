using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

#region Audio Controller
/// <summary>
/// Audio controller used to setup Audio Sources in scene.
/// </summary>
/// <remarks>
/// Still needs delayed playback (needs to be made into monobehaviour for that)
/// Needs safe transform access for 3D sound.
/// </remarks>
[System.Serializable]
public class AudioController
{
	private AudioSource _source = null;

	[SerializeField] private AudioClip _audioClip;
	public AudioClip Clip { set { _audioClip = value; _source.clip = _audioClip; } }

	[SerializeField] private AudioMixerGroup _mixerGroup = null;
	public AudioMixerGroup MixerGroup { set { _mixerGroup = value; _source.outputAudioMixerGroup = _mixerGroup; } }

	[SerializeField, Range(0.0f, 1.0f)] private float _volume = 1.0f;
	public float Volume { set { _volume = value; _source.volume = _volume; } }

	[SerializeField] private bool _playOnAwake = false;
	public bool PlayOnAwake { set { _playOnAwake = value; _source.playOnAwake = _playOnAwake; } }

	[SerializeField] private bool _loop = false;
	public bool Loop { set { _loop = value; _source.loop = _loop; } }

	public AudioController()
	{
		_audioClip = null;
		_mixerGroup = null;
		_volume = 1.0f;
		_playOnAwake = false;
		_loop = false;
	}

	public void Initialize(AudioSource source)
	{
		_source = source;

		_source.clip = _audioClip;
		_source.volume = _volume;
		_source.playOnAwake = _playOnAwake;
		_source.loop = _loop;
	}

	#region AudioSource Methods

	public void StopAudioSource()
	{
		_source.Stop();
	}

	public void PauseAudioSource()
	{
		_source.Pause();
	}

	public void PlayAudioSource()
	{
		if(!_source.isPlaying)
		{
			_source.Play();	
		}
	}

	#endregion
}

#endregion

public class AudioManager : SingletonBehaviour<AudioManager> {

	public enum AudioControllerNames
	{
        MUSIC = 0
	}
	[SerializeField] private List<AudioController> _audioControllerList = new List<AudioController>();

	void Awake () 
	{
		SetupAudioControllers();
	}

	/// <summary>
	/// Setups the audio controllers based upon AudioControllerNames enum.
	/// Adds additional Audio Controllers if there are more enum values than in the list in editor.
	/// </summary>
	private void SetupAudioControllers()
	{
		int controllerCount = Enum.GetNames(typeof(AudioControllerNames)).Length;
		GameObject newChild = null;
		for (int i = 0; i < controllerCount; ++i)
		{
			if(i >= _audioControllerList.Count)
			{
				_audioControllerList.Add(new AudioController());
				Debug.LogWarning("AudioManager: Too Many Enum Values, Adding Additional Controller");
			}

			newChild = new GameObject();
			newChild.transform.SetParent(this.transform);
			newChild.name = ((AudioControllerNames) i).ToString() + "_AudioController";

			_audioControllerList[i].Initialize( newChild.AddComponent<AudioSource>() );
		}
	}

	public override void Initialize ()
	{
		MakeMeAPersistentSingleton();

        CalculateMusicTimeState();

        _audioControllerList[(int)AudioControllerNames.MUSIC].PlayAudioSource(); 

        isInitialized = true;
	}

	#region Controller Accessors

	public void SetControllerClip(AudioControllerNames controllerName, AudioClip clip)
	{
		_audioControllerList[(int) controllerName].Clip = clip;
	}

	public void SetControllerMixer(AudioControllerNames controllerName, AudioMixerGroup mixer)
	{
		_audioControllerList[(int) controllerName].MixerGroup = mixer;
	}

	public void SetControllerVolume(AudioControllerNames controllerName, float vol)
	{
		_audioControllerList[(int) controllerName].Volume = vol;
	}

	public void SetControllerPlayOnAwake(AudioControllerNames controllerName, bool play)
	{
		_audioControllerList[(int) controllerName].PlayOnAwake = play;
	}
		
	public void SetControllerLoop(AudioControllerNames controllerName, bool loop)
	{
		_audioControllerList[(int) controllerName].Loop = loop;
	}

	#endregion


    public enum MusicTimeState
    {
        SUNRISE = 0,
        MIDDAY,
        SUNSET
    }
    MusicTimeState _musicTimeState = MusicTimeState.SUNRISE;

    [SerializeField]
    private AudioClip[] _musicAudioClips;

    public MusicTimeState MusicTime { get { return _musicTimeState; } set { SetMusicTimeState(value); } }

    void SetMusicTimeState( MusicTimeState newTimeState )
    {
        _musicTimeState = newTimeState;

        SetControllerClip( AudioControllerNames.MUSIC, _musicAudioClips[(int)_musicTimeState] );        
    }

    void CalculateMusicTimeState()
    {
        int realWorldHour = TimeManager.instance.RealWorldNow.TimeOfDay.Hours;

        if( realWorldHour > 0 && realWorldHour < 10)
        {
            SetMusicTimeState( MusicTimeState.SUNRISE );
        }
        else if (realWorldHour > 11 && realWorldHour < 17)
        {
            SetMusicTimeState( MusicTimeState.MIDDAY );
        }
        else 
        {
            SetMusicTimeState( MusicTimeState.SUNSET );
        }
    }
}
