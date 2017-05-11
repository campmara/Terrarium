using UnityEngine;

public class GroundDecalPool : MonoBehaviour 
{
	private int dataIndex;
	private ParticleSystem decalSystem;
	private ParticleSystem.EmitParams[] decalParticles;

    [SerializeField]
    bool adjustScale = true;
    [SerializeField]
    private Vector2 startSizeOffsetRange = new Vector2( -0.1f, 0.1f );

    [SerializeField]
    bool adjustStartPos = true;
    [SerializeField]
    private float startPosYOffsetScalar = 0.005f;



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

        decalParticles[dataIndex].position = pos;
        if ( adjustStartPos )
        {            
            decalParticles[dataIndex].position += ( Vector3.up * startPosYOffsetScalar );
        }        

        if ( adjustScale )
        {
            decalParticles[dataIndex].startSize = Random.Range( size + startSizeOffsetRange.x, size + startSizeOffsetRange.y );
        }
        
		Vector3 rot = Quaternion.LookRotation(Vector3.down, Vector3.up).eulerAngles;
        //Vector3 rot = Vector3.zero;
        rot.z = Random.Range(0f, 360f);
		decalParticles[dataIndex].rotation3D = rot;

		//decalParticles[dataIndex].startColor = gradient.Evaluate(Random.Range(0f, 1f));
		
		decalSystem.Emit(decalParticles[dataIndex], 1);

		dataIndex++;
	}
}
