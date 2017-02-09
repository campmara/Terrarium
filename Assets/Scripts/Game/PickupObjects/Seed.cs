﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Seed : Pickupable 
{

	[SerializeField] GameObject _plantPrefab = null;
	float _timeSinceLastPickup = 0.0f;
	float _timePassedTillDestroy = 120.0f;
	bool _isPickedUp = false;
	const float _shootForce = 20.0f;
	const float _searchRadius = 30.0f;

	void Update()
	{
		if( !_isPickedUp )
		{
			if( _timeSinceLastPickup >= _timePassedTillDestroy )
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

		_rigidbody.AddForce( Vector3.up * _shootForce );
	}

	public void TryPlanting()
	{
		if( !FoundPlantsCloseBy() )
		{
			GameObject newPlant = Instantiate( _plantPrefab, transform.position, Quaternion.identity ) as GameObject;
			PlantManager.instance.AddBigPlant( newPlant.GetComponent<Growable>()  );
			gameObject.SetActive(false);
		}
	}

	bool FoundPlantsCloseBy()
	{
		Collider[] foundObjects = Physics.OverlapSphere( transform.position, _searchRadius );
		foreach( Collider col in foundObjects ) 
		{
			Plantable plant = col.gameObject.GetComponent<Plantable>();
			if( plant )
			{
				float distance = ( col.gameObject.transform.position - transform.position ).magnitude;
				if( distance <= plant.MinDistAway )
				{
					return true;
				}
			}
		}

		return false;
	}
}
