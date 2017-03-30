using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class EnvironmentColorMan : MonoBehaviour {

    public Material terrainMaterial;

    [System.Serializable]
    public struct EnvironmentPalette
    {
        public string title;
        [Header("Ground colors")]
        public Color groundColorPrimary;
        public Color groundColorSecondary;
        [Header("Terrain colors")]
        public Color terrainColor;
        [Header("Sky / Fog colors")]
        public Color fogColor;
        public Color skyColor;
        public Color cloudRimColor;
    }

    public int currentPalette;

    public List<EnvironmentPalette> palettes;

    void Update () {
        Shader.SetGlobalColor("_GroundColorPrimary", palettes[currentPalette].groundColorPrimary);
        Shader.SetGlobalColor("_GroundColorSecondary", palettes[currentPalette].groundColorSecondary);

        RenderSettings.fogColor = palettes[currentPalette].fogColor;
        RenderSettings.skybox.SetColor("_Color1", palettes[currentPalette].cloudRimColor);
        RenderSettings.skybox.SetColor("_Color2", palettes[currentPalette].skyColor);

        if (terrainMaterial)
        {
            terrainMaterial.SetColor("_Color", palettes[currentPalette].terrainColor);
        }
    }
}
