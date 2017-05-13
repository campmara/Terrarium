using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class EditorUtilities {

    [MenuItem("Utilities/Shortcuts/Reduce Selection &%r")] // ALT + CTRL + R
    static void ReduceSelection()
    {

        GameObject[] newSelection = new GameObject[Selection.transforms.Length/2];

        for (int i = 0; i < newSelection.Length; i++)
        {
            newSelection[i] = Selection.gameObjects[i*2];
        }

        Selection.objects = newSelection;

        Debug.Log("selection reduced to: " + newSelection.Length);
    }
}
