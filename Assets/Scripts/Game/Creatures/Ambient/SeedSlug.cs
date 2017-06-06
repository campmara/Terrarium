using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeedSlug : MonoBehaviour 
{
	Animator _animator = null;

    [SerializeField]
    float _startDistance = 100.0f;
    [SerializeField]
	float _moveTime = 200.0f;
    [SerializeField]
    float _curveSpeed = 0.05f;

    [SerializeField, ReadOnly]
    float _moveTimer = 0.0f;
	[SerializeField, ReadOnlyAttribute]
	float _currMoveSpeedScalar = 1.0f;
	const float MOVESPEED_STARTSCALAR = 1.0f;
	const float MOVESPEED_HITSCALAR = 5.0f;

	float _animTimer = 0.0f;
	[SerializeField] AnimationCurve _moveAnimCurve;

    Vector3 _startPos = Vector3.zero;
    Vector3 _endPos = Vector3.zero;    
    Vector3 _forwardDir = Vector3.zero;
    Vector3 _xOffsetDir = Vector3.zero;

    Vector3 _moveDest = Vector3.zero;
	[SerializeField] float TURN_MINSPEED = 0.75f;

    Vector2 _curveRange = new Vector2( 2.0f, 3.5f );
    float _currXOffsetGoal = 0.0f;
    float _curveAmp = 0.0f;

	[SerializeField] List<GameObject> _seedSpawnables = new List<GameObject>();
	[SerializeField, ReadOnlyAttribute] Pickupable _carriedObject = null;
	[SerializeField] Transform _leftArmJoint = null;
	[SerializeField] Transform _rightArmJoint = null;
	Vector3 _handMidPos = Vector3.zero;
	[SerializeField] float _seedUpPos = 0.0f;

    AudioSource _source = null;
    [SerializeField] AudioClip _slugWalkClip = null;
    [SerializeField] AudioClip _slugYellClip = null;
    [SerializeField] AudioClip _slugRunClip = null;
    Coroutine _yellRoutine = null;

	// Use this for initialization
	void Awake ()
    {
		_animator = this.GetComponent<Animator>();
        _source = this.GetComponent<AudioSource>();

        InitMovement();
    }
	
	// Update is called once per frame
	void Update ()
    {
		if( Vector3.Distance( this.transform.position, _endPos ) <= 1.0f )
		{
			InitMovement();
		}

        UpdateMovement();
		UpdateSeedPos();

        transform.position = Vector3.Lerp( this.transform.position, _moveDest, 10.0f * Time.deltaTime );
	}

    void UpdateMovement()
    {
		_animTimer += Time.deltaTime * _animator.speed;
		_moveTimer += Time.deltaTime * ( _moveAnimCurve.Evaluate( _animTimer ) * _currMoveSpeedScalar );

        _moveDest = Vector3.Lerp( _startPos, _endPos, _moveTimer / _moveTime );

        _curveAmp = _currXOffsetGoal * Mathf.Sin( _curveSpeed * Time.time );
        _moveDest += _xOffsetDir * _curveAmp;

        _moveDest.y = PondManager.instance.Pond.GetPondY( _moveDest );

		if( _moveAnimCurve.Evaluate( _animTimer ) > TURN_MINSPEED )
		{
			this.transform.rotation = Quaternion.Slerp( this.transform.rotation, Quaternion.LookRotation( _moveDest - this.transform.position ), 20.0f * Time.deltaTime );	
		}        

        this.transform.position = _moveDest;
    }

	void UpdateSeedPos()
	{
		_handMidPos = JohnTech.Midpoint( _leftArmJoint.position, _rightArmJoint.position ) + ( -_leftArmJoint.up * _seedUpPos );

		if( _carriedObject != null )
		{
			_carriedObject.transform.position = _handMidPos;
			//_carriedSeed.transform.localRotation = Quaternion.Euler( 0.0f, 90.0f, 0.0f );
		}
	}

    void InitMovement()
    {
		_startPos = JohnTech.GenerateRandomXZDirection().normalized;                

        _forwardDir = -_startPos;

		_endPos = Quaternion.Euler( 0.0f, JohnTech.CoinFlip() ? Random.Range( -25.0f, -10.0f ) : Random.Range( 15.0f, 20.0f ), 0.0f ) * ( -_startPos * _startDistance );
        _endPos.y = PondManager.instance.Pond.GetPondY( _endPos );

        _startPos *= _startDistance;
        _startPos.y = PondManager.instance.Pond.GetPondY( _startPos );

        this.transform.position = _startPos;
        this.transform.LookAt( transform.position + _forwardDir );

		InitHeldSeed();

        _xOffsetDir = this.transform.right;
        _currXOffsetGoal = Random.Range( _curveRange.x, _curveRange.y );
        _moveTimer = 0.0f;
		_animTimer = 0.0f;

		_currMoveSpeedScalar = MOVESPEED_STARTSCALAR;
		_animator.speed = 1.0f;

        _source.loop = true;
        _source.clip = _slugWalkClip;
        _source.Play();
    }

	void InitHeldSeed()
	{
		GameObject newSeed = Instantiate( _seedSpawnables[Random.Range( 0, _seedSpawnables.Count )], this.transform ) as GameObject;

		_carriedObject = newSeed.GetComponent<Pickupable>();
		_carriedObject.OnPickup( this.transform );
		_carriedObject.gameObject.isStatic = false;

		UpdateSeedPos();

		_carriedObject.transform.Rotate( 0.0f, 90.0f, 0.0f );
	}

    public void OnHitWithRoll()
    {

		Debug.Log("YOU HIT THE SLUG YOU MONSTER.");

		if( _carriedObject != null )
		{
			_carriedObject.DropSelf();

			_carriedObject.transform.parent = null;
			_carriedObject = null;
		}

		_currMoveSpeedScalar += MOVESPEED_HITSCALAR;
		_animator.speed += 3.0f;

        if( _yellRoutine == null )
        {
            _yellRoutine = StartCoroutine( StartYellRoutine() );
        }
        
    }

    IEnumerator StartYellRoutine()
    {
        _source.loop = false;
        _source.clip = _slugYellClip;
        _source.Play();

        yield return new WaitUntil( () => !_source.isPlaying );

        _source.loop = true;
        _source.clip = _slugRunClip;
        _source.Play();

        _yellRoutine = null;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;

        Gizmos.DrawWireSphere( _startPos, 0.5f );
		Gizmos.DrawIcon(_startPos, "Slug Start Pos");
		Gizmos.DrawWireSphere( _endPos, 0.5f );
		Gizmos.DrawIcon(_endPos, "Slug End Pos");

		/*Gizmos.DrawWireSphere( _moveDest, 0.1f );
		Gizmos.DrawLine( this.transform.position, _moveDest );*/

    }
}
