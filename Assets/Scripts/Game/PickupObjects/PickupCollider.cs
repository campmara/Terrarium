using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupCollider : MonoBehaviour 
{
	[ReadOnly] public Pickupable parentPickupable;
	[SerializeField] bool _lockedRotation = true;

	void Awake()
	{
		parentPickupable = this.GetComponent(typeof(Pickupable)) as Pickupable;
	}

	void FixedUpdate()
	{
		// Always keep the trigger upright.
		if( _lockedRotation )
		{
			transform.rotation = Quaternion.identity;
		}
	}
}
