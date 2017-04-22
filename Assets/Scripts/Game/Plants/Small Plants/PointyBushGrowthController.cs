using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointyBushGrowthController : SPGrowthController {
    [SerializeField] GameObject _leaf;

    const int _numLeaves = 5;
    const int _layerCount = 3;

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

        _waitTime = (((layerIndex * _numLeaves) + leafNumber) * .5f) / _growthRate;

        //_waitTime = 0;

        StartCoroutine(WaitAndStart(newLeaf.transform.GetComponentInChildren<Animator>(), _waitTime));

        if (leafNumber == _numLeaves - 1 && layerIndex == _layerCount - 1)
        {
            _lastAnim = newLeaf.transform.GetComponentInChildren<Animator>();
        }
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
        anim.enabled = false;
        yield return new WaitForSeconds(waitTime);
        anim.enabled = true;
        anim.Play(0);
    }

    private IEnumerator WaitToSpawnChild()
    {
        float _animEndTime = _lastAnim.GetCurrentAnimatorStateInfo(0).length;
        float _curTimeAnimated = _lastAnim.GetCurrentAnimatorStateInfo(0).normalizedTime; // Mathf.Lerp(0.0f, _animEndTime, _plantAnim.GetCurrentAnimatorStateInfo(0).normalizedTime ); // i am x percent of the way through anim

        while (_curTimeAnimated < _animEndTime)
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
            _endTimeStamp = state.length;//_lastClip.length - .04f;
            StartCoroutine(WaitForLastLeaf());
        }
    }

    private IEnumerator WaitForLastLeaf()
    {
        _waiting = true;
        while (_endTimeStamp > _lastAnim.GetCurrentAnimatorStateInfo(0).normalizedTime)
        {
            yield return null;
        }
        _waiting = false;
        _myPlant.SwitchController(this);
    }
}
