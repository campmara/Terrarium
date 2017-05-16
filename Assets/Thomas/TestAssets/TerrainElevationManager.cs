using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TerrainElevationManager : MonoBehaviour {

    public Texture terrainHeightMap;
    public float terrainHeightMultiplier;
    public float terrainScale;

    void Update () {
        Shader.SetGlobalTexture("_TerrainHeightMap", terrainHeightMap);
        Shader.SetGlobalFloat("_TerrainHeightMultiplier", terrainHeightMultiplier);
        Shader.SetGlobalFloat("_TerrainScale", terrainScale);
    }
}
