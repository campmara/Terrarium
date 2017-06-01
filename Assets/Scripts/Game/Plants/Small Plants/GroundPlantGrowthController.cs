using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class GroundPlantGrowthController : SPGrowthController 
{
	[SerializeField] GameObject _leaf;
	[SerializeField] GameObject _fruitPrefab;

	const int _numLeaves = 5;
	const int _layerCount = 2;

	float _waitTime = 0.0f;
	float _leafScale = 0.0f;
	Animator _lastAnim = null;
	bool _waiting = false;
	bool _closingLeaves = false;
	float _closeSpeed = .32f;

	float _singBufferTime = .5f;
	float _enterTime = 0.0f;
	GameObject _curFruit = null;
	protected override void InitPlant()
	{
		base.InitPlant();
		StartGrowth();

	}

    void StartGrowth()
	{
		for( int _layerIndex = 0; _layerIndex < _layerCount; _layerIndex++ )
		{
			GameObject newLayer = new GameObject();
			SetupLayer( _layerIndex, newLayer );

			for( int _curLeafNum = 0; _curLeafNum < _numLeaves; _curLeafNum++ )
			{
				GrowLeaf( _curLeafNum, _layerIndex, newLayer );
			}

			StartCoroutine( TweenLocalScale( newLayer.transform, Vector3.zero, Vector3.one * ( 1 - _layerIndex * .2f ), ( 5 + _layerIndex ) * _growthRate ) );
		}

		GrowFruit();
		StartCoroutine( WaitToSpawnChild() );
		StopState();
	}

	void SetupLayer(int layerIndex, GameObject layer)
	{
		layer.name = "Layer" + layerIndex;
		layer.transform.parent = transform;
		layer.transform.position = transform.position;

		_leafScale = 1.0f - layerIndex * .2f;
	}

	void GrowLeaf( int leafNumber, int layerIndex, GameObject parentLayer )
	{
		GameObject newLeaf = Instantiate( _leaf ) ;
		newLeaf.transform.position = transform.position + Vector3.up * ( layerIndex * .01f );
		newLeaf.transform.parent = transform;
		newLeaf.transform.localScale = new Vector3( _leafScale, _leafScale, _leafScale );
		newLeaf.transform.Rotate( new Vector3( 0, leafNumber * 360.0f / _numLeaves + leafNumber, 0 ) );
		newLeaf.transform.parent = parentLayer.transform;
		newLeaf.transform.localScale = new Vector3( _leafScale, _leafScale, _leafScale );

		if( newLeaf.GetComponent<Renderer>() != null )
		{
			newLeaf.GetComponent<Renderer>().material.SetFloat( "_ColorSetSeed", _myPlant.ShaderColorSeed );
		}
		else if( newLeaf.GetComponentInChildren<Renderer>() != null )
		{
			newLeaf.GetComponentInChildren<Renderer>().material.SetFloat( "_ColorSetSeed", _myPlant.ShaderColorSeed );
		}

		Animator anim = newLeaf.GetComponent<Animator>();
		anim.speed *= _growthRate;
		_childAnimators.Add( anim );
			
		_waitTime =  ( .5f + ( ( layerIndex * _numLeaves ) + leafNumber ) * .05f ) / _growthRate;

		StartCoroutine( WaitAndStart( newLeaf.GetComponent<Animator>(), _waitTime ) );

		if( leafNumber == _numLeaves - 1 && layerIndex == _layerCount - 1 )
		{
			_lastAnim = newLeaf.GetComponent<Animator>();
		}
	}

	void GrowFruit()
	{
		_curFruit = Instantiate(_fruitPrefab);
		_curFruit.transform.position = transform.position;
		Vector3 preserveScale = _curFruit.transform.localScale;
		_curFruit.transform.parent = transform;
		_curFruit.transform.localScale = preserveScale;

		StartCoroutine( TweenLocalScale( _curFruit.transform, Vector3.zero, _curFruit.transform.localScale, 30.0f * _growthRate));
	}

	IEnumerator TweenLocalScale( Transform focusTransform, Vector3 startScale, Vector3 endScale, float moveTime )
	{
		float timer = 0.0f;

		while( timer < moveTime )
		{
			focusTransform.localScale = Vector3.Lerp( startScale, endScale, Mathf.SmoothStep( 0, 1, timer / moveTime ) );
			timer += Time.deltaTime;
			yield return 0;
		}

		focusTransform.localScale = endScale;
	}

	IEnumerator WaitAndStart( Animator anim, float waitTime )
	{
		anim.enabled = false;
		yield return new WaitForSeconds( waitTime );
		anim.enabled = true;
		anim.Play(0);
	}

	private IEnumerator WaitToSpawnChild()
	{
		float _curTimeAnimated = _lastAnim.GetCurrentAnimatorStateInfo(0).normalizedTime; // Mathf.Lerp(0.0f, _animEndTime, _plantAnim.GetCurrentAnimatorStateInfo(0).normalizedTime ); // i am x percent of the way through anim

		while( _curTimeAnimated < 1.0f )
		{
			//update
			_curTimeAnimated = _lastAnim.GetCurrentAnimatorStateInfo(0).normalizedTime; 
			yield return null;
		}

	}

	protected override void CustomStopGrowth()
	{
		if( !_waiting )
		{
			_lastAnim = _childAnimators[ _childAnimators.Count - 1 ];
			StartCoroutine( WaitForLastLeaf() );
		}
	}

	private IEnumerator WaitForLastLeaf()
	{
		_waiting = true;
		while( _lastAnim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f )
		{
			yield return null;
		}
		_waiting = false;
		_myPlant.SwitchController( this );
	}	

	protected override void CustomTouchPlant()
	{
		if( !_closingLeaves )
		{
			StartCoroutine( CloseLeaves() );
		}
	}

	IEnumerator CloseLeaves()
	{
		_closingLeaves = true;
		float closeTime = .25f;
		float openTime = 5.0f;
		float fruitScale = _curFruit.transform.localScale.x;
		// cycle all through the guys
		foreach( Animator plant in _childAnimators )
		{
			plant.SetBool("isSwaying", false );
			plant.SetBool("isClosing", true );
			plant.speed = 3.8f;
		}
		
		_curFruit.transform.DOScale( 0.0f, .5f );
		yield return new WaitForSeconds( closeTime );

		PlayerManager.instance.Player.GetComponent<RollerController>().MakeDroopyExplode();

		yield return new WaitForSeconds( .001f );

		// cycle all through the guys
		foreach( Animator plant in _childAnimators )
		{
			plant.speed = 1.0f;
			plant.SetBool("isClosing", false );
		}

		_curFruit.transform.DOScale( fruitScale, 7.0f );
		yield return new WaitForSeconds( openTime );
		_closingLeaves = false;
	}

	protected override void CustomizedSingAtPlant( bool entering )
	{
		SingController singCtrl = PlayerManager.instance.Player.GetComponent<SingController>();
		if( singCtrl.State == SingController.SingState.SINGING && entering )
		{
			//keep that bool set to true
			foreach( Animator plant in _childAnimators )
			{
				plant.SetBool("isSwaying", true );
			}
			_enterTime = Time.time;
		}
		else
		{
			if( Time.time - _enterTime >= _singBufferTime )
			{
				foreach( Animator plant in _childAnimators )
				{
					plant.SetBool("isSwaying", false );
				}
			}
		}
	}
}
