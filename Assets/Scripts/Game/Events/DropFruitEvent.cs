using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropFruitEvent : GameEvent
{
	BasePlant _tree;

	public DropFruitEvent( BasePlant tree, float timeUntilDrop ) : base( timeUntilDrop )
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
