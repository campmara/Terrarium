using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCover : MonoBehaviour 
{
	[SerializeField] Vector2 _scaleRange = new Vector2( 0.1f, .15f );
	float _scale = 0.0f;
    [SerializeField] protected GroundCoverAssetKey _assetKey = GroundCoverAssetKey.NONE;
    public GroundCoverAssetKey AssetKey { get { return _assetKey; } set { _assetKey = value; } }

	void Awake()
	{
		_scale = Random.Range( _scaleRange.x, _scaleRange.y );
		transform.localScale = new Vector3( _scale, _scale, _scale );
	}	
}
