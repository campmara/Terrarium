using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plantable : MonoBehaviour
{
	// class 2/3's need things to spawn && also have children objects with animators
    [SerializeField] private List<GameObject> _spawnables = new List<GameObject>();
	protected List<Animator> _childAnimators = new List<Animator>();

	//we use inner mesh radius and outer spawn radius to calculate where spawnables can spawn
    protected float _innerMeshRadius = 1.0f;
    protected float _outerSpawnRadius = 2.0f;
	public float OuterSpawnRadius { get { return _outerSpawnRadius; } }
	[SerializeField] const float _defaultInnerRadiusSize = 1.0f; // use for items you can't easily calculate the mesh size
	public float MinDistAway{ get { return _minDistAway; } }
	protected float _minDistAway = 2.0f; // use for items you can't easily calculate the mesh size

    protected const float _timeBetweenSpawns = 25.0f;
    [SerializeField] protected float _baseGrowthRate = .005f;
	[SerializeField] Vector2 _scaleRange = new Vector2( 8.0f, 12.0f); // we want to let these very per plant
    protected const float _wateredGrowthRate = .2f;
	protected const int _maxMinisSpawned = 5;


    protected float _curGrowthRate = 0.1f;
    protected Rigidbody _rigidbody = null;
	protected int _minisSpawned = 0;
    // SHARED FUNCTIONS BETWEEN CLASS TWOS AND THREES
    protected virtual void Awake()
    {
        InitPlant();
    }

    protected virtual void InitPlant()
    {
        // get mesh radius
        GetSetMeshRadius();
   
        _curGrowthRate = _baseGrowthRate;
        _rigidbody = GetComponent<Rigidbody>();

		float randoScale = Random.Range( _scaleRange.x, _scaleRange.y);
		transform.localScale = new Vector3( randoScale, randoScale, randoScale );
    }

    protected virtual void GetSetMeshRadius()
    {
        MeshFilter meshFilter = GetComponentInChildren<MeshFilter>();

        if( meshFilter )
        {
            Mesh plantMesh = meshFilter.mesh;
            Vector3 size = plantMesh.bounds.size;

            if( size.x > size.z )
            {
                _innerMeshRadius = size.x * transform.GetChild(0).localScale.x;
            }
            else
            {
                _innerMeshRadius = size.z * transform.GetChild(0).localScale.x;
            }
        }
        else
        {
            _innerMeshRadius = _defaultInnerRadiusSize;
        }
    }

    protected void SituatePlant()
    {
        transform.rotation = Quaternion.Euler(Vector3.zero);
        _rigidbody.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ |
                                 RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY |
                                 RigidbodyConstraints.FreezeRotationZ;
    }

    // VARIANT FUNCTIONS BETWEEN CLASS TWOS AND THREES

    public virtual void WaterPlant()
    {
        // ups the rate if it's in a certain mode
    }

    public void ResetPlant()
    {
        _curGrowthRate = _baseGrowthRate;
        _rigidbody.constraints = RigidbodyConstraints.None;
        PlantManager.ExecuteGrowth -= GrowPlant;
    }

    protected virtual void StartGrowth()
    {
        PlantManager.ExecuteGrowth += GrowPlant;
    }

    protected virtual void StopGrowth()
    {
        PlantManager.ExecuteGrowth -= GrowPlant;
       
		foreach( Animator child in _childAnimators )
		{
			child.enabled = false;
		}

		//ManageSkinMeshRenderers();
		PlantManager.instance.RequestSpawnMini( this, _timeBetweenSpawns );
    }

	void ManageSkinMeshRenderers()
	{

		// THIS FUNCTION HAS SOME PROBLEMS RIGHT NOW SO HOLD OFF
		foreach( Animator child in _childAnimators )
		{
			//bake mesh
			SkinnedMeshRenderer skinRenderer = child.gameObject.GetComponentInChildren<SkinnedMeshRenderer>();
			Mesh swapMesh = new Mesh();
			skinRenderer.BakeMesh( swapMesh );
			Material[] materials = skinRenderer.materials;

			//get rid of skinned mesh renderer
			GameObject leafObj = child.transform.GetChild(1).gameObject;
			MeshRenderer newRenderer= (MeshRenderer)leafObj.AddComponent<MeshRenderer>();
			MeshFilter newFilter = (MeshFilter)leafObj.AddComponent<MeshFilter>();
			newFilter.mesh = swapMesh;
	
			Material[] mats = newRenderer.materials;
			for( int i = 0; i < materials.Length; i++ )
			{
				mats[i] = materials[i];
			}
			newRenderer.materials = mats;

			Destroy( skinRenderer );
		}
	}

    public GameObject SpawnMiniPlant()
    {
        GameObject newPlant = null;

        if( _spawnables.Count != 0 )
        {
            //what kind of radius do i want
            Vector2 randomPoint = Random.insideUnitCircle * _outerSpawnRadius;
            Vector3 spawnPoint = new Vector3( randomPoint.x, .1f, randomPoint.y ) + transform.position;
            Vector3 direction = ( spawnPoint - transform.position ).normalized * ( _innerMeshRadius );
            spawnPoint += direction;
			spawnPoint = new Vector3( spawnPoint.x, .1f, spawnPoint.z );

			newPlant = (GameObject)Instantiate( _spawnables[Random.Range( 0, _spawnables.Count - 1)], spawnPoint, Quaternion.identity );
        }

		_minisSpawned++;

		if( _minisSpawned < _maxMinisSpawned )
		{
			PlantManager.instance.RequestSpawnMini( this, _timeBetweenSpawns );
		}

        if( newPlant == null )
        {
            Debug.Log("spawning minis plant messed up ");
        }

        return newPlant;
    }

    public virtual void GrowPlant()
    {
        //_curTimestamp = _plantAnim.GetCurrentAnimatorStateInfo(0).normalizedTime * _animEndTime;
		//
        //if( _curTimestamp >= _animEndTime )
        //{
        //    PlantManager.ExecuteGrowth -= GrowPlant;
        //    PlantManager.instance.RequestSpawnMini( this, _timeBetweenSpawns );
        //}
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        //Gizmos.DrawWireSphere( transform.position, _plantMeshRadius );
    }
}