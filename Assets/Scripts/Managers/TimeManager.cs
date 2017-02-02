using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public class TimeManager : SingletonBehaviour<TimeManager> 
{
	public enum TimeState
	{
		NORMAL,
		FROZEN
	}

	TimeState _curState = TimeState.NORMAL;
	float _curTime = 0.0f;

    DateTime _realWorldTime = new DateTime();
    public DateTime RealWorldTime { get { return _realWorldTime; } }
    public DateTime RealWorldNow { get { return DateTime.Now; } }

	List<GameEvent> _eventQueue;

	public override void Initialize ()
	{
        _realWorldTime = DateTime.Now;
        _eventQueue = new List<GameEvent>();
	    isInitialized = true;
	}

	void Update () 
	{
		if( _curState == TimeState.NORMAL )
		{
			ProcessQueue();
			PlantManager.instance.GrowPlants();

			_curTime += Time.deltaTime;
		}
	}

	void ProcessQueue()
	{
	    GameEvent curEvent = null;
	    float eventTime = 0.0f;

	    if( _eventQueue.Count != 0 )
	    {
	        curEvent = _eventQueue[0];
	        eventTime = _eventQueue[0].GameTimeExecution;
	    }

		while( _eventQueue.Count != 0  &&  eventTime <= _curTime )
		{
		    curEvent = _eventQueue[0];
		    eventTime = _eventQueue[0].GameTimeExecution;

		    _eventQueue.Remove( curEvent );
			curEvent.Execute();
		}
	}
		
	public void AddEvent( GameEvent gameEvent )
	{
	    if (_eventQueue.Contains(gameEvent) == false)
	    {
			gameEvent.SetInGameExecutionTime( _curTime + gameEvent.TimeUntilExecution );
	     	_eventQueue.Add( (GameEvent) gameEvent );
			_eventQueue = _eventQueue.OrderBy( e => e.GameTimeExecution ).ToList();
		}
	}


	public void ChangeState( TimeState newState )
	{
		_curState = newState;
	}
}
