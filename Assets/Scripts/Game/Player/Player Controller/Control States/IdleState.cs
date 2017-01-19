using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : RollerState
{
	
    public override void Enter( RollerController parent )
    {
        Debug.Log( "ENTER IDLE STATE" );
        roller = parent;
    }

    public override void Exit()
    {
        
    }

    public override void HandleInput( InputCollection input )
    {
        
    }
}
