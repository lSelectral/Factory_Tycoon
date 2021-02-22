#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(Recipe))]
public class CustomListDrawer : PropertyDrawer
{
    const float _height = 50;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var inputResources = (property.FindPropertyRelative("inputResources"));
        var inputAmounts = (property.FindPropertyRelative("inputAmounts"));
        var collectTime = property.FindPropertyRelative("collectTime");
        var outputAmount = property.FindPropertyRelative("outputAmount");

        EditorGUILayout.BeginHorizontal(new GUIStyle()
        {
            alignment = TextAnchor.MiddleLeft,
            margin = new RectOffset(30, 10, 0, 0)
            ,
            stretchHeight = true,
            fontSize = 12,
            richText = true
        });

        EditorGUILayout.PropertyField(inputResources, true, GUILayout.MinWidth(350));
        EditorGUILayout.PropertyField(inputAmounts, true, GUILayout.MinWidth(350));

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal(new GUIStyle()
        {
            fontSize = 12,
            richText = true
        });

        EditorGUILayout.PropertyField(collectTime, true);
        EditorGUILayout.PropertyField(outputAmount, true);

        EditorGUILayout.EndHorizontal();
    }

}
#endif
