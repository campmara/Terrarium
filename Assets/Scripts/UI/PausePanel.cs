using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PausePanel : PanelBase
{
	private static bool _paused = false;
	public static bool Paused { get { return PausePanel._paused; } }

	RawImage _pauseOverlay = null;

	void Awake()
	{
		_pauseOverlay = GetComponent<RawImage>();	
	}

	public void TogglePause()
	{
		if (_paused)
		{
			_pauseOverlay.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);

			Time.timeScale = 1.0f;
			Cursor.visible = true;

			_paused = false;
		}
		else
		{
			_pauseOverlay.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);

			Time.timeScale = 0.0f;
			Cursor.visible = false;

			_paused = true;
		}
	}
}
