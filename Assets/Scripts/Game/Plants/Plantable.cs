﻿using System.Collections; using System.Collections.Generic; using UnityEngine;  public class Plantable : MonoBehaviour { 	protected Animator _plantAnim = null; 	[SerializeField] List<GameObject> _spawnables = new List<GameObject>();  	protected const float _baseGrowthRate = .1f; 	protected const float _wateredGrowthRate = 6.0f;  	protected const float _timeBetweenSpawns = 25.0f; 	protected float _plantMeshRadius = 0.0f; 	protected float _spawnRadius = 2.0f;  	protected float _curTimestamp = 0.0f; 	protected float _curGrowthRate = 0.0f;  	protected float _animEndTime = 0.0f;  	protected Rigidbody _rigidbody = null;  	protected bool _canPickup = true; 	public bool CanPickup { get { return _canPickup; } }  	// SHARED FUNCTIONS BETWEEN CLASS TWOS AND THREES 	protected virtual void Awake() 	{ 		InitPlant(); 	}  	protected virtual void InitPlant() 	{ 		// get mesh radius 		GetSetMeshRadius();  		// set anim 		_plantAnim = GetComponent<Animator>();  		//set speed and end times 		_animEndTime = _plantAnim.GetCurrentAnimatorStateInfo(0).length; 		_plantAnim.speed = _baseGrowthRate; 		_curGrowthRate = _baseGrowthRate;  		_rigidbody = GetComponent<Rigidbody>(); 	} 		 	protected virtual void GetSetMeshRadius() 	{ 		Mesh plantMesh = GetComponentInChildren<MeshFilter>().mesh; 		Vector3 size = plantMesh.bounds.size; 		 		if( size.x > size.z ) 		{ 			_plantMeshRadius = size.x * transform.GetChild(0).localScale.x; 		} 		else 		{ 			_plantMeshRadius = size.z * transform.GetChild(0).localScale.x; 		} 	}  	protected void SituatePlant() 	{ 		transform.rotation = Quaternion.Euler(Vector3.zero); 		_rigidbody.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | 			RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | 			RigidbodyConstraints.FreezeRotationZ; 	}  	// VARIANT FUNCTIONS BETWEEN CLASS TWOS AND THREES  	public virtual void WaterPlant() 	{ 		// ups the rate if it's in a certain mode 	}  	public void ResetPlant() 	{ 		_curGrowthRate = _baseGrowthRate; 		_rigidbody.constraints = RigidbodyConstraints.None; 		PlantManager.ExecuteGrowth -= GrowPlant; 	}  	protected virtual void StartGrowth() 	{ 		PlantManager.ExecuteGrowth += GrowPlant; 	}  	protected virtual void StopGrowth() 	{ 		PlantManager.ExecuteGrowth -= GrowPlant; 		_plantAnim.Stop(); 	}  	public GameObject SpawnMiniPlant() 	{ 		GameObject newPlant = null;  		if( _spawnables.Count != 0 ) 		{ 			//what kind of radius do i want 			Vector2 randomPoint = Random.insideUnitCircle * _spawnRadius; 			Vector3 spawnPoint = new Vector3( randomPoint.x, .1f, randomPoint.y ) + transform.position; 			Vector3 direction = ( spawnPoint - transform.position ).normalized * ( _plantMeshRadius ); 			spawnPoint += direction; 			 			newPlant = (GameObject)Instantiate( _spawnables[0], spawnPoint, Quaternion.identity ); 		}  		PlantManager.instance.RequestSpawnMini( this, _timeBetweenSpawns );  		if( newPlant == null ) 		{ 			Debug.Log("spawning minis plant messed up "); 		} 		return newPlant; 	}  	public virtual void GrowPlant() 	{ 		_curTimestamp = _plantAnim.GetCurrentAnimatorStateInfo(0).normalizedTime * _animEndTime;  		if( _curTimestamp >= _animEndTime ) 		{ 			PlantManager.ExecuteGrowth -= GrowPlant; 			PlantManager.instance.RequestSpawnMini( this, _timeBetweenSpawns ); 		} 	}  	void OnDrawGizmos()  	{ 		Gizmos.color = Color.blue; 		//Gizmos.DrawWireSphere( transform.position, _plantMeshRadius ); 	} }  