using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigPlantPickupable : Pickupable {

	const float BIGPLANT_MINTUGDIST = 0.5f;	
	const float BIGPLANT_MAXTUGDIST = 2.0f;	
	
	Vector3 _grabberDirection = Vector3.zero;
	const float BIGPLANT_TUGANGLE_MAXOFFSET = 2.0f;
	Quaternion _tugDirection = Quaternion.identity;

	const float BIGPLANT_TUGANGLE_MAX = 10.0f;
	const float BIGPLANT_TUGANGLE_RETURNSPEED = 2.0f;

	void FixedUpdate()
	{
		if( _grabbed )
		{
			_grabberDirection = _grabTransform.position - this.transform.position; 

			_grabberBurdenInterp = Mathf.InverseLerp( BIGPLANT_MINTUGDIST, BIGPLANT_MAXTUGDIST, _grabberDirection.magnitude );

			_tugDirection = Quaternion.FromToRotation( Vector3.up, _grabberDirection.normalized );
			transform.rotation = Quaternion.Slerp( Quaternion.identity, _tugDirection, _grabberBurdenInterp );
		}
	}

	public override void OnPickup( Transform grabTransform )
	{
		_grabbed = true;
		_grabTransform = grabTransform;
				
		_grabberDirection = Vector3.zero;
		_tugDirection = Quaternion.identity;

		if( this.GetComponent<PickupCollider>() != null )
        {
            this.GetComponent<PickupCollider>().LockedRotation = false;
        }
	}

	public override void DropSelf()
	{
		_grabbed = false;
		_grabTransform = null;
				
		_grabberDirection = Vector3.zero;
		_tugDirection = Quaternion.identity;
		_grabberBurdenInterp = 0.0f;

		// Rotate plant back to being upright
		StartCoroutine( DelayedReleaseBigPlant() );

		if( this.GetComponent<PickupCollider>() != null )
        {
            this.GetComponent<PickupCollider>().LockedRotation = true;
        }
	}

	IEnumerator DelayedReleaseBigPlant()
	{
		while( Quaternion.Angle( this.transform.rotation, Quaternion.identity ) > 1.0f )
		{
			transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.identity, BIGPLANT_TUGANGLE_RETURNSPEED * Time.deltaTime);

			yield return 0;
		}

		this.transform.rotation = Quaternion.identity;
	}
}
