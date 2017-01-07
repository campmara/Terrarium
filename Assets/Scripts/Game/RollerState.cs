using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Abstract interface for character control states. Movement and other allowances will be
// different for walking / rolling, for example.
public abstract class RollerState : MonoBehaviour
{
	public static WalkingState Walking = new WalkingState();
	public static RollingState Rolling = new RollingState();

	public virtual void Enter(RollerController roller) {}
	public virtual void Exit(RollerController roller) {}

	public virtual void HandleInput(RollerController roller, InputCollection input) {}
}
