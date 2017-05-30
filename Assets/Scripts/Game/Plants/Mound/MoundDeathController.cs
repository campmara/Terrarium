using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoundDeathController : PlantController
{
	// the existence of this class is a formality
	// the existence of this class is a formality
	// the existence of this class is a formality

	public override void Init()
	{
				_controllerType = ControllerType.Death;
	}

	public override void StartState(){}
	public override void UpdateState(){}
	public override void StopState(){}		
	public override void WaterPlant(){}
	public override void TouchPlant(){}
	public override void GrabPlant(){}
	public override void StompPlant(){}
}
