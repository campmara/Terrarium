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

    int _currentSingClip = 0;
    float _singPitch = 0.0f;
    int _numVoices = 1;

    float _singStopTimer = 0.0f;

    const float SING_EFFECT_RADIUS = 10f;

	void Awake ()
    {
        _face = this.GetComponentInChildren<FaceManager>();
        _player = this.GetComponent<Player>();

        _numVoices = AudioManager.instance.GetSingingClipCount();

        StartCoroutine(ClipSwitchRoutine());
    }
	
	// Update is called once per frame
	void Update ()
    {
        HandleSinging();	
	}

    IEnumerator ClipSwitchRoutine()
    {
        yield return new WaitForSeconds(Random.Range(20f, 40f));

        _singPitch = Random.Range(0, _numVoices);

        StartCoroutine(ClipSwitchRoutine());
    }

    protected void HandleSinging()
    {
        // Y BUTTON
        if ( _state == SingState.SINGING )
        {
            //float desiredPitch = AudioManager.instance.GetCurrentMusicPitch();
            //_singPitch = Mathf.Lerp( _singPitch, desiredPitch, RollerConstants.instance.PitchLerpSpeed * Time.deltaTime );

            AudioManager.instance.PlaySing(_currentSingClip, Random.Range(0.25f, 2f));
            _face.Sing();
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
        if( _state != SingState.SINGING )
        {
            _state = SingState.SINGING;

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
                    singable = null;
                }
            }
        }
    }
}
