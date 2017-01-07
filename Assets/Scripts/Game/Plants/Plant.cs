using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plant : MonoBehaviour 
{
	public enum PlantType
	{
		RegularTree,
		Grass,
		None
	};

	public enum GrowthStage : int
	{
		Unplanted = 0,
		Sprout = 1,
		Bush = 2 ,
		Sapling = 3,
		Final = 4
	};
		
	float [] plantRadius = new float[]{ 2.0f, // sprout radius
										6.0f, // bush radius
										8.0f, // sapling radius
										12.0f }; // tree radius

	const float _growthRate = 0.0f;
	const float _waterMultiplier = 0.0f;
	float _curGrowthRate = 0.0f;
	float _animTimeStamp = 0.0f;

	GrowthStage _curStage = GrowthStage.Unplanted;
	public GrowthStage CurStage { get { return _curStage; } }


	//Animations we can either drag and drop or just load in 
	Animation [] _animations = new Animation[4];
	Animation _curAnim =  null;

	AnimationEvent _transitionEvent = new AnimationEvent();

	void Awake()
	{
		//_transitionEvent.functionName = "TryTransitionStages";
		//_transitionEvent.time = _curAnim.clip.length;
		//_curAnim.clip.AddEvent( _transitionEvent );
	}
		
	void TransitionStages()
	{
		SwitchToNextStage();

		Debug.Log("SWITCHING STAGES");
		//_curAnim = _animations[(int)_curStage-1];
		//_curGrowthRate = 0.0f;
		//_animTimeStamp = 0.0f;

		//_transitionEvent.time = _curAnim.clip.length;
		//_curAnim.clip.AddEvent( _transitionEvent );
	}

	bool TryTransitionStages()
	{
		//if we're not on the final stage, check to see if there are other things we're in the radius of 
		RaycastHit[] overlappingObjects = Physics.SphereCastAll( transform.position,  plantRadius[ (int)_curStage ], new Vector3(0f,0f,-1f));
		if( overlappingObjects.Length != 0 )
		{
			foreach( RaycastHit rHit in overlappingObjects)
			{
				if(rHit.collider.gameObject.GetComponent<Plant>() && rHit.collider.gameObject != gameObject )
				{
					return false;
				}
			}
			TransitionStages();
			return true;
		}

		return false;
	}


	public bool TryPickUp()
	{
		if( _curStage == GrowthStage.Unplanted )
		{
			//do whatever internal updates need to happen
			return TryTransitionStages();
		}
		else
		{
			return false;
		}
	}

	public bool TryDrop()
	{
		//if something is in the way and it won't be able to sit, don't let player drop
		return TryTransitionStages();
	}

	public void WaterPlant()
	{
		// ups the rate if it's in a certain mode
		if( _curStage == GrowthStage.Sprout || _curStage == GrowthStage.Bush )
		{
			_curGrowthRate *= _waterMultiplier;
			//CHANGE THE SPEED
		}
	}

	void SwitchToNextStage()
	{
		if( _curStage != GrowthStage.Final )
		{
			_curStage += 1; // they are int enums so we can just increment
		}
	}
}
