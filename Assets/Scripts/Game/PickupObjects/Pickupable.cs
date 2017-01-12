using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickupable : MonoBehaviour 
{
	protected Rigidbody rigidbody;

    protected virtual void Awake()
    {
        rigidbody = GetComponent(typeof(Rigidbody)) as Rigidbody;
    }

    // This gets called when we pick up the object. Pickupable controls its own rigidbody.
    public virtual void OnPickup()
    {
        rigidbody.useGravity = false;
        rigidbody.isKinematic = true;
    }

    public virtual void DropSelf()
    {
        transform.parent = null;
        rigidbody.useGravity = true;
        rigidbody.isKinematic = false;
    }
}
