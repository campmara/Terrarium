using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OobleGrowthController : SPGrowthController
{

    [SerializeField] GameObject _leaf;

    List<Animator> _leafAnimators = new List<Animator>();
    const int _numLeaves = 5;
    const int _layerCount = 3;

    //DANCING VARIABLES
    float _danceDelay = .15f;
	float _singBufferTime = .6f;
	float _enterTime = 0.0f;
    bool _canDance = true;
    float _waitTime = 0.0f;
    float _leafScale = 0.0f;

    float _endTimeStamp = 0.0f;
    Animator _lastAnim = null;
    bool _waiting = false;

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

            for (int _curLeafNum = 0; _curLeafNum < _numLeaves; _curLeafNum += (int)Random.Range(1,4))
            {
                GameObject leaf = GrowLeaf(_curLeafNum, _layerIndex, newLayer);
				StartCoroutine(TweenLocalScale(leaf.transform, Vector3.zero, Vector3.one * (1 - _layerIndex * .2f), (5 + _layerIndex) * _growthRate));

            }

        }
        StopState();
    }

    void SetupLayer(int layerIndex, GameObject layer)
    {
        layer.name = "Layer" + layerIndex;
        layer.transform.parent = transform;
        layer.transform.position = transform.position;

        _leafScale = 1 + (layerIndex / _layerCount);//1.0f - layerIndex * .9f;
    }

 	GameObject GrowLeaf(int leafNumber, int layerIndex, GameObject parentLayer)
    {
        GameObject newLeaf = Instantiate(_leaf);
        newLeaf.transform.position = transform.position + Vector3.up * (layerIndex * .01f);
        newLeaf.transform.parent = transform;
        newLeaf.transform.localScale = new Vector3(_leafScale, _leafScale, _leafScale);
        newLeaf.transform.Rotate(new Vector3(0, 0, (layerIndex * _numLeaves * 90) + leafNumber * 360.0f / _numLeaves + leafNumber));
        newLeaf.transform.parent = parentLayer.transform;
        newLeaf.transform.localScale = new Vector3(_leafScale, _leafScale, _leafScale);
        newLeaf.transform.Rotate(new Vector3(((-layerIndex * 25) / _layerCount) - 10, 0, 0));
        newLeaf.transform.position += newLeaf.transform.up * Random.Range(.1f, 2);
        Animator anim = newLeaf.transform.GetComponentInChildren<Animator>();
        anim.speed *= _growthRate;
        _childAnimators.Add(anim);
		newLeaf.transform.localScale = Vector3.zero;

		if( newLeaf.GetComponent<Renderer>() != null )
		{
			newLeaf.GetComponent<Renderer>().material.SetFloat( "_ColorSetSeed", _myPlant.ShaderColorSeed );
		}
		else if( newLeaf.GetComponentInChildren<Renderer>() != null )
		{
			newLeaf.GetComponentInChildren<Renderer>().material.SetFloat( "_ColorSetSeed", _myPlant.ShaderColorSeed );
		}

		int divisor = (layerIndex * _numLeaves);// + leafNumber);
		leafNumber = leafNumber > 0 ? leafNumber : 1;
        _waitTime = ( _growthRate / divisor ) * leafNumber;

        _leafAnimators.Add( anim );
        _lastAnim = anim;

		return newLeaf;
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
    protected override void CustomStopGrowth()
    {
        _myPlant.SwitchController(this);
    }
}
