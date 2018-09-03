using UnityEngine;
using System.Collections;

public class DestroySelf : MonoBehaviour 
{
	[SerializeField] float _delayTime = 0f;

	void Awake()
	{
		StartCoroutine(DelayedDestroySelf());
	}

	IEnumerator DelayedDestroySelf()
	{
		yield return new WaitForSecondsRealtime(_delayTime);
		Destroy(this.gameObject);
	}
}
