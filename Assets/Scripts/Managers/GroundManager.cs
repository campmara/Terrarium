using UnityEngine;

public class GroundManager : SingletonBehaviour<GroundManager> 
{
	[SerializeField] private GroundDisc _ground;
	public GroundDisc Ground { get { return _ground; } }

	[SerializeField] private GameObject _mask;
	public GameObject Mask { get { return _mask; } }

	[SerializeField] private GameObject _dirtParticlePrefab;

	private MeshRenderer _groundMesh;
	private MeshRenderer _maskMesh;

	[SerializeField] float _scaleFactor = 4f;

	public override void Initialize()
	{
		isInitialized = true;
	}

	void Awake()
	{
		if (_ground == null)
		{
			Debug.LogError("Please attach the ground to the GroundManager");
		}
		if (_mask == null)
		{
			Debug.LogError("Please attach the ground mask to the GroundManager.");
		}

		_groundMesh = _ground.GetComponent(typeof(MeshRenderer)) as MeshRenderer;
		_maskMesh = _mask.GetComponent(typeof(MeshRenderer)) as MeshRenderer;

		_ground.transform.localScale = new Vector3(_scaleFactor + 1f, 1f, _scaleFactor + 1f);
		_mask.transform.localScale = new Vector3(_scaleFactor + 10f, 1f, _scaleFactor + 10f);

		_groundMesh.sharedMaterial.SetFloat("_ScaleFactor", _scaleFactor);
		_maskMesh.sharedMaterial.SetFloat("_ScaleFactor", _scaleFactor);
	}

	public void EmitDirtParticles(Vector3 pos, float size = 0.25f)
	{
		GameObject dirt = Instantiate(_dirtParticlePrefab, pos, Quaternion.identity) as GameObject;
		
		var module = dirt.GetComponentInChildren<ParticleSystem>().main;
		module.startSize = size;
	}
}
