﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundGrowthController : PlantController
{
	Animator _anim = null;

	[SerializeField] float _lifetime = 10.0f;
	float _curTimer = 0.0f;
	bool _squishing = false;

	public override void Init()
	{
		_myPlant = GetComponent<BasePlant>();
		_controllerType = ControllerType.Growth;
		_anim = GetComponentInChildren<Animator>();
	}	

	public override void StartState(){}

	public override void StopState()
	{
		_myPlant.SwitchController( this );
	}

	public override void UpdateState()
	{
		if( _curTimer >= _lifetime )
		{
			StopState();
		}
		else
		{
			_curTimer += Time.deltaTime;
		}
	}
				
	void OnTriggerEnter( Collider col )
	{
		_squishing = true;
		StompPlant();
	}

	void OnTriggerExit( Collider col )
	{
		_squishing = false;
		StompPlant();
	}

	public override void StompPlant()
	{
		if( _squishing )
		{
			_anim.SetBool( "isSquishing", true );
		}
		else
		{
			_anim.SetBool( "isSquishing", false );
		}
	}

	public override void WaterPlant(){}
	public override void TouchPlant(){}
	public override void GrabPlant(){}

}
