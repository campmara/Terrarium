﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoFind : PropertyAttribute {
	public System.Type objectType;
	public bool searchInChildren;

	/// <summary>
	/// Add a Find button in the inspector.
	/// </summary>
	/// <param name="ObjectType">Type of the component.</param>
	/// <param name="SearchInChildren">If set to <c>true</c>, will search in children.</param>
	public AutoFind(System.Type ObjectType, bool SearchInChildren = false){
		this.objectType = ObjectType;
		this.searchInChildren = SearchInChildren;
	}
}
