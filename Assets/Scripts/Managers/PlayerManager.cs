using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : SingletonBehaviour<PlayerManager>
{
    [SerializeField] GameObject _playerPrefab = null;

	[SerializeField] Player _player = null;
	public Player Player { get { return _player; } }

   
    [SerializeField, Range(0.0f, 1000.0f)]
    float _maxPlayerDistance = 78f;
    [SerializeField, ReadOnly]float _playerDistanceInterp = 0.0f;
    public float DistanceFromPond 
    { 
        get 
        {
            return _playerDistanceInterp;
        } 
    }

    public Vector3 DirectionFromPond
    {
      get
        {
            return _player.transform.position.normalized; 
        }
    }

    RollerController _rollerController = null;
    [SerializeField, Space( 5 )]
    AnimationCurve _splatSizeCurve;
    Vector3 MIN_SPLATSIZE = new Vector3( 2f, 2f, 3f );
    Vector3 MAX_SPLATSIZE = new Vector3( 25f, 25f, 3f );

    public override void Initialize ()
	{		
        _player.Initialize();

        PutPlayerInPond();

        isInitialized = true;
	}

	// Use this for initialization
	void Awake () 
	{
        if (_player == null)
        {
            Debug.Assert( _playerPrefab != null );

            GameObject newPlayer = Instantiate( _playerPrefab ) as GameObject;

            _player = newPlayer.GetComponent<Player>();
            _rollerController = _player.GetComponent<RollerController>();
        }
	}

    void Update()
    {
        if( isInitialized )
        {
            _playerDistanceInterp = Mathf.InverseLerp( 0f, _maxPlayerDistance, Mathf.Abs( Vector3.Distance( _player.transform.position, Vector3.zero ) ) );

            _rollerController.SplatImprint.transform.localScale = Vector3.Lerp( MIN_SPLATSIZE, MAX_SPLATSIZE, _splatSizeCurve.Evaluate( _playerDistanceInterp ) );
        }
    }

    public void PutPlayerInPond()
    {
        _player.transform.position = PondManager.instance.Pond.transform.position + ( Vector3.down * 3f );
        _player.transform.rotation = Quaternion.identity;
        _player.GetComponent<Rigidbody>().isKinematic = true;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawLine( Vector3.zero, Vector3.right * _maxPlayerDistance );
        Gizmos.DrawWireSphere( Vector3.right * _maxPlayerDistance, 5.0f );
    }
}
