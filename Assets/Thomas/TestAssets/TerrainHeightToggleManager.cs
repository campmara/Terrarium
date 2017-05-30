using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainHeightToggleManager : MonoBehaviour 
{
	private Material terrainMat;

	private Player player;
	private float dist;
	private const float APPROX_POND_RADIUS = 4f;

	private void Awake()
	{
		terrainMat = (GetComponent(typeof(MeshRenderer)) as MeshRenderer).sharedMaterial;
		player = PlayerManager.instance.Player;

		terrainMat.SetFloat("_SplatmapEnabled", 0f);
	}

	private void Update()
	{
		if (player != null)
		{
			dist = player.transform.position.sqrMagnitude;
		}
		else
		{
			player = PlayerManager.instance.Player;
		}

		//Debug.Log("Dist: " + dist);
		
		if (dist > APPROX_POND_RADIUS * APPROX_POND_RADIUS)
		{
			terrainMat.SetFloat("_SplatmapEnabled", 1f);
		}
		else
		{
			terrainMat.SetFloat("_SplatmapEnabled", 0f);
		}
	}
}
