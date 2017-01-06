using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Panel_Score : PanelBase {

	[SerializeField] private Text _player1ScoreText;
	public Text Player1ScoreText { get { return _player1ScoreText; } }
	[SerializeField] private Text _player2ScoreText;
	public Text Player2ScoreText { get { return _player2ScoreText; } }

	[SerializeField] private RawImage _player1HeartImage;
	public RawImage Player1HeartImage { get { return _player1HeartImage; } }
	[SerializeField] private RawImage _player2HeartImage;
	public RawImage Player2HeartImage { get { return _player2HeartImage; } }

	// Update is called once per frame
	void Update () 
	{
	
	}

	void OnDestroy()
	{
	}

	protected override void PrepareDisablePanel ()
	{
		/*if(UIManager.instance.GetComponent<ScoreManager>().ScorePanel == null)
			UIManager.instance.GetComponent<ScoreManager>().ScorePanel = this;*/
		
		_player1HeartImage.rectTransform.localScale = Vector3.zero;
	}
		
}
