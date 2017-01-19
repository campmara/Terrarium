using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveManager : SingletonBehaviour<SaveManager> {

    const string SAVE_PATH = "TerrariumData";

    SaveFile _saveData = null;

    public override void Initialize()
    {
        MakeMeAPersistentSingleton();
       
        if ( ES2.Exists( Application.persistentDataPath + SAVE_PATH ) )   // Check if save data exists
        {
            ES2Reader reader = ES2Reader.Create( Application.persistentDataPath + SAVE_PATH );
            ES2.Load<SaveFile>( Application.persistentDataPath + SAVE_PATH );

            reader.Dispose();
        }
        else
        {
            // TODO: Create new Save Data
        }

        isInitialized = true;
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
        
    }

    public void LoadData()
    {
       
    }
}
