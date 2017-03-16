using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public class TimeManager : SingletonBehaviour<TimeManager> 
{
	public enum TimeState
	{
		NONE = 0,
		NORMAL,
		FROZEN
	}

	TimeState _curState = TimeState.NONE;
	float _curTime = 0.0f;

    DateTime _realWorldTime = new DateTime();
    public DateTime RealWorldTime { get { return _realWorldTime; } }
    public DateTime RealWorldNow { get { return DateTime.Now; } }

	List<GameEvent> _eventQueue;

	public delegate void MinuteDelegate();
	public MinuteDelegate MinuteCallback;
	bool minuteCallbackDone = false;

	public delegate void HourDelegate();
	public HourDelegate HourCallback;
	bool hourCallbackDone = false;

	void OnTheMinute()
	{
		// Do Something every minute.
		minuteCallbackDone = true;
	}

	void OnTheHour()
	{
		hourCallbackDone = true;
	}

	public override void Initialize ()
	{
        _realWorldTime = DateTime.Now;
        _eventQueue = new List<GameEvent>();

		MinuteCallback += OnTheMinute;
		HourCallback += OnTheHour;

		_curState = TimeState.NORMAL;

	    isInitialized = true;
	}

	void Update () 
	{
		_realWorldTime = DateTime.Now;

		if( _curState == TimeState.NORMAL )
		{
			ProcessQueue();
			PlantManager.instance.GrowPlants();

			_curTime += Time.deltaTime;
		}

		// Handle Minute Callback
		if (_realWorldTime.TimeOfDay.Seconds == 0 && minuteCallbackDone == false)
		{
			if( MinuteCallback != null )
			{
				MinuteCallback();	
			}
		}
		else if (_realWorldTime.TimeOfDay.Seconds == 5)
		{
			minuteCallbackDone = false;
		}

		// Handle Hourly Callback
		if (_realWorldTime.TimeOfDay.Minutes == 0 && hourCallbackDone == false)
		{
			if( HourCallback != null )
			{
				HourCallback();	
			}
		}
		else if (_realWorldTime.TimeOfDay.Minutes == 1)
		{
			hourCallbackDone = false;
		}
	}

	void ProcessQueue()
	{
	    GameEvent curEvent = null;
	    float eventTime = 0.0f;

		while( _eventQueue.Count != 0 )
		{
			curEvent = _eventQueue[0];
			eventTime = _eventQueue[0].GameTimeExecution;

			if( eventTime > _curTime )
			{
				break;
			}
			else
			{
				_eventQueue.Remove( curEvent );
				curEvent.Execute();
			}
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
