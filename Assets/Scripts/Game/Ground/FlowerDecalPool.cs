using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowerDecalPool : MonoBehaviour {

    private int dataIndex;
    private ParticleSystem decalSystem;
    private ParticleSystem.EmitParams[] decalParticles;

    float _flowerSpawnOffset = 0.45f;

    void Awake()
    {
        decalSystem = GetComponent( typeof( ParticleSystem ) ) as ParticleSystem;
        decalParticles = new ParticleSystem.EmitParams[decalSystem.main.maxParticles];

        dataIndex = 0;
    }

    public void AddDecal( Vector3 pos )
    {
        if (dataIndex >= decalSystem.main.maxParticles)
        {
            dataIndex = 0;
        }

        Vector3 spawnOffset = JohnTech.GenerateRandomXZDirection().normalized * Random.Range( _flowerSpawnOffset * 0.5f, _flowerSpawnOffset );
       
        decalParticles[dataIndex].position = pos + spawnOffset;

        //decalParticles[dataIndex].startColor = ColorManager.instance.flowerSplatDecalColor;

        decalSystem.Emit( decalParticles[dataIndex], 1 );

        dataIndex++;
    }
}
