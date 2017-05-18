using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BigPlantPickupable : Pickupable {

	public const float BIGPLANT_MINTUGDIST = 0.5f;	
	public const float BIGPLANT_MAXTUGDIST = 0.75f;	
	
	Vector3 _grabberDirection = Vector3.zero;
	const float BIGPLANT_TUGANGLE_MAXOFFSET = 2.0f;
	Quaternion _tugDirection = Quaternion.identity;
    public Quaternion TugDirection { get { return _tugDirection; } }

    public const float BIGPLANT_TUGANGLE_MAX = 0.07f;
    const float BIGPLANT_TUGANGLE_RETURNSPEED = 7.0f;

    const float BIGPLANT_SHIVERDURATION = 0.5f;
    const float BIGPLANT_SHIVERDURATIONDEC = 0.01f;
    const float BIGPLANT_SHIVERDIST = 0.075f;

    Coroutine _springRoutine = null;

	void FixedUpdate()
	{
		if( _grabbed )
		{
			_grabberDirection = _grabTransform.position - this.transform.position;             

			//_grabberBurdenInterp = Mathf.InverseLerp( BIGPLANT_MINTUGDIST, BIGPLANT_MAXTUGDIST, Vector3.Distance(_grabTransform.position, this.transform.position + (_grabberDirection.normalized * this.GetComponent<BasePlant>().InnerRadius ) ) );

            // TODO: Make max angle be more determined by Plant Health
            _tugDirection = Quaternion.FromToRotation( Vector3.up, Vector3.Slerp( Vector3.up, _grabberDirection, Mathf.Lerp( 0.0f, BIGPLANT_TUGANGLE_MAX, _grabberBurdenInterp ) ) );
            
            transform.rotation = _tugDirection;
		}
	}

	public override void OnPickup( Transform grabTransform )
	{
		_grabbed = true;
		_grabTransform = grabTransform;
				
		_grabberDirection = Vector3.zero;
		_tugDirection = Quaternion.identity;

		if( this.GetComponent<PickupCollider>() != null )
        {
            this.GetComponent<PickupCollider>().LockedRotation = false;
        }
	}

	public override void DropSelf()
	{
		_grabbed = false;
		_grabTransform = null;


		if( _springRoutine != null )
		{
			StopCoroutine( _springRoutine );
			_springRoutine = null;
		}

        // Rotate plant back to being upright
        this.GetComponent<BPGrowthController>().pAudioController.PlayPlantShakeSound();

        _springRoutine = StartCoroutine( DelayedReleaseBigPlant() );
	}

	IEnumerator DelayedReleaseBigPlant()
	{
		Vector3 springDirection = Vector3.Reflect( -_grabberDirection, Vector3.up );
		float springInterp = _grabberBurdenInterp * 0.75f;
		Quaternion springTarget = Quaternion.FromToRotation( Vector3.up, Vector3.Slerp( Vector3.up, springDirection, Mathf.Lerp( 0.0f, BIGPLANT_TUGANGLE_MAX, springInterp ) ) );

		while( springInterp > _grabberBurdenInterp * 0.05f )
		{
			while( Quaternion.Angle( this.transform.rotation, springTarget ) > 1.0f )
			{
				transform.rotation = Quaternion.Slerp(transform.rotation, springTarget, BIGPLANT_TUGANGLE_RETURNSPEED * 2.5f * Time.deltaTime);

				yield return 0;
			}

			springDirection = Vector3.Reflect( -springDirection, Vector3.up );
			springInterp *=  0.5f;
			springTarget = Quaternion.FromToRotation( Vector3.up, Vector3.Slerp( Vector3.up, springDirection, Mathf.Lerp( 0.0f, BIGPLANT_TUGANGLE_MAX, springInterp ) ) );
		}

		while( Quaternion.Angle( this.transform.rotation, Quaternion.identity ) > 0.0f )
		{
			transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.identity, BIGPLANT_TUGANGLE_RETURNSPEED * Time.deltaTime);

			yield return 0;
		}

		this.transform.rotation = Quaternion.identity;

		_grabberBurdenInterp = 0.0f;
		_grabberDirection = Vector3.zero;
		_tugDirection = Quaternion.identity;

        if (this.GetComponent<PickupCollider>() != null)
        {
            this.GetComponent<PickupCollider>().LockedRotation = true;
        }

		_springRoutine = null;
    }

    public void ShiverTree()
    {
        StartCoroutine( TreeShiverRoutine() );
    }

    IEnumerator TreeShiverRoutine( int shiverCount = 6 )
    {
        Vector3 randDir;
        Vector3 springDirection;
        Quaternion springTarget;
        float timer = 0.0f;
        float currShiverDuration = BIGPLANT_SHIVERDURATION;        

        for (int i = 0; i < shiverCount; ++i )
        {
            randDir = JohnTech.GenerateRandomXZDirection();
            springDirection = Vector3.Reflect( -randDir, Vector3.up );
            springTarget = Quaternion.FromToRotation( Vector3.up, Vector3.Slerp( Vector3.up, springDirection, BIGPLANT_SHIVERDIST ) );
            timer = 0.0f;

            while ( timer < currShiverDuration )
            {
                timer += Time.deltaTime;

                transform.rotation = Quaternion.Slerp( transform.rotation, springTarget, timer / currShiverDuration );

                yield return 0;
            }

            springTarget = Quaternion.identity;
            timer = 0.0f;

            while ( timer < currShiverDuration )
            {
                timer += Time.deltaTime;

                transform.rotation = Quaternion.Slerp( transform.rotation, springTarget, timer / currShiverDuration );

                yield return 0;
            }

            this.transform.rotation = Quaternion.identity;
            currShiverDuration -= BIGPLANT_SHIVERDURATIONDEC;
        }
        
    }

    public void PunchTreeRotation( float strengthScalar = 4.0f, float duration = BIGPLANT_SHIVERDURATION )
    {
        Vector3 playerDir = PlayerManager.instance.Player.transform.position - this.transform.position;
        playerDir.Normalize();
        this.transform.DOPunchRotation( playerDir * strengthScalar, duration );
    }

}
