using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class PickupCollider : MonoBehaviour 
{
	[ReadOnly] public Pickupable parentPickupable;

	void Awake()
	{
		parentPickupable = GetComponentInParent(typeof(Pickupable)) as Pickupable;
	}

	void FixedUpdate()
	{
		// Always keep the trigger upright.
		transform.rotation = Quaternion.identity;
	}
}
