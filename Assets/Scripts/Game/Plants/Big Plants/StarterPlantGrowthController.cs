using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarterPlantGrowthController : BPGrowthController
{
	[SerializeField] private Transform _bStemRoot = null;
	[SerializeField] private GameObject _leafPrefab = null;

	Vector3 _minScale = new Vector3( 0.0f, 0.0f, 0.0f );
	public Vector3 MinScale { get { return _minScale; }  set { _minScale = value; } }
	Vector3 _maxScale = new Vector3( 14.0f, 14.0f, 14.0f );

	private int _numChildren;
	private Transform[] _bones;

	int _curChildSpawned = 1;
	float _timeBetweenLeafSpawns = 0.0f;

	int _inverseIndex = 0;
	float _offset = 0.0f;
	int _ringNumber = 0;

	Transform _currentParent = null;
	Coroutine _leafSpawnRoutine = null;

	float _endTimeStamp = 0.0f;
	Animator _lastAnim = null;
	bool _waiting = false;

	void Awake()
	{
		_myPlant = GetComponent<BasePlant>();
		_controllerType = ControllerType.Growth;
        ColorManager.ExecutePaletteChange += HandlePalatteChange;
	}

    void OnDestroy()
    {
        ColorManager.ExecutePaletteChange -= HandlePalatteChange;
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

		for (int i = 0; i < _ringNumber; i++)
		{
			SetupLeaf( i );
		}

		yield return new WaitForSeconds( _timeBetweenLeafSpawns );

		_curChildSpawned++;
		_leafSpawnRoutine = null;
	}
		
	void SetupLeaf( int index )
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
	}

	protected override void CustomPlantGrowth()
	{
		if( transform.localScale.x < _maxScale.x )
		{
			transform.localScale = Vector3.Lerp( _minScale, _maxScale, Mathf.SmoothStep( 0, 1, _curPercentAnimated ) );
		}
			
		if( _leafSpawnRoutine == null && _curChildSpawned < _numChildren )
		{
			_leafSpawnRoutine = StartCoroutine( SpawnLeaves() );
		}

		if( _lastAnim )
		{
			if( _endTimeStamp >= _lastAnim.GetCurrentAnimatorStateInfo(0).normalizedTime )
			{
				_myPlant.SwitchController( this );
			}
		}
	}

	protected override void CustomStopGrowth()
	{
		if( !_waiting )
		{
			_lastAnim = _childAnimators[ _childAnimators.Count - 1 ];
			//_lastClip = _lastAnim.runtimeAnimatorController.animationClips[0];
			AnimatorStateInfo state = _lastAnim.GetCurrentAnimatorStateInfo(0);
			_endTimeStamp =  state.length / state.speed ;//_lastClip.length - .04f;

			StartCoroutine( WaitForLastLeaf() );
		}
	}

	private IEnumerator WaitForLastLeaf()
	{
		_waiting = true;
		while( _endTimeStamp > _lastAnim.GetCurrentAnimatorStateInfo(0).normalizedTime )
		{
			yield return null;
		}

		_waiting = false;
		_myPlant.SwitchController( this );
    }

    void HandlePalatteChange( ColorManager.EnvironmentPalette newPalatte )
    {

    }
}
