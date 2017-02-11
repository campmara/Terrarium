using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundManager : SingletonBehaviour<GroundManager> 
{
	[SerializeField] private GroundDisc _ground;
	public GroundDisc Ground { get { return _ground; } }

	public override void Initialize()
	{
		isInitialized = true;
	}

	void Awake()
	{
		if (_ground == null)
		{
			Debug.LogError("Please attach the ground to the GroundManager");
		}
	}
}
