using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridBase : MonoBehaviour {

	protected GridBase _gridAbove = null;
	public GridBase Above { get { return _gridAbove; } set { _gridAbove = value; } }
	protected GridBase _gridBelow = null;
	public GridBase Below { get { return _gridBelow; } set { _gridBelow = value; } }
	protected GridBase _gridRight = null;
	public GridBase Right { get { return _gridRight; } set { _gridRight = value; } }
	protected GridBase _gridLeft = null;
	public GridBase Left { get { return _gridLeft; } set { _gridLeft = value; } }

	public enum SpaceState
	{
		NONE = -1,
		PLAYER_ADJACENT,
		PLAYER_OCCUPIED
	}
	[SerializeField, ReadOnlyAttribute] protected SpaceState _currState = SpaceState.NONE;
	public SpaceState State { get { return _currState; } set { _currState = value; } }

    public enum SpaceType
    {
        NONE = -1,
        SQUARE,
        CIRCLE,
        TRIANGLE
    }
    [SerializeField, ReadOnlyAttribute]
    protected SpaceType _currType = SpaceType.NONE;

    public virtual void SetSpaceType() { }

    protected SpriteRenderer _gridSprite = null;
}
