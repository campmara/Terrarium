using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassBehaviour : PlantBehaviour 
{
	Animator _anim = null;
	bool _isSquishing = false;


	void Awake()
	{
		_anim = GetComponent<Animator>();
		_anim.SetBool( "isSquishing", false );
		//_anim.runtimeAnimatorController.
	}
		
	protected override void ReactToPlayerEntrance()
	{
		_anim.SetBool( "isSquishing", true );

	}

	protected override void ReactToPlayerExit()
	{
		_anim.SetBool( "isSquishing", false );
	}
}
