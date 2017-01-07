﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : SingletonBehaviour<PlayerManager> {

	[SerializeField] Player _player = null;
	public Player Player { get { return _player; } }

	public override void Initialize ()
	{
		if( _player != null)
		{
			_player.Initialize();
		}

		isInitialized = true;
	}

	// Use this for initialization
	void Awake () 
	{
		if( _player == null )
		{
			Debug.LogError("No Player Prefab");
			// Should Spawn Player...	
		}
	}
	
	// Update is called once per frame
	void Update () 
	{
		
	}
}