using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundDisc : MonoBehaviour 
{
	public float scaleFactor = 3f;

	[SerializeField] GameObject grassPrefab;
	[SerializeField] int grassDensity = 100;

	MeshRenderer renderer;

	GameObject grassParent;

	void Awake()
	{
		renderer = GetComponent(typeof(MeshRenderer)) as MeshRenderer;

		grassParent = new GameObject();
		grassParent.name = "Grass";

		for (int i = 0; i < grassDensity; i++)
		{
			SpawnRandomCover();
		}
	}

	void Update()
	{
		transform.localScale = new Vector3(scaleFactor, 1f, scaleFactor);
		renderer.sharedMaterial.SetFloat("_ScaleFactor", scaleFactor);
	}

	void SpawnRandomCover()
	{
		Vector3 spawnPos = Random.insideUnitSphere * 5f * scaleFactor;
		spawnPos.y = 0f;

	    while ((spawnPos - transform.position).magnitude < 2.5f)
	    {
	        spawnPos = Random.insideUnitSphere * 5f * scaleFactor;
	        spawnPos.y = 0f;
	    }

		GameObject grass = Instantiate(grassPrefab, spawnPos, Quaternion.identity);
		//grass.transform.GetChild(0).localScale = new Vector3(Random.Range(0.85f, 1.0f), Random.Range(0.85f, 1.0f), 0f);

		grass.transform.parent = grassParent.transform;
	}
}
