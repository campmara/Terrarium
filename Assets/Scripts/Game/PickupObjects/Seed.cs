using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Seed : Pickupable 
{
    [SerializeField] protected SeedAssetKey _assetKey = SeedAssetKey.NONE;
    public SeedAssetKey AssetKey { get { return _assetKey; } set { _assetKey = value; } }

    [SerializeField] GameObject _moundPrefab = null;

	float _timeSinceLastPickup = 0.0f;
	float _timePassedTillDestroy = 60.0f;
	bool _hasFallen = false;

	const int  _selfPlantProbability = 75;
	const float _searchRadius = 30.0f;

	Tween _sinkTween = null;

	void Update()
	{
		if( !_grabbed )
		{
			if( _timeSinceLastPickup >= _timePassedTillDestroy && PlantManager.instance.GetActiveSeedCount() > 2 )
			{
				Destroy( gameObject );
			}
			else
			{
				_timeSinceLastPickup += Time.deltaTime;
			}
		}
	}

    public override void OnPickup( Transform grabTransform )
	{
		if (_sinkTween != null)
		{
			_sinkTween.Rewind();
		}

		base.OnPickup( grabTransform );
	}

	public override void DropSelf()
	{
		base.DropSelf();
		_timeSinceLastPickup = 0.0f;
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
		Transform child = transform.GetChild(0);
		Vector3 endPos = child.position + (Vector3.down * 0.36f);
		float sinkTime = Random.Range(10f, 20f);

		_sinkTween = child.DOMove(endPos, sinkTime).OnComplete(EndSelfPlant);
	}

	void EndSelfPlant()
	{
		TryPlanting();
	}

	void OnCollisionEnter( Collision col ) 
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
