using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoundDeathController : PlantController
{
	[SerializeField] Material _deadMaterial = null;

	void Awake()
	{
		_myPlant = GetComponent<BasePlant>();
		_controllerType = ControllerType.Death;
	}

	public override void StartState()
	{
		_myPlant = GetComponent<BasePlant>();
		transform.GetChild(1).GetComponent<MeshRenderer>().material = _deadMaterial;
		PlantManager.ExecuteGrowth += UpdateState;
	}

	public override void UpdateState(){}

	public override void StopState()
	{
		PlantManager.instance.DeleteMound( _myPlant );
	}

	public override GameObject SpawnChildPlant(){ return null; }

	public override void WaterPlant()
	{
		StopState();
	}

	public override void TouchPlant(){}
	public override void GrabPlant(){}
	public override void StompPlant(){}
}
