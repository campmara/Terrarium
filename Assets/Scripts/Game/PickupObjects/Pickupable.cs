using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Pickupable : MonoBehaviour
{
    protected Rigidbody _rigidbody;

    protected bool _carried = false;
    public bool Carried { get { return _carried; } }

    protected virtual void Awake()
    {
        _rigidbody = GetComponent( typeof( Rigidbody ) ) as Rigidbody;
    }

    // This gets called when we pick up the object. Pickupable controls its own rigidbody.
    public virtual void OnPickup()
    {
        _carried = true;
        _rigidbody.useGravity = false;
        _rigidbody.isKinematic = true;
    }

    public virtual void DropSelf()
    {
        _carried = false;
        transform.parent = null;
        _rigidbody.useGravity = true;
        _rigidbody.isKinematic = false;
    }
}
