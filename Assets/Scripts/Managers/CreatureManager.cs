using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureManager : SingletonBehaviour<CreatureManager> {

    [SerializeField] List<Object> _creatureObjectList = new List<Object>();
    List<AmbientCreature> _spawnedCreatureList = new List<AmbientCreature>();

	// Use this for initialization
	void Awake ()
    {
		
	}

    public override void Initialize()
    {
        isInitialized = true;
    }

    public GameObject SpawnRandomCreature( Vector3 spawnPos )
    {
        AmbientCreature tmpCreature = null;
        AmbientCreature.CreatureType type = (AmbientCreature.CreatureType)Random.Range( 0, _creatureObjectList.Count );

        tmpCreature = _spawnedCreatureList.Find( x => x.CType == type && !x.gameObject.activeSelf );

        if( tmpCreature == null )
        {
            tmpCreature = (Instantiate( _creatureObjectList[(int) type], this.transform) as GameObject).GetComponent<AmbientCreature>();
        }
        else
        {
            tmpCreature.gameObject.SetActive( true );
        }

        tmpCreature.transform.position = spawnPos;

        tmpCreature.InitializeCreature( spawnPos );

        return tmpCreature.gameObject;
    }

    public void DestroyCreature( AmbientCreature creature )
    {
        creature.gameObject.SetActive( false );
    }
}
