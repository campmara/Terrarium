using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarterPlant : Growable 
{
	[SerializeField] private Transform _bStemRoot = null;
	[SerializeField] private GameObject _leafPrefab = null;

	private int _numChildren;
	private Transform[] _bones;

	int _curChildSpawned = 1;
	Coroutine leafSpawnRoutine = null;
	float _timeBetweenLeafSpawns = 0.0f;

	protected override void AnimationSetup()
	{
		_bones = _bStemRoot.GetComponentsInChildren<Transform>();
		_numChildren = _bones.Length; // we subtract one for them that exists there

		AnimatorStateInfo info = _plantAnim.GetCurrentAnimatorStateInfo(0);
		_timeBetweenLeafSpawns = ( info.length / _baseGrowthRate ) / _numChildren;

		for (int i = 0; i < _plantAnim.layerCount; i++)
		{
			_plantAnim.SetLayerWeight( i, Random.Range(0, 2) );
		}
	}

	private IEnumerator SpawnLeaves()
	{
		int inverseIndex = _numChildren - _curChildSpawned;
		Transform currentParent = _bones[inverseIndex];

		float offset = Random.Range( 0, 100 );
		int ringnumber = Random.Range( 5, 8 );

		for (int i = 0; i < ringnumber; i++)
		{
			GameObject l = Instantiate(_leafPrefab);
			l.transform.SetParent(currentParent);
			l.transform.position = currentParent.position;
			l.transform.localScale = currentParent.localScale * inverseIndex * .2f;//(inverseIndex * inverseIndex * .05f);
			l.transform.Rotate(new Vector3(0,i*360/ringnumber + offset, 0));
			l.transform.position -= l.transform.forward * .015f * transform.localScale.x;
			l.GetComponent<Animator>().speed *= _plantAnim.GetComponent<Animator>().speed;
		}

		yield return new WaitForSeconds( _timeBetweenLeafSpawns );

		_curChildSpawned++;
		leafSpawnRoutine = null;
	}
		
	protected override void CustomPlantGrowing()
	{
		if( leafSpawnRoutine == null && _curChildSpawned < _numChildren )
		{
			leafSpawnRoutine = StartCoroutine( SpawnLeaves() );
		}
	}
}
