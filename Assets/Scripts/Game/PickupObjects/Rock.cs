using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rock : Pickupable 
{
	public override void DropSelf()
    {
		base.DropSelf();

		_rigidbody.constraints = RigidbodyConstraints.None;
		//_rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
    }
}
