using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using DG.Tweening;

public class PanelIntro : PanelBase {

    [SerializeField] RawImage _introCover = null;
    const float INTRO_FADETIME = 1.0f;

    const float START_SCALE = 5.0f;

    void Awake ()
    {
        _disableOnLoad = false;	
	}

    public override void InitializePanel()
    {
        if( _introCover != null )
        {
            _introCover.transform.localScale = Vector3.one * START_SCALE;
        }
        
    }

    protected override void PrepareEnablePanel()
    {
        _enablePrepped = true;
    }

    protected override void CompleteEnablePanel()
    {
        
    }

    protected override void PrepareDisablePanel()
    {
        if ( _introCover != null )
        {
            StartCoroutine( DelayedDisable() );
        }
        else
        {
            _disablePrepped = true;
        }      
    }

    IEnumerator DelayedDisable()
    {
        // Hacky way to address wacky cam stuff at the start lol
        Color endColor = _introCover.color;
        endColor.a = 0.0f;
        Tween coverAlphaTween = _introCover.DOColor( endColor, INTRO_FADETIME );

        yield return coverAlphaTween.WaitForCompletion();

        //PondManager.instance.HandlePondReturn();

        _disablePrepped = true;
    }

    protected override void CompleteDisablePanel()
    {        
    }
}
