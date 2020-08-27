﻿using UnityEditor;
using UnityEngine;

public class CustomUpgradeEditorWindow: EditorWindow
{
    [MenuItem("Window/StatDebug")]
    public static void ShowWindow()
    {
        GetWindow<CustomUpgradeEditorWindow>("DebugStats");
    }


    string valueString = "Enter Yield value";

    private void OnGUI()
    {
        valueString = EditorGUILayout.TextField("Yield Value", valueString);

        GUILayout.Label("Mining Yield");
        if (GUILayout.Button("Set Mining Yield"))
        {
            UpgradeSystem.Instance.MiningYieldMultiplier *= float.Parse(valueString);
        }
    }
}
