using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimberPlantGrowthController : BPGrowthController
{
    
    [SerializeField]
    private Transform _bStemRoot = null;
    [SerializeField]
    private GameObject _leafPrefab = null;

    Vector3 _minScale = new Vector3(0.0f, 0.0f, 0.0f);
    public Vector3 MinScale { get { return _minScale; } set { _minScale = value; } }
    Vector3 _maxScale = new Vector3(14.0f, 14.0f, 14.0f);

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

        AnimatorStateInfo info = _plantAnim.GetCurrentAnimatorStateInfo(0);
        _timeBetweenLeafSpawns = (info.length / _baseGrowthRate) / _numChildren;

        SetRandomLayerWeight(_plantAnim);
    }

    private IEnumerator SpawnLeaves()
    {
        _inverseIndex = _numChildren - _curChildSpawned;
        _currentParent = _bones[_curChildSpawned];

        _offset = Random.Range(0, 100);
        yield return new WaitForSeconds(_timeBetweenLeafSpawns);
        if(_curChildSpawned > 2 && _curChildSpawned < 4)
        {
            SetupLeaf(0);
        }
        _curChildSpawned++;
        _leafSpawnRoutine = null;
    }

    void SetRandomLayerWeight(Animator anim)
    {
        for (int i = 0; i < anim.layerCount; i++)
        {
            anim.SetLayerWeight(i, Random.Range(.05f, 1));
        }
    }

    void SetupLeaf(int index)
    {
        GameObject leaf = Instantiate(_leafPrefab);
        float variation = Random.Range(1, 10);
        Animator anim = leaf.GetComponent<Animator>();
        _childAnimators.Add(anim);

        SetRandomLayerWeight(anim);
        leaf.transform.parent = _currentParent;
        leaf.transform.position = _currentParent.position;
        leaf.transform.localPosition += leaf.transform.up * 0.015f;// * .25f * transform.localScale.x;
        leaf.transform.rotation = Quaternion.Euler(new Vector3(Random.Range(0,360), 0, 90));
        leaf.transform.rotation *= Quaternion.Euler(new Vector3(Random.Range(-180, 180), 0, 0));
        leaf.transform.localScale = Vector3.one * .250f * Random.Range(.25f,1) + _currentParent.localScale * _curChildSpawned * .1f;

		if( leaf.GetComponent<Renderer>() != null )
		{
			leaf.GetComponent<Renderer>().material.SetFloat( "_ColorSetSeed", _myPlant.ShaderColorSeed );
		}
		else if( leaf.GetComponentInChildren<Renderer>() != null )
		{
			leaf.GetComponentInChildren<Renderer>().material.SetFloat( "_ColorSetSeed", _myPlant.ShaderColorSeed );
		}

        anim.speed *= _plantAnim.GetComponent<Animator>().speed * 2f;
    }

    protected override void CustomPlantGrowth()
    {
        if (transform.localScale.x < _maxScale.x)
        {
            transform.localScale = Vector3.Lerp(_minScale, _maxScale, Mathf.SmoothStep(0, 1, _curPercentAnimated));
        }

        if (_leafSpawnRoutine == null && _curChildSpawned < _numChildren)
        {
            _leafSpawnRoutine = StartCoroutine(SpawnLeaves());
        }

        if (_lastAnim)
        {
            if ( _lastAnim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f )
            {
                _myPlant.SwitchController(this);
            }
        }

    }

    protected override void CustomStopGrowth()
    {
        if (!_waiting)
        {
            _lastAnim = _childAnimators[_childAnimators.Count - 1];
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

}
