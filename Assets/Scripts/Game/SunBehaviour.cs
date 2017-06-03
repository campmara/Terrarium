using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunBehaviour : MonoBehaviour 
{

	MinMax _xRotRange = new MinMax( 35.0f, 125.0f );
	MinMax _yRotRange = new MinMax( 0.0f, 360.0f );

	int _colorChangesPerCycle = 5;
	int _curCycle = 1;
	float _moveSpeed = .002f;
	float _curTime = 0.0f;  
	float _curXRot = 45.0f;
	float _curYRot = 0.0f;
	bool _inReverse = false;

	// Update is called once per frame
	void Update () 
	{
		MoveSun();
	}

	void MoveSun()
	{
		_curTime += Time.deltaTime * _moveSpeed;

		if(  Mathf.Abs( _curTime - ( 1.0f / _colorChangesPerCycle ) * _curCycle ) < .1f )
		{
			ColorManager.instance.SwitchPalette();
			_curCycle++;
		}

		if( _curTime >= 1.0f )
		{
			_curTime = 0.0f;
			_inReverse = !_inReverse;
			_curCycle = 1;
		}
		
		UpdateRotations();

		Quaternion newRot = Quaternion.Euler( _curXRot, _curYRot, 0.0f );
		transform.rotation = newRot;
	}

	void UpdateRotations()
	{
		if( _inReverse )
		{
			_curXRot = Mathf.Lerp( _xRotRange.Max, _xRotRange.Min, _curTime );
		}
		else
		{
			_curXRot = Mathf.Lerp( _xRotRange.Min, _xRotRange.Max, _curTime );
		}

		_curYRot = Mathf.Lerp( _yRotRange.Min, _yRotRange.Max, _curTime );
	}
}
