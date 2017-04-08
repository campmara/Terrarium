using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

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

	enum DeathState
	{
		Decaying,
		Fading
	}
	DeathState _curState = DeathState.Decaying;

	List<Material> _componentMaterials = new List<Material>();
	Color[] _originalColors = new Color[3];
	Color[] _interpColors = new Color[3];
	int[] _shaderIDs = new int[3];

	ParticleSystem _essenceParticleSystem;
	ParticleSystem.NoiseModule _essenceNoise;

	private float _fadeTime;
	private float _cutoffValue;
	
	public override void Init()
	{
		_myPlant = GetComponent<BasePlant>();

		if (GetComponentInChildren<ParticleSystem>())
		{
			_essenceParticleSystem = GetComponentInChildren<ParticleSystem>();
			_essenceNoise = _essenceParticleSystem.noise;
		}

		_controllerType = ControllerType.Death;

		_fadeTime = Random.Range(5f, 7f);
		_cutoffValue = 0f;
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

			_essenceParticleSystem.Play();

			DOTween.To(()=> _cutoffValue, x=> _cutoffValue = x, 1f, _fadeTime)
				.SetEase(Ease.InExpo)
				.OnComplete(OnDeath);
		}
	}

	protected virtual void OnDeath()
	{
		// Fully decayed and therefore no longer visible.
		_essenceParticleSystem.Stop();
		PlantManager.instance.DeleteLargePlant(_myPlant);
	}

	void FadeEssence()
	{
		Debug.Log("Fading Essence");

		for (int i = 0; i < _componentMaterials.Count; i++)
		{
			_componentMaterials[i].SetFloat("_Cutoff", _cutoffValue);
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

