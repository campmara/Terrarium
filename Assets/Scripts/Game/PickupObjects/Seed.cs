using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class Seed : Pickupable 
{
    [SerializeField] protected SeedAssetKey _assetKey = SeedAssetKey.NONE;
    public SeedAssetKey AssetKey { get { return _assetKey; } set { _assetKey = value; } }

    [SerializeField] GameObject _moundPrefab = null;
	BasePlant.PlantType _moundType = BasePlant.PlantType.NONE;
	float _timeSinceLastPickup = 0.0f;
	float _timePassedTillDestroy = 60.0f;
	bool _hasFallen = false;
	const int  _selfPlantProbability = 50;
	const float _searchRadius = 30.0f;
	Tween _sinkTween = null;

    const float WIND_FORCESCALAR = 0.5f;


	void Awake()
	{
		base.Awake();
		_moundType = _moundPrefab.GetComponent<BasePlant>().MyPlantType;

		float endScale = transform.localScale.x;
		this.transform.DOScale( endScale, 3.0f );//.OnComplete( EndSelfPlant );
	}
	void Update()
	{
		if( !_grabbed )
		{
			if( _timeSinceLastPickup >= _timePassedTillDestroy && PlantManager.instance.GetActiveSeedCount() > 2 )
			{			
				PlantManager.instance.DestroySeed( this, _moundType );
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
			_sinkTween.Rewind();
            _sinkTween.Kill();
            _sinkTween = null;
		}

		base.OnPickup( grabTransform );

		_rigidbody.velocity = Vector3.zero;
	}

	public override void DropSelf()
	{
		base.DropSelf();
        BeginSelfPlant();
		_timeSinceLastPickup = 0.0f;
	}

	public void DropOnRitual()
	{
		base.DropSelf();
	}

	public void TryPlanting()
	{
		Vector3 plantPos = new Vector3( transform.position.x, 0.0f, transform.position.z ); 
		GameObject mound = Instantiate( _moundPrefab, plantPos, Quaternion.identity ) as GameObject; 
		PlantManager.instance.AddMound( mound.GetComponent<BasePlant>()  );
		GroundManager.instance.EmitDirtParticles(plantPos);
		gameObject.SetActive(false);
	}

	void BeginSelfPlant()
	{
		if( _sinkTween == null )
		{
			Vector3 endPos = this.transform.position + (Vector3.down * 0.36f);
			float sinkTime = Random.Range(10f, 20f);

			_sinkTween = this.transform.DOMove( endPos, sinkTime ).OnComplete( EndSelfPlant );
		}
	}

	void EndSelfPlant()
	{
		TryPlanting();
		PlantManager.instance.DestroySeed( this, _moundType );
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
