using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickupable : MonoBehaviour 
{
	Rigidbody rigidbody;

    void Awake()
    {
        rigidbody = GetComponent(typeof(Rigidbody)) as Rigidbody;
    }

    // This gets called when we pick up the object. Pickupable controls its own rigidbody.
    public void OnPickup()
    {
        rigidbody.useGravity = false;
        rigidbody.isKinematic = true;
    }

    public void DropSelf()
    {
        transform.parent = null;
        rigidbody.useGravity = true;
        rigidbody.isKinematic = false;
    }
}
