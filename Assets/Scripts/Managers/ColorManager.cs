using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorManager : SingletonBehaviour<ColorManager> {

	[System.Serializable]
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

		//making a system for these is going to require more organization / thought.
		/*
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
        */
	}
		
	public enum PaletteIndex : int
	{
		NONE = -1,
		DESERT,
		CLASSIC,
		ABZU,
		RED_MOON
	}
	[SerializeField] PaletteIndex _currentPalette = PaletteIndex.ABZU;


	[SerializeField] List<EnvironmentPalette> _environmentPaletteList = new List<EnvironmentPalette>();
	public List<EnvironmentPalette> PalletteList { get { return _environmentPaletteList; } set { _environmentPaletteList = value; } }

	void Awake () 
	{
		
	}

	public override void Initialize ()
	{

		isInitialized = true;
	}
}
