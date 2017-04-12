using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GradientList : SingletonBehaviour<GradientList> {

	[SerializeField] List<Gradient> _gradientList = new List<Gradient>();

	public Gradient GetRandomGradient { get { return _gradientList[Random.Range(0, _gradientList.Count - 1)]; }}

	// Use this for initialization
	void Awake () 
	{
		
	}
	
}
