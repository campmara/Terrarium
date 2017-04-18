using System.Collections;
using UnityEngine;

public class SplashImprint : MonoBehaviour 
{
	Material splashMat;

	void Awake()
	{
		splashMat = GetComponent<MeshRenderer>().material;
		SetSplatAlpha(0f);

		StartCoroutine(FadeRoutine());
	}

	private IEnumerator FadeRoutine()
	{
		float alpha = 0f;
		SetSplatAlpha(0f);

		float timer = 0f;
		float totalTime = 7.5f;

		while (timer < totalTime)
		{
			timer += Time.deltaTime;

			alpha = Mathf.Lerp(0f, 1f, timer / totalTime);

			SetSplatAlpha(alpha);

			yield return  null;
		}

		Destroy(this.gameObject);
	}

	void SetSplatAlpha(float alpha)
	{
		splashMat.SetColor("_SubtractiveColor", new Color(0, 0, 0, alpha));
	}
}
