using UnityEngine;
using System.Collections;
using DG.Tweening;

public class MoundGrowthController : PlantController
{
	[SerializeField] GameObject _basePlantPrefab = null;
	[SerializeField] bool _starterMound = false;

	Vector2 _sproutGrowthRange = new Vector2( 2.0f, 3.0f);
	Transform _sprout = null;
	const float _spawnHeight = .45f;

	const float _baseRate = .75f;
	const float _wateredRate = 5.5f;
	float _germinationRate = _baseRate;

	float _timerDuration = 30.0f;
	float _curTime = 0.0f;
	float _scaleInterp = 0.0f;

	const float _deathProbability = 0.0f; // probability out of 100
	bool _canLive = true;
	bool _spawnedSprout = false;

	public override void Init()
	{
		_myPlant = GetComponent<BasePlant>();
		_controllerType = ControllerType.Growth;
	}

	public override void StartState()
	{
		_myPlant = GetComponent<BasePlant>();

		_germinationRate = _baseRate;
		transform.position = transform.position.SetPosY( _spawnHeight );
		_sprout = transform.GetChild(0);
		//_sprout.localScale = new Vector3( _sproutGrowthRange.x, _sproutGrowthRange.x, _sproutGrowthRange.x);

		// scale it down!
		//transform.localScale = Vector3.zero;

		SpinLifeLottery();
	//	StartCoroutine(StartPlantRoutine());
	}

	void SpinLifeLottery()
	{
		if( StablePlantPopulation() )
		{
			int dieRoll = (int)Random.Range( 0.0f, 100.0f );
			if( dieRoll < _deathProbability && !_starterMound )
			{
				_canLive = false;
				StopState();
			}
		}
	}

	bool StablePlantPopulation()
	{
		return PlantManager.instance.IsPopulationStable( _basePlantPrefab.GetComponent<BasePlant>() );
	}

	public override void UpdateState()
	{
		if( _curTime < _timerDuration )
		{
			_curTime += Time.deltaTime * _germinationRate;	
			_scaleInterp = Mathf.Lerp( _sproutGrowthRange.x, _sproutGrowthRange.y, ( _curTime / _timerDuration ) );
			float heightInterp = Mathf.Lerp( _spawnHeight, 0.0f, ( _curTime / _timerDuration ) );
			_sprout.transform.localScale = new Vector3( _scaleInterp, _scaleInterp, _scaleInterp );
			if( _sprout.transform.position.y > 0.0f )
			{
				transform.position = new Vector3( transform.position.x, heightInterp, transform.position.z );
			}
		}
		else if( !_spawnedSprout )
		{
			SpawnSprout();
		}
	}

	void SpawnSprout()
	{
		Vector3 plantPos = _sprout.position;
		plantPos = plantPos.SetPosY( 0.0f );
		BasePlant plant = ( (GameObject)Instantiate( _basePlantPrefab, plantPos, Quaternion.identity ) ).GetComponent<BasePlant>();
		BPGrowthController controller = plant.GetComponent<BPGrowthController>();
		if ( controller )
		{
			controller.UpdateToMoundScale( _sprout.transform.lossyScale.x );
		}

		StarterPlantGrowthController sPlant = plant.GetComponent<StarterPlantGrowthController>();
		if( sPlant )
		{
			sPlant.MinScale = new Vector3( _scaleInterp, _scaleInterp, _scaleInterp );
		}

		PlantManager.instance.AddBasePlant( plant );
		_spawnedSprout = true;

		StopState();
	}

	public override void StopState()
	{
		//switch to a destroyed mode 
		if( _canLive )
		{
			_myPlant.SwitchController( this );
		}
		
		PlantManager.instance.DeleteMound( _myPlant );
	}
		
	public override void WaterPlant()
	{
		_germinationRate = _wateredRate;
	}

	public override void TouchPlant(){}
	public override void GrabPlant(){}
	public override void StompPlant(){}
}
