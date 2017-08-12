using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bubble : MonoBehaviour 
{
	enum State
	{
		GROWING,
		WAITING,
		NULLMAX
	}
	private State _state;

	private Rigidbody _rb;
	private Material _mat;

	private float _timer;
	private float _growthTime;
	private float _growthSize;
	private Color _color; 

	private float _scaleFactor;

	private void Awake()
	{
		_rb = GetComponent(typeof(Rigidbody)) as Rigidbody;
		_mat = (GetComponent(typeof(MeshRenderer)) as MeshRenderer).material;

		_timer = 0f;

		transform.localScale = Vector3.zero;
	}

	private void Update()
	{
		if (_state == State.WAITING || _state == State.NULLMAX) return;

		_timer += Time.deltaTime;

		_scaleFactor = Mathf.Lerp(0f, _growthSize, _timer / _growthTime);
		SetScale(_scaleFactor);

		if (_timer > _growthTime)
		{
			_state = State.WAITING;
			_timer = 0f;
		}
	}

	public void Setup(float growthTime, float growthSize, Color color1, Color color2)
	{
		_growthTime = growthTime;
		_growthSize = growthSize;
		
		SetColors(color1, color2);


		// Initiate growth.
		_state = State.GROWING;
	}

	public void Drop()
	{
		_rb.useGravity = true;
		Destroy(this.gameObject, 10f);
	}

	private void SetScale(float scaleFactor)
	{
		transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
	}

	private void SetColors(Color col1, Color col2)
	{
		_mat.SetColor("_Color", col1);
		_mat.SetColor("_Color2", col2);
	}
}
