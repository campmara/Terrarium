using UnityEngine;
using System.Collections;

public class PanelBase : MonoBehaviour 
{
    [SerializeField]
    protected bool _disableOnLoad = true;
    public bool DisableOnLoad { get { return _disableOnLoad; } set { _disableOnLoad = false; } }

    protected bool _enablePrepped = false;
    protected bool _disablePrepped = false;
    

	void Start()
	{
		InitializePanel();
	}

	public void Enable()
	{
        StartCoroutine( EnablePanel() );
	}

	public void Disable()
	{
		StartCoroutine( DisablePanel() );
	}

	IEnumerator EnablePanel() 
	{
		if( !gameObject.activeSelf )
		{
			PrepareEnablePanel();

            yield return new WaitUntil( () => _enablePrepped );

			gameObject.SetActive( true );

			CompleteEnablePanel();

            _enablePrepped = false;
		}
	}

    IEnumerator DisablePanel() 
	{
		if( gameObject.activeSelf )
		{
			PrepareDisablePanel();

            yield return new WaitUntil( () => _disablePrepped );

			gameObject.SetActive( false );

			CompleteDisablePanel();

            _disablePrepped = false;
		}
	}

	protected virtual void PrepareEnablePanel() { _enablePrepped = true; }
	protected virtual void CompleteEnablePanel() {}
	protected virtual void PrepareDisablePanel() { _disablePrepped = true; }
	protected virtual void CompleteDisablePanel() {}
	public virtual void InitializePanel() {}
}
