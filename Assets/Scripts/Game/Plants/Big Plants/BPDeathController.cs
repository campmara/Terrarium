using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BPDeathController : PlantController 
{	
	[SerializeField] Color[] _deathColors = new Color[3];

	DeathState _curState = DeathState.Dying;

	List<Material> _componentMaterials = new List<Material>();
	Color[] _originalColors = new Color[3];
	Color[] _interpColors = new Color[3];
	int[] _shaderIDs = new int[3];

	enum DeathState
	{
		Dying,
		Flying
	}

	void Awake()
	{
		_myPlant = GetComponent<BasePlant>();
		_controllerType = ControllerType.Death;
	}

	public override void StartState()
	{
		_myPlant.CurDecayRate = _myPlant.BaseDecayRate;
		GetComponentMaterials();
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

	public override void UpdateState()
	{
		if( _curState == DeathState.Dying )
		{
			Decay();
		}
		else
		{
			FlyAway();
		}
	}

	protected virtual void Decay()
	{
		if( _myPlant.DeathTimer < _myPlant.DeathDuration )
		{
			FadeColor();
			_myPlant.DeathTimer += Time.deltaTime * _myPlant.CurDecayRate;
		}
		else
		{
			_curState = DeathState.Flying;
		}
	}

	protected virtual void FlyAway()
	{
		//while rising
	}

	void FadeColor()
	{
		//iterate through all items and move their color
		//THIS IS HARD CODED AS HELL!
		_interpColors[0] = Color.Lerp( _originalColors[0], _deathColors[0], _myPlant.DeathTimer / _myPlant.DeathDuration );
		_interpColors[1] = Color.Lerp( _originalColors[1], _deathColors[1], _myPlant.DeathTimer / _myPlant.DeathDuration );
		_interpColors[2] = Color.Lerp( _originalColors[2], _deathColors[2], _myPlant.DeathTimer / _myPlant.DeathDuration );

		foreach( Material mat in _componentMaterials )
		{
			mat.SetColor( _shaderIDs[0], _interpColors[0] );
			mat.SetColor( _shaderIDs[1], _interpColors[1] );
			mat.SetColor( _shaderIDs[2], _interpColors[2] );
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
	}

	public override void TouchPlant(){}
	public override void GrabPlant(){}
	public override void StompPlant(){}
	public override GameObject SpawnChildPlant()
	{
		//don't do SHEET!
		return null;
	}
}
