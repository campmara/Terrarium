using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SinisterManController : MonoBehaviour {

    [SerializeField]
    List<Transform> _sinisterManTransforms = new List<Transform>();

    [SerializeField]
    float _rotateSpeed = 3.0f;

    Tween _walkTween = null;
    [SerializeField]
    float _walkWaitTime = 1.0f;
    [SerializeField]
    float _walkTime = 1.0f;
    float _walkHeight = 5.0f;    

	// Use this for initialization
	void Awake ()
    {
        StartCoroutine( DelayedWalkTween() );
	}
	
	// Update is called once per frame
	void Update ()
    {
        if( _walkTween != null )
        {
            this.transform.Rotate( 0.0f, _rotateSpeed * Time.deltaTime, 0.0f );
        }
        
        foreach( Transform t in _sinisterManTransforms )
        {
            t.LookAt( new Vector3(0.0f, t.position.y, 0.0f) );
        }

	}
    
    IEnumerator DelayedWalkTween()
    {
        yield return new WaitForSeconds( _walkWaitTime );

        StartWalkTween();

        yield return _walkTween.WaitForCompletion();

        StartCoroutine( DelayedWalkTween() );

        _walkTween = null;
    }

    void StartWalkTween()
    {
        _walkTween = this.transform.DOJump( this.transform.position, _walkHeight, 1, _walkTime );
    }
}
