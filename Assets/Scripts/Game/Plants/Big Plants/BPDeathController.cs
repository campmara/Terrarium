using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BPDeathController : PlantController 
{	
	[SerializeField] private ParticleSystem _essenceSystemPrefab;
	[SerializeField] private SkinnedMeshRenderer _essenceMesh;
	ParticleSystem _essenceParticleSystem;
	ParticleSystem.NoiseModule _essenceNoise;
	[SerializeField] float _waterDecayReturnTime = 20.0f;

	enum DeathState
	{
		Decaying,
		Fading
	}
	DeathState _curState = DeathState.Decaying;

	List<Material> _componentMaterials = new List<Material>();
	Color[] _originalColors = new Color[3];
	Color[] _interpColors = new Color[6];
	int[] _shaderIDs = new int[3];

	

	private float _fadeTime;
	private float _cutoffValue;
	
	public override void Init()
	{
		_myPlant = GetComponent<BasePlant>();

		if (_essenceSystemPrefab != null)
		{
			_essenceParticleSystem = Instantiate(_essenceSystemPrefab, transform.position, Quaternion.identity) as ParticleSystem;
			_essenceParticleSystem.Stop();
			_essenceNoise = _essenceParticleSystem.noise;
		}

		_controllerType = ControllerType.Death;
	}

	public override void StartState()
	{
		_myPlant.CurDecayRate = _myPlant.BaseDecayRate;

		GetComponentMaterials();

		_interpColors[0] = _originalColors[0]; 
		_interpColors[1] = _originalColors[1];
		_interpColors[2] = _originalColors[2];

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

		ParticleSystem.ShapeModule shape = _essenceParticleSystem.shape;
		if (_essenceMesh)
		{
			shape.skinnedMeshRenderer = _essenceMesh;
		}
		else
		{
			
			if (renderers[0])
			{
				shape.skinnedMeshRenderer = renderers[0];
			}
			else if (otherRenderers[0])
			{
				shape.meshRenderer = otherRenderers[0];
			}
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
		if( _curState == DeathState.Decaying )
		{
			Decay();
		}
		else
		{
			FadeEssence();
		}
	}

	protected virtual void Decay()
	{
		if( _myPlant.DeathTimer < _myPlant.DeathDuration )
		{
			// Increment the decay timer.
			_myPlant.DeathTimer += Time.deltaTime * _myPlant.CurDecayRate;
		}
		else
		{
			_curState = DeathState.Fading;

			ParticleSystem.MinMaxGradient essenceTrailColor = _essenceParticleSystem.trails.colorOverLifetime;
			essenceTrailColor.color = _componentMaterials[0].GetColor(_shaderIDs[1]);

			_essenceParticleSystem.Play();

			_cutoffValue = 0f;
			_fadeTime = Random.Range(5f, 7f);

			DOTween.To(()=> _cutoffValue, x=> _cutoffValue = x, 1f, _fadeTime)
				.SetEase(Ease.InExpo)
				.OnComplete(OnDeath);
		}
	}

	protected virtual void OnDeath()
	{
		// Fully decayed and therefore no longer visible.
		_essenceParticleSystem.Stop();
		_essenceParticleSystem.GetComponent<EssenceParticles>().MarkForDestroy(5f);
		

		PlantManager.instance.DeleteLargePlant( _myPlant.GetComponent<BPBasePlant>() );
	}

	void FadeEssence()
	{
		for (int i = 0; i < _componentMaterials.Count; i++)
		{
			_componentMaterials[i].SetFloat("_Dissolve", _cutoffValue);
		}

		if (_essenceParticleSystem != null)
		{
			_essenceNoise.strengthXMultiplier = WeatherManager.instance.WindForce.x;
			_essenceNoise.strengthYMultiplier = WeatherManager.instance.WindForce.y;
			_essenceNoise.strengthZMultiplier = WeatherManager.instance.WindForce.z;
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

	public override void TouchPlant(){}
	public override void GrabPlant(){}
	public override void StompPlant(){}

	void OnDestroy()
	{
		ColorManager.ExecutePaletteChange -= HandlePalatteChange;
	}

	void HandlePalatteChange( ColorManager.EnvironmentPalette newPalette, ColorManager.EnvironmentPalette prevPalette  )
	{		
		Debug.Log( "transitionin a dyin plant color ");
		BPBasePlant.BigPlantType _type = _myPlant.GetComponent<BPBasePlant>().PlantType;
		switch( _type )
		{
		case BPBasePlant.BigPlantType.POINT:
			StartCoroutine( DelayedTransitionPointColors( newPalette, prevPalette ) );
			break;
		case BPBasePlant.BigPlantType.FLOWERING:
			StartCoroutine( DelayedTransitionMossColors( newPalette.mossPlant ) );
			break;
		case BPBasePlant.BigPlantType.LEAFY:
			StartCoroutine( DelayedTransitionLeafyColors( newPalette, prevPalette ) );
			break;
		case BPBasePlant.BigPlantType.LIMBER:
			StartCoroutine( DelayedTransitionLimberColors( newPalette.limberPlant ) );
			break;
		case BPBasePlant.BigPlantType.PBUSH:
			StartCoroutine( DelayedTransitionPBushColors( newPalette.pointyBush ) );
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
				if( mat.name == "GroundLeaf" || mat.name == "GroundLeaf (Instance)" )	// TODO make a new leaf material ?
				{
					_interpColors[0] = Colorx.Slerp( prevPalette.leafyGroundPlantLeaf.Evaluate(0.0f), newPalette.leafyGroundPlantLeaf.Evaluate(0.0f), timer / transitionTime );
					_interpColors[1] = Colorx.Slerp( prevPalette.leafyGroundPlantLeaf.Evaluate(0.5f), newPalette.leafyGroundPlantLeaf.Evaluate(0.5f), timer / transitionTime );
					_interpColors[2] = Colorx.Slerp( prevPalette.leafyGroundPlantLeaf.Evaluate(1.0f), newPalette.leafyGroundPlantLeaf.Evaluate(1.0f), timer / transitionTime );

					mat.SetColor( _shaderIDs[0], _interpColors[0] );
					mat.SetColor( _shaderIDs[1], _interpColors[1] );
					mat.SetColor( _shaderIDs[2], _interpColors[2] );
				}
				else if( mat.name == "Fruit" || mat.name == "Fruit (Instance)" )
				{
					_interpColors[3] = Colorx.Slerp( prevPalette.leafyGroundPlantBulb.Evaluate(0.0f), newPalette.leafyGroundPlantBulb.Evaluate(0.0f), timer / transitionTime );
					_interpColors[4] = Colorx.Slerp( prevPalette.leafyGroundPlantBulb.Evaluate(0.5f), newPalette.leafyGroundPlantBulb.Evaluate(0.5f), timer / transitionTime );
					_interpColors[5] = Colorx.Slerp( prevPalette.leafyGroundPlantBulb.Evaluate(1.0f), newPalette.leafyGroundPlantBulb.Evaluate(1.0f), timer / transitionTime );

					mat.SetColor( _shaderIDs[0], _interpColors[3] );
					mat.SetColor( _shaderIDs[1], _interpColors[4] );
					mat.SetColor( _shaderIDs[2], _interpColors[5] );
				}
			}

			yield return 0;
		}
	}

	void LateUpdateLeafyColors()
	{
		foreach( Material mat in _componentMaterials )
		{
			if( mat.name == "GroundLeaf" || mat.name == "GroundLeaf (Instance)" )	// TODO make a new leaf material ?
			{
				mat.SetColor( _shaderIDs[0], _interpColors[0] );
				mat.SetColor( _shaderIDs[1], _interpColors[1] );
				mat.SetColor( _shaderIDs[2], _interpColors[2] );
			}
			else if( mat.name == "Fruit" || mat.name == "Fruit (Instance)" )
			{
				mat.SetColor( _shaderIDs[0], _interpColors[3] );
				mat.SetColor( _shaderIDs[1], _interpColors[4] );
				mat.SetColor( _shaderIDs[2], _interpColors[5] );
			}
		}			
	}

	IEnumerator DelayedTransitionLimberColors( Gradient newLimberGradient, float transitionTime = ColorManager.PALATTE_TRANSITIONTIME )
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
				mat.SetColor( _shaderIDs[0], Colorx.Slerp( topColor, newLimberGradient.Evaluate(0.0f), timer / transitionTime ) );
				mat.SetColor( _shaderIDs[1], Colorx.Slerp( midColor, newLimberGradient.Evaluate(0.5f), timer / transitionTime ) );
				mat.SetColor( _shaderIDs[2], Colorx.Slerp( botColor, newLimberGradient.Evaluate(1.0f), timer / transitionTime ) );;
			}

			yield return 0;
		}
	}

	IEnumerator DelayedTransitionPBushColors( Gradient newPBushGradient, float transitionTime = ColorManager.PALATTE_TRANSITIONTIME )
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
				mat.SetColor( _shaderIDs[0], Colorx.Slerp( topColor, newPBushGradient.Evaluate(0.0f), timer / transitionTime ) );
				mat.SetColor( _shaderIDs[1], Colorx.Slerp( midColor, newPBushGradient.Evaluate(0.5f), timer / transitionTime ) );
				mat.SetColor( _shaderIDs[2], Colorx.Slerp( botColor, newPBushGradient.Evaluate(1.0f), timer / transitionTime ) );;
			}

			yield return 0;
		}
	}
}

 