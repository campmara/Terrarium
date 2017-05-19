using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Pickupable : MonoBehaviour
{
    protected Rigidbody _rigidbody;
    protected Transform _grabTransform;

    [SerializeField] protected bool _carryable = false;
    public bool Carryable { get { return _carryable; } }

    [SerializeField, ReadOnlyAttribute] protected bool _grabbed = false;
    public bool Grabbed { get { return _grabbed; } }

    [SerializeField, ReadOnlyAttribute]protected float _grabberBurdenInterp = 0.0f;
    public float GrabberBurdenInterp { get { return _grabberBurdenInterp; } set { _grabberBurdenInterp = Mathf.Clamp01( value ); } }

    float _lowVelocity = 6.5f;

    protected virtual void Awake()
    {
        _rigidbody = GetComponent( typeof( Rigidbody ) ) as Rigidbody;
    }

    // This gets called when we pick up the object. Pickupable controls its own rigidbody.
    public virtual void OnPickup( Transform grabTransform )
    {
        _grabbed = true;
        _grabTransform = grabTransform;
        _grabberBurdenInterp = 0.0f;
        _rigidbody.useGravity = false;
        _rigidbody.isKinematic = true;

        if( this.GetComponent<PickupCollider>() != null )
        {
            this.GetComponent<PickupCollider>().LockedRotation = false;
        }
    }

    public virtual void DropSelf()
    {
        _grabbed = false;
        _grabTransform = null;
        _grabberBurdenInterp = 0.0f;
        transform.parent = null;
        _rigidbody.useGravity = true;
        _rigidbody.isKinematic = false;

        if( this.GetComponent<PickupCollider>() != null )
        {
            this.GetComponent<PickupCollider>().LockedRotation = true;
        }
    }

    protected virtual void HandleCollision( Collision col )
    {
        _rigidbody.constraints = RigidbodyConstraints.FreezeRotation;//FreezeAll;
    }

    void OnCollisionEnter( Collision col )
	{
		//once you touch the ground 
		if( col.gameObject.GetComponent<GroundDisc>() || col.gameObject.GetComponent<GroundCover>() )
		{
			float vel = _rigidbody.velocity.magnitude;
			if( _rigidbody.velocity.magnitude <= _lowVelocity )
			{
				HandleCollision( col );//FreezeRotation;//All;
			}
		}
	}
}
