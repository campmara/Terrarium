using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rock : Pickupable 
{
	[SerializeField] Vector2 _sizeRange = new Vector2( .2f, .8f );
	float _scale = 0.0f;
	void Start()
	{
		_scale = Random.Range( _sizeRange.x, _sizeRange.y );
		transform.localScale = new Vector3( _scale, _scale, _scale );
	}
	public override void DropSelf()
    {
		base.DropSelf();
		_rigidbody.constraints = RigidbodyConstraints.None;
    }
}
