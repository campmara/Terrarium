using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public struct MouthPoseData
{
	[SerializeField] private string _poseName;
	public float WideOpenBlendValue;
	public float RightSmileBlendValue;
	public float LeftSmileBlendValue;
	public float LeftFrownBlendValue;
	public float RightFrownBlendValue;
	public float OMouthBlendValue;
}

[Serializable]
public struct TempMouthBlendData
{
	[SerializeField] private string _poseName;
	public float OMouthBlendValue;
	public float FrownBlendValue;
	public float SmileBlendValue;
}

public class MouthPoseManager : ScriptableObjectSingleton<MouthPoseManager> 
{
	public List<MouthPoseData> MouthPoseArray;

	public List<TempMouthBlendData> TempMouthBlendList;

	public int StartMouthPoseIndex = 0;

	public float MouthPoseTransitionTime = 0.0f;

	public AnimationCurve MouthTransitionAnimCurve;
}
