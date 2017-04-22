using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public enum RabbitState : int
{
    INIT = 0,
    SPAWNING,
    IDLE,
    WALKING,
    EVADE
}

[Serializable]
public class RabbitData
{
    public RabbitState rState = RabbitState.INIT;

    public Transform RabbitTransform = null;    
    
    public float HopSpeed = 0.0f;

    public Coroutine IdleRoutine = null;
    public float HopWaitTimer = 0.0f;
    public float SpawnTime = 0.0f;

    public Tween MoveTween = null;
    public Vector3 TargetMovePosition = Vector3.zero;
    public Vector3 MoveDirection = Vector3.zero;

    public RabbitData() { }
} 

public class RabbitMound : MonoBehaviour
{
    [SerializeField]
    GameObject _rabbitMoundObject = null;

    [SerializeField]
    GameObject _rabbitSpawnObject = null;

    [SerializeField, ReadOnlyAttribute]
    List<RabbitData> _rabbitList = new List<RabbitData>();

    const int RABBIT_COUNT = 7;

	[SerializeField, Space(10)] float RABBIT_HOPSPEED_MIN = 1.0f;
    [SerializeField] float RABBIT_HOPSPEED_MAX = 1.0f;
    [SerializeField] float RABBIT_MAXHOPDIST = 1.0f;
    [SerializeField] float RABBIT_HOPHEIGHT = 1.0f;
    [SerializeField] float RABBIT_HOPWAIT = 0.25f;
    [SerializeField] float RABBIT_YPOS = 1.0f;
    [SerializeField]
    float RABBIT_RAYCAST_YOFFSET = 7.0f;

	[SerializeField, Space(10)] float RABBIT_EVADESPEED_MIN = 0.25f;
	[SerializeField] float RABBIT_EVADESPEED_MAX = 0.5f;
	[SerializeField] float RABBIT_MAXEVADEHOPDIST = 1.25f;
	[SerializeField] float RABBIT_EVADEHEIGHT = 1.0f;
	[SerializeField] float RABBIT_EVADEWAIT = 0.025f;
	[SerializeField] float RABBIT_EVADEANGLE = 45f;
    [SerializeField]
    float RABBIT_MINEVADEDIST = 2f;
    [SerializeField]
    float RABBIT_MAXEVADEDIST = 7f;

    [SerializeField, Space(10)] float RABBIT_IDLETIME_MIN = 1.0f;
    [SerializeField] float RABBIT_IDLETIME_MAX = 5.0f;

	[SerializeField, Space(10)] float RABBIT_SPAWNTIME_MIN = 1.0f;
    [SerializeField] float RABBIT_SPAWNTIME_MAX = 2.0f;
    [SerializeField] float RABBIT_SPAWNHOPTIME = 0.5f;
    [SerializeField] float RABBIT_SPAWNHOPHEIGHT = 2.0f;

	[SerializeField, Space(10)] float RABBIT_MAXMOVEDIST = 2.0f;
    [SerializeField] float RABBIT_MINMOVEDIST = 0.25f;

	[SerializeField] float RABBIT_PLAYERCHECKSIZE = 1.0f;

    RaycastHit yPosRay;

    // Use this for initialization
    void Awake ()
    {
        InitializeRabbits();

	}
	
    void InitializeRabbits()
    {
        GameObject newRabbit = null;

        for( int i = 0; i < RABBIT_COUNT; i++ )
        {
            newRabbit = Instantiate( _rabbitSpawnObject, this.transform ) as GameObject;

            _rabbitList.Add( new RabbitData() );

            _rabbitList[i].RabbitTransform = newRabbit.transform;
            _rabbitList[i].HopSpeed = UnityEngine.Random.Range( RABBIT_HOPSPEED_MIN, RABBIT_HOPSPEED_MAX );
            _rabbitList[i].SpawnTime = UnityEngine.Random.Range( RABBIT_SPAWNTIME_MIN, RABBIT_SPAWNTIME_MAX );

            newRabbit.SetActive( false );
        }

        StartCoroutine( RabbitBirthRoutine() );
    }

    // Hop the inactive rabbit outta its hole
    void SpawnRabbit( RabbitData rabbit )
    {
        rabbit.rState = RabbitState.SPAWNING;

        rabbit.RabbitTransform.gameObject.SetActive( true );

        rabbit.RabbitTransform.position = _rabbitMoundObject.transform.position;

        Vector2 spawnPos = UnityEngine.Random.insideUnitCircle * 1.75f;
        Vector3 jumpEndPos = new Vector3( spawnPos.x, RABBIT_YPOS, spawnPos.y );

        jumpEndPos += rabbit.RabbitTransform.position;

        if ( Physics.Raycast( jumpEndPos, Vector3.down, out yPosRay, 7.0f, 1 << LayerMask.NameToLayer( "Ground" ) ) )
        {
            jumpEndPos.y = yPosRay.point.y + RABBIT_YPOS;
        }
        else
        {
            jumpEndPos.y = RABBIT_YPOS;
        }

        rabbit.MoveTween = rabbit.RabbitTransform.DOJump( jumpEndPos, RABBIT_SPAWNHOPHEIGHT, 1, RABBIT_SPAWNHOPTIME ).OnComplete( () => ChangeRabbitState( rabbit, RabbitState.IDLE ) );
    }

