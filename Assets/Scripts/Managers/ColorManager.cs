using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


[Serializable]
public struct PlantColorPalette
{
	//0
	[HeaderAttribute("Gradient 0"), Space(5)] 
	public Gradient TopGradient0;
	public Gradient MidGradient0;
	public Gradient BotGradient0;
	//	//1 
	[HeaderAttribute("Gradient 1"), Space(5)] 
	public Gradient TopGradient1;
	public Gradient MidGradient1;
	public Gradient BotGradient1;
	//	//2 
	[HeaderAttribute("Gradient 2"), Space(5)] 
	public Gradient TopGradient2;
	public Gradient MidGradient2;
	public Gradient BotGradient2;
	//	//3
	[HeaderAttribute("Gradient 3"), Space(5)] 
	public Gradient TopGradient3;
	public Gradient MidGradient3;
	public Gradient BotGradient3;
	//	//4
	[HeaderAttribute("Gradient 4"), Space(5)] 
	public Gradient TopGradient4;
	public Gradient MidGradient4;
	public Gradient BotGradient4;
	//	//5
	[HeaderAttribute("Gradient 5"), Space(5)] 
	public Gradient TopGradient5;
	public Gradient MidGradient5;
	public Gradient BotGradient5;
	//	//6
	[HeaderAttribute("Gradient 6"), Space(5)] 
	public Gradient TopGradient6;
	public Gradient MidGradient6;
	public Gradient BotGradient6;
	//	//7
	[HeaderAttribute("Gradient 7"), Space(5)] 
	public Gradient TopGradient7;
	public Gradient MidGradient7;
	public Gradient BotGradient7;
	//	//8
	[HeaderAttribute("Gradient 8"), Space(5)] 
	public Gradient TopGradient8;
	public Gradient MidGradient8;
	public Gradient BotGradient8;
	//	//9
	[HeaderAttribute("Gradient 9"), Space(5)] 
	public Gradient TopGradient9;
	public Gradient MidGradient9;
	public Gradient BotGradient9;
}

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
		public Color pondRockColor;
		public Color terrainRockColor;
		public Color terrainMossRockColor;

		[Header("Sky / Fog colors"), Space(5)]
		public Color fogColor;
		public Color skyColor;
		public Color cloudRimColor;

		[Header( "Creatures" ), Space( 5 )]

		public Gradient rabbitGradient;
		public Gradient butterflyGradient;

		public PlantColorPalette plantColors;
	}

	public static event Action<EnvironmentPalette, EnvironmentPalette> ExecutePaletteChange;

	[SerializeField] int _paletteIndex = 0;
	EnvironmentPalette _activePalette;
	public EnvironmentPalette ActivePalette { get { return _activePalette; } }
	public const float PALETTE_TRANSITIONTIME = 5.0f;
	const float PALATTE_ADVANCETIMER_MIN = 90.0f;
	const float PALATE_ADVANCETIMER_MAX = 120.0f;


	[SerializeField, Space(5)] List<EnvironmentPalette> _environmentPaletteList = new List<EnvironmentPalette>();
	public List<EnvironmentPalette> OldPalletteList { get { return _environmentPaletteList; } set { _environmentPaletteList = value; } }

	[SerializeField, Space(5)]
	List<int> _paletteOrderList = new List<int>();

	List<int> _uniformIdentifierList = new List<int>();

	// TODO make as many of these global shader things as possible?
	[Header("Global Materials"), Space(5)]
	[SerializeField] Material terrainMaterial;
	[SerializeField] ParticleSystem groundSplatDecal;
    [SerializeField]
    public Color flowerSplatDecalColor;
    [SerializeField] Material terrainRockMat;
	[SerializeField] Material terrainMossRockMat;
	[SerializeField] Material pondRockMat;

	[SerializeField] Material rabbitMat;

    [SerializeField]
    ParticleSystem _twistParticleSystem;
    [SerializeField]
    ParticleSystem _cappParticleSystem;

	void Start()
	{
		// Setting up Global Shader value ID's
		InitShaderIDList();

		_paletteIndex = 0;
		_activePalette = _environmentPaletteList[_paletteOrderList[_paletteIndex]];
		UpdatePalette( _paletteOrderList[_paletteIndex] );	
	}

	public override void Initialize ()
	{
		
		isInitialized = true;
	}

	void UpdatePalette( int newPalatteIndex )
	{
		EnvironmentPalette prevPalette = _activePalette;
		_activePalette = _environmentPaletteList[newPalatteIndex];

		if( _uniformIdentifierList.Count == 0)
		{
			InitShaderIDList();
		}

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

            if( _twistParticleSystem != null )
            {
                ParticleSystem.MainModule twistDecalMain = groundSplatDecal.main;
				twistDecalMain.startColor = new Color( _activePalette.groundColorSecondary.r * _activePalette.groundDecalTint.r, _activePalette.groundColorSecondary.g * _activePalette.groundDecalTint.g, _activePalette.groundColorSecondary.b * _activePalette.groundDecalTint.b, 1.0f );
            }

            if ( _cappParticleSystem != null )
            {
                ParticleSystem.MainModule cappDecalMain = groundSplatDecal.main;
				cappDecalMain.startColor = new Color( _activePalette.groundColorSecondary.r * _activePalette.groundDecalTint.r, _activePalette.groundColorSecondary.g * _activePalette.groundDecalTint.g, _activePalette.groundColorSecondary.b * _activePalette.groundDecalTint.b, 1.0f );
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

			pondRockMat.SetColor( "_Color", _activePalette.pondRockColor );
            //ApplyThreePartGradient( pondRockMat, _activePalette.pondRockGradient );

			ApplyThreePartGradient( rabbitMat, _activePalette.rabbitGradient );

			SetPlantColors( prevPalette.plantColors, _activePalette.plantColors );
		}
		else
		{			
			//Debug.Log( "Transitioning Colors" );
			GeneralTransitionColors( prevPalette );
			       
			TransitionThreePartGradient( rabbitMat, _activePalette.rabbitGradient );

			TransitionPlantColors( prevPalette );

		}

		if (ExecutePaletteChange != null)
		{
			//ExecutePaletteChange( _activePalette, prevPalatte );
		} 
	}

	void GeneralTransitionColors( EnvironmentPalette prevPalette, float duration = PALETTE_TRANSITIONTIME )
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

			pondRockMat.SetColor( "_Color", Colorx.Slerp( prevPalette.pondRockColor, _activePalette.pondRockColor, timer / duration ) );

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
		//Debug.Assert( material != null && material.HasProperty("_TopColor"), "Must be a gradient 
		objectMaterial.SetColor("_ColorTop", gradient.Evaluate(0f));

		objectMaterial.SetColor("_ColorMid", gradient.Evaluate(.5f));

		objectMaterial.SetColor("_ColorBot", gradient.Evaluate(1f));  
	}


	public void TransitionThreePartGradient( Material objectMaterial, Gradient gradient, float transitionTime = PALETTE_TRANSITIONTIME )
	{
		StartCoroutine( DelayedTransitionThreePartGradient( objectMaterial, gradient, transitionTime ) );
	}


	IEnumerator DelayedTransitionThreePartGradient( Material objectMaterial, Gradient gradient, float transitionTime )
	{
		float timer = 0.0f;

		Color topColor = objectMaterial.GetColor( "_ColorTop" );
		Color midColor = objectMaterial.GetColor( "_ColorMid" );
		Color botColor = objectMaterial.GetColor( "_ColorBot" );

		while( timer < transitionTime )
		{
			timer += Time.deltaTime;

			objectMaterial.SetColor("_ColorTop", Colorx.Slerp( topColor, gradient.Evaluate(0f), timer / transitionTime ) );
			objectMaterial.SetColor("_ColorMid", Colorx.Slerp( midColor, gradient.Evaluate(.5f), timer / transitionTime ) );
			objectMaterial.SetColor("_ColorBot", Colorx.Slerp( botColor, gradient.Evaluate(1f), timer / transitionTime ) );

			yield return 0;
		}

		ApplyThreePartGradient( objectMaterial, gradient );
	}

	void TransitionPlantColors( EnvironmentPalette prevPalette, float duration = PALETTE_TRANSITIONTIME )
	{
		StartCoroutine( DelayedPlantColorTransition( prevPalette, duration ) );
	}

	IEnumerator DelayedPlantColorTransition( EnvironmentPalette prevPalette, float duration )
	{
		float timer = 0.0f;

		while( timer < duration )
		{
			timer += Time.deltaTime;

			SetNewGlobalShaderValues( prevPalette.plantColors, _activePalette.plantColors, timer / duration );

			yield return 0;
		}

		SetPlantColors( prevPalette.plantColors, _activePalette.plantColors );
	}

	void SetPlantColors( PlantColorPalette prevPalette, PlantColorPalette newPalette )
	{
		SetNewGlobalShaderValues( prevPalette, newPalette, 1.0f );
	}

	void SetNewGlobalShaderValues( PlantColorPalette prevPalette, PlantColorPalette newPalette, float progress )
	{
		if( prevPalette.TopGradient0 == null )
			return;	// For Now Just Fail

		Shader.SetGlobalColor( _uniformIdentifierList[0], Colorx.Slerp( prevPalette.TopGradient0.Evaluate(0.0f), newPalette.TopGradient0.Evaluate(0.0f), progress ) );
		Shader.SetGlobalColor( _uniformIdentifierList[1], Colorx.Slerp( prevPalette.TopGradient0.Evaluate(1.0f), newPalette.TopGradient0.Evaluate(1.0f), progress ) );
		Shader.SetGlobalColor( _uniformIdentifierList[2], Colorx.Slerp( prevPalette.MidGradient0.Evaluate(0.0f), newPalette.MidGradient0.Evaluate(0.0f), progress ) );
		Shader.SetGlobalColor( _uniformIdentifierList[3], Colorx.Slerp( prevPalette.MidGradient0.Evaluate(1.0f), newPalette.MidGradient0.Evaluate(1.0f), progress ) );
		Shader.SetGlobalColor( _uniformIdentifierList[4], Colorx.Slerp( prevPalette.BotGradient0.Evaluate(0.0f), newPalette.BotGradient0.Evaluate(0.0f), progress ) );
		Shader.SetGlobalColor( _uniformIdentifierList[5], Colorx.Slerp( prevPalette.BotGradient0.Evaluate(1.0f), newPalette.BotGradient0.Evaluate(1.0f), progress ) );

		Shader.SetGlobalColor( _uniformIdentifierList[6], Colorx.Slerp( prevPalette.TopGradient1.Evaluate(0.0f), newPalette.TopGradient1.Evaluate(0.0f), progress ) );
		Shader.SetGlobalColor( _uniformIdentifierList[7], Colorx.Slerp( prevPalette.TopGradient1.Evaluate(1.0f), newPalette.TopGradient1.Evaluate(1.0f), progress ) );
		Shader.SetGlobalColor( _uniformIdentifierList[8], Colorx.Slerp( prevPalette.MidGradient1.Evaluate(0.0f), newPalette.MidGradient1.Evaluate(0.0f), progress ) );
		Shader.SetGlobalColor( _uniformIdentifierList[9], Colorx.Slerp( prevPalette.MidGradient1.Evaluate(1.0f), newPalette.MidGradient1.Evaluate(1.0f), progress ) );
		Shader.SetGlobalColor( _uniformIdentifierList[10], Colorx.Slerp( prevPalette.BotGradient1.Evaluate(0.0f), newPalette.BotGradient1.Evaluate(0.0f), progress ) );
		Shader.SetGlobalColor( _uniformIdentifierList[11], Colorx.Slerp( prevPalette.BotGradient1.Evaluate(1.0f), newPalette.BotGradient1.Evaluate(1.0f), progress ) );

		Shader.SetGlobalColor( _uniformIdentifierList[12], Colorx.Slerp( prevPalette.TopGradient2.Evaluate(0.0f), newPalette.TopGradient2.Evaluate(0.0f), progress ) );
		Shader.SetGlobalColor( _uniformIdentifierList[13], Colorx.Slerp( prevPalette.TopGradient2.Evaluate(1.0f), newPalette.TopGradient2.Evaluate(1.0f), progress ) );
		Shader.SetGlobalColor( _uniformIdentifierList[14], Colorx.Slerp( prevPalette.MidGradient2.Evaluate(0.0f), newPalette.MidGradient2.Evaluate(0.0f), progress ) );
		Shader.SetGlobalColor( _uniformIdentifierList[15], Colorx.Slerp( prevPalette.MidGradient2.Evaluate(1.0f), newPalette.MidGradient2.Evaluate(1.0f), progress ) );
		Shader.SetGlobalColor( _uniformIdentifierList[16], Colorx.Slerp( prevPalette.BotGradient2.Evaluate(0.0f), newPalette.BotGradient2.Evaluate(0.0f), progress ) );
		Shader.SetGlobalColor( _uniformIdentifierList[17], Colorx.Slerp( prevPalette.BotGradient2.Evaluate(1.0f), newPalette.BotGradient2.Evaluate(1.0f), progress ) );

		Shader.SetGlobalColor( _uniformIdentifierList[18], Colorx.Slerp( prevPalette.TopGradient3.Evaluate(0.0f), newPalette.TopGradient3.Evaluate(0.0f), progress ) );
		Shader.SetGlobalColor( _uniformIdentifierList[19], Colorx.Slerp( prevPalette.TopGradient3.Evaluate(1.0f), newPalette.TopGradient3.Evaluate(1.0f), progress ) );
		Shader.SetGlobalColor( _uniformIdentifierList[20], Colorx.Slerp( prevPalette.MidGradient3.Evaluate(0.0f), newPalette.MidGradient3.Evaluate(0.0f), progress ) );
		Shader.SetGlobalColor( _uniformIdentifierList[21], Colorx.Slerp( prevPalette.MidGradient3.Evaluate(1.0f), newPalette.MidGradient3.Evaluate(1.0f), progress ) );
		Shader.SetGlobalColor( _uniformIdentifierList[22], Colorx.Slerp( prevPalette.BotGradient3.Evaluate(0.0f), newPalette.BotGradient3.Evaluate(0.0f), progress ) );
		Shader.SetGlobalColor( _uniformIdentifierList[23], Colorx.Slerp( prevPalette.BotGradient3.Evaluate(1.0f), newPalette.BotGradient3.Evaluate(1.0f), progress ) );

		Shader.SetGlobalColor( _uniformIdentifierList[24], Colorx.Slerp( prevPalette.TopGradient4.Evaluate(0.0f), newPalette.TopGradient4.Evaluate(0.0f), progress ) );
		Shader.SetGlobalColor( _uniformIdentifierList[25], Colorx.Slerp( prevPalette.TopGradient4.Evaluate(1.0f), newPalette.TopGradient4.Evaluate(1.0f), progress ) );
		Shader.SetGlobalColor( _uniformIdentifierList[26], Colorx.Slerp( prevPalette.MidGradient4.Evaluate(0.0f), newPalette.MidGradient4.Evaluate(0.0f), progress ) );
		Shader.SetGlobalColor( _uniformIdentifierList[27], Colorx.Slerp( prevPalette.MidGradient4.Evaluate(1.0f), newPalette.MidGradient4.Evaluate(1.0f), progress ) );
		Shader.SetGlobalColor( _uniformIdentifierList[28], Colorx.Slerp( prevPalette.BotGradient4.Evaluate(0.0f), newPalette.BotGradient4.Evaluate(0.0f), progress ) );
		Shader.SetGlobalColor( _uniformIdentifierList[29], Colorx.Slerp( prevPalette.BotGradient4.Evaluate(1.0f), newPalette.BotGradient4.Evaluate(1.0f), progress ) );

		Shader.SetGlobalColor( _uniformIdentifierList[30], Colorx.Slerp( prevPalette.TopGradient5.Evaluate(0.0f), newPalette.TopGradient5.Evaluate(0.0f), progress ) );
		Shader.SetGlobalColor( _uniformIdentifierList[31], Colorx.Slerp( prevPalette.TopGradient5.Evaluate(1.0f), newPalette.TopGradient5.Evaluate(1.0f), progress ) );
		Shader.SetGlobalColor( _uniformIdentifierList[32], Colorx.Slerp( prevPalette.MidGradient5.Evaluate(0.0f), newPalette.MidGradient5.Evaluate(0.0f), progress ) );
		Shader.SetGlobalColor( _uniformIdentifierList[33], Colorx.Slerp( prevPalette.MidGradient5.Evaluate(1.0f), newPalette.MidGradient5.Evaluate(1.0f), progress ) );
		Shader.SetGlobalColor( _uniformIdentifierList[34], Colorx.Slerp( prevPalette.BotGradient5.Evaluate(0.0f), newPalette.BotGradient5.Evaluate(0.0f), progress ) );
		Shader.SetGlobalColor( _uniformIdentifierList[35], Colorx.Slerp( prevPalette.BotGradient5.Evaluate(1.0f), newPalette.BotGradient5.Evaluate(1.0f), progress ) );

		Shader.SetGlobalColor( _uniformIdentifierList[36], Colorx.Slerp( prevPalette.TopGradient6.Evaluate(0.0f), newPalette.BotGradient6.Evaluate(0.0f), progress ) );
		Shader.SetGlobalColor( _uniformIdentifierList[37], Colorx.Slerp( prevPalette.MidGradient6.Evaluate(1.0f), newPalette.BotGradient6.Evaluate(1.0f), progress ) );
		Shader.SetGlobalColor( _uniformIdentifierList[38], Colorx.Slerp( prevPalette.BotGradient6.Evaluate(0.0f), newPalette.BotGradient6.Evaluate(0.0f), progress ) );
		Shader.SetGlobalColor( _uniformIdentifierList[39], Colorx.Slerp( prevPalette.TopGradient6.Evaluate(1.0f), newPalette.BotGradient6.Evaluate(1.0f), progress ) );
		Shader.SetGlobalColor( _uniformIdentifierList[40], Colorx.Slerp( prevPalette.MidGradient6.Evaluate(0.0f), newPalette.BotGradient6.Evaluate(0.0f), progress ) );
		Shader.SetGlobalColor( _uniformIdentifierList[41], Colorx.Slerp( prevPalette.BotGradient6.Evaluate(1.0f), newPalette.BotGradient6.Evaluate(1.0f), progress ) );

		Shader.SetGlobalColor( _uniformIdentifierList[42], Colorx.Slerp( prevPalette.TopGradient7.Evaluate(0.0f), newPalette.BotGradient7.Evaluate(0.0f), progress ) );
		Shader.SetGlobalColor( _uniformIdentifierList[43], Colorx.Slerp( prevPalette.MidGradient7.Evaluate(1.0f), newPalette.BotGradient7.Evaluate(1.0f), progress ) );
		Shader.SetGlobalColor( _uniformIdentifierList[44], Colorx.Slerp( prevPalette.BotGradient7.Evaluate(0.0f), newPalette.BotGradient7.Evaluate(0.0f), progress ) );
		Shader.SetGlobalColor( _uniformIdentifierList[45], Colorx.Slerp( prevPalette.TopGradient7.Evaluate(1.0f), newPalette.BotGradient7.Evaluate(1.0f), progress ) );
		Shader.SetGlobalColor( _uniformIdentifierList[46], Colorx.Slerp( prevPalette.MidGradient7.Evaluate(0.0f), newPalette.BotGradient7.Evaluate(0.0f), progress ) );
		Shader.SetGlobalColor( _uniformIdentifierList[47], Colorx.Slerp( prevPalette.BotGradient7.Evaluate(1.0f), newPalette.BotGradient7.Evaluate(1.0f), progress ) );

		Shader.SetGlobalColor( _uniformIdentifierList[48], Colorx.Slerp( prevPalette.TopGradient8.Evaluate(0.0f), newPalette.BotGradient8.Evaluate(0.0f), progress ) );
		Shader.SetGlobalColor( _uniformIdentifierList[49], Colorx.Slerp( prevPalette.MidGradient8.Evaluate(1.0f), newPalette.BotGradient8.Evaluate(1.0f), progress ) );
		Shader.SetGlobalColor( _uniformIdentifierList[50], Colorx.Slerp( prevPalette.BotGradient8.Evaluate(0.0f), newPalette.BotGradient8.Evaluate(0.0f), progress ) );
		Shader.SetGlobalColor( _uniformIdentifierList[51], Colorx.Slerp( prevPalette.TopGradient8.Evaluate(1.0f), newPalette.BotGradient8.Evaluate(1.0f), progress ) );
		Shader.SetGlobalColor( _uniformIdentifierList[52], Colorx.Slerp( prevPalette.MidGradient8.Evaluate(0.0f), newPalette.BotGradient8.Evaluate(0.0f), progress ) );
		Shader.SetGlobalColor( _uniformIdentifierList[53], Colorx.Slerp( prevPalette.BotGradient8.Evaluate(1.0f), newPalette.BotGradient8.Evaluate(1.0f), progress ) );

		Shader.SetGlobalColor( _uniformIdentifierList[54], Colorx.Slerp( prevPalette.TopGradient9.Evaluate(0.0f), newPalette.BotGradient9.Evaluate(0.0f), progress ) );
		Shader.SetGlobalColor( _uniformIdentifierList[55], Colorx.Slerp( prevPalette.MidGradient9.Evaluate(1.0f), newPalette.BotGradient9.Evaluate(1.0f), progress ) );
		Shader.SetGlobalColor( _uniformIdentifierList[56], Colorx.Slerp( prevPalette.BotGradient9.Evaluate(0.0f), newPalette.BotGradient9.Evaluate(0.0f), progress ) );
		Shader.SetGlobalColor( _uniformIdentifierList[57], Colorx.Slerp( prevPalette.TopGradient9.Evaluate(1.0f), newPalette.BotGradient9.Evaluate(1.0f), progress ) );
		Shader.SetGlobalColor( _uniformIdentifierList[58], Colorx.Slerp( prevPalette.MidGradient9.Evaluate(0.0f), newPalette.BotGradient9.Evaluate(0.0f), progress ) );
		Shader.SetGlobalColor( _uniformIdentifierList[59], Colorx.Slerp( prevPalette.BotGradient9.Evaluate(1.0f), newPalette.BotGradient9.Evaluate(1.0f), progress ) );
	}

	void InitShaderIDList()
	{
		_uniformIdentifierList.Add( Shader.PropertyToID("_PlantColorSet0_Top1") );
		_uniformIdentifierList.Add( Shader.PropertyToID("_PlantColorSet0_Top2") );
		_uniformIdentifierList.Add( Shader.PropertyToID("_PlantColorSet0_Mid1") );
		_uniformIdentifierList.Add( Shader.PropertyToID("_PlantColorSet0_Mid2") );
		_uniformIdentifierList.Add( Shader.PropertyToID("_PlantColorSet0_Bot1") );
		_uniformIdentifierList.Add( Shader.PropertyToID("_PlantColorSet0_Bot2") );

		_uniformIdentifierList.Add( Shader.PropertyToID("_PlantColorSet1_Top1") );
		_uniformIdentifierList.Add( Shader.PropertyToID("_PlantColorSet1_Top2") );
		_uniformIdentifierList.Add( Shader.PropertyToID("_PlantColorSet1_Mid1") );
		_uniformIdentifierList.Add( Shader.PropertyToID("_PlantColorSet1_Mid2") );
		_uniformIdentifierList.Add( Shader.PropertyToID("_PlantColorSet1_Bot1") );
		_uniformIdentifierList.Add( Shader.PropertyToID("_PlantColorSet1_Bot2") );

		_uniformIdentifierList.Add( Shader.PropertyToID("_PlantColorSet2_Top1") );
		_uniformIdentifierList.Add( Shader.PropertyToID("_PlantColorSet2_Top2") );
		_uniformIdentifierList.Add( Shader.PropertyToID("_PlantColorSet2_Mid1") );
		_uniformIdentifierList.Add( Shader.PropertyToID("_PlantColorSet2_Mid2") );
		_uniformIdentifierList.Add( Shader.PropertyToID("_PlantColorSet2_Bot1") );
		_uniformIdentifierList.Add( Shader.PropertyToID("_PlantColorSet2_Bot2") );

		_uniformIdentifierList.Add( Shader.PropertyToID("_PlantColorSet3_Top1") );
		_uniformIdentifierList.Add( Shader.PropertyToID("_PlantColorSet3_Top2") );
		_uniformIdentifierList.Add( Shader.PropertyToID("_PlantColorSet3_Mid1") );
		_uniformIdentifierList.Add( Shader.PropertyToID("_PlantColorSet3_Mid2") );
		_uniformIdentifierList.Add( Shader.PropertyToID("_PlantColorSet3_Bot1") );
		_uniformIdentifierList.Add( Shader.PropertyToID("_PlantColorSet3_Bot2") );

		_uniformIdentifierList.Add( Shader.PropertyToID("_PlantColorSet4_Top1") );
		_uniformIdentifierList.Add( Shader.PropertyToID("_PlantColorSet4_Top2") );
		_uniformIdentifierList.Add( Shader.PropertyToID("_PlantColorSet4_Mid1") );
		_uniformIdentifierList.Add( Shader.PropertyToID("_PlantColorSet4_Mid2") );
		_uniformIdentifierList.Add( Shader.PropertyToID("_PlantColorSet4_Bot1") );
		_uniformIdentifierList.Add( Shader.PropertyToID("_PlantColorSet4_Bot2") );

		_uniformIdentifierList.Add( Shader.PropertyToID("_PlantColorSet5_Top1") );
		_uniformIdentifierList.Add( Shader.PropertyToID("_PlantColorSet5_Top2") );
		_uniformIdentifierList.Add( Shader.PropertyToID("_PlantColorSet5_Mid1") );
		_uniformIdentifierList.Add( Shader.PropertyToID("_PlantColorSet5_Mid2") );
		_uniformIdentifierList.Add( Shader.PropertyToID("_PlantColorSet5_Bot1") );
		_uniformIdentifierList.Add( Shader.PropertyToID("_PlantColorSet5_Bot2") );

		_uniformIdentifierList.Add( Shader.PropertyToID("_PlantColorSet6_Top1") );
		_uniformIdentifierList.Add( Shader.PropertyToID("_PlantColorSet6_Top2") );
		_uniformIdentifierList.Add( Shader.PropertyToID("_PlantColorSet6_Mid1") );
		_uniformIdentifierList.Add( Shader.PropertyToID("_PlantColorSet6_Mid2") );
		_uniformIdentifierList.Add( Shader.PropertyToID("_PlantColorSet6_Bot1") );
		_uniformIdentifierList.Add( Shader.PropertyToID("_PlantColorSet6_Bot2") );

		_uniformIdentifierList.Add( Shader.PropertyToID("_PlantColorSet7_Top1") );
		_uniformIdentifierList.Add( Shader.PropertyToID("_PlantColorSet7_Top2") );
		_uniformIdentifierList.Add( Shader.PropertyToID("_PlantColorSet7_Mid1") );
		_uniformIdentifierList.Add( Shader.PropertyToID("_PlantColorSet7_Mid2") );
		_uniformIdentifierList.Add( Shader.PropertyToID("_PlantColorSet7_Bot1") );
		_uniformIdentifierList.Add( Shader.PropertyToID("_PlantColorSet7_Bot2") );

		_uniformIdentifierList.Add( Shader.PropertyToID("_PlantColorSet8_Top1") );
		_uniformIdentifierList.Add( Shader.PropertyToID("_PlantColorSet8_Top2") );
		_uniformIdentifierList.Add( Shader.PropertyToID("_PlantColorSet8_Mid1") );
		_uniformIdentifierList.Add( Shader.PropertyToID("_PlantColorSet8_Mid2") );
		_uniformIdentifierList.Add( Shader.PropertyToID("_PlantColorSet8_Bot1") );
		_uniformIdentifierList.Add( Shader.PropertyToID("_PlantColorSet8_Bot2") );

		_uniformIdentifierList.Add( Shader.PropertyToID("_PlantColorSet9_Top1") );
		_uniformIdentifierList.Add( Shader.PropertyToID("_PlantColorSet9_Top2") );
		_uniformIdentifierList.Add( Shader.PropertyToID("_PlantColorSet9_Mid1") );
		_uniformIdentifierList.Add( Shader.PropertyToID("_PlantColorSet9_Mid2") );
		_uniformIdentifierList.Add( Shader.PropertyToID("_PlantColorSet9_Bot1") );
		_uniformIdentifierList.Add( Shader.PropertyToID("_PlantColorSet9_Bot2") );
	}

	void OnValidate()
	{		
		if( _paletteIndex < 0 )
		{
			_paletteIndex = 0;
		}
		else if( _paletteIndex >= _paletteOrderList.Count )
		{
			_paletteIndex = _paletteOrderList.Count - 1;
		}
			
		UpdatePalette( _paletteOrderList[_paletteIndex] );

		//		if( _paletteIndex != _environmentPaletteList.FindIndex( x => x.title == _activePalette.title ) )    // Should be b a better way to do this
		//		{
		//			
		//		}

	}

	public void SwitchPalette()
	{
		_paletteIndex++;
		if( _paletteIndex >= _paletteOrderList.Count )
		{
			_paletteIndex = 0;
		}

		UpdatePalette( _paletteOrderList[_paletteIndex] );
	}
}
