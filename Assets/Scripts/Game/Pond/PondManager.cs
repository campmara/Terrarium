using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PondManager : SingletonBehaviour<PondManager> {

    [SerializeField]    private PondTech _pond = null;
    public PondTech Pond { get { return _pond; } }

	// Use this for initialization
	void Awake ()
    {
		if(_pond == null)
        {
            // Should handle instantiating this if null ?
            Debug.LogError( "No Pond Prefab Referenced" );           
        }
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    public void PopPlayerFromPond()
    {

    }

    public void ReturnPlayerToPond()
    {
        
    }

    IEnumerator DelayedReturnPlayer()
    {
        yield return 0;
    }
}
