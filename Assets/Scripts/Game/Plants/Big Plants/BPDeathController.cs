using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BPDeathController : PlantController 
{	
	// TODO REMOVE THIS ADDE MORE SCRIPTS GDI
	public enum BigPlantType : int 
	{
		NONE = -1,
		POINT,
		MOSS,
		LEAFY
	}
	[SerializeField] BigPlantType _type = BigPlantType.NONE;

	[SerializeField] Color[] _deathColors = new Color[3];
	[SerializeField] float _waterDecayReturnTime = 20.0f;

	DeathState _curState = DeathState.Dying;

	List<Material> _componentMaterials = new List<Material>();
	Color[] _originalColors = new Color[3];
	Color[] _interpColors = new Color[3];
	int[] _shaderIDs = new int[3];

	enum FlyState
	{
		PLUCK,
		SLOWDOWN,
		FLOAT
	}
	private FlyState _currentFlyState = FlyState.PLUCK;

	private Rigidbody _rb;
	private float _flyTimer = 0f;
	private float _flyPluckTime;

	[SerializeField] private float windEffect = 30f;

	const float PLUCK_FORCE = 17f;
	const float ASCEND_FORCE = 2.25f;

	const float PLUCK_MIN_TIME = 0.05f;
	const float PLUCK_MAX_TIME = 0.15f;
	const float SLOWDOWN_TIME = 1.1f;

	const float KILL_Y = 100f;

	enum DeathState
	{
		Flying,
		Dying
	}

	public override void Init()
	{
		_myPlant = GetComponent<BasePlant>();
		_rb = GetComponent<Rigidbody>();
		_controllerType = ControllerType.Death;

		_flyPluckTime = Random.Range(PLUCK_MIN_TIME, PLUCK_MAX_TIME);
	}

	public override void StartState()
	{
		_myPlant.CurDecayRate = _myPlant.BaseDecayRate;
		GetComponentMaterials();

		ColorManager.ExecutePaletteChange += HandlePalatteChange;
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
			GroundManager.instance.EmitDirtParticles(transform.position, 1f);
		}
	}

	protected virtual void FlyAway()
	{
		CheckForKill();
		HandleFlyStateChanges();

		_rb.isKinematic = false;
		Vector3 upDir = ( ( Vector3.up * 5f ) + ( WeatherManager.instance.WindForce ) ).normalized;

		// Apply an upward force.
		if ( _currentFlyState == FlyState.FLOAT )
		{
			_rb.AddForce( upDir * ASCEND_FORCE * Time.deltaTime, ForceMode.Impulse );
		}
		else if ( _currentFlyState == FlyState.PLUCK )
		{
			_rb.AddForce( upDir * PLUCK_FORCE * Time.deltaTime, ForceMode.Impulse );
		}
		else if ( _currentFlyState == FlyState.SLOWDOWN )
		{
			_rb.AddForce( upDir * Mathf.Lerp( PLUCK_FORCE, ASCEND_FORCE, _flyTimer / SLOWDOWN_TIME ) * Time.deltaTime, ForceMode.Impulse );
		}
		
		// Apply a weird constant random rotation.
		_rb.AddTorque(WeatherManager.instance.WindForce * windEffect * Time.deltaTime);
	}

	void CheckForKill()
	{
		if (transform.position.y > KILL_Y)
		{
			PlantManager.instance.DeleteLargePlant(_myPlant);
		}
	}

	void HandleFlyStateChanges()
	{
		if (_currentFlyState == FlyState.FLOAT)
		{
			return;
		}

		// increment the fly flyTimer
		_flyTimer += Time.deltaTime;

		if (_currentFlyState == FlyState.PLUCK && _flyTimer >= _flyPluckTime)
		{
			_currentFlyState = FlyState.SLOWDOWN;
			_flyTimer = 0f;
		}

		if (_currentFlyState == FlyState.SLOWDOWN && _flyTimer >= SLOWDOWN_TIME)
		{
			_currentFlyState = FlyState.FLOAT;
			_flyTimer = 0f;
		}
	}

	void FadeColor()
	{
		//iterate through all items and move their color
		//THIS IS HARD CODED AS HELL!
//		_interpColors[0] = Color.Lerp( _originalColors[0], _deathColors[0], _myPlant.DeathTimer / _myPlant.DeathDuration );
//		_interpColors[1] = Color.Lerp( _originalColors[1], _deathColors[1], _myPlant.DeathTimer / _myPlant.DeathDuration );
//		_interpColors[2] = Color.Lerp( _originalColors[2], _deathColors[2], _myPlant.DeathTimer / _myPlant.DeathDuration );
//
//		foreach( Material mat in _componentMaterials )
//		{
//			mat.SetColor( _shaderIDs[0], _interpColors[0] );
//			mat.SetColor( _shaderIDs[1], _interpColors[1] );
//			mat.SetColor( _shaderIDs[2], _interpColors[2] );
//		}
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

	public override void TouchPlant(){}
	public override void GrabPlant(){}
	public override void StompPlant(){}
	public override GameObject SpawnChildPlant()
	{
		//don't do SHEET!
		return null;
	}

	void OnDestroy()
	{
		ColorManager.ExecutePaletteChange -= HandlePalatteChange;
	}

	void HandlePalatteChange( ColorManager.EnvironmentPalette newPalette, ColorManager.EnvironmentPalette prevPalette  )
	{		
		Debug.Log( "transitionin a dyin plant color ");

		switch( _type )
		{
		case BigPlantType.POINT:
			StartCoroutine( DelayedTransitionPointColors( newPalette, prevPalette ) );
			break;
		case BigPlantType.MOSS:
			StartCoroutine( DelayedTransitionMossColors( newPalette.mossPlant ) );
			break;
		case BigPlantType.LEAFY:
			StartCoroutine( DelayedTransitionLeafyColors( newPalette, prevPalette ) );
			break;
		default:
			break;
		}
	}
		
	IEnumerator DelayedTransitionPointColors(ColorManager.EnvironmentPalette newPalette, ColorManager.EnvironmentPalette prevPalette, float transitionTime = ColorManager.PALATTE_TRANSITIONTIME )
	{
		float timer = 0.0f;

		while( timer < transitionTime )
		{
			timer +=  Time.deltaTime;

			foreach( Material mat in _componentMaterials )
			{
				if( mat.name == "GroundLeaf" )
				{
					mat.SetColor( _shaderIDs[0], Colorx.Slerp( prevPalette.pointPlantLeaf.Evaluate(0.0f), newPalette.pointPlantLeaf.Evaluate(0.0f), timer / transitionTime ) );
					mat.SetColor( _shaderIDs[1], Colorx.Slerp( prevPalette.pointPlantLeaf.Evaluate(0.5f), newPalette.pointPlantLeaf.Evaluate(0.5f), timer / transitionTime ) );
					mat.SetColor( _shaderIDs[2], Colorx.Slerp( prevPalette.pointPlantLeaf.Evaluate(1.0f), newPalette.pointPlantLeaf.Evaluate(1.0f), timer / transitionTime ) );
				}
				else if( mat.name == "GreenGradient")
				{
					mat.SetColor( _shaderIDs[0], Colorx.Slerp( prevPalette.pointPlantStem.Evaluate(0.0f), newPalette.pointPlantStem.Evaluate(0.0f), timer / transitionTime ) );
					mat.SetColor( _shaderIDs[1], Colorx.Slerp( prevPalette.pointPlantStem.Evaluate(0.5f), newPalette.pointPlantStem.Evaluate(0.5f), timer / transitionTime ) );
					mat.SetColor( _shaderIDs[2], Colorx.Slerp( prevPalette.pointPlantStem.Evaluate(1.0f), newPalette.pointPlantStem.Evaluate(1.0f), timer / transitionTime ) );
				}
			}

			yield return 0;
		}
	}

	IEnumerator DelayedTransitionMossColors( Gradient newGradient, float transitionTime = ColorManager.PALATTE_TRANSITIONTIME )
	{
		float timer = 0.0f;
		Color topColor = _componentMaterials[0].GetColor( _shaderIDs[0] );
		Color midColor = _componentMaterials[0].GetColor( _shaderIDs[1] );
		Color botColor = _componentMaterials[0].GetColor( _shaderIDs[2] );

		while( timer < transitionTime )
		{
			timer +=  Time.deltaTime;

			foreach( Material mat in _componentMaterials )
			{
				mat.SetColor( _shaderIDs[0], Colorx.Slerp( topColor, newGradient.Evaluate(0.0f), timer / transitionTime ) );
				mat.SetColor( _shaderIDs[1], Colorx.Slerp( midColor, newGradient.Evaluate(0.5f), timer / transitionTime ) );
				mat.SetColor( _shaderIDs[2], Colorx.Slerp( botColor, newGradient.Evaluate(1.0f), timer / transitionTime ) );;
			}

			yield return 0;
		}	
	}

	IEnumerator DelayedTransitionLeafyColors( ColorManager.EnvironmentPalette newPalette, ColorManager.EnvironmentPalette prevPalette, float transitionTime = ColorManager.PALATTE_TRANSITIONTIME ) 
	{
		float timer = 0.0f;

		while( timer < transitionTime )
		{
			timer +=  Time.deltaTime;

			foreach( Material mat in _componentMaterials )
			{
				if( mat.name == "GroundLeaf" )	// TODO make a new leaf material ?
				{
					mat.SetColor( _shaderIDs[0], Colorx.Slerp( prevPalette.leafyGroundPlantLeaf.Evaluate(0.0f), newPalette.leafyGroundPlantLeaf.Evaluate(0.0f), timer / transitionTime ) );
					mat.SetColor( _shaderIDs[1], Colorx.Slerp( prevPalette.leafyGroundPlantLeaf.Evaluate(0.5f), newPalette.leafyGroundPlantLeaf.Evaluate(0.5f), timer / transitionTime ) );
					mat.SetColor( _shaderIDs[2], Colorx.Slerp( prevPalette.leafyGroundPlantLeaf.Evaluate(1.0f), newPalette.leafyGroundPlantLeaf.Evaluate(1.0f), timer / transitionTime ) );
				}
				else if( mat.name == "Fruit" )
				{
					mat.SetColor( _shaderIDs[0], Colorx.Slerp( prevPalette.leafyGroundPlantBulb.Evaluate(0.0f), newPalette.leafyGroundPlantBulb.Evaluate(0.0f), timer / transitionTime ) );
					mat.SetColor( _shaderIDs[1], Colorx.Slerp( prevPalette.leafyGroundPlantBulb.Evaluate(0.5f), newPalette.leafyGroundPlantBulb.Evaluate(0.5f), timer / transitionTime ) );
					mat.SetColor( _shaderIDs[2], Colorx.Slerp( prevPalette.leafyGroundPlantBulb.Evaluate(1.0f), newPalette.leafyGroundPlantBulb.Evaluate(1.0f), timer / transitionTime ) );
				}
			}

			yield return 0;
		}
	}
}

