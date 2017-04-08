using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EssenceParticles : MonoBehaviour 
{
	public void MarkForDestroy(float delay)
	{
		StartCoroutine(Destroy(delay));
	}

	IEnumerator Destroy(float delay)
	{
		yield return new WaitForSeconds(delay);
		Destroy(gameObject);
	}
}
