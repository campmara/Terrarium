using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class FacePose
{
	public string PoseName = null;

	[HeaderAttribute("Left Eye Blend Values"), Space(2.5f)]
	public EyeBlendData LeftEyePose;

	[HeaderAttribute("Right Eye Blend Values"), Space(2.5f)]
	public EyeBlendData RightEyePose;

	[HeaderAttribute("Mouth Blend Values"), Space(2.5f)]
	public MouthBlendData MouthPose;
}

[Serializable]
public struct MouthBlendData
{	
	public float OMouthBlendValue;
	public float FrownBlendValue;
	public float SmileBlendValue;
}

[Serializable]
public struct EyeBlendData
{
	public float DespairBlendValue;
	public float WideBlendValue;
	public float AngryBlendValue;
	public float HalfOpenBlendValue;
	public float ClosedBlendValue;
	public float SadBlendValue;
	public float HappyBlendValue;
}

public class MouthPoseManager : ScriptableObjectSingleton<MouthPoseManager> 
{
	public List<FacePose> FacePoseList;

	public int StartMouthPoseIndex = 0;

	public float MouthPoseTransitionTime = 0.0f;

	public AnimationCurve MouthTransitionAnimCurve;
}
