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

    float _singPitch = 0.0f;

    float _singStopTimer = 0.0f;

	void Awake ()
    {
        _face = this.GetComponentInChildren<FaceManager>();
        _player = this.GetComponent<Player>();
    }
	
	// Update is called once per frame
	void Update ()
    {
        HandleSinging();	
	}

    protected void HandleSinging()
    {
        // Y BUTTON
        if ( _state == SingState.SINGING )
        {
            //float desiredPitch = AudioManager.instance.GetCurrentMusicPitch();
            //_singPitch = Mathf.Lerp( _singPitch, desiredPitch, RollerConstants.instance.PitchLerpSpeed * Time.deltaTime );

            AudioManager.instance.PlaySing(Random.Range(0.75f, 1.25f));
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
}
