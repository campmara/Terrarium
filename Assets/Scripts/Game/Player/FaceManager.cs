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

    [Header("Mouth")]
	[SerializeField] private Sprite _mouthIdle;
    [SerializeField] private Sprite _mouthOh;

    private Coroutine _blinkRoutine;

	void Awake()
	{
	    if (_eyeRenderer == null || _mouthRenderer == null)
	    {
	        Debug.LogError("One or more of the face sprite renderers are unhooked.");
	    }

		NormalFace();

	    InitiateBlinkLoop();
	}

	public void NormalFace()
	{
		_mouthRenderer.sprite = _mouthIdle;
	}

	public void SingFace()
	{
	    _mouthRenderer.sprite = _mouthOh;
	}

    private void InitiateBlinkLoop()
    {
        if (_blinkRoutine != null)
            StopCoroutine(_blinkRoutine);

        _blinkRoutine = StartCoroutine(BlinkRoutine(Random.Range(1f, 6f)));
    }

    private IEnumerator BlinkRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        // BLINK
        _eyeRenderer.sprite = _eyesClosed;
        yield return new WaitForSeconds(0.1f);
        _eyeRenderer.sprite = _eyesOpen;

        InitiateBlinkLoop();
    }
}
