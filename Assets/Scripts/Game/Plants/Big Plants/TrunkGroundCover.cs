using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TrunkGroundCover : MonoBehaviour 
{
	[SerializeField] GameObject _groundCoverPrefab = null;
	Transform _parent;
	Vector3 _parentPos = Vector3.zero;
	Vector2 _smallScaleRange = new Vector2( .75f, .8f );
	Vector2 _largeScaleRange = new Vector2( .8f, 1.1f );
	Vector2 _scaleRange = Vector2.zero;
	float _parentMeshRadius = 1.0f;
	float _maxDistFromParent = 5.0f;
	float _timeBetweenSpawns = .5f;
	float _grassHeight = 0.2f;

	int _numTrunkSpawns = 12;
	int _numSurroundingSpawns = 22;

	int _curTrunkSpawns = 0;
	int _curSurroundingSpawns = 0;


	public void SetupSpawner( BasePlant myTree )
	{
		_parentMeshRadius = myTree.GetComponent<BoxCollider>().size.x * myTree.transform.localScale.x * .67f;
		_numTrunkSpawns = Mathf.FloorToInt( myTree.transform.localScale.x * .7f  );
		_parentPos = myTree.transform.position;
		_parent = myTree.transform;

		if( _parentMeshRadius < 1.25f )
		{
			_scaleRange = _smallScaleRange;
		}
		else
		{
			_scaleRange = _largeScaleRange;
		}

		StartCoroutine( SpawnGroundCoverAroundTrunk() );
	}

	IEnumerator SpawnGroundCoverAroundTrunk()
	{
		// go around circle and spawn
		while( _curTrunkSpawns < _numTrunkSpawns )
		{
			SpawnGrass( GetPointAroundTrunk(), true );
			_curTrunkSpawns++;
			yield return new WaitForSeconds( _timeBetweenSpawns );
		}

		StartCoroutine( SpawnSurroundingGroundCover() );
	}
	
	IEnumerator SpawnSurroundingGroundCover()
	{
		while( _curSurroundingSpawns < _numSurroundingSpawns )
		{
			Vector2 randomPoint = Random.insideUnitCircle * _maxDistFromParent;
			Vector3 pos = _parentPos + new Vector3( randomPoint.x, _grassHeight, randomPoint.y );
			SpawnGrass( pos, false );
			_curSurroundingSpawns++;

			yield return new WaitForSeconds( _timeBetweenSpawns );
		}

		yield return null;
	}

	void SpawnGrass( Vector3 spawnPoint, bool trunkPlant )
	{
		float randomScale = Random.Range( _scaleRange.x, _scaleRange.y );
        Transform grass = ((GameObject)Instantiate( _groundCoverPrefab, spawnPoint, Quaternion.identity )).transform;
		float yPos = trunkPlant ? _parentPos.y + .5f : -2.0f;
		Vector3 parentLookAt = new Vector3( _parentPos.x, yPos, _parentPos.z );
		grass.localScale = Vector3.zero;
		grass.LookAt( parentLookAt );
		grass.DOScale( randomScale, 2.5f ).OnComplete( () => grass.parent = transform  );
	}

	Vector3 GetPointAroundTrunk()
	{
         float ang = 360.0f / _numTrunkSpawns * _curTrunkSpawns;
		 float offset = Random.Range( 0.0f, 10.0f );
		 ang += offset;
         Vector3 pos;
         pos.x = _parentPos.x + _parentMeshRadius * Mathf.Sin( ang * Mathf.Deg2Rad );
         pos.z = _parentPos.z + _parentMeshRadius * Mathf.Cos( ang * Mathf.Deg2Rad );
         pos.y = _grassHeight;
         return pos;
	}
}
