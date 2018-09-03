using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PausePanel : PanelBase
{
	private static bool _paused = false;
	public static bool Paused { get { return PausePanel._paused; } }

	RawImage _pauseOverlay = null;

	[ReadOnly]
	float _restartTimer = 0.0f;

	[SerializeField]
	AnimationCurve _restartFadeCurve;

	void Awake()
	{
		_pauseOverlay = GetComponent<RawImage>();
		
		if(_paused)
		{
			_restartTimer = 0.0f;
			TogglePause();
		}
	}

	void Update()
	{
		InputCollection input = ControlManager.instance.getInput();
		if (GameManager.Instance.State == GameManager.GameState.MAIN && input.StartButton.WasPressed && Mathf.Approximately(_restartTimer, 0.0f))
		{
			TogglePause();
		}
		else if (_paused)
		{
			if(input.StartButton.IsPressed)
			{
				_restartTimer += Time.unscaledDeltaTime;
				float restartTransitionProgress = _restartFadeCurve.Evaluate(_restartTimer);

				if (restartTransitionProgress >= 1.0f)
				{
					SceneManager.LoadScene(0);
					Time.timeScale = 1.0f;
					_paused = false;
				}
				else
				{
					UIManager.GetPanelOfType<PanelOverlay>().BlackOverlay.color = new Color(0.0f, 0.0f, 0.0f, restartTransitionProgress);
				}
			}
			else
			{
				_restartTimer = 0.0f;
				UIManager.GetPanelOfType<PanelOverlay>().BlackOverlay.color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
			}
		}
	}

	public void TogglePause()
	{
		if (_paused)
		{
			_pauseOverlay.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);

			UIManager.GetPanelOfType<PanelOverlay>().BlackOverlay.color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
			Time.timeScale = 1.0f;

			_paused = false;
		}
		else
		{
			_pauseOverlay.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);

			_restartTimer = 0.0f;
			Time.timeScale = 0.0f;

			_paused = true;
		}
	}

}
