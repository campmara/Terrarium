using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundPlant : SmallPlant
{

    [SerializeField] GameObject _leaf;
    [SerializeField] GameObject _fruit;

    const int _numLeaves = 5;
    const int _layerCount = 2;

    float _waitTime = 0.0f;
    float _leafScale = 0.0f;

	Animator _lastAnim = null; // some classic jank

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

        newLeaf.GetComponent<Animator>().speed *= _growthRate;
        _waitTime =  ( .5f + ( ( layerIndex * _numLeaves ) + leafNumber ) * .05f ) / _growthRate;

        StartCoroutine( WaitAndStart( newLeaf.GetComponent<Animator>(), _waitTime ) );

		if( leafNumber == _numLeaves - 1 && layerIndex == _layerCount - 1 )
		{
			_lastAnim = newLeaf.GetComponent<Animator>();
		}
    }

    void GrowFruit()
    {
        GameObject curFruit = Instantiate(_fruit);
        curFruit.transform.position = transform.position;
		Vector3 preserveScale = curFruit.transform.localScale;
        curFruit.transform.parent = transform;
		curFruit.transform.localScale = preserveScale;

        StartCoroutine( TweenLocalScale( curFruit.transform, Vector3.zero, curFruit.transform.localScale, 7.0f * _growthRate));
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
		float _animEndTime = _lastAnim.GetCurrentAnimatorStateInfo(0).length;
		float _curTimeAnimated = _lastAnim.GetCurrentAnimatorStateInfo(0).normalizedTime; // Mathf.Lerp(0.0f, _animEndTime, _plantAnim.GetCurrentAnimatorStateInfo(0).normalizedTime ); // i am x percent of the way through anim

		while( _curTimeAnimated < _animEndTime )
		{
			//update
			_curTimeAnimated = _lastAnim.GetCurrentAnimatorStateInfo(0).normalizedTime; 
			yield return null;
		}

		PlantManager.instance.RequestSpawnMini( this, _timeBetweenSpawns );
	}
}
