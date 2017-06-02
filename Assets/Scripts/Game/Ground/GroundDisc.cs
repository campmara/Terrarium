using System.Collections.Generic;
using UnityEngine;

public class GroundDisc : MonoBehaviour 
{
	const float PAINT_FADE_SPEED = 0.3f;

	[SerializeField] private GameObject _grassPrefab;

	[SerializeField] private GroundDecalPool waterSplatDecalPool;
    [SerializeField] private List<FlowerDecalPool> flowerSplatDecalPool;

    [SerializeField, Range( 0.0f, 1.0f )]
    float flowerSplatOdds = 0.25f; 

	private void Awake()
	{
	}

	public void DrawSplatDecal(Vector3 pos, float size)
	{
		pos.y = 0.007f;
        waterSplatDecalPool.AddDecal( pos, size );

        // TODO: add random circle offset from Drop Point
        // TODO: add a different method for BIg SPLATS when more flowers should spwan more quickly
        if( Random.value <= flowerSplatOdds )
        {
            DrawFlowerDecal( pos );            
        }        

    }

    public void DrawFlowerDecal( Vector3 pos )
    {
        flowerSplatDecalPool[Random.Range( 0, flowerSplatDecalPool.Count )].AddDecal( pos );
    }

}