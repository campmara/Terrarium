using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rock : Pickupable 
{
	Vector2 _sizeRange = new Vector2( .5f, .75f );
	float _scale = 0.0f;
	void Start()
	{
		_scale = Random.Range( _sizeRange.x, _sizeRange.y );
		transform.localScale = new Vector3( _scale, _scale, _scale );
		if( _grabTransform == null )
		{
			DropSelf();	
		}
		//DropSelf();	
	}
	public override void DropSelf()
    {
		base.DropSelf();
		_rigidbody.constraints = RigidbodyConstraints.None;
    }

	public override void OnPickup( Transform grabTransform )
	{
		if( _grabTransform != null )
		{
			if( _grabTransform.GetComponent<SeedSlug>() != null )
			{
				_grabTransform.GetComponent<SeedSlug>().OnPlayerPickup();
				this.transform.SetParent( grabTransform );
			}	
		}

		base.OnPickup( grabTransform );
	}

	protected override void OnCollisionEnter( Collision col)
	{
		Player player = col.gameObject.GetComponent<Player>();
		if( player )
		{
			if( !_grabbed )
			{
				float groundThreshold = 1.0f;
				if( transform.position.y > groundThreshold || player.GetComponent<RollerController>().State == P_ControlState.ROLLING )
				{
					player.GetComponent<RollerController>().HandleRockDropCollision();
				}
			}
		}
	}
}
