using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Seed : Pickupable 
{
    [SerializeField] protected SeedAssetKey _assetKey = SeedAssetKey.NONE;
    public SeedAssetKey AssetKey { get { return _assetKey; } set { _assetKey = value; } }

    [SerializeField] GameObject _plantPrefab = null;
	float _timeSinceLastPickup = 0.0f;
	float _timePassedTillDestroy = 60.0f;
	bool _isPickedUp = false;
	const float _shootForce = 30.0f;
	const float _searchRadius = 30.0f;

	void Update()
	{
		if( !_isPickedUp )
		{
			if( _timeSinceLastPickup >= _timePassedTillDestroy && PlantManager.instance.GetActiveSeedCount() > 2 )
			{
				Destroy( gameObject );
			}
			else
			{
				_timeSinceLastPickup += Time.deltaTime;
			}
		}
	}

    public override void OnPickup()
	{
		base.OnPickup();
		_isPickedUp = true;
	}

	public override void DropSelf()
	{
		base.DropSelf();
		_timeSinceLastPickup = 0.0f;
		_isPickedUp = false;

	//	_rigidbody.AddForce( Vector3.up * _shootForce );
	}

	public void TryPlanting()
	{
		Vector3 plantPos = new Vector3( transform.position.x, 0.0f, transform.position.z ); 
		GameObject newPlant = Instantiate( _plantPrefab, plantPos, Quaternion.identity ) as GameObject; 
		PlantManager.instance.AddBigPlant( newPlant.GetComponent<Growable>()  );
		gameObject.SetActive(false);
	}
}
