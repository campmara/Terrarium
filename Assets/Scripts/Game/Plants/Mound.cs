using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mound : BasePlant
{
	[SerializeField] Material _deadMaterial = null;
	[SerializeField] GameObject _bigPlantPrefab = null;
	[SerializeField] bool _starterMound = false;

	Vector2 _sproutGrowthRange = new Vector2( .75f, 5.0f);
	Transform _sprout = null;
	const float _spawnHeight = .33f;

	const float _baseRate = 1.0f;
	const float _wateredRate = 3.5f;
	float _germinationRate = _baseRate;

	float _timerDuration = 40.0f;
	float _curTime = 0.0f;
	float _scaleInterp = 0.0f;

	const float _deathProbability = 15.0f; // probability out of 100
	bool _canLive = true;
	public bool IsLiving { get { return _canLive; } }

	void Awake()
	{
		StartPlantGrowth();
	}

	//********************************
	// STARTING GROWTH SETUP FUNCTIONS
	//********************************

	protected override void StartPlantGrowth()
	{
		_germinationRate = _baseRate;
		transform.position = transform.position.SetPosY( _spawnHeight );
		_sprout = transform.GetChild(0);
		_sprout.localScale = new Vector3( _sproutGrowthRange.x, _sproutGrowthRange.x, _sproutGrowthRange.x);

		SpinLifeLottery();
		StartPlantUpdate();
	}

	protected override void StartPlantUpdate()
	{
		if( _canLive )
		{
			PlantManager.ExecuteGrowth += UpdatePlant;
		}
		else
		{
			BeginDeath();
		}
	}

	void SpinLifeLottery()
	{
		int dieRoll = (int)Random.Range( 0.0f, 100.0f );

		if( dieRoll > _deathProbability || _starterMound )
		{
			_canLive = true;
		}
		else
		{
			_canLive = false;
		}

		Debug.Log( " OBJECT IS DEAD " + _canLive.ToString() );
	}

	//********************************
	// PLANT UPDATE FUNCTIONS
	//********************************

	public override void UpdatePlant()
	{
		UpdateTimer();
	} 

	void UpdateTimer()
	{
		if( _canLive )
		{
			if( _curTime < _timerDuration )
			{
				_curTime += Time.deltaTime * _germinationRate;	
				_scaleInterp = Mathf.Lerp( _sproutGrowthRange.x, _sproutGrowthRange.y, ( _curTime / _timerDuration ) );
				_sprout.transform.localScale = new Vector3( _scaleInterp, _scaleInterp, _scaleInterp );
			}
			else
			{
				SpawnSprout();
			}
		}
	}

	void SpawnSprout()
	{
		Vector3 plantPos = _sprout.position;
		plantPos = plantPos.SetPosY( 0.0f );
		BigPlant plant = ( (GameObject)Instantiate( _bigPlantPrefab, plantPos, Quaternion.identity ) ).GetComponent<BigPlant>();

		StarterPlant sPlant = plant.GetComponent<StarterPlant>();
		if( sPlant )
		{
			sPlant.MinScale = new Vector3( _scaleInterp, _scaleInterp, _scaleInterp );
		}

		PlantManager.instance.AddBigPlant( plant );

		Die();
	}
		
	//********************************
	// PLANT DEATH FUNCTIONS
	//********************************

	protected override void StopPlantGrowth()
	{
		StopPlantUpdate();
	}

	protected override void StopPlantUpdate()
	{
		PlantManager.ExecuteGrowth -= UpdatePlant;
	}
		
	protected override void BeginDeath()
	{
		transform.GetChild(1).GetComponent<MeshRenderer>().material = _deadMaterial;
	}
		
	protected override void Die()
	{
		if( _canLive )
		{
			StopPlantGrowth();
		}

		PlantManager.instance.DeleteMound( this );
	}

	//********************************
	// INTERACTION FUNCTIONS
	//********************************
	public override void WaterPlant()
	{
		if( _canLive )
		{
			_germinationRate = _wateredRate;
		}
		else
		{
			Die();
		}
	}

	// UNIMPLEMENTED INTERACTIONS
	public override void GrabPlant(){}
	public override void TouchPlant(){}
	protected override void Decay(){}
}
