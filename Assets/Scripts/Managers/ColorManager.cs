using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ColorManager : SingletonBehaviour<ColorManager> {

	[Serializable]
	public struct EnvironmentPalette
	{
		public string title;

		[Header("Ground colors")]
		public Color groundColorPrimary;
		public Color groundColorSecondary;
		public Color groundDecalTint;

		[Header("Terrain colors")]
		public Color terrainColor;

		[Header("Sky / Fog colors")]
		public Color fogColor;
		public Color skyColor;
		public Color cloudRimColor;
		
        [Header("Plant colors")]
        [Header("Class 1")]
        public Gradient mossPlant;
        public Gradient pointPlantStem;
        public Gradient pointPlantLeaf;
        [Header("Class 2")]
        public Gradient leafyGroundPlantBulb;
        public Gradient leafyGroundPlantLeaf;
        [Header("Class 3")]
		public Gradient twistPlant;
		public Gradient cappPlant;
		
	}
		
	public static event Action<EnvironmentPalette> ExecutePaletteChange;

	[SerializeField] int _paletteIndex = 0;
	EnvironmentPalette _activePalette;
	public EnvironmentPalette ActivePalatte { get { return _activePalette; } }

	[SerializeField] List<EnvironmentPalette> _environmentPaletteList = new List<EnvironmentPalette>();
	public List<EnvironmentPalette> PalletteList { get { return _environmentPaletteList; } set { _environmentPaletteList = value; } }

	[SerializeField] Material terrainMaterial;
	[SerializeField] ParticleSystem groundSplatDecal;

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
        if (_paletteIndex >= _environmentPaletteList.Count)
        {
            _paletteIndex = _environmentPaletteList.Count - 1;
        }

        _activePalette = _environmentPaletteList[_paletteIndex];

        if ( ExecutePaletteChange != null )
        {
            ExecutePaletteChange( _activePalette );
        }
    }

	void EditorUpdatePalatte( int newPalatteIndex )
	{
		if( _paletteIndex != _environmentPaletteList.FindIndex( x => x.title == _activePalette.title ) )    // Should be b a better way to do this
		{
			_activePalette = _environmentPaletteList[_paletteIndex];

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
           
            if (ExecutePaletteChange != null)
            {
                ExecutePaletteChange( _activePalette );
            }
        }
	}

	/// <summary>
	/// Applies the three part gradient to our Gradient Shader
	/// </summary>
	/// <param name="material">Material.</param>
	/// <param name="gradient">Gradient.</param>
	public static void ApplyThreePartGradient( Material material, Gradient gradient )
	{
		Debug.Assert( material != null && material.HasProperty("Top Color"), "Must be a gradient shader!" );

		material.SetColor("Top Color", gradient.Evaluate(0f));
		material.SetColor("Mid Color", gradient.Evaluate(.5f));
		material.SetColor("Bot Color", gradient.Evaluate(1f));		
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

		EditorUpdatePalatte( _paletteIndex );
	}
}
