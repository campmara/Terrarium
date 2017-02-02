using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEvent
{
	public float TimeUntilExecution = 0.0f;
    public float GameTimeExecution = 0.0f;

	public GameEvent(){}

	public GameEvent( float time )
	{
		TimeUntilExecution = time;
	}

    public void SetInGameExecutionTime(float time)
    {
        GameTimeExecution = time;
    }

	public virtual void Execute(){}
}