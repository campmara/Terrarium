using UnityEngine;

public class FootTrigger : MonoBehaviour 
{
	void OnTriggerEnter(Collider other)
	{
		GroundManager.instance.Ground.DrawSplatDecal(transform.position, 0.25f);
	}
}