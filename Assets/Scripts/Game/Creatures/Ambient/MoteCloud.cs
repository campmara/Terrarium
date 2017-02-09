using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoteCloud : AmbientCreature {

	ParticleSystem _particleSystem = null;

	const float CLOUD_RADIUSMIN = 7.5f;
	const float CLOUD_RADIUSMAX = 10.0f;

	Transform _focusTrans = null;
	float _focusDistance = 0.0f;
	int index = 0;
	const float PLAYER_CHECKRADIUS = 10.0f;

	const float SPAWN_YPOSITION = 5.0f;

	// Use this for initialization
	void Awake () 
	{
		_particleSystem = this.GetComponent<ParticleSystem>();
	}

	public override void InitializeCreature (Vector3 startPos)
	{
		this.transform.position = startPos;
		this.transform.SetPosY( SPAWN_YPOSITION );
	}

	void Update()
	{
		if( _focusTrans != null )
		{
			_focusDistance = (this.transform.position - _focusTrans.position).magnitude;
			if( _focusDistance > PLAYER_CHECKRADIUS )
			{				
				ParticleSystem.ShapeModule shape = _particleSystem.shape;
				shape.radius = CLOUD_RADIUSMIN;
				_focusTrans = null;
			}
		}
		else
		{
			Collider[] colArr = Physics.OverlapSphere( this.transform.position, PLAYER_CHECKRADIUS );

			if ( colArr.Length > 0 )
			{
				index = 0;
				while ( _focusTrans == null && index < colArr.Length )
				{
					if ( colArr[index].gameObject.GetComponent<Player>() )
					{
						_focusTrans = colArr[index].transform;
						ParticleSystem.ShapeModule shape = this.GetComponent<ParticleSystem.ShapeModule>();
						shape.radius = CLOUD_RADIUSMAX;
					}

					index++;
				}
			}
		}
	}


}
