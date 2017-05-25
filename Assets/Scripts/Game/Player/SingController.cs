using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    int _currentSingClip = 0;
    int _numVoices = 0;

    [SerializeField]
    List<Vector2> _singPitchRangeList = new List<Vector2>();
    [SerializeField, ReadOnly]Vector2 _currSingPitchRange = Vector2.zero;
    [SerializeField]
    Vector2 _voiceChangeWaitRange = new Vector2( 20f, 40f );

    float _singStopTimer = 0.0f;

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
        // Y BUTTON
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

            AudioManager.instance.PlaySing( _currentSingClip, Random.Range( _currSingPitchRange.x, _currSingPitchRange.y) );
        }
        else if ( _state == SingState.STOPPING )
        {
            _singStopTimer += Time.deltaTime;

            AudioManager.instance.StopSing();

            if ( _singStopTimer < RollerConstants.instance.SingingReturnTime )
            {
                EndSinging();
            }         
        }
    }

    public void BeginSinging()
    {       
        if ( _state != SingState.SINGING )
        {
            Debug.Log( "Began Singing" );
            _state = SingState.SINGING;

			_face.TransitionFacePose( "Singing" );

            _flowerSpawnTimer = 0.0f;
            _currFlowerSpawnWait = _baseFlowerSpawnWait;

            CastSingSphere();
        }        
    }

    public void StopSinging()
    {
        if( _state != SingState.IDLE )
        {
            _state = SingState.STOPPING;
            _singStopTimer = 0.0f;
        }        
    }

    void EndSinging()
    {
        _state = SingState.IDLE;
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
