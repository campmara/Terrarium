using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SingController : MonoBehaviour {

    Player _player = null;
    public Player OwnerPlayer { get { return _player; } set { _player = value; } }

    FaceManager _face = null;

    public enum SingState : int
    {
        IDLE = 0,
        SINGING,
        STOPPING
    }
    SingState _state = SingState.IDLE;
    public SingState State { get { return _state; } }

    [SerializeField] AudioMixer _audioMixer = null;
    Vector2 _singVolumeRange = new Vector2( -40.0f, 0.0f );
    string _singVolParamName = "s_volume";
    float _singVolEnterSpeed = 25.0f; 

    int _currentSingClip = 0;
    int _numVoices = 0;

    [SerializeField]
    List<Vector2> _singPitchRangeList = new List<Vector2>();
    [SerializeField, ReadOnly]Vector2 _currSingPitchRange = Vector2.zero;
    [SerializeField]
    Vector2 _voiceChangeWaitRange = new Vector2( 60f, 120f );

    float _singStopTimer = 0.0f;
    [SerializeField, ReadOnly]float _stopVolume = 0.0f;

    const float SING_EFFECT_RADIUS = 10f;

    float _flowerSpawnTimer = 0.0f;
    [SerializeField] float _baseFlowerSpawnWait = 1.5f;
    [SerializeField] float _flowerSpawnWaitIncrease = 0.25f;
    [SerializeField, ReadOnly]float _currFlowerSpawnWait = 0.0f;

	void Awake ()
    {
        _face = this.GetComponentInChildren<FaceManager>();
        _player = this.GetComponent<Player>();

        _numVoices = AudioManager.instance.GetSingingClipCount();

        StartCoroutine(ClipSwitchRoutine());

        _currentSingClip = Random.Range( 0, _numVoices );
        _currSingPitchRange = _singPitchRangeList[Random.Range( 0, _singPitchRangeList.Count )];

        _singStopTimer = 0.0f;
        _audioMixer.SetFloat( _singVolParamName, _singVolumeRange.x );
        _stopVolume = _singVolumeRange.x;
    }

    // Update is called once per frame
    void Update ()
    {
        HandleSinging();	
	}

    IEnumerator ClipSwitchRoutine()
    {
        yield return new WaitForSeconds( Random.Range( _voiceChangeWaitRange.x, _voiceChangeWaitRange.y) );

        if( Random.value > 0.75f && _numVoices > 0 )
        {
            _currentSingClip = Random.Range( 0, _numVoices );
        }

        _currSingPitchRange = _singPitchRangeList[Random.Range( 0, _singPitchRangeList.Count )];

        StartCoroutine(ClipSwitchRoutine());
    }

    protected void HandleSinging()
    {
        if ( _state == SingState.SINGING )
        {
            //float desiredPitch = AudioManager.instance.GetCurrentMusicPitch();
            //_singPitch = Mathf.Lerp( _singPitch, desiredPitch, RollerConstants.instance.PitchLerpSpeed * Time.deltaTime );
            _flowerSpawnTimer += Time.deltaTime;

            if( _flowerSpawnTimer > _currFlowerSpawnWait )
            {
                _flowerSpawnTimer = 0.0f;
                _currFlowerSpawnWait += _flowerSpawnWaitIncrease;

                SpawnFlowerInRadius();
            }

            _audioMixer.GetFloat( _singVolParamName, out _stopVolume );
            _audioMixer.SetFloat( _singVolParamName, Mathf.Lerp( _stopVolume, _singVolumeRange.y, _singVolEnterSpeed * Time.deltaTime ) );

            if( !AudioManager.instance.IsSinging )
            {
                AudioManager.instance.PlaySing( _currentSingClip, Random.Range( _currSingPitchRange.x, _currSingPitchRange.y ) );
                _face.TransitionSingPose();
            }
            
        }
        else if ( _state == SingState.STOPPING )
        {
            _singStopTimer += Time.deltaTime;

            _audioMixer.SetFloat( _singVolParamName, Mathf.Lerp( _stopVolume, _singVolumeRange.x, _singStopTimer / RollerConstants.instance.SingingReturnTime ) );

            if ( _singStopTimer > RollerConstants.instance.SingingReturnTime )
            {
                AudioManager.instance.StopSing();

                EndSinging();
            }         
        }
    }

    public void BeginSinging()
    {       
        if ( _state != SingState.SINGING )
        {
            _state = SingState.SINGING;

            _face.TransitionSingPose();

            _flowerSpawnTimer = 0.0f;
            _currFlowerSpawnWait = _baseFlowerSpawnWait;

            CastSingSphere();
        }        
    }

    public void StopSinging()
    {
        if( _state == SingState.SINGING )
        {
            _state = SingState.STOPPING;

            _singStopTimer = 0.0f;
            _audioMixer.GetFloat( _singVolParamName, out _stopVolume );
        }
    }

    void EndSinging()
    {
        Debug.Log( "[SingController] End Singing" );

        _state = SingState.IDLE;

        //_audioMixer.SetFloat( _singVolParamName, _singVolumeRange.x );
        //_stopVolume = _singVolumeRange.x;

        _face.BecomeIdle();    
    }

    void CastSingSphere()
    {
        // do a spherecast and disrupt Singables
        Collider[] cols = Physics.OverlapSphere( transform.position, SING_EFFECT_RADIUS );
        Singable singable = null;
        if( cols.Length > 0 )
        {
            foreach( Collider col in cols )
            {
                singable = col.GetComponent<Singable>();
                if (singable != null)
                {
                    singable.OnAffectedBySinging();
                    
					if( singable.GetComponent<Bibi>() )
					{						
						_face.TransitionFacePose( "Bibi", true );
					}

					singable = null;
                }
            }
        }
    }

    void SpawnFlowerInRadius()
    {
        Vector3 spawnPos = transform.position;
        spawnPos.y = 0.0f;
        spawnPos += JohnTech.GenerateRandomXZDirection() * Random.Range( 1.0f, SING_EFFECT_RADIUS );

        GroundManager.instance.Ground.DrawFlowerDecal( spawnPos );
    }
}
