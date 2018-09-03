using UnityEngine;
using System.Collections;
using DG.Tweening;

public class MoundGrowthController : PlantController
{
	[SerializeField] GameObject _basePlantPrefab = null;
	[SerializeField] bool _starterMound = false;

	[SerializeField] Vector2 _sproutGrowthRange = new Vector2( 1.0f, 2.4f);
	Transform _sprout = null;
	Transform _dirt = null;
	const float _dirtHiddenHeight = -.15f;

	const float _baseRate = .75f;
	const float _wateredRate = 5.5f;
	float _germinationRate = _baseRate;

	float _timerDuration = 30.0f;
	float _curTime = 0.0f;
	float _scaleInterp = 0.0f;
	float _dirtInterp = 0.0f;

	const float _deathProbability = 0.0f; // probability out of 100
	bool _canLive = true;
	bool _spawnedSprout = false;

	Tween _sinkTween = null;

	public override void Init()
	{
		_myPlant = GetComponent<BasePlant>();
		_controllerType = ControllerType.Growth;
	}

	public override void StartState()
	{
		_myPlant = GetComponent<BasePlant>();
		_germinationRate = _baseRate;
		_sprout = transform.GetChild(0);
		_dirt = transform.GetChild(1);

		SpinLifeLottery();
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
			_curTime += Time.unscaledDeltaTime * _germinationRate;
			float percentVal = 	_curTime / _timerDuration;
			
			_scaleInterp = Mathf.Lerp( _sproutGrowthRange.x, _sproutGrowthRange.y, percentVal );
			_sprout.transform.localScale = new Vector3( _scaleInterp, _scaleInterp, _scaleInterp );

			_dirtInterp = Mathf.Lerp( 0.0f, _dirtHiddenHeight, percentVal );
			_dirt.position = new Vector3( _dirt.position.x, _dirtInterp, _dirt.position.z );

		}
		else if( !_spawnedSprout )
		{
			SpawnSprout();
		}
	}

	void SpawnSprout()
	{
		if( _sinkTween == null )
		{
			Vector3 plantPos = _sprout.position;
			plantPos = plantPos.SetPosY( 0.0f );
			BasePlant plant = ( (GameObject)Instantiate( _basePlantPrefab, plantPos, Quaternion.identity ) ).GetComponent<BasePlant>();
			BPGrowthController controller = plant.GetComponent<BPGrowthController>();
			if ( controller )
			{
				controller.UpdateToMoundScale( _sprout.transform.lossyScale.x );
			}

			PlantManager.instance.AddBasePlant( plant );
			_spawnedSprout = true;

			StopState();
		}
	}

	public override void StopState()
	{
		// don't even switch controllers just kill here 
		_sinkTween = _sprout.DOScale( Vector3.zero, 1.6f).OnComplete( () => PlantManager.instance.DeleteMound( _myPlant ) );
	}
		
	public override void WaterPlant()
	{
		_germinationRate = _wateredRate;
	}

	public override void TouchPlant(){}
	public override void GrabPlant(){}
	public override void StompPlant(){}
}
