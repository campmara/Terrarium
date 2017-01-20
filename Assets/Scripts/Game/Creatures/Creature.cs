using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Creature : MonoBehaviour 
{
	enum BehaviorType
	{
		IDLE,
		WANDER,
		FOLLOW
	}
	[SerializeField] BehaviorType behavior;

	protected CreatureBehavior currentBehavior;
	IdleBehavior idle;
	WanderBehavior wander;
	FollowBehavior follow;

	protected void ChangeBehavior(CreatureBehavior desiredBehavior)
	{
		if (currentBehavior != null)
		{
			currentBehavior.Exit();
		}

		currentBehavior = desiredBehavior;
		currentBehavior.Enter();
	}

	protected virtual void Awake()
	{
		idle = gameObject.AddComponent(typeof(IdleBehavior)) as IdleBehavior;
		wander = gameObject.AddComponent(typeof(WanderBehavior)) as WanderBehavior;
		follow = gameObject.AddComponent(typeof(FollowBehavior)) as FollowBehavior;

		switch(behavior)
		{
		case BehaviorType.IDLE:
			ChangeBehavior(idle);
			break;
		case BehaviorType.WANDER:
			ChangeBehavior(wander);
			break;
		case BehaviorType.FOLLOW:
			ChangeBehavior(follow);
			break;
		}
	}

	protected virtual void Start()
	{
		
	}

	protected virtual void Update()
	{
		currentBehavior.Handle();
	}

	protected virtual void FixedUpdate()
	{
		
	}
}
