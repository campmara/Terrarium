using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class Seed : Pickupable 
{
    [SerializeField] GameObject _moundPrefab = null;
	BasePlant.PlantType _moundType = BasePlant.PlantType.NONE;
	float _timeSinceLastPickup = 0.0f;
	float _timePassedTillDestroy = 60.0f;
	bool _hasFallen = false;
	const int  _selfPlantProbability = 50;
	const float _searchRadius = 30.0f;
	Tween _sinkTween = null;
	Collider _col;

    const float WIND_FORCESCALAR = 0.5f;

	[SerializeField] Vector2 _sinkSpeedRange = new Vector2( 10.0f, 20.0f );

	Coroutine _plantWaitRoutine = null;

	protected override void Awake()
	{
		base.Awake();
		_moundType = _moundPrefab.GetComponent<BasePlant>().MyPlantType;
		_col = GetComponent(typeof(Collider)) as Collider;

		float endScale = transform.localScale.x;
		this.transform.DOScale( endScale, 3.0f );//.OnComplete( EndSelfPlant );
	}
	void Update()
	{
		if( !_grabbed )
		{
			if( _timeSinceLastPickup >= _timePassedTillDestroy && PlantManager.instance.GetActiveSeedCount() > 2 )
			{			
				PlantManager.instance.DestroySeed( this, _moundType, false );
			}
			else
			{
				_timeSinceLastPickup += Time.deltaTime;
			}
		}
	}

    private void FixedUpdate()
    {
		if( !_grabbed )
		{
			_rigidbody.AddForce( WeatherManager.instance.WindForce * WIND_FORCESCALAR * Time.deltaTime );	
		}

    }

    public override void OnPickup( Transform grabTransform )
	{
		if (_sinkTween != null)
		{
			//_sinkTween.Rewind();
            _sinkTween.Kill();
            _sinkTween = null;
		}

		if( _plantWaitRoutine != null )
		{
			StopCoroutine( _plantWaitRoutine );
			_plantWaitRoutine = null;
		}

		base.OnPickup( grabTransform );

		_hasFallen = false;

		_rigidbody.velocity = Vector3.zero;
		_col.isTrigger = true;
	}

	public override void DropSelf()
	{
		base.DropSelf();
        BeginSelfPlant();
		_timeSinceLastPickup = 0.0f;
		_col.isTrigger = false;
	}

	public void DropOnRitual()
	{
		base.DropSelf();
	}

	public void TryPlanting()
	{
		if( _sinkTween == null )
		{
			if( _plantWaitRoutine != null )
			{
				StopCoroutine( _plantWaitRoutine );
				_plantWaitRoutine = null;
			}

			Vector3 plantPos = new Vector3( transform.position.x, 0.0f, transform.position.z ); 
			GameObject mound = Instantiate( _moundPrefab, plantPos, Quaternion.identity ) as GameObject; 
			PlantManager.instance.AddMound( mound.GetComponent<BasePlant>()  );
			GroundManager.instance.EmitDirtParticles(plantPos);
			PlantManager.instance.DestroySeed( this, _moundType, true );
		}
	}

	void BeginSelfPlant()
	{
		if( !_hasFallen )
		{
			if( _plantWaitRoutine == null && _sinkTween == null  )
			{
				StartCoroutine( DelayedBeginSelfPlant() );
			}
			else
			{
				Debug.Log( "Already Waiting to Fall & Plant", this );
			}
		}
		else if( _sinkTween == null )
		{
			Vector3 endPos = this.transform.position + (Vector3.down * 0.36f);
			float sinkTime = Random.Range( _sinkSpeedRange.x, _sinkSpeedRange.y );

			_sinkTween = this.transform.DOMove( endPos, sinkTime ).OnComplete( EndSelfPlant );
		}
	}

	IEnumerator DelayedBeginSelfPlant()
	{
		yield return new WaitUntil( () => _hasFallen );

		int dieRoll = (int)Random.Range( 0, 100 );

		if( dieRoll <= _selfPlantProbability )
		{
			BeginSelfPlant();
		}
	}

	void EndSelfPlant()
	{
		_sinkTween = null;
		TryPlanting();
	}

    protected override void HandleCollision( Collision col )
	{
		if( !_hasFallen )
		{
			if( col.gameObject.GetComponent<GroundDisc>() )
			{

				int dieRoll = (int)Random.Range( 0, 100 );

				if( dieRoll <= _selfPlantProbability )
				{
					BeginSelfPlant();
				}

				_hasFallen = true;
			}
		}
	}
}
