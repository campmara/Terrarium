using UnityEngine;

// Interface for character control states.
public class RollerState : MonoBehaviour
{
	// =================
	// V A R I A B L E S
	// =================

	protected RollerController _roller;
	public RollerController RollerParent { get { return _roller; } set { _roller = value; } }

	// ============================
	// V I R T U A L  M E T H O D S
	// ============================

	public virtual void Enter( P_ControlState prevState ) {}
	public virtual void Exit( P_ControlState nextState ) {}

	public virtual void HandleInput(InputCollection input) {}

	// ==========================
	// H E L P E R  M E T H O D S
	// ==========================

	protected void HandlePickup()
	{
		CheckForPickupObject();
	}
		
	private void CheckForPickupObject()
	{
		Vector3 _pickupCenter = _roller.transform.position + ( Vector3.up * RollerConstants.PICKUP_CHECKHEIGHT ) + _roller.transform.forward;

		Collider[] overlapArray = Physics.OverlapSphere( _pickupCenter, RollerConstants.PICKUP_CHECKRADIUS );

		if ( overlapArray.Length > 0 )
		{
			Pickupable pickup = null;
			//if the pickupable is a plant, we can only pick it up if it's still in seed stage
			foreach( Collider col in overlapArray )
			{
				pickup = col.gameObject.GetComponent<Pickupable>();
				if( pickup != null )
				{					
					break;
				}
			}

			ObjectArmFocus( pickup );
		}
	}

    protected void CheckForReachable( bool rightArmReaching )
    {
        Vector3 _pickupCenter = _roller.transform.position + ( Vector3.up * RollerConstants.PICKUP_CHECKHEIGHT );

        Collider[] overlapArray = Physics.OverlapSphere( _pickupCenter, RollerConstants.IK_REACH_CHECKRADIUS, 1 << LayerMask.NameToLayer("Touchable") );

        if (overlapArray.Length > 0)
        {           
            // Pick a random reach point
            SetArmReach( overlapArray[Random.Range( 0, overlapArray.Length )].transform, rightArmReaching );
        }
    }

    private void SetArmReach( Transform reachable, bool rightArmReaching )
    {
        _roller.IK.SetReachTarget( reachable, rightArmReaching );
    }

	private void ObjectArmFocus( Pickupable pickup )
	{
		// update the ik
		if (pickup != null )
		{
			_roller.IK.SetArmTarget( pickup.transform );
		}		
		else
		{
			_roller.IK.SetArmTarget( null );
		}
	}

	public void HandleGrabObject()
	{
		PickUpObject( _roller.IK.LeftArmTargetTrans.GetComponent<Pickupable>() );		
	}

	private void PickUpObject( Pickupable pickup )
	{
		_roller.CurrentHeldObject = pickup;
	    _roller.CurrentHeldObject.gameObject.layer = LayerMask.NameToLayer("HeldObject");
		_roller.ChangeState( P_ControlState.PICKINGUP);		

		AudioManager.instance.PlayClipAtIndex( AudioManager.AudioControllerNames.PLAYER_ACTIONFX, 1 );
	}

	protected void HandleDropHeldObject()
	{
		DropHeldObject();
	}

	void DropHeldObject()
	{
		if (_roller.CurrentHeldObject != null)
		{
		    _roller.CurrentHeldObject.gameObject.layer = LayerMask.NameToLayer("Default");  // TODO: make sure seeds go back to their original layer ? Do we reach for seeds?
		    _roller.CurrentHeldObject.DropSelf();
			_roller.CurrentHeldObject = null;
		}

		// update the ik
		_roller.IK.LetGo();

		AudioManager.instance.PlayClipAtIndex( AudioManager.AudioControllerNames.PLAYER_ACTIONFX, 0 );
	}
}
