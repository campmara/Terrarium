using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Seed : Pickupable 
{

	[SerializeField] GameObject _plantPrefab = null;

	public override void OnPickup()
	{
		base.OnPickup();
	}

	public override void DropSelf()
	{
		base.DropSelf();
	}

	public void Plant()
	{
		GameObject newPlant = Instantiate( _plantPrefab, transform.position, Quaternion.identity ) as GameObject;
		PlantManager.instance.AddBigPlant( newPlant.GetComponent<Growable>()  );
		gameObject.SetActive(false);
	}
}
