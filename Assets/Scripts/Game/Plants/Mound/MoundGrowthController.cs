using UnityEngine;
using System.Collections;
using DG.Tweening;

public class MoundGrowthController : PlantController
{
	[SerializeField] GameObject _BasePlantPrefab = null;
	[SerializeField] bool _starterMound = false;

	Vector2 _sproutGrowthRange = new Vector2( .75f, 5.0f);
	Transform _sprout = null;
	const float _spawnHeight = .45f;

	const float _growthTweenTime = 1f;
	const float _baseRate = 1.0f;
	const float _wateredRate = 5.5f;
	float _germinationRate = _baseRate;

	float _timerDuration = 30.0f;
	float _curTime = 0.0f;
	float _scaleInterp = 0.0f;

	const float _deathProbability = 0.0f; // probability out of 100
	bool _canLive = true;

	public override void Init()
	{
		_myPlant = GetComponent<BasePlant>();
		_controllerType = ControllerType.Growth;
	}

	public override void StartState()
	{
		_myPlant = GetComponent<BasePlant>();
		StartCoroutine(StartPlantRoutine());
	}

	protected IEnumerator StartPlantRoutine()
	{
		_germinationRate = _baseRate;
		transform.position = transform.position.SetPosY( _spawnHeight );
		_sprout = transform.GetChild(0);
		_sprout.localScale = new Vector3( _sproutGrowthRange.x, _sproutGrowthRange.x, _sproutGrowthRange.x);

		// scale it down!
		transform.localScale = Vector3.zero;

		// scale it up!
		Tween growTween = transform.DOScale(Vector3.one, _growthTweenTime);
		yield return growTween.WaitForCompletion();

		SpinLifeLottery();
	}

	void SpinLifeLottery()
	{
		int dieRoll = (int)Random.Range( 0.0f, 100.0f );

		if( dieRoll < _deathProbability && !_starterMound )
		{
			_canLive = false;
			StopState();
		}
	}

	public override void UpdateState()
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

	void SpawnSprout()
	{
		Vector3 plantPos = _sprout.position;
		plantPos = plantPos.SetPosY( 0.0f );
		BasePlant plant = ( (GameObject)Instantiate( _BasePlantPrefab, plantPos, Quaternion.identity ) ).GetComponent<BasePlant>();

		StarterPlantGrowthController sPlant = plant.GetComponent<StarterPlantGrowthController>();
		if( sPlant )
		{
			sPlant.MinScale = new Vector3( _scaleInterp, _scaleInterp, _scaleInterp );
		}

		PlantManager.instance.AddBasePlant( plant );

		StopState();
	}

	public override void StopState()
	{
		//switch to a destroyed mode 
		if( _canLive )
		{
			PlantManager.instance.DeleteMound( _myPlant );
		}
		else
		{
			_myPlant.SwitchController( this );
		}
	}

	public override GameObject SpawnChildPlant(){ return null; }

	public override void WaterPlant()
	{
		_germinationRate = _wateredRate;
	}

	public override void TouchPlant(){}
	public override void GrabPlant(){}
	public override void StompPlant(){}
}
