using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SPDeathController : PlantController
{
	void Awake()
	{
		_myPlant = GetComponent<BasePlant>();
		_controllerType = ControllerType.Death;
	}
		
	public override void UpdateState(){}
	public override void StopState(){}
	public override void StartState(){}

	public override GameObject SpawnChildPlant(){ return null; }

	public override void WaterPlant(){}
	public override void TouchPlant(){}
	public override void GrabPlant(){}
	public override void StompPlant(){}
}
