using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelOverlay : PanelBase {

	[SerializeField] RawImage _screenshotOverlay = null;
	public RawImage ScreenshotOverlay { get { return _screenshotOverlay; } set { _screenshotOverlay = value; } }

	[SerializeField] RawImage _blackOverlay = null;
	public RawImage BlackOverlay { get { return _blackOverlay; } set { _screenshotOverlay = value; } }
	
}
