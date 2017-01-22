using System.Collections;
using System.Collections.Generic;
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

	SortedList<float, Event> _eventQueue = new SortedList<float, Event>();

	public override void Initialize ()
	{
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
		Event curEvent = _eventQueue.Values.Count == 0 ? null : _eventQueue.Values[0];
		float eventTime = _eventQueue.Values.Count == 0 ? 0.0f : _eventQueue.Keys[0];

		while( curEvent != null && _eventQueue.Count != 0 &&  eventTime <= _curTime )
		{
			curEvent = _eventQueue.Values[0];
			eventTime = _eventQueue.Keys[0];
			_eventQueue.Remove( eventTime );
			curEvent.Execute();
		}
	}
		
	public void AddEvent(Event gameEvent )
	{
		//WARNING THIS IS BAD AND NEEDS FIXED!!
		if( !_eventQueue.ContainsKey( _curTime + gameEvent.TimeUntilExecution )  )
		{
			_eventQueue.Add( _curTime + gameEvent.TimeUntilExecution, gameEvent );
		}
	}

	public void ChangeState( TimeState newState )
	{
		_curState = newState;
	}
}
