using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class PlantController : MonoBehaviour
{
	protected BasePlant _myPlant = null;
	protected ControllerType _controllerType = ControllerType.Unassigned;
	public ControllerType ControlType { get { return _controllerType; } }

	// ** SPAWN VARS ** // 

	public enum ControllerType
	{
		Death,
		Growth,
		Unassigned
	}

	public abstract void Init();
	public abstract void UpdateState();
	public abstract void StopState();
	public abstract void StartState();

	public virtual GameObject DropFruit(){ return null; }

	public abstract void WaterPlant();
	public abstract void TouchPlant();
	public abstract void GrabPlant();
	public abstract void StompPlant();

	public virtual void HandleSinging( bool entering ){}
}