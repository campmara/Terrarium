﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//this should be moved to a more appropriate document
//new way to deal with parenting
public class LeafTransformations
{
    public Transform leaf;
    public Transform leafParent;

    public Vector3 scaleOffset;
    public Vector3 positionOffset;
    public Quaternion rotationOffset;

    public LeafTransformations()
    {
        leaf = null;
        leafParent = null;

        scaleOffset = Vector3.zero;
        positionOffset = Vector3.zero;
        rotationOffset = Quaternion.identity;
    }
}

public class PointPlantGrowthController : BPGrowthController
{
    [SerializeField]
    private Transform _bStemRoot = null;
    [SerializeField]
    private GameObject _leafPrefab = null;

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
    bool _waiting = false;

    List<LeafTransformations> _leafTransformations;

    void Awake()
    {
        _myPlant = GetComponent<BasePlant>();
        _controllerType = ControllerType.Growth;
        _leafTransformations = new List<LeafTransformations>();
    
    }

    protected override void OnChangeGrowthRate()
    {
        AnimatorStateInfo info = _plantAnim.GetCurrentAnimatorStateInfo(0);

        // see if it's actually updated yet
        if( _animEndTime == info.length * _baseGrowthRate )
        {
            _timeBetweenLeafSpawns = ( info.length * _growthRate ) / _growthRate / _numChildren;
        }
        else
        {
            _timeBetweenLeafSpawns = info.length / _numChildren;
        }
    }

    protected override void InitPlant()
    {
        base.InitPlant();

        _bones = _bStemRoot.GetComponentsInChildren<Transform>();
        _numChildren = _bones.Length; // we subtract one for them that exists there

        AnimatorStateInfo info = _plantAnim.GetCurrentAnimatorStateInfo(0);
        _timeBetweenLeafSpawns = (info.length / _baseGrowthRate) / _numChildren;

        for (int i = 0; i < _plantAnim.layerCount; i++)
        {
            _plantAnim.SetLayerWeight(i, Random.Range(0, 2));
        }
    }

    private IEnumerator SpawnLeaves()
    {
        _inverseIndex = _numChildren - _curChildSpawned;
        _currentParent = _bones[_curChildSpawned];

        _offset = Random.Range(0, 100);
        _ringNumber = Random.Range(5, 8);
        Animator leafAnim = null;
        
        for (int i = 0; i < _ringNumber; i++)
        {
            leafAnim = SetupLeaf(i);
            yield return new WaitForSeconds((_timeBetweenLeafSpawns - .1f) / _ringNumber);
        }

        //yield return new WaitForSeconds(_timeBetweenLeafSpawns);

        _curChildSpawned++;
        _leafSpawnRoutine = null;

        if( _curChildSpawned == _numChildren )
        {
            _lastAnim = leafAnim;
        }
    }

   Animator SetupLeaf(int index)
    {
        GameObject leaf = Instantiate(_leafPrefab);
        float variation = Random.Range(1, 10);
        Animator anim = leaf.GetComponent<Animator>();
        _childAnimators.Add(anim);

        leaf.transform.parent = _currentParent;

        LeafTransformations transformedLeaf = new LeafTransformations();
        transformedLeaf.leaf = leaf.transform;
        transformedLeaf.leafParent = _currentParent;
        transformedLeaf.positionOffset = -leaf.transform.forward * 0 * transform.localScale.x;
        transformedLeaf.rotationOffset = Quaternion.Euler(new Vector3((index * 360 / _ringNumber + _offset), 0, 90));
        transformedLeaf.rotationOffset *= Quaternion.Euler(new Vector3((_curChildSpawned * 3f) + 60,0,0));
        transformedLeaf.scaleOffset = Vector3.one * .125f; //_currentParent.localScale * _curChildSpawned * .1f;

        anim.speed *= _plantAnim.GetComponent<Animator>().speed * 2f;

        _leafTransformations.Add(transformedLeaf);
		UpdateLeafTransforms();

        return anim;
    }

    protected override void CustomPlantGrowth()
    {
        if (_leafSpawnRoutine == null && _curChildSpawned < _numChildren)
        {
            _leafSpawnRoutine = StartCoroutine(SpawnLeaves());
        }

        if( _lastAnim )
        {
            if( _lastAnim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f )
            {
                _myPlant.SwitchController(this);
            }
        }

        //this is new!!
        UpdateLeafTransforms();
    }

    private void UpdateLeafTransforms()
    {
        for(int i = 0; i < _leafTransformations.Count; i++)
        {
            Transform currentLeaf = _leafTransformations[i].leaf;
            Transform currentLeafParent = _leafTransformations[i].leafParent;

            currentLeaf.localScale =_leafTransformations[i].scaleOffset; // currentLeafParent.localScale + 
            currentLeaf.rotation = currentLeafParent.rotation * _leafTransformations[i].rotationOffset;
            currentLeaf.position = currentLeafParent.position + _leafTransformations[i].positionOffset;
        }
    }
}
