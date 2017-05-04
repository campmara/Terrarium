using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BumbleAnim : MonoBehaviour 
{
	[SerializeField] Vector2 _growRateRange = new Vector2( .1f, 1.0f );
	float _curScale = 0.0f;
	float _endScale = 1.0f;
	float _growthRate = 1.0f;

	void Awake()
	{
		_endScale = transform.localScale.x;
		_growthRate = Random.Range( _growRateRange.x, _growRateRange.y );
	}

	// Update is called once per frame
	void Update () 
	{
		if( _curScale <= _endScale )
		{
			_curScale += Time.deltaTime * _growthRate; 
			transform.localScale = new Vector3( _curScale, _curScale, _curScale );
		}
		else
		{
			Destroy( this );
		}
	}
}
