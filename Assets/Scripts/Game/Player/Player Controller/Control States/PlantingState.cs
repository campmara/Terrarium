using UnityEngine;
using DG.Tweening;

public class PlantingState : RollerState 
{
    Tween _plantTween = null;

    public override void Enter( P_ControlState prevState )
    {
        Debug.Log( "ENTER PLANTING STATE" );

        // Handle transition
        switch ( prevState )
        {
            case P_ControlState.CARRYING:            
                HandleBeginPlanting();
                break;
        }

    }

    public override void Exit( P_ControlState nextState )
    {
        Debug.Log( "EXITTING PLANTING STATE" );

        switch( nextState )
        {
            case P_ControlState.CARRYING:
                // Bring Seed back into hands
                _plantTween.Restart();
                break;
            case P_ControlState.WALKING:
                if( _plantTween == null || !_plantTween.IsComplete() )
                {
                    HandleDropHeldObject();
                }                
                break;
        }

        if (_plantTween != null)
        {
			_plantTween.Complete();
            _plantTween.Kill();
            _plantTween = null;
        }
    }

    public override void HandleInput( InputCollection input )
    {
        // A BUTTON
        if (!input.AButton.IsPressed)
        {
            // Return to Carry State
            _roller.ChangeState( P_ControlState.PLANTING, P_ControlState.CARRYING );
        }

        // B BUTTON
        if (input.BButton.IsPressed)
        {
            // Drop Seed
            _roller.ChangeState( P_ControlState.PLANTING, P_ControlState.WALKING );
        }

    }

    void HandleBeginPlanting()
    {
        // Right now just gonna move seed Down...
        if ( _roller.CurrentHeldObject != null )
        {
			_plantTween = _roller.CurrentHeldObject.transform.DOMoveY(RollerConstants.PLANTING_ENDY, RollerConstants.PLANTING_TIME ).OnComplete( () => HandlePlantingEnd() ).SetAutoKill( false ).SetEase(Ease.InBack);
        }        
    }

    void HandlePlantingEnd()
    {
        // Handle a separate function for planting the seed

		Seed seed = _roller.CurrentHeldObject.GetComponent<Seed>();
		if( seed != null )
		{
			seed.TryPlanting();
		}

        HandleDropHeldObject();
        _roller.ChangeState( P_ControlState.PLANTING, P_ControlState.WALKING );
    }
}
