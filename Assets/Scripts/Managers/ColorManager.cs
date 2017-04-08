using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ColorManager : SingletonBehaviour<ColorManager> {

	[Serializable]
	public struct EnvironmentPalette
	{
		public string title;

		// TODO Rename EVERYTHING to make more sense

		[Header("Ground colors"), Space(5)]
		public Color groundColorPrimary;
		public Color groundColorSecondary;
		public Color groundDecalTint;

		[Header("Terrain colors"), Space(5)]
		public Color terrainColor;
		public Color pondRockGradient;
		public Color terrainRockColor;
		public Color terrainMossRockColor;

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
		
	public static event Action<EnvironmentPalette, EnvironmentPalette> ExecutePaletteChange;

	[SerializeField] int _paletteIndex = 0;
	EnvironmentPalette _activePalette;
	public EnvironmentPalette ActivePalatte { get { return _activePalette; } }
	public const float PALATTE_TRANSITIONTIME = 5.0f;


	[SerializeField, Space(5)] List<EnvironmentPalette> _environmentPaletteList = new List<EnvironmentPalette>();
	public List<EnvironmentPalette> PalletteList { get { return _environmentPaletteList; } set { _environmentPaletteList = value; } }

	// TODO make as many of these global shader things as possible?
	[Header("Global Materials"), Space(5)]
	[SerializeField] Material terrainMaterial;
	[SerializeField] ParticleSystem groundSplatDecal;
	[SerializeField] Material terrainRockMat;
	[SerializeField] Material terrainMossRockMat;
	[SerializeField] Material pondRockMat;

	[SerializeField] Material mossPlantMat;
	[SerializeField] Material mossPlantSeedMat;

	public Material pointPlantSeedMat;
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

		UpdatePalette( _paletteIndex );
    }

	void UpdatePalette( int newPalatteIndex )
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

			terrainRockMat.SetColor( "_Color", _activePalette.terrainRockColor);
			terrainMossRockMat.SetColor( "_Color", _activePalette.terrainMossRockColor );

			//skybox
			RenderSettings.fogColor = _activePalette.fogColor;
			RenderSettings.skybox.SetColor( "_Color1", _activePalette.cloudRimColor );
			RenderSettings.skybox.SetColor( "_Color2", _activePalette.skyColor );

			//terrain
			if ( terrainMaterial != null )
			{
				terrainMaterial.SetColor( "_Color", _activePalette.terrainColor );
			}

			pondRockMat.SetColor( "_Color", _activePalette.pondRockGradient );
			//ApplyThreePartGradient( pondRockMat, _activePalette.pondRockGradient );

			ApplyThreePartGradient( mossPlantSeedMat, _activePalette.mossPlantSeed );
			ApplyThreePartGradient( mossPlantMat, _activePalette.mossPlant );
			ApplyThreePartGradient( pointPlantSeedMat, _activePalette.pointPlantSeed );			
			ApplyThreePartGradient( pointPlantLeafMat, _activePalette.pointPlantLeaf );
			ApplyThreePartGradient( pointPlantStemMat, _activePalette.pointPlantStem );
			ApplyThreePartGradient( leafyGroundPlantBulbMat, _activePalette.leafyGroundPlantBulb );
			ApplyThreePartGradient( leafyGroundPlantLeafMat, _activePalette.leafyGroundPlantLeaf );

			ApplyTwoPartGradient( twistPlantMat, _activePalette.twistPlant );			
			ApplyTwoPartGradient( cappPlantMat, _activePalette.cappPlant );
						  
		}
		else
		{
			// TODO TRANSITION VIA AN OVERARCHING LERP BETWEEN EVERYTHING IN THE PALATTE
			//Debug.Log( "Transitioning Colors" );

			GeneralTransitionColors( prevPalatte );

			//TransitionThreePartGradient( pondRockMat, _activePalette.pondRockGradient );

			TransitionThreePartGradient( mossPlantSeedMat, _activePalette.mossPlantSeed );
			TransitionThreePartGradient( mossPlantMat, _activePalette.mossPlant );
			TransitionThreePartGradient( pointPlantSeedMat, _activePalette.pointPlantSeed );			
			TransitionThreePartGradient( pointPlantLeafMat, _activePalette.pointPlantLeaf );
			TransitionThreePartGradient( pointPlantStemMat, _activePalette.pointPlantStem );
			TransitionThreePartGradient( leafyGroundPlantBulbMat, _activePalette.leafyGroundPlantBulb );
			TransitionThreePartGradient( leafyGroundPlantLeafMat, _activePalette.leafyGroundPlantLeaf );

			TransitionTwoPartGradient( twistPlantMat, _activePalette.twistPlant );			
			TransitionTwoPartGradient( cappPlantMat, _activePalette.cappPlant );

			//StartCoroutine( DelayedUpdatePalatteEvent() );
		}
		     
		if (ExecutePaletteChange != null)
		{
			ExecutePaletteChange( _activePalette, prevPalatte );
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
			terrainRockMat.SetColor( "_Color", Colorx.Slerp( prevPalette.terrainRockColor, _activePalette.terrainRockColor, timer / duration ) );
			terrainMossRockMat.SetColor( "_Color", Colorx.Slerp( prevPalette.terrainMossRockColor, _activePalette.terrainMossRockColor, timer / duration ) );

			pondRockMat.SetColor( "_Color", Colorx.Slerp( prevPalette.pondRockGradient, _activePalette.pondRockGradient, timer / duration ) );

			yield return 0;
		}
	}
	/// <summary>
	/// Applies the three part gradient to our Gradient Shader
	/// </summary>
	/// <param name="material">Material.</param>
	/// <param name="gradient">Gradient.</param>
	public void ApplyThreePartGradient( Material objectMaterial, Gradient gradient )
	{
		//Debug.Assert( material != null && material.HasProperty("_TopColor"), "Must be a gradient shader!" );

		objectMaterial.SetColor("_ColorTop", gradient.Evaluate(0f));
		objectMaterial.SetColor("_ColorMid", gradient.Evaluate(.5f));
		objectMaterial.SetColor("_ColorBot", gradient.Evaluate(1f));	

	}
	public void ApplyTwoPartGradient( Material objectMaterial, Gradient gradient )
	{
		//Debug.Assert( material != null && material.HasProperty("_Color1"), "Must be a two part gradient shader!" );

		objectMaterial.SetColor("_Color", gradient.Evaluate(0f));
		objectMaterial.SetColor("_Color2", gradient.Evaluate(.5f));
	}

	public void TransitionThreePartGradient( Material objectMaterial, Gradient gradient, float transitionTime = PALATTE_TRANSITIONTIME )
	{
		StartCoroutine( DelayedChangeMaterialGradient( objectMaterial, gradient, transitionTime ) );
	}

	public void TransitionTwoPartGradient( Material objectMaterial, Gradient gradient, float transitionTime = PALATTE_TRANSITIONTIME )
	{
		StartCoroutine( DelayedChangeTwoGradientMaterial( objectMaterial, gradient, transitionTime ) );
	}

	IEnumerator DelayedChangeMaterialGradient( Material objectMaterial, Gradient gradient, float transitionTime )
	{
		float timer = 0.0f;
		Color topColor = objectMaterial.GetColor( "_ColorTop" );
		Color midColor = objectMaterial.GetColor( "_ColorMid" );
		Color botColor = objectMaterial.GetColor( "_ColorBot" );

		while( timer < transitionTime )
		{
			timer +=  Time.deltaTime;
					
			objectMaterial.SetColor("_ColorTop", Colorx.Slerp( topColor, gradient.Evaluate(0f), timer / transitionTime ) );
			objectMaterial.SetColor("_ColorMid", Colorx.Slerp( midColor, gradient.Evaluate(.5f), timer / transitionTime ) );
			objectMaterial.SetColor("_ColorBot", Colorx.Slerp( botColor, gradient.Evaluate(1f), timer / transitionTime ) );		

			yield return 0;
		}			
	}

	public IEnumerator DelayedChangeTwoGradientMaterial( Material objectMaterial, Gradient gradient, float transitionTime = PALATTE_TRANSITIONTIME )
	{
		float timer = 0.0f;
		Color topColor = objectMaterial.GetColor( "_Color" );
		Color midColor = objectMaterial.GetColor( "_Color2" );


		while( timer < transitionTime )
		{
			timer +=  Time.deltaTime;

			objectMaterial.SetColor("_Color", Colorx.Slerp( topColor, gradient.Evaluate(0f), timer / transitionTime ) );
			objectMaterial.SetColor("_Color2", Colorx.Slerp( midColor, gradient.Evaluate(0.5f), timer / transitionTime ) );

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

		UpdatePalette( _paletteIndex );

//		if( _paletteIndex != _environmentPaletteList.FindIndex( x => x.title == _activePalette.title ) )    // Should be b a better way to do this
//		{
//			
//		}

	}
}
