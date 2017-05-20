using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : SingletonBehaviour<PlayerManager>
{
    [SerializeField] GameObject _playerPrefab = null;

	[SerializeField] Player _player = null;
	public Player Player { get { return _player; } }

    public float DistanceFromPond { get { return Mathf.Abs(Vector3.Distance(transform.position, Vector3.zero)); } }

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

    public void PutPlayerInPond()
    {
        _player.transform.position = PondManager.instance.Pond.transform.position + ( Vector3.down * 3f );
        _player.transform.rotation = Quaternion.identity;
    }
}
