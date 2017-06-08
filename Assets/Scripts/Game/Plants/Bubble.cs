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
	private BPDeathController _parent;

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

	public void Setup(BPDeathController parent, float growthTime, float growthSize, Color color)
	{
		_parent = parent;
		_growthTime = growthTime;
		_growthSize = growthSize;
		
		SetColor(color);

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

	private void SetColor(Color col)
	{
		_mat.SetColor("_Color", col);
	}

	private void OnDestroy()
	{
		_parent.OnBubbleDestroyed(this);
	}
}
