using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCover : MonoBehaviour {

    [SerializeField] protected GroundCoverAssetKey _assetKey = GroundCoverAssetKey.NONE;
    public GroundCoverAssetKey AssetKey { get { return _assetKey; } set { _assetKey = value; } }
 
}
