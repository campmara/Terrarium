using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterDroplet : Pickupable 
{
	Material dropletMat;

	protected override void Awake()
	{
		base.Awake();

		dropletMat = GetComponentInChildren<MeshRenderer>().sharedMaterial;
	}

	// This gets called when we pick up the object. Pickupable controls its own rigidbody.
    public override void OnPickup()
    {
        base.OnPickup();
    }

    public override void DropSelf()
    {
        base.DropSelf();
    }
}
