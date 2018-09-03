﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointyBushGrowthController : SPGrowthController {
    [SerializeField] GameObject _leaf;
    List<Animator> _leafAnimators = new List<Animator>();
    const int _numLeaves = 5;
    const int _layerCount = 3;

    float _waitTime = 0.0f;
    float _leafScale = 0.0f;
    Animator _lastAnim = null;
    bool _waiting = false;
    
    //DANCING VARIABLES
    float _danceDelay = .35f;
	float _singBufferTime = .6f;
	float _enterTime = 0.0f;
    bool _canDance = true;

    protected override void InitPlant()
    {
        base.InitPlant();
        StartGrowth();

    }

    void StartGrowth()
    {
        for (int _layerIndex = 0; _layerIndex < _layerCount; _layerIndex++)
        {
            GameObject newLayer = new GameObject();
            SetupLayer(_layerIndex, newLayer);

            for (int _curLeafNum = 0; _curLeafNum < _numLeaves; _curLeafNum++)
            {
                GrowLeaf(_curLeafNum, _layerIndex, newLayer);
            }

            StartCoroutine(TweenLocalScale(newLayer.transform, Vector3.zero, Vector3.one * (1 - _layerIndex * .2f), (5 + _layerIndex) * _growthRate));
        }

        StartCoroutine(WaitToSpawnChild());
        StopState();
    }

    void SetupLayer(int layerIndex, GameObject layer)
    {
        layer.name = "Layer" + layerIndex;
        layer.transform.parent = transform;
        layer.transform.position = transform.position;

        _leafScale = 1 + (layerIndex / _layerCount);//1.0f - layerIndex * .9f;
    }

    void GrowLeaf(int leafNumber, int layerIndex, GameObject parentLayer)
    {
        GameObject newLeaf = Instantiate(_leaf);
        newLeaf.transform.position = transform.position + Vector3.up * (layerIndex * .01f);
        newLeaf.transform.parent = transform;
        newLeaf.transform.localScale = new Vector3(_leafScale, _leafScale, _leafScale);
        newLeaf.transform.Rotate(new Vector3(0, 0, (layerIndex * _numLeaves * 90) + leafNumber * 360.0f / _numLeaves + leafNumber));
        newLeaf.transform.parent = parentLayer.transform;
        newLeaf.transform.localScale = new Vector3(_leafScale, _leafScale, _leafScale);
        newLeaf.transform.Rotate(new Vector3(((-layerIndex * 45) / _layerCount) - 30, 0, 0));
        Animator anim = newLeaf.transform.GetComponentInChildren<Animator>();
        anim.speed *= _growthRate;
        _childAnimators.Add(anim);

		if( newLeaf.GetComponent<Renderer>() != null )
		{
			newLeaf.GetComponent<Renderer>().material.SetFloat( "_ColorSetSeed", _myPlant.ShaderColorSeed );
		}
		else if( newLeaf.GetComponentInChildren<Renderer>() != null )
		{
			newLeaf.GetComponentInChildren<Renderer>().material.SetFloat( "_ColorSetSeed", _myPlant.ShaderColorSeed );
		}

        _waitTime = (((layerIndex * _numLeaves) + leafNumber) * .5f) / _growthRate;
         _lastAnim = anim;
         _leafAnimators.Add( anim );

        StartCoroutine(WaitAndStart(newLeaf.transform.GetComponentInChildren<Animator>(), _waitTime));
    }

    IEnumerator TweenLocalScale(Transform focusTransform, Vector3 startScale, Vector3 endScale, float moveTime)
    {
        float timer = 0.0f;

        while (timer < moveTime)
        {
            focusTransform.localScale = Vector3.Lerp(startScale, endScale, Mathf.SmoothStep(0, 1, timer / moveTime));
            timer += Time.unscaledDeltaTime;
            yield return 0;
        }

        focusTransform.localScale = endScale;
    }

    IEnumerator WaitAndStart(Animator anim, float waitTime)
    {
        anim.enabled = false;
        yield return new WaitForSecondsRealtime(waitTime);
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
        if (!_waiting)
        {
            _lastAnim = _childAnimators[_childAnimators.Count - 1];
            AnimatorStateInfo state = _lastAnim.GetCurrentAnimatorStateInfo(0);
            StartCoroutine(WaitForLastLeaf());
        }
    }

    private IEnumerator WaitForLastLeaf()
    {
        _waiting = true;
        while ( _lastAnim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f )
        {
            yield return null;
        }
        _waiting = false;
        _myPlant.SwitchController(this);
    }

    IEnumerator PointyDance()
    {
        foreach( Animator plant in _leafAnimators )
	    {			
            if( _canDance )
            {
                plant.SetBool("IsDancing", true );
             //   yield return new WaitForSeconds( _danceDelay );
            }
            else
            {
                break;
            }
	    }
        yield return null;
    }
    protected override void CustomizedSingAtPlant( bool entering )
	{
		SingController singCtrl = PlayerManager.instance.Player.GetComponent<SingController>();
        if( singCtrl.State == SingController.SingState.SINGING && entering)
        {
            _enterTime = Time.time;
            _canDance = true;
            StartCoroutine( PointyDance() );
        }
        else 
        {
            if( !entering || (Time.time - _enterTime >= _singBufferTime ) )                
            {
                _canDance = false;
                foreach( Animator plant in _leafAnimators )
			    {
			        plant.SetBool("IsDancing", false );
		        }
            }            
        }
	}
}
