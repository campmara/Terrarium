using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundPlant : Plantable
{

    [SerializeField] GameObject _leaf;
    [SerializeField] GameObject _fruit;

    const int _numLeaves = 5;
    const int _layerCount = 2;

    float _waitTime = 0.0f;
    float _leafScale = 0.0f;

    protected override void Awake()
    {
		base.Awake();
		StartGrowth();
    }
		
    protected override void StartGrowth()
    {
        for( int _layerIndex = 0; _layerIndex < _layerCount; _layerIndex++ )
        {
            GameObject newLayer = new GameObject();
            SetupLayer( _layerIndex, newLayer );

            for( int _curLeafNum = 0; _curLeafNum < _numLeaves; _curLeafNum++ )
            {
                GrowLeaf( _curLeafNum, _layerIndex, newLayer );
            }
				
            StartCoroutine( TweenLocalScale( newLayer.transform, Vector3.zero, Vector3.one * ( 1 - _layerIndex * .2f ), ( 5 + _layerIndex ) * _curGrowthRate ) );
        }

		PrepStoppingAfterLastAnim();

        GrowFruit();
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

        newLeaf.GetComponent<Animator>().speed *= _curGrowthRate;
        _waitTime =  ( .5f + ( ( layerIndex * _numLeaves ) + leafNumber ) * .05f ) / _curGrowthRate;

		_childAnimators.Add( newLeaf.GetComponent<Animator>() );

        StartCoroutine( WaitAndStart( newLeaf.GetComponent<Animator>(), _waitTime ) );
    }

	void PrepStoppingAfterLastAnim()
	{
		float duration = _childAnimators[ _childAnimators.Count - 1 ].runtimeAnimatorController.animationClips[0].length;
		duration /= _curGrowthRate;
		duration += _waitTime * 5.0f; //this is a hack because for some reason it's not calculating correctly yay
		StartCoroutine( "WaitForLastAnimEnd", duration );
	}

    void GrowFruit()
    {
        GameObject curFruit = Instantiate(_fruit);
        curFruit.transform.position = transform.position;
		Vector3 preserveScale = curFruit.transform.localScale;
        curFruit.transform.parent = transform;
		curFruit.transform.localScale = preserveScale;

        StartCoroutine( TweenLocalScale( curFruit.transform, Vector3.zero, curFruit.transform.localScale, 7.0f * _curGrowthRate));
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

	IEnumerator WaitForLastAnimEnd( float animLen )
	{
		yield return new WaitForSeconds( animLen );
		base.StopGrowth();
	}
}
