using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceManager : MonoBehaviour
{
    [Header("Renderers")]
    [SerializeField] private SpriteRenderer _eyeRenderer;
    [SerializeField] private SpriteRenderer _mouthRenderer;

    [Header("Eyes")]
    [SerializeField] private Sprite _eyesOpen;
    [SerializeField] private Sprite _eyesClosed;
	[SerializeField] private Sprite _eyesDesire;
	[SerializeField] private Sprite _eyesHappy;
	[SerializeField] private Sprite _eyesWink;
	[SerializeField] private Sprite _eyesSurprised;
	[SerializeField] private Sprite _eyesSad;
	[SerializeField] private Sprite _eyesAngry;
	[SerializeField] private Sprite _eyesAnnoyed;
	private Sprite _currentEyes;

    [Header("Mouth")]
	[SerializeField] private bool enableMouthSprite;
	[SerializeField] private Sprite _mouthIdle;
    [SerializeField] private Sprite _mouthOh;
	[SerializeField] private Sprite _mouthD;
	[SerializeField] private Sprite _mouthSmile;
	[SerializeField] private Sprite _mouthSideSmile;
	[SerializeField] private Sprite _mouthDiagonal;
	[SerializeField] private Sprite _mouthSad;
	[SerializeField] private Sprite _mouthVerySad;



    private Coroutine _blinkRoutine;
    private Coroutine _idleRoutine;

	[SerializeField] SkinnedMeshRenderer _mouthSkinnedMesh; 
	private Coroutine _mouthPoseRoutine;
	[SerializeField] private int _mouthPoseIndex = 0;
	private MouthPoseData _currMouthPose;


	void Awake()
	{
	    if (_eyeRenderer == null || _mouthRenderer == null)
	    {
	        Debug.LogError("One or more of the face sprite renderers are unhooked.");
	    }

		BecomeIdle();
		InitiateBlinkLoop();

		//_mouthPoseIndex = MouthPoseManager.instance.StartMouthPoseIndex;
		//SetMouthPose( MouthPoseManager.instance.MouthPoseArray[_mouthPoseIndex] );
	}

	// ===============
	// E M O T I O N S
	// ===============

	public void BecomeIdle()
	{
		SetEyes(_eyesOpen);
		SetMouth(_mouthIdle);
    }

	public void Sing()
	{
		SetEyes(_eyesHappy);
		SetMouth(_mouthOh);
    }

	public void BecomeHappy()
	{
		SetEyes(_eyesHappy);
		SetMouth(_mouthSmile);

        StartReturnIdle();
    }

	public void Wink()
	{
		SetEyes(_eyesWink);
		SetMouth(_mouthSideSmile);

        StartReturnIdle();
    }

	public void BecomeAnnoyed()
	{
		SetEyes(_eyesAnnoyed);
		SetMouth(_mouthSad);

        StartReturnIdle();
    }

	public void BecomeSad()
	{
		SetEyes(_eyesSad);
		SetMouth(_mouthVerySad);

        StartReturnIdle();
    }

	public void BecomeInterested()
	{
		SetEyes(_eyesOpen);
		SetMouth(_mouthOh);

        StartReturnIdle();
    }

	public void BecomeSurprised()
	{
		SetEyes(_eyesSurprised);
		SetMouth(_mouthOh);

        StartReturnIdle();
    }

	public void BecomeDesirous()
	{
		SetEyes(_eyesDesire);
		SetMouth(_mouthD);

        StartReturnIdle();
    }

	public void BecomeEncumbered()
	{
		SetEyes(_eyesAngry);
		SetMouth(_mouthDiagonal);

        StartReturnIdle();
    }

	public void BecomeFeisty()
	{
		SetEyes(_eyesAngry);
		SetMouth(_mouthOh);

        StartReturnIdle();
    }

	// ===========
	// H E L P E R
	// ===========

	private void SetEyes(Sprite eyes)
	{
		_eyeRenderer.sprite = eyes;
		_currentEyes = eyes;
	}

	private void SetMouth(Sprite mouth)
	{
		_mouthRenderer.sprite = mouth;
	}

    private void InitiateBlinkLoop()
    {
        if (_blinkRoutine != null)
            StopCoroutine(_blinkRoutine);

        _blinkRoutine = StartCoroutine(BlinkRoutine(Random.Range(1f, 5f)));
    }

    private IEnumerator BlinkRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        // BLINK
        _eyeRenderer.sprite = _eyesClosed;
        yield return new WaitForSeconds(0.1f);
		_eyeRenderer.sprite = _currentEyes;

        InitiateBlinkLoop();
    }

    void StartReturnIdle()
    {
        if (_idleRoutine != null)
        {
            StopCoroutine( _idleRoutine );
        }
        _idleRoutine = StartCoroutine( DelayedDefaultExpression() );
    }

    IEnumerator DelayedDefaultExpression()
    {
        yield return new WaitForSeconds( 5.0f );

        BecomeIdle();

        _idleRoutine = null;
    }

	void SetMouthPose( MouthPoseData mouthPose )
	{
		_mouthSkinnedMesh.SetBlendShapeWeight( 0, mouthPose.WideOpenBlendValue );
		_mouthSkinnedMesh.SetBlendShapeWeight( 1, mouthPose.RightSmileBlendValue );
		_mouthSkinnedMesh.SetBlendShapeWeight( 2, mouthPose.LeftSmileBlendValue );
		_mouthSkinnedMesh.SetBlendShapeWeight( 3, mouthPose.LeftFrownBlendValue );
		_mouthSkinnedMesh.SetBlendShapeWeight( 4, mouthPose.RightFrownBlendValue );
		_mouthSkinnedMesh.SetBlendShapeWeight( 5, mouthPose.OMouthBlendValue );

		_currMouthPose = mouthPose;
	}

	void TransitionMouthPose( MouthPoseData newPose )
	{
		if( _mouthPoseRoutine != null )
		{
			StartCoroutine( WaitForMouthTransition( newPose ) );
		}
		else
		{
			_mouthPoseRoutine = StartCoroutine( MouthTransitionRoutine( newPose ) );
		}
	
	}

	IEnumerator WaitForMouthTransition( MouthPoseData newPose )
	{
		yield return new WaitUntil( () => _mouthPoseRoutine == null );

		_mouthPoseRoutine = StartCoroutine( MouthTransitionRoutine( newPose ) );
	}

	IEnumerator MouthTransitionRoutine( MouthPoseData newPose )
	{
		float timer = 0.0f;
		float mouthTransProgress = 0.0f;

		while( timer < MouthPoseManager.instance.MouthPoseTransitionTime )
		{
			mouthTransProgress = MouthPoseManager.instance.MouthTransitionAnimCurve.Evaluate( timer / MouthPoseManager.instance.MouthPoseTransitionTime );

			_mouthSkinnedMesh.SetBlendShapeWeight( 0, Mathf.Lerp( _currMouthPose.WideOpenBlendValue, newPose.WideOpenBlendValue, mouthTransProgress ) );
			_mouthSkinnedMesh.SetBlendShapeWeight( 1, Mathf.Lerp( _currMouthPose.RightSmileBlendValue, newPose.RightSmileBlendValue, mouthTransProgress ) );
			_mouthSkinnedMesh.SetBlendShapeWeight( 2, Mathf.Lerp( _currMouthPose.LeftSmileBlendValue, newPose.LeftSmileBlendValue, mouthTransProgress ) );
			_mouthSkinnedMesh.SetBlendShapeWeight( 3, Mathf.Lerp( _currMouthPose.LeftFrownBlendValue, newPose.LeftFrownBlendValue, mouthTransProgress ) );
			_mouthSkinnedMesh.SetBlendShapeWeight( 4, Mathf.Lerp( _currMouthPose.RightFrownBlendValue, newPose.RightFrownBlendValue, mouthTransProgress ) );
			_mouthSkinnedMesh.SetBlendShapeWeight( 5, Mathf.Lerp( _currMouthPose.OMouthBlendValue, newPose.OMouthBlendValue, mouthTransProgress ) );

			timer += Time.deltaTime;

			yield return 0;
		}

		SetMouthPose( newPose );

		_mouthPoseRoutine = null;
	}

//	void OnValidate()
//	{
//		if( Application.isPlaying )
//		{
//			TransitionMouthPose( MouthPoseManager.instance.MouthPoseArray[_mouthPoseIndex] );
//		}
//
//		if( enableMouthSprite != _mouthRenderer.enabled )
//		{
//			_mouthRenderer.enabled = enableMouthSprite;
//		}
//	}
}