	// Update is called once per frame
	void Update ()
    {
        CheckRabbitMovement();	
	}

    private void CheckRabbitMovement()
    {
        RabbitData currRabbitData = null;

        for ( int i = 0; i < _rabbitList.Count; i++ )
        {
            currRabbitData = _rabbitList[i];
            if ( currRabbitData.rState == RabbitState.WALKING )
            {
                if( currRabbitData.MoveTween == null )
                {
                    if( currRabbitData.HopWaitTimer < RABBIT_HOPWAIT )
                    {
                        currRabbitData.HopWaitTimer += Time.deltaTime;

                        CheckRabbitSurroundings( currRabbitData );
                    }
                    else
                    {
                        StartRabbitHop( currRabbitData );
                    }
                }          
            }
			else if( currRabbitData.rState == RabbitState.EVADE )
			{
				if( currRabbitData.MoveTween == null )
				{
					if( currRabbitData.HopWaitTimer < RABBIT_EVADEWAIT )
					{
						currRabbitData.HopWaitTimer += Time.deltaTime;
					}
					else
					{
						UpdateRabbitEvade( currRabbitData );
					}
				}
			}
            else if( currRabbitData.rState == RabbitState.IDLE )
            {
                CheckRabbitSurroundings( currRabbitData );
            }

        }
    }

    void SetRabbitMoveDestination( RabbitData rabbit )
    {
        Vector2 spawnPos = UnityEngine.Random.insideUnitCircle;
        Vector3 movePos = _rabbitMoundObject.transform.position + ( new Vector3( spawnPos.x, 0.0f, spawnPos.y ) * UnityEngine.Random.Range( RABBIT_MINMOVEDIST, RABBIT_MAXMOVEDIST ) );

        if ( Physics.Raycast( movePos + Vector3.up * RABBIT_RAYCAST_YOFFSET, Vector3.down, out yPosRay, 15.0f, 1 << LayerMask.NameToLayer( "Ground" ) ) )
        {
            movePos.y = yPosRay.point.y + RABBIT_YPOS;
        }
        else
        {
            movePos.y = RABBIT_YPOS;
        }

        rabbit.MoveDirection = ( movePos - rabbit.RabbitTransform.position ).normalized;

        // Make sure that the new destination isn't outside of the range of where the rabbit can move
        //if ( Vector3.Distance( movePos, rabbit.RabbitTransform.position ) > RABBIT_MAXDESTDIST )
        //{
        //    movePos = rabbit.MoveDirection * RABBIT_MAXDESTDIST;  // If so readjust move position
        //}

        rabbit.TargetMovePosition = movePos;       

        StartRabbitHop( rabbit );
    }

    void StartRabbitHop( RabbitData rabbit )
    {
		CheckRabbitSurroundings( rabbit );

        if ( Vector3.Distance( rabbit.TargetMovePosition, rabbit.RabbitTransform.position ) > RABBIT_MAXHOPDIST )
        {
            rabbit.MoveTween = rabbit.RabbitTransform.DOJump( rabbit.RabbitTransform.position + ( rabbit.MoveDirection * RABBIT_MAXHOPDIST ), RABBIT_HOPHEIGHT, 1, rabbit.HopSpeed ).OnComplete( () => EndHop( rabbit ) );
        }
        else
        {
            rabbit.MoveTween = rabbit.RabbitTransform.DOJump( rabbit.TargetMovePosition, RABBIT_HOPHEIGHT, 1, rabbit.HopSpeed ).OnComplete( () => ChangeRabbitState( rabbit, RabbitState.IDLE ) );
        }
       
    }

    void EndHop( RabbitData rabbit )
    {
        rabbit.MoveTween.Kill();
        rabbit.MoveTween = null;
        rabbit.HopWaitTimer = 0.0f;
    }

    void StartIdling( RabbitData rabbit )
    {
        rabbit.IdleRoutine = StartCoroutine( RabbitIdleRoutine( rabbit ) );
    }

    IEnumerator RabbitIdleRoutine( RabbitData rabbit )
    {
        yield return new WaitForSeconds( UnityEngine.Random.Range( RABBIT_IDLETIME_MIN, RABBIT_IDLETIME_MAX ) );

        ChangeRabbitState( rabbit, RabbitState.WALKING );

        rabbit.IdleRoutine = null;
    }

