using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropFruitEvent : Event
{
	Growable _tree;

	public DropFruitEvent( Growable tree, float timeUntilDrop ) : base(timeUntilDrop)
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
