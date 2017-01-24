using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plantable : Pickupable
{
	[SerializeField] List<GameObject> _spawnables = new List<GameObject>();

	protected const float _baseGrowthRate = 1.0f;
	protected const float _maxGrowthRate = 6.0f;
	protected const float _decayRate = .01f;
	protected const float _waterMultiplier = 3.0f;

	protected const float _timeBetweenSpawns = 5.0f;
	protected const float _minisRadiusMultiplier = 3.0f;

	protected float _plantMeshRadius = 0.0f;
	protected float _curGrowthRate = 1.0f;
	protected float _curGrowTime = 0.0f;
	protected float _endTime = 5.0f;

	protected bool _canPickup = true;
	public bool CanPickup { get { return _canPickup; } }

	// SHARED FUNCTIONS BETWEEN CLASS TWOS AND THREES
	protected override void Awake()
	{
		base.Awake();
		InitPlant();
	}

	protected virtual void InitPlant()
	{
		GetSetMeshRadius();
	}

	public override void DropSelf()
	{
		base.DropSelf();
		SituatePlant();
		PlantManager.ExecuteGrowth += GrowPlant;
	}

	public override void OnPickup()
	{
		// NOTE: WALKING STATE CHECKS CANPICKUP TO SEE IF WE CAN PICKUP BEFORE PICKING UP 
		base.OnPickup();
		ResetPlant();
		PlantManager.ExecuteGrowth -= GrowPlant;
	}

	protected void GetSetMeshRadius()
	{
		Mesh plantMesh = GetComponentInChildren<MeshFilter>().mesh;
		Vector3 size = plantMesh.bounds.size;

		if( size.x > size.z )
		{
			_plantMeshRadius = size.x * transform.GetChild(0).localScale.x;
		}
		else
		{
			_plantMeshRadius = size.z * transform.GetChild(0).localScale.x;
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
		_curGrowthRate *= _waterMultiplier;
	}

	public void ResetPlant()
	{
		_curGrowthRate = _baseGrowthRate;
		_curGrowTime = 0.0f;

		_rigidbody.constraints = RigidbodyConstraints.None;

		PlantManager.ExecuteGrowth -= GrowPlant;
	}

	public virtual void EatPlant(){}

	public GameObject SpawnMiniPlant()
	{
		GameObject newPlant = null;

		if( _spawnables.Count != 0 )
		{
			//what kind of radius do i want
			Vector2 randomPoint = Random.insideUnitCircle * _minisRadiusMultiplier;
			Vector3 spawnPoint = new Vector3( randomPoint.x, 2.0f, randomPoint.y ) + transform.position;
			Vector3 direction = ( spawnPoint - transform.position ).normalized * ( _plantMeshRadius );
			spawnPoint += direction;

			newPlant = (GameObject)Instantiate( _spawnables[0], spawnPoint, Quaternion.identity );
		}

		PlantManager.instance.RequestSpawnMini( this, _timeBetweenSpawns );

		if( newPlant == null )
		{
			Debug.Log("this is messed up ");
		}
		return newPlant;
	}

	public virtual void GrowPlant()
	{
		if( _curGrowTime >= _endTime )
		{
			PlantManager.ExecuteGrowth -= GrowPlant;
			PlantManager.instance.RequestSpawnMini( this, _timeBetweenSpawns );
		}
		else
		{
			_curGrowTime += _curGrowthRate * Time.deltaTime;
		}
	}

	void OnDrawGizmos() 
	{
		Gizmos.color = Color.blue;
		Gizmos.DrawWireSphere( transform.position, _plantMeshRadius );
	}
}
