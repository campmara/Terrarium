using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnsettlingNoiseManager : MonoBehaviour 
{
	[SerializeField] private AudioClip[] noises;
	private AudioSource source;

	private const float MIN_SWITCH_TIME = 20f;
	private const float MAX_SWITCH_TIME = 100f;
	[ReadOnly, SerializeField] private float switchTime;
	[ReadOnly, SerializeField] private float timer;

	void Awake()
	{
		source = GetComponent(typeof(AudioSource)) as AudioSource;

		switchTime = Random.Range(MIN_SWITCH_TIME, MAX_SWITCH_TIME);
	}

	void Update()
	{
		timer += Time.deltaTime;

		if (timer >= switchTime)
		{
			SwitchClips();

			switchTime = Random.Range(MIN_SWITCH_TIME, MAX_SWITCH_TIME);
			timer = 0f;
		}
	}

	void SwitchClips()
	{
		if (noises.Length > 0)
		{
			source.pitch = Random.Range(0.9f, 1.1f);
			source.Play();
			source.clip = noises[Random.Range(0, noises.Length)];
		}
	}
}
