using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Event
{
	public float TimeUntilExecution = 0.0f;

	public Event(){}

	public Event(float time)
	{
		TimeUntilExecution = time;
	}

	public virtual void Execute(){}
}