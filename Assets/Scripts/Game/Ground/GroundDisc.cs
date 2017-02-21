using System.Collections.Generic;
using UnityEngine;

public class GroundDisc : MonoBehaviour 
{
	const float PAINT_FADE_SPEED = 0.3f;

	public float ScaleFactor = 3f;

	[SerializeField] private GameObject _grassPrefab;
	[SerializeField] private int _grassDensity = 100;

	[ReadOnly, SerializeField] private GroundDecalPool decalPool;

	//const float TEXELS_PER_WORLD_UNIT = 6.4f;
	float TEXELS_PER_WORLD_UNIT = 0f;

	private MeshRenderer _mesh;

	private GameObject _grassParent;

	private Texture2D _splatTex;

	private static Color32 paintColor = new Color32(0, 0, 0, 0);
	private static Color32 baseColor = new Color32(0, 0, 0, 255);
	private Color32[] currentColors;

	public class PaintPixel
	{
		private int index;
		private float alpha;

		public int Index { get { return this.index; } }
		public float Alpha { get { return this.alpha; } set { this.alpha = value; } }

		public PaintPixel(int i, float a)
		{
			this.index = i;
			this.alpha = a;
		}
	}
	public class PaintPixelComparer : EqualityComparer<PaintPixel>
	{
		private IEqualityComparer<int> _c = EqualityComparer<int>.Default;

		public override bool Equals(PaintPixel l, PaintPixel r)
		{
			return _c.Equals(l.Index, r.Index);
		}

		public override int GetHashCode(PaintPixel rule)
		{
			return _c.GetHashCode(rule.Index);
		}
	}
	private HashSet<PaintPixel> pixelData;

	private void Awake()
	{
		_mesh = GetComponent(typeof(MeshRenderer)) as MeshRenderer;
	    _grassParent = new GameObject {name = "Grass"};
		decalPool = GetComponentInChildren(typeof(GroundDecalPool)) as GroundDecalPool;

		transform.localScale = new Vector3(ScaleFactor + 1f, 1f, ScaleFactor + 1f);
		_mesh.sharedMaterial.SetFloat("_ScaleFactor", ScaleFactor);

		/*
	    for (int i = 0; i < _grassDensity; i++)
		{
			SpawnRandomCover();
		}
		*/

		CreateSplatTexture();
	}

	private void Update()
	{
		//UpdateTexture();
	}

	private float FadeAlpha(float alpha)
	{
		return Mathf.Lerp(alpha, 255f, PAINT_FADE_SPEED * Time.deltaTime);
		//this.alpha = Color32.Lerp(paintColor, baseColor, PAINT_FADE_SPEED * Time.deltaTime).a;
		//this.alpha = Mathf.Lerp(this.alpha, 255f, PAINT_FADE_SPEED * Time.deltaTime);
	}

	private void UpdateTexture()
	{
		// Process and update the pixels.
		foreach (PaintPixel p in pixelData)
		{
			if (p.Alpha >= 255f)
			{
				p.Alpha = 255f;
				currentColors[p.Index] = baseColor;
				continue;
			}

			p.Alpha = FadeAlpha(p.Alpha);

			// Update the actual color array with the updated pixel data.
			currentColors[p.Index].a = (byte)p.Alpha;
		}

		// Clean up faded pixels.
		if (pixelData.Count > 0)
		{
			pixelData.RemoveWhere(p => p.Alpha >= 255f);
		}

		// Update the splat texture.
		_splatTex.SetPixels32(currentColors);
		_splatTex.Apply();
	}

	private void SpawnRandomCover()
	{
		Vector3 spawnPos = Random.insideUnitSphere * 5f * ScaleFactor;
		spawnPos.y = 0f;

	    while ((spawnPos - transform.position).magnitude < 3f)
	    {
	        spawnPos = Random.insideUnitSphere * 5f * ScaleFactor;
	        spawnPos.y = 0f;
	    }

		GameObject grass = Instantiate(_grassPrefab, spawnPos, Quaternion.identity);
		//grass.transform.GetChild(0).localScale = new Vector3(Random.Range(0.8f, 1.1f), Random.Range(0.8f, 1.1f), 0f);

		grass.transform.parent = _grassParent.transform;
	}

	public void DrawSplatDecal(Vector3 pos, float size)
	{
		decalPool.AddDecal(pos, size);
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
		int r2 = radius * radius;
		int area = r2 << 2;
		int rr = radius << 1;
		int width = _splatTex.width;
		//Color32 col = new Color32(0, 0, 0, 0);
		Vector2 diffFromCenter = Vector2.zero;
		float alpha = 0f;

		for (int i = 0; i < area; i++)
		{
			int tx = (i % rr) - radius;
			int ty = (i / rr) - radius;

			diffFromCenter.x = (float)tx;
			diffFromCenter.y = (float)ty;

			alpha = Mathf.Lerp(0f, 255f, diffFromCenter.sqrMagnitude / (float)r2);

			//col.a = (byte)alpha;

			if (tx * tx + ty * ty <= r2)
			{
				//currentColors[((cy + ty) * width) + (cx + tx)] = col;
				pixelData.Add(new PaintPixel(((cy + ty) * width) + (cx + tx), alpha));
			}
		}
	}

	private void CreateSplatTexture()
	{
		_splatTex = new Texture2D(512, 512, TextureFormat.Alpha8, true, true);
		_splatTex.filterMode = FilterMode.Point;
		TEXELS_PER_WORLD_UNIT = (float)_splatTex.width / ((ScaleFactor + 1f) * 10f);

		currentColors = new Color32[_splatTex.width * _splatTex.height];
		pixelData = new HashSet<PaintPixel>(new PaintPixelComparer());

		// Send to the shader.
		_mesh.sharedMaterial.SetTexture("_MainTex", _splatTex);

		ClearTexture();
	}

	private void ClearTexture()
	{
		for (int i = 0; i < _splatTex.width * _splatTex.height; i++)
		{
			currentColors[i] = baseColor;
		}

		_splatTex.SetPixels32(currentColors);
		_splatTex.Apply();
	}
}