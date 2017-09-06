using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TBFAnalytics : AnalyticsManager
{
	static bool _bibiActivated = false;		

	public static void BibiFirstActivatedEvent()
	{
		if(!_bibiActivated)
		{
			SendAnalyticEvent("BibiActivated", null);

			_bibiActivated = true;
		}
	}


}
