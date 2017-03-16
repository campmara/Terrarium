using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceManager : MonoBehaviour
{
    [Header("Renderers")]
    [SerializeField] private SpriteRenderer _eyeRenderer;
    [SerializeField] private SpriteRenderer _mouthRenderer;

    [Header("Eyes")]
    [SerializeField] private Sprite _eyesOpen;
    [SerializeField] private Sprite _eyesClosed;
	[SerializeField] private Sprite _eyesDesire;
	[SerializeField] private Sprite _eyesHappy;
	[SerializeField] private Sprite _eyesWink;
	[SerializeField] private Sprite _eyesSurprised;
	[SerializeField] private Sprite _eyesSad;
	[SerializeField] private Sprite _eyesAngry;
	[SerializeField] private Sprite _eyesAnnoyed;
	private Sprite _currentEyes;

    [Header("Mouth")]
	[SerializeField] private Sprite _mouthIdle;
    [SerializeField] private Sprite _mouthOh;
	[SerializeField] private Sprite _mouthD;
	[SerializeField] private Sprite _mouthSmile;
	[SerializeField] private Sprite _mouthSideSmile;
	[SerializeField] private Sprite _mouthDiagonal;
	[SerializeField] private Sprite _mouthSad;
	[SerializeField] private Sprite _mouthVerySad;

    private Coroutine _blinkRoutine;

	void Awake()
	{
	    if (_eyeRenderer == null || _mouthRenderer == null)
	    {
	        Debug.LogError("One or more of the face sprite renderers are unhooked.");
	    }

		BecomeIdle();
		InitiateBlinkLoop();
	}

	// ===============
	// E M O T I O N S
	// ===============

	public void BecomeIdle()
	{
		SetEyes(_eyesOpen);
		SetMouth(_mouthIdle);
	}

	public void Sing()
	{
		SetEyes(_eyesHappy);
		SetMouth(_mouthOh);
	}

	public void BecomeHappy()
	{
		SetEyes(_eyesHappy);
		SetMouth(_mouthSmile);
	}

	public void Wink()
	{
		SetEyes(_eyesWink);
		SetMouth(_mouthSideSmile);
	}

	public void BecomeAnnoyed()
	{
		SetEyes(_eyesAnnoyed);
		SetMouth(_mouthSad);
	}

	public void BecomeSad()
	{
		SetEyes(_eyesSad);
		SetMouth(_mouthVerySad);
	}

	public void BecomeInterested()
	{
		SetEyes(_eyesOpen);
		SetMouth(_mouthOh);
	}

	public void BecomeSurprised()
	{
		SetEyes(_eyesSurprised);
		SetMouth(_mouthOh);
	}

	public void BecomeDesirous()
	{
		SetEyes(_eyesDesire);
		SetMouth(_mouthD);
	}

	public void BecomeEncumbered()
	{
		SetEyes(_eyesAngry);
		SetMouth(_mouthDiagonal);
	}

	public void BecomeFeisty()
	{
		SetEyes(_eyesAngry);
		SetMouth(_mouthOh);
	}

	// ===========
	// H E L P E R
	// ===========

	private void SetEyes(Sprite eyes)
	{
		_eyeRenderer.sprite = eyes;
		_currentEyes = eyes;
	}

	private void SetMouth(Sprite mouth)
	{
		_mouthRenderer.sprite = mouth;
	}

    private void InitiateBlinkLoop()
    {
        if (_blinkRoutine != null)
            StopCoroutine(_blinkRoutine);

        _blinkRoutine = StartCoroutine(BlinkRoutine(Random.Range(1f, 5f)));
    }

    private IEnumerator BlinkRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        // BLINK
        _eyeRenderer.sprite = _eyesClosed;
        yield return new WaitForSeconds(0.1f);
		_eyeRenderer.sprite = _currentEyes;

        InitiateBlinkLoop();
    }
}
