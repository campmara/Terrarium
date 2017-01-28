using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceManager : SingletonBehaviour<FaceManager> 
{
	[SerializeField] Sprite idleFace;
	[SerializeField] Sprite singFace;

	[AutoFind(typeof(SpriteRenderer)), SerializeField] SpriteRenderer spriteRenderer;

	public override void Initialize ()
	{
		isInitialized = true;
	}

	void Awake()
	{
		if (spriteRenderer == null)
		{
			spriteRenderer = GetComponent(typeof(SpriteRenderer)) as SpriteRenderer;
		}

		NormalFace();
	}

	public void NormalFace()
	{
		spriteRenderer.sprite = idleFace;
	}

	public void SingFace()
	{
		spriteRenderer.sprite = singFace;
	}
}
