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
    List<Plantable> _plantableObjectList = new List<Plantable>();

    [SerializeField]
    List<UnityEngine.Object> _BigPlantAssetList = new List<UnityEngine.Object>();
    List<BigPlant> _BigPlantObjectList = new List<BigPlant>();


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

    public Plantable InstantiatePlantable( PlantableAssetKey assetKey )
    {
        Debug.Assert( _plantableAssetList.Count > 0 );

        Plantable newPlantable = null;

        // Check if seed of the same asset key has already been instantiated but has been "destroyed"
        if (_seedObjectList.Count > 0)
        {
            newPlantable = _plantableObjectList.Find( x => x.PAssetKey == assetKey );
        }

        // If none found, spawn new Asset
        if (newPlantable == null)
        {
            Debug.Assert( (int)assetKey < _plantableAssetList.Count && (int)assetKey >= 0 );
            newPlantable = Instantiate( _plantableAssetList[(int)assetKey] ) as Plantable;
        }
        else   // If asset found remove parenting to AssetManager & remove them from the list
        {
            newPlantable.transform.SetParent( null );
            newPlantable.gameObject.SetActive( true );
            _plantableObjectList.Remove( newPlantable );
        }

        return newPlantable;
    }

    public void DestroyPlantable( Plantable plantable )
    {
        plantable.transform.SetParent( this.transform );

        plantable.gameObject.SetActive( false );

        _plantableObjectList.Add( plantable );
    }

    #endregion

    #region BigPlant Methods

    public BigPlant InstantiateBigPlant( BigPlantAssetKey assetKey )
    {
        Debug.Assert( _BigPlantAssetList.Count > 0 );

        BigPlant newBigPlant = null;

        // Check if seed of the same asset key has already been instantiated but has been "destroyed"
        if (_seedObjectList.Count > 0)
        {
            newBigPlant = _BigPlantObjectList.Find( x => x.GAssetKey == assetKey );
        }

        // If none found, spawn new Asset
        if (newBigPlant == null)
        {
            Debug.Assert( (int)assetKey < _BigPlantAssetList.Count && (int)assetKey >= 0 );
            newBigPlant = Instantiate( _BigPlantAssetList[(int)assetKey] ) as BigPlant;
        }
        else   // If asset found remove parenting to AssetManager & remove them from the list
        {
            newBigPlant.transform.SetParent( null );
            newBigPlant.gameObject.SetActive( true );
            _BigPlantObjectList.Remove( newBigPlant );
        }

        return newBigPlant;
    }

    public void DestroyBigPlant( BigPlant BigPlant )
    {
        BigPlant.transform.SetParent( this.transform );

        BigPlant.gameObject.SetActive( false );

        _BigPlantObjectList.Add( BigPlant );
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

public enum PlantableAssetKey
{
    NONE = -1,
    STARTER = 0
}

public enum BigPlantAssetKey
{
    NONE = -1,
    STARTER = 0
}

#endregion