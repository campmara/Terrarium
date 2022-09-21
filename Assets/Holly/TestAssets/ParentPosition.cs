using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParentPosition : MonoBehaviour {

    public Transform target;
    private Quaternion startRotation;

    private void Start()
    {
        startRotation = transform.rotation;
    }

	void Update () {
        transform.position = target.position;
        transform.rotation = startRotation * target.localRotation;
	}
}
