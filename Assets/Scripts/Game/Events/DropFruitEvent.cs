using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropFruitEvent : GameEvent
{
	BPGrowthController _tree;

	public DropFruitEvent( BPGrowthController tree, float timeUntilDrop ) : base( timeUntilDrop )
	{
		_tree = tree;
	}

	public override void Execute()
	{
		if( _tree )
		{
			_tree.DropFruit();
		}
	}
}
