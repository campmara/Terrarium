using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SPGrowthController : PlantController 
{
	protected List<Animator> _childAnimators = new List<Animator>();

	protected float _spawnRadius = 2.5f;
	protected const float _timeBetweenSpawns = 5.0f;

	[SerializeField] const float _defaultInnerRadiusSize = 1.0f; // use for items you can't easily calculate the mesh size

	// plant n' growth vars
	[SerializeField, ReadOnlyAttribute] protected float _growthRate = 0.0f;
	[SerializeField] protected float _baseGrowthRate = .005f;
	[SerializeField] Vector2 _scaleRange = new Vector2( 8.0f, 12.0f ); // we want to let these vary per plant

	protected Rigidbody _rigidbody = null;

	public override void Init()
	{
		_myPlant = GetComponent<BasePlant>();
		_controllerType = ControllerType.Growth;

		float innerRad = GetComponent<BoxCollider>().bounds.size.x;
		_myPlant.InnerRadius = innerRad * transform.localScale.x;
		_myPlant.OuterRadius = 2.5f;
		_myPlant.SpawnHeight = .1f;
	}

	public override void StartState()
	{
		InitPlant();
	}
		
	protected virtual void InitPlant()
	{
		_myPlant = GetComponent<BasePlant>();
		// get mesh radius
		GetSetMeshRadius();

		_growthRate = _baseGrowthRate;
		_rigidbody = GetComponent<Rigidbody>();

		AssignRandomSize();
		CalculateObjectRadius();

		float randoScale = Random.Range( _scaleRange.x, _scaleRange.y);
		transform.localScale = new Vector3( randoScale, randoScale, randoScale );

		PlantManager.ExecuteGrowth += UpdateState;
	}

	//********************************
	// UPDATE GROWTH FUNCTIONS
	//********************************

	public override void UpdateState()
	{
		CustomPlantGrowth();
	}

	protected virtual void CustomPlantGrowth(){}

	//********************************
	// STOP GROWTH FUNCTIONS
	//********************************

	public override void StopState()
	{
		CustomStopGrowth();
	}

	protected virtual void CustomStopGrowth()
	{
		foreach( Animator child in _childAnimators )
		{
			child.enabled = false;
		}
	}
				
	//********************************
	// INTERACTIONS
	//********************************

	public override void WaterPlant(){}
	public override void TouchPlant(){}
	public override void GrabPlant(){}
	public override void StompPlant(){}

	//********************************
	// HELPER FUNCTIONS
	//********************************
	void CalculateObjectRadius()
	{
		// PLEASE FILL 
	}

	void AssignRandomSize()
	{
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
				_myPlant.InnerRadius = size.x * transform.GetChild(0).localScale.x;
			}
			else
			{
				_myPlant.InnerRadius = size.z * transform.GetChild(0).localScale.x;
			}
		}
		else
		{
			_myPlant.InnerRadius = _defaultInnerRadiusSize;
		}
	}

		void OnDrawGizmos() 
	{
		Gizmos.color = Color.yellow;
		//Gizmos.DrawCube( transform.position, new Vector3( _myPlant.InnerRadius, _myPlant.InnerRadius, _myPlant.InnerRadius));
	}
}
