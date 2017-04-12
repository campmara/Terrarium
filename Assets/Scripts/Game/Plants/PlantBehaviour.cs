using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantBehaviour : MonoBehaviour 
{
	public enum ScaleStatus : int
	{
		NotScaling = 0,
		ScalingUp,
		ScalingDown
	};

	const float _scaleDownRate = .22f;
	const float _scaleUpRate = .015f;
	protected float _minScale = 1.0f;
	const float _spinMax = 5.0f;
	const float _spinMin = 1.0f;

	float _spinSpeed = 0.0f;
	float _origScale = 0.0f;
	float _interpVal = 0.5f;
	float _curScale = 0.0f;

	ScaleStatus _status = ScaleStatus.NotScaling;

	void Start()
	{
		_origScale = transform.localScale.x;
		_spinSpeed = Random.Range( _spinMin, _spinMax );
	}

	void Update()
	{
		if( _status != ScaleStatus.NotScaling )
		{
			Scale();
		}

	}

	void Scale()
	{
		_interpVal = _status == ScaleStatus.ScalingUp ? _interpVal + _scaleUpRate : _interpVal - _scaleDownRate;
		_curScale = Mathf.Lerp( _minScale, _origScale, _interpVal );


		transform.localScale = new Vector3( transform.localScale.x, _curScale, transform.localScale.z );

		if( _status == ScaleStatus.ScalingDown )
		{
			if( _curScale == _minScale )
			{
				StopScaling();
			}
		}
		else
		{
			if( _curScale == _origScale )
			{
				StopScaling();
			}
		}
	}

	void StopScaling()
	{
		_status = ScaleStatus.NotScaling;
	}

	void OnTriggerEnter( Collider col ) 
	{
		if( col.GetComponent<Player>() )
		{
			ReactToPlayerEntrance();
		}
	}

	void OnTriggerExit( Collider col )
	{
		if( col.GetComponent<Player>() )
		{
			ReactToPlayerExit();
		}
	} 

	protected virtual void ReactToPlayerEntrance()
	{
		_status = ScaleStatus.ScalingDown;
	}

	protected virtual void ReactToPlayerExit()
	{
		_status = ScaleStatus.ScalingUp;
	}
}
