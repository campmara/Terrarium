using UnityEngine;

public class GroundDisc : MonoBehaviour 
{
	public float ScaleFactor = 3f;

	[SerializeField] private GameObject _grassPrefab;
	[SerializeField] private int _grassDensity = 100;

	private MeshRenderer _mesh;

	private GameObject _grassParent;

	private void Awake()
	{
		_mesh = GetComponent(typeof(MeshRenderer)) as MeshRenderer;

	    _grassParent = new GameObject {name = "Grass"};

	    for (int i = 0; i < _grassDensity; i++)
		{
			SpawnRandomCover();
		}
	}

	private void Update()
	{
		transform.localScale = new Vector3(ScaleFactor, 1f, ScaleFactor);
		_mesh.sharedMaterial.SetFloat("_ScaleFactor", ScaleFactor);
	}

	private void SpawnRandomCover()
	{
		Vector3 spawnPos = Random.insideUnitSphere * 5f * ScaleFactor;
		spawnPos.y = 0f;

	    while ((spawnPos - transform.position).magnitude < 2.5f)
	    {
	        spawnPos = Random.insideUnitSphere * 5f * ScaleFactor;
	        spawnPos.y = 0f;
	    }

		GameObject grass = Instantiate(_grassPrefab, spawnPos, Quaternion.identity);
		//grass.transform.GetChild(0).localScale = new Vector3(Random.Range(0.85f, 1.0f), Random.Range(0.85f, 1.0f), 0f);

		grass.transform.parent = _grassParent.transform;
	}
}
