using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class LogoColorSlerp : MonoBehaviour {

	[SerializeField]
    RawImage _rawImage = null;

    float _colorValTimer = 0.0f;
    [SerializeField]
    float _colorChangeSpeed = 0.5f;
    [SerializeField]
    AnimationCurve _colorChangeCurve;
    [SerializeField, ReadOnlyAttribute]
    Gradient _logoColorGradient;
	public Gradient ColorGradient { get { return _logoColorGradient; } set { _logoColorGradient = value; } }

	[SerializeField]
	float _scaleScreenPercentage = 1.0f;
	[SerializeField]
	float _posScreenPercentage = 0.0f;

    void Awake()
    {
		if( _rawImage == null )
		{
			_rawImage = this.GetComponent<RawImage>();	
		} 
		_rawImage.rectTransform.sizeDelta = new Vector2( Camera.main.pixelWidth * _scaleScreenPercentage, Camera.main.pixelWidth * _scaleScreenPercentage );
		_rawImage.rectTransform.anchoredPosition = new Vector2( 0.0f, _posScreenPercentage * Camera.main.pixelHeight );
    }

    private void Update()
    {
		if( Application.isPlaying )
		{
			if( GameManager.Instance.State == GameManager.GameState.NONE )
			{
				_colorValTimer += _colorChangeSpeed * Time.deltaTime;
				_rawImage.color = _logoColorGradient.Evaluate( _colorChangeCurve.Evaluate( _colorValTimer ) );
			}	
		}
		else
		{
			_colorValTimer += _colorChangeSpeed * Time.deltaTime;
			_rawImage.color = _logoColorGradient.Evaluate( _colorChangeCurve.Evaluate( _colorValTimer ) );
		}
    }

	void OnValidate()
	{
		_rawImage.rectTransform.sizeDelta = new Vector2( Camera.main.pixelWidth * _scaleScreenPercentage, Camera.main.pixelWidth * _scaleScreenPercentage );
		_rawImage.rectTransform.anchoredPosition = new Vector2( 0.0f, _posScreenPercentage * Camera.main.pixelHeight );
	}
}
