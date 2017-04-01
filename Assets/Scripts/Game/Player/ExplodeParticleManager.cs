using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplodeParticleManager : MonoBehaviour 
{
	private ParticleSystem pSystem;
    private List<ParticleCollisionEvent> collisionEvents;

	void Awake()
	{
		pSystem = GetComponent<ParticleSystem>();
        collisionEvents = new List<ParticleCollisionEvent>();
	}

	void OnParticleCollision(GameObject other) 
	{
		int numCollisionEvents = pSystem.GetCollisionEvents(other, collisionEvents);

		int i = 0;
        while (i < numCollisionEvents)
        {
			Vector3 pos = collisionEvents[i].intersection;
			float paintSize = Random.Range(1f, 3f);

			GroundManager.instance.Ground.DrawSplatDecal(pos, paintSize);
			WaterPlantsCloseBy(paintSize, pos);

            i++;
        }
    }

	void WaterPlantsCloseBy( float searchRadius, Vector3 pos )
	{
		Collider[] cols = Physics.OverlapSphere( pos, searchRadius );
		BasePlant plant = null;
		if( cols.Length > 0 )
		{
			foreach( Collider col in cols )
			{
				plant = col.GetComponent<BasePlant>();
				if( plant != null )
				{
					plant.WaterPlant();

					plant = null;
				}
			}
		}
	}
}
