using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SinisterManController : MonoBehaviour {

    [SerializeField]
    List<Transform> _sinisterManTransforms = new List<Transform>();
	SkinnedMeshRenderer[] _sinisterSkinnedMeshArray;

	[SerializeField, Space(5)]
    float _rotateSpeed = 3.0f;
	[SerializeField, Space(5)]
	float _baseRotateSpeed = 3.0f;
	[SerializeField]
	AnimationCurve _rotateJumpCurve;

    Tween _walkTween = null;
	float _walkElapsed = 0.0f;
	[SerializeField, Space(5)]
    Vector2 _walkWaitTimeRange;
    [SerializeField]
    float _walkTime = 1.0f;
    [SerializeField]
	float _walkHeight = 5.0f;    
	[SerializeField]
	AnimationCurve _skinnedTailUpCurve;
	int _tailUpID;
	[SerializeField]
	AnimationCurve _skinnedTailDownCurve;
	int _tailDownID;
	[SerializeField]
	AnimationCurve _zRotateCurve;

	// Use this for initialization
	void Awake ()
    {
		_sinisterSkinnedMeshArray = this.GetComponentsInChildren<SkinnedMeshRenderer>();


		StartCoroutine( DelayedWalkTween() );
	}
	
	// Update is called once per frame
	void Update ()
    {
        if( _walkTween != null )
        {
			_walkElapsed = _walkTween.ElapsedPercentage();
			this.transform.Rotate( 0.0f, -_rotateSpeed * _rotateJumpCurve.Evaluate( _walkElapsed ) * Time.deltaTime, 0.0f );
			for( int i = 0; i < _sinisterSkinnedMeshArray.Length; ++i )
			{
				_sinisterSkinnedMeshArray[i].SetBlendShapeWeight(0, _skinnedTailUpCurve.Evaluate( _walkElapsed ) );
				_sinisterSkinnedMeshArray[i].SetBlendShapeWeight(1, _skinnedTailDownCurve.Evaluate( _walkElapsed ) );
			}
        }
		else
		{
			this.transform.Rotate( 0.0f, -_baseRotateSpeed * Time.deltaTime, 0.0f );
		}
        
        foreach( Transform t in _sinisterManTransforms )
        {
            t.LookAt( new Vector3(0.0f, t.position.y, 0.0f) );
			t.Rotate( 0.0f, 0.0f, _zRotateCurve.Evaluate( _walkElapsed ) );
        }

	}
    
    IEnumerator DelayedWalkTween()
    {
		yield return new WaitForSeconds( Random.Range( _walkWaitTimeRange.x, _walkWaitTimeRange.y ) );

        StartWalkTween();

        yield return _walkTween.WaitForCompletion();

        StartCoroutine( DelayedWalkTween() );

		_walkElapsed = 0.0f;
        _walkTween = null;
    }

    void StartWalkTween()
    {
        _walkTween = this.transform.DOJump( this.transform.position, _walkHeight, 1, _walkTime );

    }	
}
