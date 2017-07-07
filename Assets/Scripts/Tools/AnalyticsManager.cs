using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

public class AnalyticsManager
{
	protected static void SendAnalyticEvent(string EventName, Dictionary<string, object> EventParameters)
	{
#if UNITY_ANALYTICS
		AnalyticsResult Result = Analytics.CustomEvent(EventName, EventParameters);

		if( Result != AnalyticsResult.Ok )
		{
			Debug.Log("Analytics Event Failed: " + Result.ToString());
		}
#endif
	}
}
