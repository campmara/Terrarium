using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterDroplet : Pickupable 
{
    [SerializeField] GameObject modelObj;
    Material dropletMat;

    Vector3 position;
    Vector3 prevPosition;

    float desiredExtrusion = 0f;
    float currentExtrusion = 0f;

	protected override void Awake()
	{
		base.Awake();

        dropletMat = modelObj.GetComponent<MeshRenderer>().sharedMaterial;

        position = transform.position;
        prevPosition = transform.position;
	}

    void Update()
    {
        prevPosition = position;
        position = transform.position;

        dropletMat.SetVector("_Position", position);
        dropletMat.SetVector("_PrevPosition", prevPosition);

        currentExtrusion = Mathf.Lerp(currentExtrusion, desiredExtrusion, 7f * Time.deltaTime);
        dropletMat.SetFloat("_PushAmount", currentExtrusion);

        Vector3 leftSide = -modelObj.transform.right * 0.6f;
        Vector3 rightSide = modelObj.transform.right * 0.6f;
        dropletMat.SetVector("_PushPosA", rightSide);
        dropletMat.SetVector("_PushPosB", leftSide);
    }

	// This gets called when we pick up the object. Pickupable controls its own rigidbody.
    public override void OnPickup()
    {
        base.OnPickup();

        transform.rotation = Quaternion.identity;
        modelObj.transform.rotation = Quaternion.identity;

        desiredExtrusion = 1f;
    }

    public override void DropSelf()
    {
        base.DropSelf();

        desiredExtrusion = 0f;
    }
}
