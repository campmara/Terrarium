using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmallPlantPickupable : Pickupable {

	[SerializeField] GameObject _tugUpParticle = null;
	const float PARTICLE_YOFFSET = 0.25f;

	Vector3 _grabberDirection = Vector3.zero;

	const float SMALLPLANT_MINTUGDIST = 0.25f;
	const float SMALLPLANT_MAXTUGDIST = 1.2f;	// Max distance the player can move away while tugging
	const float TUG_MINVAL = 0.9f;	// How far away the player needs to be to be tugging the plant out of the ground
	const float SMALLPLANT_DEATHTUGTIME = 1.0f;
	float _tugTimer = 0.0f;

	void FixedUpdate()
	{
		if( _grabbed )
		{
			_grabberDirection = _grabTransform.position - this.transform.position; 

			_grabberBurdenInterp = Mathf.InverseLerp( SMALLPLANT_MINTUGDIST, SMALLPLANT_MAXTUGDIST, _grabberDirection.magnitude );

			// Gettin rid o this lol
//			if( _tugTimer > SMALLPLANT_DEATHTUGTIME)
//			{                
//				Instantiate<GameObject>( _tugUpParticle ).transform.position = this.transform.position + (Vector3.up * PARTICLE_YOFFSET);
//
//				PlantManager.instance.DeleteLargePlant( this.GetComponent<BasePlant>() );
//
//				DropSelf();
//			}
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


	public void IncrementTug()
	{
		if( _grabberBurdenInterp > TUG_MINVAL )
		{
			_tugTimer += Time.deltaTime;
		}
	}

	public void ResetTug()
	{
		_tugTimer = 0.0f;		
	}
}
