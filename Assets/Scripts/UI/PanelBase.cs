using UnityEngine;
using System.Collections;

public class PanelBase : MonoBehaviour 
{
	// Use this for initialization
	void Awake () 
	{
		//gameObject.SetActive(false);
	}

	void Start()
	{
		InitializePanel();
	}

	// Update is called once per frame
	void Update () 
	{

	}

	public void Enable()
	{
		EnablePanel();
	}

	public void Disable()
	{
		DisablePanel();	
	}

	protected void EnablePanel() 
	{
		if(!gameObject.activeSelf)
		{
			PrepareEnablePanel();

			gameObject.SetActive(true);

			CompleteEnablePanel();
		}
	}

	protected virtual void DisablePanel() 
	{
		if(gameObject.activeSelf)
		{
			PrepareDisablePanel();

			gameObject.SetActive(false);

			CompleteDisablePanel();
		}
	}

	protected virtual void PrepareEnablePanel() {}
	protected virtual void CompleteEnablePanel() {}
	protected virtual void PrepareDisablePanel() {}
	protected virtual void CompleteDisablePanel() {}
	protected virtual void InitializePanel() {}
}
