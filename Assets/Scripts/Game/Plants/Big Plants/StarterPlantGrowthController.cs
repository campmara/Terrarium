using System.Collections;
using UnityEngine;

public class StarterPlantGrowthController : BPGrowthController
{
	[SerializeField] private Transform _bStemRoot = null;
	[SerializeField] private GameObject _leafPrefab = null;

	private int _numChildren;
	private Transform[] _bones;

	int _curChildSpawned = 1;
	float _timeBetweenLeafSpawns = 0.0f;

	int _inverseIndex = 0;
	float _offset = 0.0f;
	int _ringNumber = 0;

	Transform _currentParent = null;
	Coroutine _leafSpawnRoutine = null;
	Animator _lastAnim = null;

	void Awake()
	{
		_myPlant = GetComponent<BasePlant>();
		_controllerType = ControllerType.Growth;
	}

	protected override void InitPlant()
	{
		base.InitPlant();

		_bones = _bStemRoot.GetComponentsInChildren<Transform>();
		_numChildren = _bones.Length; // we subtract one for them that exists there

		AnimatorStateInfo info = _plantAnim.GetCurrentAnimatorStateInfo( 0 );
		_timeBetweenLeafSpawns = ( info.length / _baseGrowthRate ) / _numChildren;

		for( int i = 0; i < _plantAnim.layerCount; i++ )
		{
			_plantAnim.SetLayerWeight( i, Random.Range( 0, 2 ) );
		}
	}

	private IEnumerator SpawnLeaves()
	{
		_inverseIndex = _numChildren - _curChildSpawned;
		_currentParent = _bones[ _inverseIndex];

		_offset = Random.Range( 0, 100 );
		_ringNumber = Random.Range( 5, 8 );
		Animator leafAnim = null;

		for (int i = 0; i < _ringNumber; i++)
		{
			leafAnim = SetupLeaf( i );
		}

		yield return new WaitForSeconds( _timeBetweenLeafSpawns );

		_curChildSpawned++;
		_leafSpawnRoutine = null;

		if( _numChildren == _curChildSpawned )
		{
			_lastAnim = leafAnim;
		}
	}
		
	Animator SetupLeaf( int index )
	{
		GameObject leaf = Instantiate( _leafPrefab);
		Animator anim = leaf.GetComponent<Animator>();
		_childAnimators.Add( anim );
		leaf.transform.SetParent( _currentParent );
		leaf.transform.position = _currentParent.position;

		leaf.transform.localScale = _currentParent.localScale * _inverseIndex * .2f;//(inverseIndex * inverseIndex * .05f);
		leaf.transform.Rotate(new Vector3(0, index * 360 / _ringNumber + _offset, 0));
		leaf.transform.position -= leaf.transform.forward * .015f * transform.localScale.x;
		anim.speed *= _plantAnim.GetComponent<Animator>().speed;

		return anim;
	}

	protected override void CustomPlantGrowth()
	{
		if( _leafSpawnRoutine == null && _curChildSpawned < _numChildren )
		{
			_leafSpawnRoutine = StartCoroutine( SpawnLeaves() );
		}

		if( _lastAnim )
		{
			if( _lastAnim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f )
			{
				_myPlant.SwitchController( this );
			}
		}
	}
}
