using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetManager : SingletonBehaviour<AssetManager> {

    [SerializeField]
    List<UnityEngine.Object> _seedAssetList = new List<UnityEngine.Object>();
    List<Seed> _seedObjectList = new List<Seed>();
    
    [SerializeField]
    List<UnityEngine.Object> _groundCoverAssetList = new List<UnityEngine.Object>();
    List<GroundCover> _groundCoverObjectList = new List<GroundCover>();
    
    [SerializeField]
    List<UnityEngine.Object> _plantableAssetList = new List<UnityEngine.Object>();
	List<BasePlant> _plantableObjectList = new List<BasePlant>();

    [SerializeField]
    List<UnityEngine.Object> _BasePlantAssetList = new List<UnityEngine.Object>();
    List<BasePlant> _BasePlantObjectList = new List<BasePlant>();


    // Use this for initialization
    void Awake ()
    {
		
	}

    public override void Initialize()
    {
        isInitialized = true;
    }

    // TODO: Make these NOT be repeated instances of code.  Easier for now but BAd Boy!

    #region Seed Methods

    public Seed InstantiateSeed( SeedAssetKey assetKey )
    {
        Debug.Assert( _seedAssetList.Count > 0 );

        Seed newSeed = null;

        // Check if seed of the same asset key has already been instantiated but has been "destroyed"
        if ( _seedObjectList.Count > 0 )
        {
            newSeed = _seedObjectList.Find( x => x.AssetKey == assetKey );            
        }

        // If none found, spawn new Asset
        if( newSeed == null )
        {
            Debug.Assert( (int)assetKey < _seedAssetList.Count && (int)assetKey >= 0 );
            newSeed = Instantiate( _seedAssetList[(int)assetKey] ) as Seed;
        }
        else   // If asset found remove parenting to AssetManager & remove them from the list
        {
            newSeed.transform.SetParent( null );
            newSeed.gameObject.SetActive( true );
            _seedObjectList.Remove( newSeed );
        }

        return newSeed;
    }

    public void DestroySeed( Seed seed )
    {
        seed.transform.SetParent( this.transform );

        seed.gameObject.SetActive( false );

        _seedObjectList.Add( seed );
    }

    #endregion

    #region Ground Cover Methods

    public GroundCover InstantiateGroundCover( GroundCoverAssetKey assetKey )
    {
        Debug.Assert( _seedAssetList.Count > 0 );

        GroundCover newGC = null;

        // Check if seed of the same asset key has already been instantiated but has been "destroyed"
        if (_seedObjectList.Count > 0)
        {
            newGC = _groundCoverObjectList.Find( x => x.AssetKey == assetKey );
        }

        // If none found, spawn new Asset
        if (newGC == null)
        {
            Debug.Assert( (int)assetKey < _groundCoverAssetList.Count && (int)assetKey >= 0 );
            newGC = Instantiate( _groundCoverAssetList[(int)assetKey] ) as GroundCover;
        }
        else   // If asset found remove parenting to AssetManager & remove them from the list
        {
            newGC.transform.SetParent( null );
            newGC.gameObject.SetActive( true );
            _groundCoverObjectList.Remove( newGC );
        }

        return newGC;
    }

    public void DestroyGroundCover( GroundCover groundCover )
    {
        groundCover.transform.SetParent( this.transform );

        groundCover.gameObject.SetActive( false );

        _groundCoverObjectList.Add( groundCover );
    }

    #endregion

    #region Plantable Methods

	public BasePlant InstantiatePlantable( BasePlantAssetKey assetKey )
    {
        Debug.Assert( _plantableAssetList.Count > 0 );

		BasePlant newPlantable = null;

        // Check if seed of the same asset key has already been instantiated but has been "destroyed"
        if (_seedObjectList.Count > 0)
        {
            newPlantable = _plantableObjectList.Find( x => x.PAssetKey == assetKey );
        }

        // If none found, spawn new Asset
        if (newPlantable == null)
        {
            Debug.Assert( (int)assetKey < _plantableAssetList.Count && (int)assetKey >= 0 );
			newPlantable = Instantiate( _plantableAssetList[(int)assetKey] ) as BasePlant;
        }
        else   // If asset found remove parenting to AssetManager & remove them from the list
        {
            newPlantable.transform.SetParent( null );
            newPlantable.gameObject.SetActive( true );
            _plantableObjectList.Remove( newPlantable );
        }

        return newPlantable;
    }

	public void DestroyPlantable( BasePlant plantable )
    {
        plantable.transform.SetParent( this.transform );

        plantable.gameObject.SetActive( false );

        _plantableObjectList.Add( plantable );
    }

    #endregion

    #region BasePlant Methods

    public BasePlant InstantiateBasePlant( BasePlantAssetKey assetKey )
    {
        Debug.Assert( _BasePlantAssetList.Count > 0 );

        BasePlant newBasePlant = null;

        // Check if seed of the same asset key has already been instantiated but has been "destroyed"
        if (_seedObjectList.Count > 0)
        {
           // newBasePlant = _BasePlantObjectList.Find( x => x.GAssetKey == assetKey );
        }

        // If none found, spawn new Asset
        if (newBasePlant == null)
        {
            Debug.Assert( (int)assetKey < _BasePlantAssetList.Count && (int)assetKey >= 0 );
            newBasePlant = Instantiate( _BasePlantAssetList[(int)assetKey] ) as BasePlant;
        }
        else   // If asset found remove parenting to AssetManager & remove them from the list
        {
            newBasePlant.transform.SetParent( null );
            newBasePlant.gameObject.SetActive( true );
            _BasePlantObjectList.Remove( newBasePlant );
        }

        return newBasePlant;
    }

    public void DestroyBasePlant( BasePlant BasePlant )
    {
        BasePlant.transform.SetParent( this.transform );

        BasePlant.gameObject.SetActive( false );

        _BasePlantObjectList.Add( BasePlant );
    }

    #endregion
}

#region Asset Enums

public enum SeedAssetKey
{
    NONE = -1,
    STARTER = 0
}

public enum GroundCoverAssetKey
{
    NONE = -1,
    STARTER = 0
}

public enum SmallPlantAssetKey
{
    NONE = -1,
    STARTER = 0
}

public enum BasePlantAssetKey
{
    NONE = -1,
    STARTER = 0
}

#endregion