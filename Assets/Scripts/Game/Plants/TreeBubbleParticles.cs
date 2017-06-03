using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeBubbleParticles : MonoBehaviour 
{
	ParticleSystem bubbles;
	BigPlantPickupable parentPickupable;

	bool readyToLetLoose;

	void Awake()
	{
		bubbles = GetComponent(typeof(ParticleSystem)) as ParticleSystem;
	}

	public void Setup(BigPlantPickupable parent)
	{
		parentPickupable = parent;
		parentPickupable.SetupBubbles(this);
	}

	void Update()
	{
		if (readyToLetLoose == false && parentPickupable.TreeShaken)
		{
			readyToLetLoose = true;
			LetBubblesGo(5);
		}
	}

	public void OnPlantLetGo()
	{
		LetBubblesGo(10);
	}

	void LetBubblesGo(int bubblesToLetGo)
	{
		ParticleSystem.Particle[] particles = new ParticleSystem.Particle[bubbles.particleCount];
		bubbles.GetParticles(particles);

		if (bubblesToLetGo > bubbles.particleCount)
		{
			bubblesToLetGo = bubbles.particleCount;
		}

		for (int i = 0; i < bubblesToLetGo; i++)
		{
			int bubbleIndex = Random.Range(0, bubbles.particleCount);

			particles[i].remainingLifetime = 0f;
		}
	}

	public void KillAllBubbles()
	{
		ParticleSystem.Particle[] particles = new ParticleSystem.Particle[bubbles.particleCount];
		bubbles.GetParticles(particles);

		for (int i = 0; i < bubbles.particleCount; i++)
		{
			particles[i].remainingLifetime = 0f;
		}
	}

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
