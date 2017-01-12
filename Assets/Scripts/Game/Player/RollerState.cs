using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// interface for character control states. Movement and other allowances will be
// different for walking / rolling, for example.
public class RollerState : MonoBehaviour
{
	// Create static instances of all the states you implement.
	public static WalkingState Walking = new WalkingState();
	public static RollingState Rolling = new RollingState();

	public virtual void Enter(RollerController parent) {}
	public virtual void Exit() {}

	public virtual void HandleInput(InputCollection input) {}
}