    void ChangeRabbitState( RabbitData rabbit, RabbitState newState )
    {
        switch( newState )
        {
            case RabbitState.IDLE:
                StartIdling( rabbit );
                break;
            case RabbitState.WALKING:				
				SetRabbitMoveDestination( rabbit );
                break;
			case RabbitState.EVADE:
                if( rabbit.rState == RabbitState.IDLE && rabbit.IdleRoutine != null )
                {
                    StopCoroutine( rabbit.IdleRoutine );
                    rabbit.IdleRoutine = null;
                }
                rabbit.HopSpeed = UnityEngine.Random.Range( RABBIT_EVADESPEED_MIN, RABBIT_EVADESPEED_MAX );			
				break;
        }

        rabbit.rState = newState;
    }

    IEnumerator RabbitBirthRoutine()
    {
        RabbitData newRabbit = _rabbitList.Find( x => x != null && !x.RabbitTransform.gameObject.activeSelf );

        if( newRabbit != null )
        {
            yield return new WaitForSeconds( newRabbit.SpawnTime );

            SpawnRabbit( newRabbit );

            StartCoroutine( RabbitBirthRoutine() );
        }

    }

	bool CheckRabbitSurroundings( RabbitData rabbit )
	{
		Collider[] rabbitSurroundings = Physics.OverlapSphere( rabbit.RabbitTransform.position, RABBIT_PLAYERCHECKSIZE, 1 << LayerMask.NameToLayer( "Player" ) );

		if( rabbitSurroundings.Length > 0 )
		{
			Rigidbody _playerRb = rabbitSurroundings[0].attachedRigidbody;

			if( Vector3.Distance( _playerRb.transform.position, rabbit.RabbitTransform.position ) < RABBIT_PLAYERCHECKSIZE && _playerRb.velocity.magnitude > 0.025f )
			{
				ChangeRabbitState( rabbit, RabbitState.EVADE );
							
				StartRabbitEvade( rabbit, _playerRb );

                return true;
			}
		}

        return false;
	}

	void StartRabbitEvade( RabbitData rabbit, Rigidbody evadeRB )
	{
		Vector3 rbMoveDir = evadeRB.velocity.normalized;
		//rbMoveDir = Quaternion.Euler(0.0f, UnityEngine.Random.Range( -RABBIT_EVADEANGLE, RABBIT_EVADEANGLE ), 0.0f ) * rbMoveDir;

		rabbit.TargetMovePosition = rabbit.RabbitTransform.position + ( rbMoveDir * UnityEngine.Random.Range( RABBIT_MINEVADEDIST, RABBIT_MAXEVADEDIST ) );

        if (Physics.Raycast( rabbit.TargetMovePosition + Vector3.up * RABBIT_RAYCAST_YOFFSET, Vector3.down, out yPosRay, 15.0f, 1 << LayerMask.NameToLayer( "Ground" ) ))
        {
            rabbit.TargetMovePosition.y = yPosRay.point.y + RABBIT_YPOS;
        }
        else
        {
            rabbit.TargetMovePosition.y = RABBIT_YPOS;
        }

        rabbit.MoveDirection = (rabbit.TargetMovePosition - rabbit.RabbitTransform.position).normalized;

		UpdateRabbitEvade( rabbit ); 
	}

	void UpdateRabbitEvade( RabbitData rabbit )
	{
		if ( Vector3.Distance( rabbit.TargetMovePosition, rabbit.RabbitTransform.position ) > RABBIT_MAXEVADEHOPDIST )
		{
			rabbit.MoveTween = rabbit.RabbitTransform.DOJump( rabbit.RabbitTransform.position + ( rabbit.MoveDirection * RABBIT_MAXEVADEHOPDIST ), RABBIT_EVADEHEIGHT, 1, rabbit.HopSpeed ).OnComplete( () => EndHop( rabbit ) );
		}
		else
		{
			rabbit.MoveTween = rabbit.RabbitTransform.DOJump( rabbit.TargetMovePosition, RABBIT_EVADEHEIGHT, 1, rabbit.HopSpeed ).OnComplete( () => EndEvade( rabbit ) );
		}
	}

	void EndEvade( RabbitData rabbit )
	{		
		if( !CheckRabbitSurroundings( rabbit ) )
		{
			rabbit.HopSpeed = UnityEngine.Random.Range( RABBIT_HOPSPEED_MIN, RABBIT_HOPSPEED_MAX );

			ChangeRabbitState( rabbit, RabbitState.IDLE );
		}
	}

    void OnDrawGizmos()
    {
        for (int i = 0; i < _rabbitList.Count; i++)
        {
            if( _rabbitList[i].RabbitTransform.gameObject.activeSelf )
            {
                Gizmos.color = Color.grey;
                Gizmos.DrawWireSphere( _rabbitList[i].TargetMovePosition, 0.5f );

                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere( _rabbitList[i].RabbitTransform.position, RABBIT_PLAYERCHECKSIZE );
                Gizmos.DrawLine( _rabbitList[i].RabbitTransform.position, _rabbitList[i].RabbitTransform.position + _rabbitList[i].MoveDirection );
            }
        }
    }
}
