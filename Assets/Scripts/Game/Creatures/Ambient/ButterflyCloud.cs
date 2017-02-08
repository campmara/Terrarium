using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ButterflyData
{
	public Transform _butterflyTransform = null;
	public Vector3 _parentOffset = Vector3.zero;
	public Vector3 _targetPosition = Vector3.zero;
	public float _moveSpeed = 0.0f;

	const float MOVESPEED_MIN = 1.0f;
	const float MOVESPEED_MAX = 15.0f;

	public ButterflyData()
	{
		_moveSpeed = Random.Range( MOVESPEED_MIN, MOVESPEED_MAX );
	}
}

public class ButterflyCloud : AmbientCreature {

    [SerializeField] GameObject _creatureObject = null;
    [SerializeField] private int _creatureCount = 2;

	[SerializeField] Gradient _butterflyGradient = null;	

    Vector3 _idlePosition = Vector3.zero;   // pivot point for butterfly group
    const float IDLE_MINDIST = 0.5f;    // How close butterflies get to idle position until the stop tryna get back

    int index = 0;  // b/c i'm a big baby and i'm doing gross iterations rn
    
	List<ButterflyData> _butterflyList = new List<ButterflyData>();

    // Sine Movement Values
	const float FLOATING_YOFFSET = 0.0f;
    const float FLOATING_YAMP = 0.05f;
    const float FLOATING_YVEL = 0.25f;

    // butterfly spawn values
    const float SPAWN_MINDIST = 0.5f;
    const float SPAWN_MAXDIST = 4f;

	[ReadOnlyAttribute, SerializeField]Transform _focusTrans = null;
	Vector3 _focusDir = Vector3.zero;
    const float PLAYER_CHECKRADIUS = 20.0f;     // How big of a radius Butteflies check    
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
        if(_focusTrans == null )
        {
			// Check if an object of interest is within radius 
            Collider[] colArr = Physics.OverlapSphere( _idlePosition, PLAYER_CHECKRADIUS);

            if( colArr.Length > 0 )
            {
                index = 0;
                while ( _focusTrans == null && index < colArr.Length )
                {
                    if( colArr[index].gameObject.GetComponent<Player>() )
                    {
                        _focusTrans = colArr[index].transform;
                    }

                    index++;
                }
            }

			// Move toward idle position if no focus transform
            if( ( transform.position - _idlePosition ).magnitude > IDLE_MINDIST )
            {
                transform.position = Vector3.Lerp( transform.position, _idlePosition, PLAYER_APPROACHSPEED * Time.deltaTime );
            }
        }
			
		MoveButterflies();
    }

    private void FixedUpdate()
    {
        if( _focusTrans != null )
        {
            _focusDir = _focusTrans.position - this.transform.position;
			if ( ( this.transform.position - _idlePosition ).magnitude < PLAYER_CHECKRADIUS )
            {
                this.transform.position = Vector3.Lerp( transform.position, _focusTrans.position, PLAYER_APPROACHSPEED * Time.deltaTime );
            }
            else
            {
                _focusTrans = null;
            }
        }
    }

	private void MoveButterflies()
	{
		// Sine Float the Swarm
		this.transform.SetPosY( FLOATING_YAMP * Mathf.Sin( FLOATING_YVEL * ( Time.time ) ) );

		// Adjust the Target Positions for each butterfly
		// And move the butterflies towards their target pos
		foreach( ButterflyData bData in _butterflyList )
		{
			bData._targetPosition = this.transform.position + bData._parentOffset;

			bData._butterflyTransform.position = Vector3.Lerp( bData._butterflyTransform.position, bData._targetPosition, bData._moveSpeed * Time.deltaTime );
		}
	}

    private void SpawnCreatures()
    {
        GameObject tmpCreature = null;
		ButterflyData tmpData = null;
        for ( int i = 0; i < _creatureCount; i++ )
        {
            tmpCreature = Instantiate( _creatureObject, this.transform ) as GameObject;

            tmpCreature.transform.position = transform.position + Random.insideUnitSphere * Random.Range(SPAWN_MINDIST, SPAWN_MAXDIST);

			Color newColor = _butterflyGradient.Evaluate( Random.value );
			foreach( MeshRenderer m in tmpCreature.gameObject.GetComponentsInChildren<MeshRenderer>() )
			{
				m.material.color = newColor;
			}

			tmpData = new ButterflyData();

			tmpData._butterflyTransform = tmpCreature.transform;
			tmpData._parentOffset = tmpCreature.transform.position - this.transform.position;
			tmpData._targetPosition = tmpCreature.transform.position;

			_butterflyList.Add( tmpData );                     
        }
    }

	// Worried about mem leak, need to look into disposing of non Mono classes
	private void OnDestroy()
	{
		_butterflyList.Clear();
	}

}
