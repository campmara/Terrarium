using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassBehaviour : PlantBehaviour 
{
	Animator _anim = null;
	[SerializeField] List<Color> _possibleColors = new List<Color>();

	void Awake()
	{
		int colorIndex = Random.Range( 0, _possibleColors.Count );
		GetComponent<SpriteRenderer>().color = _possibleColors[ colorIndex];
		_anim = GetComponent<Animator>();
		_anim.SetBool( "isSquishing", false );
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
