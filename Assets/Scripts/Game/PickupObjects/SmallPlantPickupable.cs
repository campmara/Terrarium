using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmallPlantPickupable : Pickupable {

	Vector3 _grabberDirection = Vector3.zero;

	const float SMALLPLANT_MINTUGDIST = 0.25f;
	const float SMALLPLANT_MAXTUGDIST = 1.0f;
	const float TUG_MINVAL = 0.9f;
	const float SMALLPLANT_TUGTIME = 0.25f;
	float _tugTimer = 0.0f;

	void FixedUpdate()
	{
		if( _grabbed )
		{
			_grabberDirection = _grabTransform.position - this.transform.position; 

			_grabberBurdenInterp = Mathf.InverseLerp( SMALLPLANT_MINTUGDIST, SMALLPLANT_MAXTUGDIST, _grabberDirection.magnitude );

			if( _grabberBurdenInterp > TUG_MINVAL )
			{
				_tugTimer += Time.deltaTime;
			}
			else if( _tugTimer > 0.0f ) // Reset timer if not bein tugged anymore
			{				
				_tugTimer = 0.0f;
			}
			
			if( _tugTimer > SMALLPLANT_TUGTIME)
			{
                // TODO: DESTROY ME 
                Debug.Log("SHould Drop Here");

                _grabbed = false;
			}
		}
	}

	public override void OnPickup( Transform grabTransform )
	{
		_grabbed = true;
		_grabTransform = grabTransform;

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

		if( this.GetComponent<PickupCollider>() != null )
        {
            this.GetComponent<PickupCollider>().LockedRotation = true;
        }		
	}

}
