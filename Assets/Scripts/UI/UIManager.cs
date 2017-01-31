using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class UIManager : SingletonBehaviour<UIManager>
{
	Dictionary<System.Type, List<PanelBase>> _typeToPanelListMap = new Dictionary<System.Type, List<PanelBase>>();

	#region Canvas Properties
	Canvas _worldUICanvas = null;
	public static Canvas worldUICanvas
	{
		get
		{
			if (!instance._worldUICanvas)
			{
				instance._worldUICanvas = GameObject.Find("WorldUICanvas").GetComponent<Canvas>();
				if (!instance._worldUICanvas)
				{
					Debug.LogError("WorldUICanvas should exist at game start.", instance.gameObject);
					instance._worldUICanvas = new GameObject("WorldUICanvas", typeof(Canvas)).GetComponent<Canvas>();
				}
			}

			return instance._worldUICanvas;
		}
	}

	Canvas _screenUICanvas = null;
	public static Canvas screenUICanvas
	{
		get
		{
			if (!instance._screenUICanvas)
			{
				instance._screenUICanvas = GameObject.Find("ScreenUICanvas").GetComponent<Canvas>();
				if (!instance._screenUICanvas)
				{
					Debug.LogError("ScreenUICanvas should exist at game start.", instance.gameObject);
					instance._screenUICanvas = new GameObject("ScreenUICanvas", typeof(Canvas)).GetComponent<Canvas>();
				}
			}

			return instance._screenUICanvas;
		}
	}
	#endregion

	void Awake()
	{
		MakeMeAPersistentSingleton();
	}

	public override void Initialize()
	{
		foreach( PanelBase panel in GetComponentsInChildren<PanelBase>(true) )
		{
			if( _typeToPanelListMap.ContainsKey( panel.GetType() ) )
            {
                _typeToPanelListMap[panel.GetType()].Add( panel );
            }                
			else
			{
				List<PanelBase> newPanelList = new List<PanelBase>();
                newPanelList.Add( panel );

                _typeToPanelListMap.Add( panel.GetType(), newPanelList );
			}

            panel.InitializePanel();

            if( panel.DisableOnLoad )
            {
                panel.Disable();
            }
			
		}
			
		isInitialized = true;
	}

	
	public static T GetPanelOfType<T>() where T : PanelBase
	{
		return (T)instance._typeToPanelListMap[typeof(T)].FirstOrDefault();
	}

	public static List<T> GetPanelsOfType<T>() where T : PanelBase
	{
		return instance._typeToPanelListMap[typeof(T)].Cast<T>().ToList();
	}
}