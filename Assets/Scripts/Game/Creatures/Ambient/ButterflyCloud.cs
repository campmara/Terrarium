using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButterflyCloud : AmbientCreature {

    [SerializeField] GameObject _creatureObject = null;
    [SerializeField] private int _creatureCount = 2;

    Vector3 _idlePosition = Vector3.zero;   // pivot point for butterfly group
    const float IDLE_MINDIST = 0.5f;    // How close butterflies get to idle position until the stop tryna get back

    int index = 0;  // b/c i'm a big baby and i'm doing gross iterations rn
    List<Transform> _creatureList = new List<Transform>();
    List<float> _creatureYStartPos = new List<float>();
    List<float> _creatureYOffset = new List<float>();
    // Sine Movement Values
    const float FLOATING_YOFFSETMIN = 1.0f;
    const float FLOATING_YOFFSETMAX = 5.0f;
    const float FLOATING_YAMP = 0.25f;
    const float FLOATING_YVEL = 2f;

    // butterfly spawn values
    const float SPAWN_MINDIST = 0.5f;
    const float SPAWN_MAXDIST = 4f;

    [ReadOnlyAttribute, SerializeField]Transform _playerTrans = null;
    Vector3 _playerDir = Vector3.zero;
    const float PLAYER_CHECKRADIUS = 20.0f;     // How big of a radius Butteflies check
    const float PLAYER_BREAKDISTANCE = 20.0f;   // How far from idle Position butterflies can go    
    const float PLAYER_APPROACHSPEED = 0.5f;    // How quickly butterflies chase

    // Use this for initialization
    void Awake ()
    {
        _idlePosition = transform.position;

        SpawnCreatures();
	}
	
	// Update is called once per frame
	void Update ()
    {
        if(_playerTrans == null )
        {
            Collider[] colArr = Physics.OverlapSphere( _idlePosition, PLAYER_CHECKRADIUS);

            if( colArr.Length > 0 )
            {
                index = 0;
                while ( _playerTrans == null && index < colArr.Length )
                {
                    if( colArr[index].gameObject.GetComponent<Player>() )
                    {
                        _playerTrans = colArr[index].transform;
                    }

                    index++;
                }
            }

            if( ( transform.position - _idlePosition ).magnitude > IDLE_MINDIST )
            {
                transform.position = Vector3.Lerp( transform.position, _idlePosition, PLAYER_APPROACHSPEED * Time.deltaTime );
            }
        }

        for ( index = 0; index < _creatureList.Count; index++ )
        {
            float posChange = _creatureYStartPos[index] + ( FLOATING_YAMP * Mathf.Sin( FLOATING_YVEL * ( Time.time + _creatureYOffset[index] ) ) );
            _creatureList[index].SetPosY( posChange );
            //_creatureList[index].SetPosX( JohnTech.RandomSign() * Mathf.Cos(posChange ));
        }
    }

    private void FixedUpdate()
    {
        if( _playerTrans != null )
        {
            _playerDir = _playerTrans.position - transform.position;
            if (( transform.position - _idlePosition ).magnitude < PLAYER_BREAKDISTANCE)
            {
                transform.position = Vector3.Lerp( transform.position, _playerTrans.position, PLAYER_APPROACHSPEED * Time.deltaTime );
            }
            else
            {
                _playerTrans = null;
            }
        }
    }

    private void SpawnCreatures()
    {
        GameObject tmpCreature = null;
        for ( int i = 0; i < _creatureCount; i++ )
        {
            tmpCreature = Instantiate( _creatureObject, this.transform ) as GameObject;

            tmpCreature.transform.position = transform.position + Random.insideUnitSphere * Random.Range(SPAWN_MINDIST, SPAWN_MAXDIST);

            _creatureList.Add( tmpCreature.transform );
            _creatureYStartPos.Add( tmpCreature.transform.position.y );
            _creatureYOffset.Add( Random.Range( FLOATING_YOFFSETMIN, FLOATING_YOFFSETMAX ) );
        }
    }
}
