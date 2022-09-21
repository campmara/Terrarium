﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SplatmapCamera : MonoBehaviour 
{
    private Camera cam;
    public Texture border;
    public Color neutralColor;

    private void Start()
    {
        cam = GetComponent<Camera>();
    }

    void Update () {
        Shader.SetGlobalTexture("_SplatMap", cam.targetTexture);
        Shader.SetGlobalTexture("_ClipEdges", border);
        Shader.SetGlobalColor("_SplatmapNeutralColor", neutralColor);
        Shader.SetGlobalFloat("_OrthoCameraScale", cam.orthographicSize);
        Shader.SetGlobalVector("_CameraWorldPos", transform.position);

        transform.position = CameraManager.instance.FocusPoint;
        transform.rotation = Quaternion.LookRotation(Vector3.down, Vector3.forward);
    }
}
