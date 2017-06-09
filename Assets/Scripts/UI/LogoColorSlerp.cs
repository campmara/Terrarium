using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LogoColorSlerp : MonoBehaviour {

    RawImage _rawImage = null;

    float _colorValTimer = 0.0f;
    [SerializeField]
    float _colorChangeSpeed = 0.5f;
    [SerializeField]
    AnimationCurve _colorChangeCurve;
    [SerializeField]
    Gradient _logoColorGradient;

    void Awake()
    {
        _rawImage = this.GetComponent<RawImage>();
    }

    private void Update()
    {
        if( GameManager.Instance.State == GameManager.GameState.NONE )
        {
            _colorValTimer += _colorChangeSpeed * Time.deltaTime;
            _rawImage.color = _logoColorGradient.Evaluate( _colorChangeCurve.Evaluate( _colorValTimer ) );
        }
    }
}
