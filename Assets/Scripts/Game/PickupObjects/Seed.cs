using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class Seed : Pickupable 
{
    [SerializeField] GameObject _moundPrefab = null;
	BasePlant.PlantType _moundType = BasePlant.PlantType.NONE;
	[HideInInspector] public BasePlant PlantData;
	float _timeSinceLastPickup = 0.0f;
	float _timePassedTillDestroy = 60.0f;
	bool _isGrounded = false;
	const float _searchRadius = 30.0f;
	const float _stoppedThreshold = .1f;
	const float _sinkHeight = .4f;
	Tween _sinkTween = null;
	Collider _col;

    const float WIND_FORCESCALAR = 0.5f;

	[SerializeField] MinMax _sinkSpeedRange = new MinMax( 17.0f, 25.0f );

	protected override void Awake()
	{
		base.Awake();
		PlantData = _moundPrefab.GetComponent<BasePlant>();
		_moundType = PlantData.MyPlantType;
		_col = GetComponent(typeof(Collider)) as Collider;
		float endScale = transform.localScale.x;
		this.transform.DOScale( endScale, 3.0f );

	}
	void Update()
	{
		if( !_grabbed )
		{
			if( _timeSinceLastPickup >= _timePassedTillDestroy && PlantManager.instance.GetActiveSeedCount() > 2 )
			{			
				PlantManager.instance.DestroySeed( this, _moundType, false );
			}
			else if( _rigidbody.velocity.magnitude <= _stoppedThreshold )
			{
				if( _isGrounded && transform.position.y < _sinkHeight )
				{
					BeginSelfPlant();
				}
			}
			else
			{
				_timeSinceLastPickup += Time.unscaledDeltaTime;
			}
		}
	}

    private void FixedUpdate()
    {
		if( !_grabbed )
		{
			_rigidbody.AddForce( WeatherManager.instance.WindForce * WIND_FORCESCALAR * Time.unscaledDeltaTime);	
		}
    }

    public override void OnPickup( Transform grabTransform )
	{
		if (_sinkTween != null)
		{
            _sinkTween.Kill();
            _sinkTween = null;
		}

		if( _grabTransform != null )
		{
			if( _grabTransform.GetComponent<SeedSlug>() != null )
			{
				_grabTransform.GetComponent<SeedSlug>().OnPlayerPickup();
				this.transform.SetParent( grabTransform );
			}	
		}

		base.OnPickup( grabTransform );

		_isGrounded = false;
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
		if( _sinkTween == null || _isGrounded )
		{
			Vector3 plantPos = new Vector3( transform.position.x, 0.0f, transform.position.z ); 
			GameObject mound = Instantiate( _moundPrefab, plantPos, Quaternion.identity ) as GameObject; 
			PlantManager.instance.AddMound( mound.GetComponent<BasePlant>()  );
			GroundManager.instance.EmitDirtParticles(plantPos);
			PlantManager.instance.DestroySeed( this, _moundType, true );
			_sinkTween = null;

			// seed planting sound
			AudioManager.instance.PlayClipAtIndex( AudioManager.AudioControllerNames.PLAYER_ACTIONFX, 0 );
		}
	}

    public void DestroyThisSeed()
    {
        PlantManager.instance.DestroySeed( this, _moundType, true );
    }

	void BeginSelfPlant()
	{
		if( !_isGrounded )
		{
			if( _sinkTween == null  )
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
			Vector3 endPos = transform.position + ( Vector3.down * 0.36f );
			float sinkTime = Random.Range( _sinkSpeedRange.Min, _sinkSpeedRange.Max );

			_sinkTween = transform.DOMove( endPos, sinkTime ).OnComplete( EndSelfPlant ).SetUpdate(UpdateType.Normal, true);
			_rigidbody.freezeRotation = true;
		}
	}

	IEnumerator DelayedBeginSelfPlant()
	{
		yield return new WaitUntil( () => _isGrounded );

		BeginSelfPlant();
	}

	void EndSelfPlant()
	{
		_sinkTween = null;
		TryPlanting();
	}

    protected override void HandleCollision( Collision col )
	{
		if( !_isGrounded)
		{
			if( col.gameObject.GetComponent<GroundDisc>() )
			{
				_isGrounded = true;
			}
		}
	}
}
