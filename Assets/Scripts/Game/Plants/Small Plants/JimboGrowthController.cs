using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JimboGrowthController : SPGrowthController {
    [SerializeField] GameObject _leaf;

    const int _numLeaves = 5;
    const int _layerCount = 3;

    float _waitTime = 0.0f;
    float _leafScale = 0.0f;
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

            GrowLeaf(0, _layerIndex, newLayer);

            StartCoroutine(TweenLocalScale(newLayer.transform, Vector3.zero, Vector3.one * (1 - _layerIndex * .2f), (5 + _layerIndex) * _growthRate));
            _lastLayer = newLayer;
        }

        StopState();
    }

    GameObject _lastLayer = null;

    void SetupLayer(int layerIndex, GameObject layer)
    {
        layer.name = "Layer" + layerIndex;
        layer.transform.parent = transform;
        layer.transform.position = transform.position;

        _leafScale = (1 + (layerIndex / _layerCount)) * .9f;//1.0f - layerIndex * .9f;
    }

    void GrowLeaf(int leafNumber, int layerIndex, GameObject parentLayer)
    {
        GameObject newLeaf = Instantiate(_leaf);
        newLeaf.transform.position = transform.position;
        newLeaf.transform.rotation = transform.rotation;
        newLeaf.transform.parent = transform;
        newLeaf.transform.Rotate(new Vector3(0, 360 * Random.Range(-1f, 1f), 0));
        newLeaf.transform.Rotate(new Vector3(30 * Random.Range(-1f, 1f), 0, 0));
        newLeaf.transform.localScale = new Vector3(_leafScale, _leafScale, _leafScale);
        newLeaf.transform.parent = parentLayer.transform;
        newLeaf.transform.position += newLeaf.transform.up * (layerIndex) * 2f;
        newLeaf.transform.localScale = new Vector3(_leafScale, _leafScale, _leafScale);

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
        _lastAnim = newLeaf.transform.GetComponentInChildren<Animator>();

        StartCoroutine(WaitAndStart(newLeaf.transform.GetComponentInChildren<Animator>(), _waitTime));
    }

    IEnumerator TweenLocalScale(Transform focusTransform, Vector3 startScale, Vector3 endScale, float moveTime)
    {
        float timer = 0.0f;

        while (timer < moveTime)
        {
            focusTransform.localScale = Vector3.Lerp(startScale, endScale, Mathf.SmoothStep(0, 1, timer / moveTime));
            timer += Time.deltaTime;
            yield return 0;
        }

        focusTransform.localScale = endScale;
    }

    IEnumerator WaitAndStart(Animator anim, float waitTime)
    {
        //anim.enabled = false;
        yield return new WaitForSeconds(waitTime);
     //   anim.enabled = true;
   //     anim.Play(0);
    }

    protected override void CustomStopGrowth()
    {
        _myPlant.SwitchController(this);
    }
}
