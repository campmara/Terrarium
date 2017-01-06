#if UNITY_EDITOR

using UnityEngine;
using System.IO;
using UnityEditor;

public static class ScriptableObjectUtility
{
	/// <summary>
	///	This makes it easy to create, name and place unique new ScriptableObject asset files.
	/// </summary>
	public static T CreateAsset<T> (string path = "") where T : ScriptableObject
	{
		T asset = ScriptableObject.CreateInstance<T> ();

		if (path == "") 
			path = "Assets/Resources";
		
		string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath (path + "/New " + typeof(T).ToString() + ".asset");
		
		AssetDatabase.CreateAsset (asset, assetPathAndName);
		
		AssetDatabase.SaveAssets ();
		AssetDatabase.Refresh();
		EditorUtility.FocusProjectWindow ();
		Selection.activeObject = asset;

		return asset;
	}
}
#endif