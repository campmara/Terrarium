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
			if( col.gameObject.GetComponent<BPGrowthController>() != null && col.gameObject.GetComponent<BPGrowthController>().CurStage != BPGrowthController.GrowthStage.Sprout )
			{
				_otherObject = col.GetComponent<Collider>();

				_distRange.x = _col.radius;
				//_distRange.x = Vector3.Distance( col.contacts[0].point, col.gameObject.transform.position ) + _col.radius;

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
