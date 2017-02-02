using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveManager : SingletonBehaviour<SaveManager> {

    const string SAVE_PATH = "TerrariumData";

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

		isInitialized = true;
	}

	/// <summary>
	/// Save data routine.
	/// </summary>
	/// <remarks> MAKE SURE TO SAVE SEQUENTIALLY. </remarks>
	IEnumerator SaveDataRoutine()
	{
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
