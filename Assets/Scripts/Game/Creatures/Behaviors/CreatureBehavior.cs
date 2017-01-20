using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureBehavior : MonoBehaviour 
{
	// Don't inherit these.
	private void Awake() {}
	private void Start() {}
	private void Update() {}
	private void FixedUpdate() {}

	public virtual void Enter() {}
	public virtual void Exit() {}

	public virtual void Handle() {}
}
