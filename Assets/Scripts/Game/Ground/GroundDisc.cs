using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundDisc : MonoBehaviour 
{
	public float ScaleFactor = 3f;

	[SerializeField] private GameObject _grassPrefab;
	[SerializeField] private int _grassDensity = 100;

	//const float TEXELS_PER_WORLD_UNIT = 6.4f;
	float TEXELS_PER_WORLD_UNIT = 0f;

	private MeshRenderer _mesh;

	private GameObject _grassParent;

	private Texture2D _splatTex;

	private void Awake()
	{
		_mesh = GetComponent(typeof(MeshRenderer)) as MeshRenderer;

	    _grassParent = new GameObject {name = "Grass"};

	    for (int i = 0; i < _grassDensity; i++)
		{
			SpawnRandomCover();
		}

		CreateSplatTexture();
	}

	private void Update()
	{
		transform.localScale = new Vector3(ScaleFactor + 1f, 1f, ScaleFactor + 1f);
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
		grass.transform.GetChild(0).localScale = new Vector3(Random.Range(0.5f, 1.25f), Random.Range(0.5f, 1.25f), 0f);

		grass.transform.parent = _grassParent.transform;
	}

	// DYNAMIC TEXTURE STUFF
	public void DrawOnPosition(Vector3 center, float radius)
	{
		int x = Mathf.RoundToInt(-center.x * TEXELS_PER_WORLD_UNIT) + (_splatTex.width / 2);
		int y = Mathf.RoundToInt(-center.z * TEXELS_PER_WORLD_UNIT) + (_splatTex.height / 2);
		int r = Mathf.RoundToInt(radius);

		if (x < 0)
			x = 0;
		else if (x > _splatTex.width)
			x = _splatTex.width;

		if (y < 0)
			y = 0;
		else if (y > _splatTex.height)
			y = _splatTex.height;

		DrawCircle(x, y, r);
	}

	private void DrawCircle(int cx, int cy, int radius)
	{
		int x, y, px, nx, py, ny, d;
		Color32[] colors = _splatTex.GetPixels32();

		for (x = 0; x <= radius; x++)
		{
			d = (int)Mathf.Ceil(Mathf.Sqrt(radius * radius - x * x));
			for (y = 0; y <= d; y++)
			{
				px = cx + x;
				nx = cx - x;
				py = cy + y;
				ny = cy - y;

				byte val = (byte)(d * _splatTex.width);

				colors[py * _splatTex.width + px] = new Color32(0, 0, 0, val);
				colors[py * _splatTex.width + nx] = new Color32(0, 0, 0, val);
				colors[ny * _splatTex.width + px] = new Color32(0, 0, 0, val);
				colors[ny * _splatTex.width + nx] = new Color32(0, 0, 0, val);
			}
		}    
		_splatTex.SetPixels32(colors);
		_splatTex.Apply();
	}

	private void CreateSplatTexture()
	{
		_splatTex = new Texture2D(256, 256, TextureFormat.Alpha8, true, true);
		_splatTex.filterMode = FilterMode.Bilinear;
		TEXELS_PER_WORLD_UNIT = (float)_splatTex.width / ((ScaleFactor + 1f) * 10f);

		// Send to the shader.
		_mesh.sharedMaterial.SetTexture("_MainTex", _splatTex);

		//_splatTex.Apply();
		ResetTexture();
	}

	private void ResetTexture()
	{
		Color32[] colors = _splatTex.GetPixels32();
		for (int i = 0; i < _splatTex.width * _splatTex.height; i++)
		{
			colors[i] = new Color32(0, 0, 0, 255);
		}
		_splatTex.SetPixels32(colors);
		_splatTex.Apply();
	}
}