using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickTest : MonoBehaviour 
{
	[SerializeField] GameObject seed = null;
	// Use this for initialization
	
	// Update is called once per frame
	void Update () 
	{
		//if yr clicking 
		if ( Input.GetMouseButtonDown(0))
		{
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);

			if (Physics.Raycast (ray, out hit, 100.0f))
			{
				Debug.Log(hit.collider.name);
				Plant newSeed = GameObject.Instantiate( seed, hit.point, Quaternion.identity ).GetComponent<Plant>();
				if(newSeed)
				{
					if( !newSeed.TryDrop() )
					{
						Destroy( newSeed.gameObject );
					}
				}
			}
		}
	}
}
