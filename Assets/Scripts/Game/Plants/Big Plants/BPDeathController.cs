using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BPDeathController : PlantController 
{	
	enum DeathState
	{
		REPAIRING,
		DECAYING
	}
	[SerializeField, ReadOnly] DeathState _curState = DeathState.DECAYING;

	[Header("Bubble Properties"), SerializeField] private GameObject _bubblePrefab;
	[SerializeField] private float _spawnRate = 0.9f;
	[SerializeField] private FloatRange _growthTimeRange = new FloatRange(7f, 12f);
	[SerializeField] private FloatRange _growthSizeRange = new FloatRange(0.25f, 0.45f);
	[SerializeField] private AnimationCurve _dissolveCurve;

	[Header("Other"), SerializeField] private float _waterDecayReturnTime = 20.0f;

	private const float REPAIR_TIME = 5f;
	private float _repairTimer = 0f;

	private List<Bubble> _bubbles;
	private List<Vector3> _verts;

	private Coroutine _bubbleSpawnRoutine;
	private float _dissolveValue;
	private float _dissolveReturnValue;
	private bool _readyToDie;

	// Color Handling
	List<Material> _componentMaterials = new List<Material>();
	Color[] _originalColors = new Color[3];
	Color[] _interpColors = new Color[6];
	int[] _shaderIDs = new int[3];
	
	public override void Init()
	{
		_myPlant = GetComponent<BasePlant>();
		_controllerType = ControllerType.Death;
	}

	public override void StartState()
	{
		_myPlant.CurDecayRate = _myPlant.BaseDecayRate;

		GetComponentMaterials();
		GetBubbleInfo();

		_interpColors[0] = _originalColors[0]; 
		_interpColors[1] = _originalColors[1];
		_interpColors[2] = _originalColors[2];

		ColorManager.ExecutePaletteChange += HandlePaletteChange;
	}

	void GetComponentMaterials()
	{
		SkinnedMeshRenderer[] renderers = GetComponentsInChildren<SkinnedMeshRenderer>();
		foreach( SkinnedMeshRenderer renderer in renderers )
		{
			_componentMaterials.Add( renderer.material );
		}

		MeshRenderer[] otherRenderers = GetComponentsInChildren<MeshRenderer>();
		foreach( MeshRenderer renderer in otherRenderers )
		{
			_componentMaterials.Add( renderer.material );
		}

		if( _componentMaterials.Count > 0 )
		{
			_shaderIDs[0] = Shader.PropertyToID( "_ColorTop");
			_shaderIDs[1] = Shader.PropertyToID( "_ColorMid");
			_shaderIDs[2] = Shader.PropertyToID( "_ColorBot");

			_originalColors[0] = _componentMaterials[0].GetColor( _shaderIDs[0] );
			_originalColors[1] = _componentMaterials[0].GetColor( _shaderIDs[1] );
			_originalColors[2] = _componentMaterials[0].GetColor( _shaderIDs[2] );
		}
	}

	private void GetBubbleInfo()
	{
		// Populate the list of filters and vertices.
		_bubbles = new List<Bubble>();
		_verts = new List<Vector3>();

		SkinnedMeshRenderer[] _skins = GetComponentsInChildren<SkinnedMeshRenderer>();
		for (int i = 0; i < _skins.Length; i++)
		{
			foreach (Vector3 vert in _skins[i].sharedMesh.vertices)
			{
				Vector3 transformedVert = _skins[i].transform.TransformPoint(vert);
				_verts.Add(transformedVert);
			}
		}

		// BACKUP! use meshrenderer
		if (_skins.Length == 0)
		{
			MeshFilter[] _filters = GetComponentsInChildren<MeshFilter>();
			for (int i = 0; i < _filters.Length; i++)
			{
				foreach (Vector3 vert in _filters[i].mesh.vertices)
				{
					Vector3 transformedVert = _filters[i].transform.TransformPoint(vert);
					_verts.Add(transformedVert);
				}
			}
		}
	}

	public override void UpdateState()
	{
		if( _curState == DeathState.REPAIRING )
		{
			HandleRepair();
		}
		else
		{
			HandleDecay();
		}
	}

	protected virtual void HandleRepair()
	{
		_repairTimer += Time.deltaTime;

		_dissolveValue = Mathf.Lerp(_dissolveReturnValue, 0f, _repairTimer / REPAIR_TIME);
		UpdateDissolveEffect();

		if (_repairTimer >= REPAIR_TIME)
		{
			// Begin the bubbling process.
			_dissolveValue = 0f;
			_bubbleSpawnRoutine = StartCoroutine(BubbleSpawnRoutine());
			_curState = DeathState.DECAYING;
		}
	}

	void HandleDecay()
	{
		if (_readyToDie) return;

		// Increment the decay timer.
		_myPlant.DeathTimer += Time.deltaTime * _myPlant.CurDecayRate;

		// Handle the dissolve effect.
		//_dissolveValue = Mathf.Lerp(0f, 1f, _deathTimer / _deathTime);
		_dissolveValue = _dissolveCurve.Evaluate(_myPlant.DeathTimer / _myPlant.DeathDuration);
		UpdateDissolveEffect();

		// Handle time checking.
		if (_myPlant.DeathTimer >= _myPlant.DeathDuration)
		{
			StopCoroutine(_bubbleSpawnRoutine);
			DropAllBubbles();
			_readyToDie = true;
		}
	}

	/*
		B U B B L I N G
	*/

	private IEnumerator BubbleSpawnRoutine()
	{
		yield return new WaitForSeconds(_spawnRate);

		Bloop();

		_bubbleSpawnRoutine = StartCoroutine(BubbleSpawnRoutine());
	}

	private void Bloop()
	{
		GameObject bubbleObj = Instantiate(_bubblePrefab, RandomPointOnPlant(), Quaternion.identity);

		Bubble bubble = bubbleObj.GetComponent(typeof(Bubble)) as Bubble;
		bubble.Setup(this,
					 Random.Range(_growthTimeRange.min, _growthTimeRange.max),
					 Random.Range(_growthSizeRange.min, _growthSizeRange.max),
					 _interpColors[0]);

		_bubbles.Add(bubble);
	}

	// Returns a random point on the mesh.
	private Vector3 RandomPointOnPlant()
	{
		return _verts[Random.Range(0, _verts.Count)];
	}

	private void DropAllBubbles()
	{
		for(int i = 0; i < _bubbles.Count; i++)
		{
			_bubbles[i].Drop();
		}
	}

	public void OnBubbleDestroyed(Bubble bubble)
	{
		_bubbles.Remove(bubble);

		// We check to kill the plant here. It'll only die if it's ready, once the death timer runs out.
		if (_readyToDie && _bubbles.Count == 0)
		{
			KillPlant();
		}
	}

	protected virtual void KillPlant()
	{
		// Delete me!!! Bye bye!!!
		PlantManager.instance.DeleteLargePlant( _myPlant.GetComponent<BasePlant>() );
	}

	private void UpdateDissolveEffect()
	{
		for (int i = 0; i < _componentMaterials.Count; i++)
		{
			_componentMaterials[i].SetFloat("_Dissolve", _dissolveValue);
		}
	}

	public override void StopState()
	{
		//destroy the object!!
	}
		
	public override void WaterPlant()
	{
		//stave off the death 
		_myPlant.CurDecayRate = _myPlant.WateredDecayRate;

		if( _myPlant.DecayReturnRoutine != null )
		{
			StopCoroutine( _myPlant.DecayReturnRoutine );
		}

		_myPlant.DecayReturnRoutine = StartCoroutine( _myPlant.DelayedReturnDecayRate( _waterDecayReturnTime ) );
	}

	// When we shake the plant, we should revert the death process. It'll still go back to bubbling eventually.
	public override void GrabPlant()
	{
		if (_curState == DeathState.REPAIRING) return;

		// Stop spawning bubbles.
		StopCoroutine(_bubbleSpawnRoutine);

		// Drop all the bubbles.
		DropAllBubbles();

		// Prepare for the decay process again.
		_readyToDie = false;
		_myPlant.DeathTimer = 0f;
		_repairTimer = 0f;
		_dissolveReturnValue = _dissolveValue;
		_curState = DeathState.REPAIRING;
	}

	public override void TouchPlant(){}
	public override void StompPlant(){}

	void OnDestroy()
	{
		ColorManager.ExecutePaletteChange -= HandlePaletteChange;
	}

	void HandlePaletteChange(ColorManager.EnvironmentPalette newPalette, ColorManager.EnvironmentPalette prevPalette)
	{

	}
}

 