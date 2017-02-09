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
		CheckForPickup();
	}
		
	private void CheckForPickup()
	{
		Vector3 _pickupCenter = _roller.transform.position + ( Vector3.up * RollerConstants.PICKUP_CHECKHEIGHT ) + _roller.transform.forward;

		Collider[] overlapArray = Physics.OverlapSphere( _pickupCenter, RollerConstants.PICKUP_CHECKRADIUS );

		if ( overlapArray.Length > 0 )
		{
			//if the pickupable is a plant, we can only pick it up if it's still in seed stage
			foreach( Collider col in overlapArray )
			{
				Pickupable pickup = col.gameObject.GetComponent<Pickupable>();
				if( pickup )
				{
					PickUpObject( pickup );
					break;
				}
			}
		}
	}

	private void PickUpObject( Pickupable pickup )
	{
		_roller.CurrentHeldObject = pickup;
	    _roller.CurrentHeldObject.gameObject.layer = LayerMask.NameToLayer("HeldObject");
		_roller.ChangeState( _roller.State, P_ControlState.PICKINGUP );

		// update the ik
		_roller.IK.SetArmTarget(pickup.transform);
	}

	protected void HandleDropHeldObject()
	{
		DropHeldObject();
	}

	void DropHeldObject()
	{
		if (_roller.CurrentHeldObject != null)
		{
		    _roller.CurrentHeldObject.gameObject.layer = LayerMask.NameToLayer("Default");
		    _roller.CurrentHeldObject.DropSelf();
			_roller.CurrentHeldObject = null;

			// update the ik
			_roller.IK.LetGo();
		}
	}
}
