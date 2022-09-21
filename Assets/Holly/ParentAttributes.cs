using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParentAttributes : MonoBehaviour {

    public Transform parent;

    public bool parentPosition;
    public bool parentRotation;
    public bool parentScale;

    public Vector3 scaleOffset;
    public Vector3 positionOffset;
    public Quaternion rotationOffset;

	void Update () {
        if (parent)
        {
            if (parentScale)
            {
                transform.localScale = parent.localScale + scaleOffset;
            }
            if (parentRotation)
            {
                transform.rotation = parent.rotation * rotationOffset;
            }
            if (parentPosition)
            {
                transform.position = parent.position + positionOffset;
            }
        }
	}
}
