using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinMax
{
	float _min = 0.0f;
	public float Min { get { return _min; } set { _min = value; } }
	float _max = 0.0f;

	public float Max { get { return _max; } set { _max = value; } }

	public MinMax( float inMin, float inMax )
	{
		_min = inMin;
		_max = inMax;
	}

	public MinMax(){}
}
