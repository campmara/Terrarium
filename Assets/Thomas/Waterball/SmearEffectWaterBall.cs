using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SmearEffectWaterBall : MonoBehaviour
{
    Queue<Vector3> _recentPositions = new Queue<Vector3>();

    [SerializeField]
    int _frameLag = 0;

    Material _smearMat = null;
    public Material smearMat
    {
        get
        {
            if (!_smearMat)
                _smearMat = GetComponent<MeshRenderer>().material;

            if (!_smearMat.HasProperty("_PrevPosition"))
                _smearMat.shader = Shader.Find("Custom/Smear");

            return _smearMat;
        }
    }

    void LateUpdate()
    {
        GetComponent<MeshRenderer>().material.SetVector("_NoiseOffset", new Vector3(0, 1 - Time.time, 0));
        if (_recentPositions.Count > _frameLag)
            smearMat.SetVector("_PrevPosition", _recentPositions.Dequeue());

        smearMat.SetVector("_Position", transform.position);
        _recentPositions.Enqueue(transform.position);
    }
}