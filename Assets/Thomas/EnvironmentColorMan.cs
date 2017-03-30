using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class EnvironmentColorMan : MonoBehaviour {

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
        
        [Header("Plant colors")]
        [Header("Class 1")]
        public Gradient mossPlant;
        public Gradient pointPlant;
        [Header("Class 2")]
        public Gradient leafyGroundPlantBulb;
        public Gradient leafyGroundPlantLeaf;
        [Header("Class 3")]
        public Gradient twistPlant;
        public Gradient cappPlant;
        
    }

    public int currentPalette;
    public List<EnvironmentPalette> palettes;
    public Material terrainMaterial;

    
    //this is a terrible way to deal with this please help me find a better way
    //basically anything that references a single item is a problem.
    //...
    
    //3 gradient
    public Material mossPlantMaterial;
    public Material pointPlantMaterial;
    public Material leafyGroundPlantBulbMaterial;
    public Material leafyGroundPlantLeafMaterial;

    //2 gradient
    public Material cappPlantMaterial;
    public Material twistPlantMaterial;

    //one color
    public Material pondMaterial;
    //...
    

    public ParticleSystem groundDecal;
    private Color decalStartColor;

    void Update () {
        EnvironmentPalette palette = palettes[currentPalette];

        //groundcolor
        Shader.SetGlobalColor("_GroundColorPrimary", palette.groundColorPrimary);
        Shader.SetGlobalColor("_GroundColorSecondary", palette.groundColorSecondary);
        Shader.SetGlobalColor("_GroundColorSecondary", palette.groundColorSecondary);


        if (groundDecal)
        {
            ParticleSystem.MainModule groundDecalMain = groundDecal.main;
            groundDecalMain.startColor = new Color(
                palette.groundColorSecondary.r * palette.groundDecalTint.r,
                palette.groundColorSecondary.g * palette.groundDecalTint.g,
                palette.groundColorSecondary.b * palette.groundDecalTint.b,
                1);
        }

        //skybox
        RenderSettings.fogColor = palette.fogColor;
        RenderSettings.skybox.SetColor("_Color1", palette.cloudRimColor);
        RenderSettings.skybox.SetColor("_Color2", palette.skyColor);

        //terrain
        if (terrainMaterial)
        {
            terrainMaterial.SetColor("_Color", palette.terrainColor);
        }
        if (pondMaterial)
        {
            pondMaterial.SetColor("_Color", palette.terrainColor);
        }


        //plants
        //3 part
        ApplyThreePartGradient(mossPlantMaterial, palette.mossPlant);
        ApplyThreePartGradient(pointPlantMaterial, palette.pointPlant);
        ApplyThreePartGradient(leafyGroundPlantBulbMaterial, palette.leafyGroundPlantBulb);
        ApplyThreePartGradient(leafyGroundPlantLeafMaterial, palette.leafyGroundPlantLeaf);

        //2 part
        ApplyTwoPartGradient(twistPlantMaterial, palette.twistPlant);
        ApplyTwoPartGradient(cappPlantMaterial, palette.cappPlant);
        
}

    
    void ApplyThreePartGradient(Material mat, Gradient grad)
    {
        if (mat)
        {
            mat.SetColor("_ColorTop", grad.Evaluate(0f));
            mat.SetColor("_ColorMid", grad.Evaluate(.5f));
            mat.SetColor("_ColorBot", grad.Evaluate(1f));
        }
    }
    
    void ApplyTwoPartGradient(Material mat, Gradient grad)
    {
        if (mat)
        {
            mat.SetColor("_Color", grad.Evaluate(0f));
            mat.SetColor("_Color2", grad.Evaluate(1f));
        }
    }
    
}
