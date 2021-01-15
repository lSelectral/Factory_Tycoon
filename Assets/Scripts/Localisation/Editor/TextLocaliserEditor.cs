using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TextLocaliserEditorWindow : EditorWindow
{
    public static void Open(string key)
    {
        TextLocaliserEditorWindow window = new TextLocaliserEditorWindow();
        window.titleContent = new GUIContent("Localiser Window");
        window.ShowUtility();
        window.key = key;
    }

    public string key;
    public string value;

    public void OnGUI()
    {
        key = EditorGUILayout.TextField("Key: ", key);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Value:", GUILayout.MaxWidth(50));

        EditorStyles.textArea.wordWrap = true;
        value = EditorGUILayout.TextArea(value, EditorStyles.textArea, GUILayout.Height(100), GUILayout.Width(400));
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Add"))
        {
            if (LocalisationSystem.GetLocalisedValue(key) != string.Empty)
            {
                LocalisationSystem.Replace(key, value);
                this.Close();
            }
            else
            {
                LocalisationSystem.Add(key, value);
            }
        }

        minSize = new Vector2(460, 250);
        maxSize = minSize;
    }
}

public class TextLocaliserSearchWindow : EditorWindow
{
    public static TextLocaliserUI localiserUI;

    public static void Open()
    {
        TextLocaliserSearchWindow window = new TextLocaliserSearchWindow();
        window.titleContent = new GUIContent("Localisation Search");

        Vector2 mouse = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
        Rect r = new Rect(mouse.x - 450, mouse.y + 10, 10, 10);
        window.ShowAsDropDown(r, new Vector2(500, 300));
    }

    public string value = "";
    public Vector2 scroll;
    public Dictionary<string, string> dictionary;

    private void OnEnable()
    {
        dictionary = LocalisationSystem.GetDictionaryForEditor();
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginHorizontal("Box");
        EditorGUILayout.LabelField("Search: ", EditorStyles.boldLabel);
        value = EditorGUILayout.TextField(value);
        EditorGUILayout.EndHorizontal();

        GetSearchResults();
    }

    void GetSearchResults()
    {
        EditorGUIUtility.SetIconSize(new Vector2(16, 16));

        if (value == null) return;

        EditorGUILayout.BeginVertical();
        scroll = EditorGUILayout.BeginScrollView(scroll);
        foreach (var element in dictionary)
        {
            if (element.Key.ToLower().Contains(value.ToLower()) || element.Value.ToLower().Contains(value.ToLower()))
            {
                CreateSearchPanelValues(element);
            }
        }
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    void CreateSearchPanelValues(KeyValuePair<string,string> element)
    {
        EditorGUILayout.BeginHorizontal("box");

        Texture selectIcon = (Texture)Resources.Load("select");
        GUIContent selectContent = new GUIContent(selectIcon, "Selected object will applied as new id");
        GUIContent selectButtonTooltip = new GUIContent("Select Object", "Selected id will be applied");
        if (GUILayout.Button(selectContent, GUILayout.MaxHeight(24), GUILayout.MaxWidth(24) ))
        {
            if (localiserUI != null)
            {
                localiserUI.localisedString = new LocalisedString(element.Key);
                //EditorUtility.DisplayDialog(localiserUI.name + " value will change", "Are you sure", "I'm sure")
                this.Close();
            }
        }

        Texture closeIcon = (Texture)Resources.Load("close");
        GUIContent content = new GUIContent(closeIcon, "Delete this id");

        if (GUILayout.Button(content, GUILayout.MaxHeight(24), GUILayout.MaxWidth(24)))
        {
            if (EditorUtility.DisplayDialog("Remove Key " + element.Key + "?", "This will remove the element from localisation, are you sure?", "Accept", "Cancel"))
            {
                LocalisationSystem.Remove(element.Key);
                AssetDatabase.Refresh();
                LocalisationSystem.Init();
                dictionary = LocalisationSystem.GetDictionaryForEditor();
            }
        }

        EditorGUILayout.TextField(element.Key);
        EditorGUILayout.LabelField(element.Value);
        EditorGUILayout.EndHorizontal();
    }
}
