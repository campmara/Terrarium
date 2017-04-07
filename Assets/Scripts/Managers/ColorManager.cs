using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ColorManager : SingletonBehaviour<ColorManager> {

	[Serializable]
	public struct EnvironmentPalette
	{
		public string title;

		[Header("Ground colors"), Space(5)]
		public Color groundColorPrimary;
		public Color groundColorSecondary;
		public Color groundDecalTint;

		[Header("Terrain colors"), Space(5)]
		public Color terrainColor;

		[Header("Sky / Fog colors"), Space(5)]
		public Color fogColor;
		public Color skyColor;
		public Color cloudRimColor;

		// GRADIENTS GO TOP TO BOTTOM (0 is top, 1 is bottom)
		[Header("Plant colors"), Space(5)]
        [Header("Class 1")]
		public Gradient mossPlantSeed;
        public Gradient mossPlant;
		public Gradient pointPlantSeed;
        public Gradient pointPlantStem;
        public Gradient pointPlantLeaf;
		[Header("Class 2"), Space(5)]
        public Gradient leafyGroundPlantBulb;
        public Gradient leafyGroundPlantLeaf;
		[Header("Class 3"), Space(5)]
		public Gradient twistPlant;
		public Gradient cappPlant;
		
	}
		
	public static event Action<EnvironmentPalette> ExecutePaletteChange;

	[SerializeField] int _paletteIndex = 0;
	EnvironmentPalette _activePalette;
	public EnvironmentPalette ActivePalatte { get { return _activePalette; } }
	public const float PALATTE_TRANSITIONTIME = 10.0f;


	[SerializeField, Space(5)] List<EnvironmentPalette> _environmentPaletteList = new List<EnvironmentPalette>();
	public List<EnvironmentPalette> PalletteList { get { return _environmentPaletteList; } set { _environmentPaletteList = value; } }


	[Header("Global Materials"), Space(5)]
	[SerializeField] Material terrainMaterial;
	[SerializeField] ParticleSystem groundSplatDecal;

	[SerializeField] Material mossPlantMat;
	[SerializeField] Material mossPlantSeedMat;

	[SerializeField] Material pointPlantSeedMat;
	[SerializeField] Material pointPlantStemMat;
	[SerializeField] Material pointPlantLeafMat;

	[SerializeField] Material leafyGroundPlantBulbMat;
	[SerializeField] Material leafyGroundPlantLeafMat;

	[SerializeField] Material twistPlantMat;

	[SerializeField] Material cappPlantMat;


	void Awake () 
	{
	}

	public override void Initialize ()
	{

		isInitialized = true;
	}

    void AdvanceActivePalatte()
    {
        _paletteIndex++;
        if ( _paletteIndex >= _environmentPaletteList.Count )
        {
            _paletteIndex = 0;
        }

		UpdatePalatte( _paletteIndex );
    }

	void UpdatePalatte( int newPalatteIndex )
	{
		EnvironmentPalette prevPalatte = _activePalette;
		_activePalette = _environmentPaletteList[_paletteIndex];

		if( !Application.isPlaying )
		{
			//groundcolor
			Shader.SetGlobalColor("_GroundColorPrimary", _activePalette.groundColorPrimary );
			Shader.SetGlobalColor("_GroundColorSecondary", _activePalette.groundColorSecondary );
			Shader.SetGlobalColor("_GroundColorTint", _activePalette.groundDecalTint );

			if (groundSplatDecal != null )
			{
				ParticleSystem.MainModule groundDecalMain = groundSplatDecal.main;
				groundDecalMain.startColor = new Color( _activePalette.groundColorSecondary.r * _activePalette.groundDecalTint.r, _activePalette.groundColorSecondary.g * _activePalette.groundDecalTint.g, _activePalette.groundColorSecondary.b * _activePalette.groundDecalTint.b, 1.0f );
			}

			//skybox
			RenderSettings.fogColor = _activePalette.fogColor;
			RenderSettings.skybox.SetColor( "_Color1", _activePalette.cloudRimColor );
			RenderSettings.skybox.SetColor( "_Color2", _activePalette.skyColor );

			//terrain
			if ( terrainMaterial != null )
			{
				terrainMaterial.SetColor( "_Color", _activePalette.terrainColor );
			}


			ApplyThreePartGradient( mossPlantSeedMat, _activePalette.mossPlantSeed );
			ApplyThreePartGradient( mossPlantMat, _activePalette.mossPlant );
			ApplyThreePartGradient( pointPlantSeedMat, _activePalette.pointPlantSeed );			
			ApplyThreePartGradient( pointPlantLeafMat, _activePalette.pointPlantLeaf );
			ApplyThreePartGradient( pointPlantStemMat, _activePalette.pointPlantStem);
			ApplyThreePartGradient( leafyGroundPlantBulbMat, _activePalette.leafyGroundPlantBulb );
			ApplyThreePartGradient( leafyGroundPlantLeafMat, _activePalette.leafyGroundPlantLeaf );

			ApplyTwoPartGradient( twistPlantMat, _activePalette.twistPlant );			
			ApplyTwoPartGradient( cappPlantMat, _activePalette.pointPlantLeaf );

			if (ExecutePaletteChange != null)
			{
				ExecutePaletteChange( _activePalette );
			}   
		}
		else
		{
			// TODO TRANSITION VIA AN OVERARCHING LERP BETWEEN EVERYTHING IN THE PALATTE
			//Debug.Log( "Transitioning Colors" );

			GeneralTransitionColors( prevPalatte );

			TransitionThreePartGradient( mossPlantSeedMat, _activePalette.mossPlantSeed );
			TransitionThreePartGradient( mossPlantMat, _activePalette.mossPlant );
			TransitionThreePartGradient( pointPlantSeedMat, _activePalette.pointPlantSeed );			
			TransitionThreePartGradient( pointPlantLeafMat, _activePalette.pointPlantLeaf );
			TransitionThreePartGradient( pointPlantStemMat, _activePalette.pointPlantStem);
			TransitionThreePartGradient( leafyGroundPlantBulbMat, _activePalette.leafyGroundPlantBulb );
			TransitionThreePartGradient( leafyGroundPlantLeafMat, _activePalette.leafyGroundPlantLeaf );

			TransitionTwoPartGradient( twistPlantMat, _activePalette.twistPlant );			
			TransitionTwoPartGradient( cappPlantMat, _activePalette.pointPlantLeaf );

			StartCoroutine( DelayedUpdatePalatteEvent() );
		}
		     
	}

	void GeneralTransitionColors( EnvironmentPalette prevPalette, float duration = PALATTE_TRANSITIONTIME )
	{
		StartCoroutine( DelayedGeneralColorTransition( prevPalette, duration ) );
	}

	IEnumerator DelayedGeneralColorTransition( EnvironmentPalette prevPalette, float duration )
	{
		float timer = 0.0f;

		while ( timer < duration )
		{
			timer += Time.deltaTime;

			//groundcolor
			Shader.SetGlobalColor("_GroundColorPrimary", Colorx.Slerp( prevPalette.groundColorPrimary, _activePalette.groundColorPrimary, timer / duration ) );
			Shader.SetGlobalColor("_GroundColorSecondary", Colorx.Slerp( prevPalette.groundColorSecondary, _activePalette.groundColorSecondary, timer / duration ) );
			Shader.SetGlobalColor("_GroundColorTint",  Colorx.Slerp( prevPalette.groundDecalTint, _activePalette.groundDecalTint, timer / duration ) );

			ParticleSystem.MainModule groundDecalMain = groundSplatDecal.main;
			groundDecalMain.startColor = new Color( _activePalette.groundColorSecondary.r * _activePalette.groundDecalTint.r, _activePalette.groundColorSecondary.g * _activePalette.groundDecalTint.g, _activePalette.groundColorSecondary.b * _activePalette.groundDecalTint.b, 1.0f );			

			//skybox
			RenderSettings.fogColor = Colorx.Slerp(prevPalette.fogColor, _activePalette.fogColor, timer / duration );
			RenderSettings.skybox.SetColor( "_Color1", Colorx.Slerp( prevPalette.cloudRimColor, _activePalette.cloudRimColor, timer/ duration )  );
			RenderSettings.skybox.SetColor( "_Color2", Colorx.Slerp( prevPalette.skyColor, _activePalette.skyColor, timer / duration ) );

			//terrain
			terrainMaterial.SetColor( "_Color", Colorx.Slerp( prevPalette.terrainColor, _activePalette.terrainColor, timer / duration ) );		
				
			yield return 0;
		}
	}
	/// <summary>
	/// Applies the three part gradient to our Gradient Shader
	/// </summary>
	/// <param name="material">Material.</param>
	/// <param name="gradient">Gradient.</param>
	public static void ApplyThreePartGradient( Material material, Gradient gradient )
	{
		//Debug.Assert( material != null && material.HasProperty("_TopColor"), "Must be a gradient shader!" );

		material.SetColor("_TopColor", gradient.Evaluate(0f));
		material.SetColor("_MidColor", gradient.Evaluate(.5f));
		material.SetColor("_BotColor", gradient.Evaluate(1f));		
	}
	public static void ApplyTwoPartGradient( Material material, Gradient gradient )
	{
		//Debug.Assert( material != null && material.HasProperty("_Color1"), "Must be a two part gradient shader!" );

		material.SetColor("_Color", gradient.Evaluate(0f));
		material.SetColor("_Color2", gradient.Evaluate(.5f));
	}

	public void TransitionThreePartGradient( Material material, Gradient gradient, float transitionTime = PALATTE_TRANSITIONTIME )
	{
		StartCoroutine( DelayedChangeMaterialGradient( material, gradient, transitionTime ) );
	}

	public void TransitionTwoPartGradient( Material material, Gradient gradient, float transitionTime = PALATTE_TRANSITIONTIME )
	{
		StartCoroutine( DelayedChangeTwoGradientMaterial( material, gradient, transitionTime ) );
	}

	IEnumerator DelayedChangeMaterialGradient( Material material, Gradient gradient, float transitionTime )
	{
		float timer = 0.0f;
		Color topColor = material.GetColor( "_TopColor" );
		Color midColor = material.GetColor( "_MidColor" );
		Color botColor = material.GetColor( "_BotColor" );

		while( timer < transitionTime )
		{
			timer +=  Time.deltaTime;
					
			material.SetColor("_TopColor", Colorx.Slerp( topColor, gradient.Evaluate(0f), timer / transitionTime ) );
			material.SetColor("_MidColor", Colorx.Slerp( midColor, gradient.Evaluate(.5f), timer / transitionTime ) );
			material.SetColor("_BotColor", Colorx.Slerp( botColor, gradient.Evaluate(1f), timer / transitionTime ) );		

			yield return 0;
		}			
	}

	IEnumerator DelayedChangeTwoGradientMaterial( Material material, Gradient gradient, float transitionTime )
	{
		float timer = 0.0f;
		Color topColor = material.GetColor( "_Color" );
		Color midColor = material.GetColor( "_Color2" );


		while( timer < transitionTime )
		{
			timer +=  Time.deltaTime;

			material.SetColor("_Color", Colorx.Slerp( topColor, gradient.Evaluate(0f), timer / transitionTime ) );
			material.SetColor("_Color2", Colorx.Slerp( midColor, gradient.Evaluate(0.5f), timer / transitionTime ) );

			yield return 0;
		}			
	}

	IEnumerator DelayedUpdatePalatteEvent( float duration = PALATTE_TRANSITIONTIME )
	{
		float timer = 0.0f;

		while( timer < duration )
		{
			timer += Time.deltaTime;

			if ( ExecutePaletteChange != null )
			{
				ExecutePaletteChange( _activePalette );
			}

			yield return 0;
		}
	}

	void OnValidate()
	{		
		if( _paletteIndex < 0 )
		{
			_paletteIndex = 0;
		}
		else if( _paletteIndex >= _environmentPaletteList.Count )
		{
			_paletteIndex = _environmentPaletteList.Count - 1;
		}

		if( _paletteIndex != _environmentPaletteList.FindIndex( x => x.title == _activePalette.title ) )    // Should be b a better way to do this
		{
			UpdatePalatte( _paletteIndex );
		}

	}
}
