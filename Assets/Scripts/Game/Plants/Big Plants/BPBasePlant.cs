using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class BPBasePlant : BasePlant
{
	public enum BigPlantType : int 
	{
		NONE = -1,
		POINT,
		FLOWERING,
		LEAFY,
		PBUSH,
		LIMBER
	}
	[SerializeField] BigPlantType _type = BigPlantType.NONE;
	public BigPlantType PlantType { get { return _type; } }
}
