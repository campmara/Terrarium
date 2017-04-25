using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeedSlug : MonoBehaviour {

    [SerializeField]
    private float _startDistance = 100.0f;
    [SerializeField]
    private float _moveTime = 200.0f;
    [SerializeField]
    private float _curveSpeed = 0.05f;

    [SerializeField, ReadOnly]
    private float _moveTimer = 0.0f;

    private Vector3 _startPos = Vector3.zero;
    private Vector3 _endPos = Vector3.zero;
    private Vector3 _moveDir = Vector3.zero;
    private Vector3 _forwardDir = Vector3.zero;
    private Vector3 _xOffsetDir = Vector3.zero;

    Vector3 _moveDest = Vector3.zero;
    
    Vector2 _curveRange = new Vector2( 2.5f, 7.0f );
    float _currXOffsetGoal = 0.0f;
    float _curveAmp = 0.0f;

	// Use this for initialization
	void Awake ()
    {
        InitMovement();
    }
	
	// Update is called once per frame
	void Update ()
    {
        UpdateMovement();

        transform.position = Vector3.Lerp( this.transform.position, _moveDest, 20.0f * Time.deltaTime );
	}

    void UpdateMovement()
    {
        _moveTimer += Time.deltaTime;

        _moveDest = Vector3.Lerp( _startPos, _endPos, _moveTimer / _moveTime );

        _curveAmp = _currXOffsetGoal * Mathf.Sin( _curveSpeed * Time.time );
        _moveDest += _xOffsetDir * _curveAmp;

        _moveDest.y = PondManager.instance.Pond.GetPondY( _moveDest );

        this.transform.LookAt( _moveDest );

        this.transform.position = _moveDest;
    }

    void InitMovement()
    {
        _startPos = new Vector3( Random.value, 0.0f, Random.value );                

        _forwardDir = -_startPos;

        _endPos = -_startPos * _startDistance;
        _endPos.y = PondManager.instance.Pond.GetPondY( _endPos );

        _startPos *= _startDistance;

        _startPos.y = PondManager.instance.Pond.GetPondY( _startPos );

        this.transform.position = _startPos;
        this.transform.LookAt( transform.position + _forwardDir );

        _xOffsetDir = this.transform.right;
        _currXOffsetGoal = Random.Range( _curveRange.x, _curveRange.y );
        _moveTimer = 0.0f;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;

        Gizmos.DrawWireSphere( _startPos, 0.5f );
        Gizmos.DrawWireSphere( _endPos, 0.5f );
    }
}
