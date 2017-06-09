using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelOverlay : PanelBase {

	[SerializeField] RawImage _screenshotOverlay = null;
	public RawImage ScreenshotOverlay { get { return _screenshotOverlay; } set { _screenshotOverlay = value; } }
	[SerializeField] List<Texture> _screenshotOverlayTextureList = new List<Texture>();

	[SerializeField, Space(10)] RawImage _blackOverlay = null;
	public RawImage BlackOverlay { get { return _blackOverlay; } set { _blackOverlay = value; } }

	[SerializeField, Space(10)]
    RawImage _logoOverlay = null;
    public RawImage LogoOverlay { get { return _logoOverlay; } set { _logoOverlay = value; } }

	public void RandomizeScreenshotOverlay()
	{
		_screenshotOverlay.texture = _screenshotOverlayTextureList[Random.Range( 0, _screenshotOverlayTextureList.Count )];
	}
}
