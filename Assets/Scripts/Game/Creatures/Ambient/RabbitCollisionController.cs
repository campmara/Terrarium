using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RabbitCollisionController : MonoBehaviour {

	int colLayer;

	[SerializeField] RabbitMound parentMound = null;


	const float PLAYER_COLRADIUS = 6.0f;
	SphereCollider _collider = null;

	void Awake()
	{
		colLayer = LayerMask.NameToLayer("Player");
		_collider = this.GetComponent<SphereCollider>();
		_collider.radius = PLAYER_COLRADIUS;
	}

	void OnTriggerEnter( Collider other )
	{
		if( other.gameObject.layer == colLayer )
		{
			//parentMound.HandlePlayerEnter( other.gameObject.transform );
		}
	}
		
	void OnTriggerExit( Collider other )
	{
		if( other.gameObject.layer == colLayer )
		{
			//parentMound.HandlePlayerExit( other.gameObject.transform );
		}
	}
}
