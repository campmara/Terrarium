using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrowthEndHandler : MonoBehaviour
{
	public PlantController _controller = null;

	public void FinishedGrowing()
	{
		_controller.FinishedGrowing();
	}
}
