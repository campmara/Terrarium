using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SaveManager : SingletonBehaviour<SaveManager> {

    const string SAVE_PATH = "TerrariumData";

    public static event Action PrepSave;
    public static event Action CompleteLoad;

    PlantSaveData _plantSave = null;
    public PlantSaveData PlantSave { get { return _plantSave; } set { _plantSave = value; } }

    public override void Initialize()
    {
        MakeMeAPersistentSingleton();
       
		// Save & Load on Initialize
        if ( ES2.Exists( Application.persistentDataPath + SAVE_PATH ) )   // Check if save data exists
        {
			LoadData();
        }
        else
        {            
			SaveData();
        }		   
    }

    // Use this for initialization
    void Awake ()
    {
		
	}

    // Update is called once per frame
    void Update()
    {

    }

	public void SaveData()
	{
		StartCoroutine( SaveDataRoutine() );
	}

	public void LoadData()
	{
		StartCoroutine( LoadDataRoutine() );
	}

	/// <summary>
	/// Load data routine.
	/// </summary>
	/// <remarks> MAKE SURE TO LOAD SEQUENTIALLY. </remarks>
    IEnumerator LoadDataRoutine()
	{
		ES2Reader reader = ES2Reader.Create( Application.persistentDataPath + SAVE_PATH );

		yield return 0;

		reader.Dispose();

        //CompleteLoad();

		isInitialized = true;
	}

	/// <summary>
	/// Save data routine.
	/// </summary>
	/// <remarks> MAKE SURE TO SAVE SEQUENTIALLY. </remarks>
	IEnumerator SaveDataRoutine()
	{
        //PrepSave();

        //yield return new WaitUntil( () => _plantSave != null );

        // TODO: Create new Save Data
		ES2Writer writer = ES2Writer.Create( Application.persistentDataPath + SAVE_PATH );

		yield return 0;

		writer.Dispose();

		if( !isInitialized )
		{
			isInitialized = true;
		}
	}

	public void DeleteSave()
	{
		// TODO: What is implied by Deleting Save?

		if ( ES2.Exists( Application.persistentDataPath + SAVE_PATH ) )   // Check if save data exists
		{
			ES2.Delete( Application.persistentDataPath + SAVE_PATH );
		}
	}

	void OnApplicationQuit()
	{
		SaveData();	// Need to test if this Works
	}

}



#region Save Data Classes 

public class PlantSaveData
{
    public int _seedCount = 0;
    public List<SeedData> _seedData = new List<SeedData>();
    public int _groundCoverCount = 0;
    public List<GroundCoverData> _groundCoverData = new List<GroundCoverData>();
    public int _BigPlantCount = 0;
	public List<BigPlantData> _BigPlantData = new List<BigPlantData>();
    public int _plantableCount = 0;
	public List<SmallPlantData> _plantableData = new List<SmallPlantData>();

    public PlantSaveData()
    {
    }
}

public class SeedData
{
    public SeedAssetKey _assetKey = SeedAssetKey.NONE;

    public Vector3 _seedPosition = Vector3.zero;
    public Quaternion _seedRotation = Quaternion.identity;

    public float _timeSinceLastPickup = 0.0f;
    public float _timePassedTillDestroy = 0.0f;

    public SeedData()
    {

    }
}

public class GroundCoverData
{
    public GroundCoverAssetKey _assetKey = GroundCoverAssetKey.NONE;

    public Vector3 _groundCoverPosition = Vector3.zero;
    public Quaternion _groundCoverRotation = Quaternion.identity;

    public GroundCoverData()
    {

    }
}
// TODO: Need a system to pull and process the animator/leaf data from the plants
public class BigPlantData
{
	public BigPlantAssetKey _assetKey = BigPlantAssetKey.NONE;

	BigPlant.GrowthStage _growthStage = BigPlant.GrowthStage.Sprout;

	public BigPlantData()
    {

    }
}

public class SmallPlantData
{
	public SmallPlantAssetKey _assetKey = SmallPlantAssetKey.NONE;

    public Vector3 _plantablePosition = Vector3.zero;
    public Quaternion _plantableRotation = Quaternion.identity;
    public Vector3 _localScale = Vector3.zero;

    public float _curGrowthRate = 0.0f;


	public SmallPlantData()
    {

    }
}
#endregion