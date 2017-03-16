using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigPlantPickupable : Pickupable {

	const float BIGPLANT_MINTUGDIST = 0.15f;	
	const float BIGPLANT_MAXTUGDIST = 1.76f;	
	
	Vector3 _grabberDirection = Vector3.zero;
	const float BIGPLANT_TUGANGLE_MAXOFFSET = 2.0f;
	Quaternion _tugDirection = Quaternion.identity;

    const float BIGPLANT_TUGANGLE_MAX = 0.12f;
    const float BIGPLANT_TUGANGLE_RETURNSPEED = 7.0f;


	void FixedUpdate()
	{
		if( _grabbed )
		{
			_grabberDirection = _grabTransform.position - this.transform.position; 

			_grabberBurdenInterp = Mathf.InverseLerp( BIGPLANT_MINTUGDIST, BIGPLANT_MAXTUGDIST, _grabberDirection.magnitude );

            // TODO: Make max angle be more determined by Plant Health
            _tugDirection = Quaternion.FromToRotation( Vector3.up, Vector3.Slerp( Vector3.up, _grabberDirection, Mathf.Lerp( 0.0f, BIGPLANT_TUGANGLE_MAX, _grabberBurdenInterp ) ) );
            
            transform.rotation = _tugDirection;
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

	}

	IEnumerator DelayedReleaseBigPlant()
	{
		while( Quaternion.Angle( this.transform.rotation, Quaternion.identity ) > 0.0f )
		{
			transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.identity, BIGPLANT_TUGANGLE_RETURNSPEED * Time.deltaTime);

			yield return 0;
		}

		this.transform.rotation = Quaternion.identity;

        if (this.GetComponent<PickupCollider>() != null)
        {
            this.GetComponent<PickupCollider>().LockedRotation = true;
        }
    }
}
