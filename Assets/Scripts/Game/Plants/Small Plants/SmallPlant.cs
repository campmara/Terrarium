using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmallPlant : BasePlant
{
//	[SerializeField] protected SmallPlantAssetKey _pAssetKey = SmallPlantAssetKey.NONE;
//	public SmallPlantAssetKey PAssetKey { get { return _pAssetKey; } set { _pAssetKey = value; } }
//
//	void Awake()
//	{
//		_activeController = GetComponent<SPGrowthController>();
//		_activeController.StartState();
//	}
//
//	public override void SwitchController( PlantController prevState )
//	{
//		DerivedSwitchController( prevState );
//	}
//
//	protected virtual void DerivedSwitchController( PlantController prevState )
//	{
//		if( prevState.GetComponent<SPGrowthController>() )
//		{
//			_activeController = GetComponent<SPDeathController>();
//		}
//		else
//		{
//			_activeController = GetComponent<SPDeathController>();
//		}
//
//		_activeController.StartState();
//	}
//
//	void OnDestroy()
//	{
//		PlantManager.ExecuteGrowth -= UpdatePlant;
//	}
}
