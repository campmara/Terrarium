using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropFruitEvent : GameEvent
{
	BigPlant _tree;

	public DropFruitEvent( BigPlant tree, float timeUntilDrop ) : base( timeUntilDrop )
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
