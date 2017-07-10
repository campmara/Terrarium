using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraOverlapController : MonoBehaviour {

	Rigidbody _rb = null;
	SphereCollider _col = null;

	// x value is maximum distance away
	Vector3 _targetPos = Vector3.zero;

	[SerializeField, ReadOnlyAttribute] bool _inObject = false;
	Collider _otherObject = null; 

	Vector2 _distRange = new Vector2( 1.0f, 0.0f );
	[SerializeField, ReadOnlyAttribute]float _currBlackVal = 0.0f;
	[SerializeField] AnimationCurve _rangeCurve;

	void Awake() 
	{
		_rb = this.GetComponent<Rigidbody>();

		_col = this.GetComponent<SphereCollider>();

	}

	void Update() 
	{
		_targetPos = this.transform.parent.position;
		_targetPos.y = 0.0f;

		this.transform.position = _targetPos;
		//_rb.MovePosition( _targetPos );

		if( _inObject )
		{
			_currBlackVal = _rangeCurve.Evaluate( Mathf.InverseLerp( _distRange.x, _distRange.y, Vector3.Distance( this.transform.position, _otherObject.transform.position ) ) );

			UIManager.GetPanelOfType<PanelOverlay>().BlackOverlay.color = new Color( 0.0f, 0.0f, 0.0f, _currBlackVal );
		}
		else if( !_inObject && _currBlackVal > Mathf.Epsilon )
		{
			_currBlackVal = Mathf.Lerp( _currBlackVal, 0.0f, 20.0f * Time.deltaTime );

			if( _currBlackVal <= Mathf.Epsilon )
			{
				_currBlackVal = 0.0f;
			}

			UIManager.GetPanelOfType<PanelOverlay>().BlackOverlay.color = new Color( 0.0f, 0.0f, 0.0f, _currBlackVal );
		}

	}

	void OnTriggerEnter( Collider col )
	{
		if ( !_inObject )
		{
			BPGrowthController plantCol = col.gameObject.GetComponent<BPGrowthController>();
			if(plantCol != null 
				&& plantCol.CurStage != BPGrowthController.GrowthStage.Sprout 
				&& plantCol.GetComponent<StarterPlantGrowthController>())
			{
				_otherObject = col.GetComponent<Collider>();

                //_distRange.x = _col.radius;
                Vector3 pointClosestToSphere = col.ClosestPoint( this.transform.position + ( ( col.gameObject.transform.position - this.transform.position ).normalized * _col.radius ) );
				_distRange.x = Vector3.Distance( pointClosestToSphere, col.gameObject.transform.position ) + _col.radius;

				_inObject = true;	
			}
		}
	}

	void OnTriggerExit( Collider col )
	{
		if( _inObject && _otherObject == col.GetComponent<Collider>() )
		{
			_otherObject = null;
			_inObject = false;
		}
	}
}
