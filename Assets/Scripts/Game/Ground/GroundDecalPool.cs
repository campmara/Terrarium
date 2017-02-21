using UnityEngine;

public class GroundDecalPool : MonoBehaviour 
{
	private int dataIndex;
	private ParticleSystem decalSystem;
	private ParticleSystem.EmitParams[] decalParticles;

	void Awake()
	{
		decalSystem = GetComponent(typeof(ParticleSystem)) as ParticleSystem;
		decalParticles = new ParticleSystem.EmitParams[decalSystem.main.maxParticles];

		dataIndex = 0;
	}

	public void AddDecal(Vector3 pos, float size)
	{
		if (dataIndex >= decalSystem.main.maxParticles)
		{
			dataIndex = 0;
		}

		decalParticles[dataIndex].position = pos + (Vector3.up * 0.005f);
		decalParticles[dataIndex].startSize = Random.Range(size - 0.1f, size + 0.1f);

		Vector3 rot = Quaternion.LookRotation(Vector3.up, Vector3.up).eulerAngles;
		rot.z = Random.Range(0f, 360f);
		decalParticles[dataIndex].rotation3D = rot;

		//decalParticles[dataIndex].startColor = gradient.Evaluate(Random.Range(0f, 1f));
		
		decalSystem.Emit(decalParticles[dataIndex], 1);

		dataIndex++;
	}
}
