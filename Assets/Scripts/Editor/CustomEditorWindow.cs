using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class CustomEditorWindow : EditorWindow
{
    [MenuItem("Window/DEBUGGER")]
    public static void ShowWindow()
    {
        GetWindow<CustomEditorWindow>("DebugWindow");
    }

    BaseResources resource = BaseResources.ironOre;

    string resourceAmountString = "Add Resource Amount";
        
    private void OnGUI()
    {
        GUILayout.BeginVertical("Add Resource");
        resourceAmountString = EditorGUILayout.TextField("Resource Amout", resourceAmountString);
        GUILayout.Label("Add Iron Ore", EditorStyles.boldLabel);

        resource = (BaseResources)EditorGUI.EnumPopup(new Rect(5, 80, 380, 20), "Select Resource", resource);

        if (GUILayout.Button("+" + resourceAmountString + " " + ResourceManager.Instance.GetValidName(resource.ToString()) ))
        {
            ResourceManager.Instance.AddResource(resource, int.Parse(resourceAmountString));
        }
        GUILayout.EndVertical();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("SAVE"))
        {
            SaveSystem.Instance.Save();
        }
        if (GUILayout.Button("LOAD"))
        {
            SaveSystem.Instance.Load();
        }
        GUILayout.EndHorizontal();

        //var resources = Enum.GetValues(typeof(BaseResources)).Cast<BaseResources>();
        //Vector2 scrollView = GUILayout.BeginScrollView(new Vector2(100, 100),false,true);
        

        //foreach (var resource in resources)
        //{
        //    GUILayout.BeginHorizontal();
        //    GUILayout.Label(ResourceManager.Instance.GetValidName(resource.ToString()));
        //    GUILayout.EndHorizontal();
        //}
        //GUILayout.EndScrollView();
    }
}

public enum OPTIONS
{
    Position = 0,
    Rotation = 1,
    Scale = 2,
}