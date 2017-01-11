using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterDroplet : Pickupable 
{
    [ReadOnly, SerializeField] Material dropletMat;

    Vector3 position;
    Vector3 prevPosition;

	protected override void Awake()
	{
		base.Awake();

		dropletMat = GetComponentInChildren<MeshRenderer>().sharedMaterial;

        position = transform.position;
        prevPosition = transform.position;
	}

    void Update()
    {
        prevPosition = position;
        position = transform.position;

        dropletMat.SetVector("_Position", position);
        dropletMat.SetVector("_PrevPosition", prevPosition);
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
