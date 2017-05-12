using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlendTransitionHelper : MonoBehaviour {

	SkinnedMeshRenderer _skinnedMeshRenderer = null;

	Coroutine _mouthPoseRoutine;
	[SerializeField] int _mouthPoseIndex = 0;
	TempMouthBlendData _currMouthPose;

	// Use this for initialization
	void Awake () 
	{
		_skinnedMeshRenderer = this.GetComponent<SkinnedMeshRenderer>();

		Debug.Assert( _skinnedMeshRenderer != null );
	}
		
	void SetMouthPose( TempMouthBlendData mouthPose )
	{
		_skinnedMeshRenderer.SetBlendShapeWeight( 0, mouthPose.OMouthBlendValue );
		_skinnedMeshRenderer.SetBlendShapeWeight( 1, mouthPose.FrownBlendValue );
		_skinnedMeshRenderer.SetBlendShapeWeight( 2, mouthPose.SmileBlendValue );

		_currMouthPose = mouthPose;
	}

	void TransitionMouthPose( TempMouthBlendData newPose )
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

	IEnumerator WaitForMouthTransition( TempMouthBlendData newPose )
	{
		yield return new WaitUntil( () => _mouthPoseRoutine == null );

		_mouthPoseRoutine = StartCoroutine( MouthTransitionRoutine( newPose ) );
	}

	IEnumerator MouthTransitionRoutine( TempMouthBlendData newPose )
	{
		float timer = 0.0f;
		float mouthTransProgress = 0.0f;

		while( timer < MouthPoseManager.instance.MouthPoseTransitionTime )
		{
			mouthTransProgress = MouthPoseManager.instance.MouthTransitionAnimCurve.Evaluate( timer / MouthPoseManager.instance.MouthPoseTransitionTime );

			_skinnedMeshRenderer.SetBlendShapeWeight( 0, Mathf.Lerp( _currMouthPose.OMouthBlendValue, newPose.OMouthBlendValue, mouthTransProgress ) );
			_skinnedMeshRenderer.SetBlendShapeWeight( 1, Mathf.Lerp( _currMouthPose.FrownBlendValue, newPose.FrownBlendValue, mouthTransProgress ) );
			_skinnedMeshRenderer.SetBlendShapeWeight( 2, Mathf.Lerp( _currMouthPose.SmileBlendValue, newPose.SmileBlendValue, mouthTransProgress ) );

			timer += Time.deltaTime;

			yield return 0;
		}

		SetMouthPose( newPose );

		_mouthPoseRoutine = null;
	}



	void OnValidate()
	{
		if( _mouthPoseIndex > MouthPoseManager.instance.TempMouthBlendList.Count - 1 )
		{
			_mouthPoseIndex = MouthPoseManager.instance.TempMouthBlendList.Count - 1;
		}
		else if( _mouthPoseIndex < 0 )
		{
			_mouthPoseIndex = 0;
		}

		if( Application.isPlaying )
		{
			TransitionMouthPose( MouthPoseManager.instance.TempMouthBlendList[_mouthPoseIndex] );
		}
		else
		{
			// Need to have Skinned Mesh renderer addded not in Awake if u wannad do this
			//SetMouthPose( MouthPoseManager.instance.TempMouthBlendList[_mouthPoseIndex] );
		}
	}
}
