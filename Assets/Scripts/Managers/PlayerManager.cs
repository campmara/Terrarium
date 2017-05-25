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
        }
	}

    void Update()
    {
        if( isInitialized )
        {
            _playerDistanceInterp = Mathf.InverseLerp( 0f, _maxPlayerDistance, Mathf.Abs( Vector3.Distance( _player.transform.position, Vector3.zero ) ) );
        }
    }

    public void PutPlayerInPond()
    {
        _player.transform.position = PondManager.instance.Pond.transform.position + ( Vector3.down * 3f );
        _player.transform.rotation = Quaternion.identity;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawLine( Vector3.zero, Vector3.right * _maxPlayerDistance );
        Gizmos.DrawWireSphere( Vector3.right * _maxPlayerDistance, 5.0f );
    }
}
