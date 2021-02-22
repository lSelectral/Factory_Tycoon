using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;
[CanEditMultipleObjects]
[CustomEditor(typeof(ScriptableMine))]
public class ScriptableMineEditorDrawer : Editor
{
    SerializedProperty _description;
    SerializedProperty _translatedName;
    SerializedProperty _product;
    SerializedProperty _itemTypes;
    SerializedProperty _foodAmount;
    SerializedProperty _housingAmount;
    SerializedProperty _attackAmount;
    SerializedProperty _collectTime;
    SerializedProperty _outputValue;

    SerializedProperty _recipes;
    SerializedProperty _ageBelongsTo;
    SerializedProperty _tier;

    SerializedProperty _basePricePerProduct;
    SerializedProperty _pricePerProduct;
    SerializedProperty _pricePerProductText;
    SerializedProperty _pricePerProductExpText;
    SerializedProperty _incomePerSecond;
    SerializedProperty _incomePerSecondText;

    SerializedProperty _unlockLevel;
    SerializedProperty xpAmount;
    SerializedProperty pricePerProductionUntilLevel50;

    SerializedProperty _toolImage;
    SerializedProperty _sourceImage;
    SerializedProperty _iconImage;

    bool foldoutForMoney = true;
    bool foldoutForImages = true;
    BigDouble lastPricePerProduct;
    bool isOptimal;
    bool isValidate;

    private void OnEnable()
    {
        _description = serializedObject.FindProperty("Description");
        _translatedName = serializedObject.FindProperty("TranslatedName");
        _product = serializedObject.FindProperty("product");
        _itemTypes = serializedObject.FindProperty("itemTypes");
        _foodAmount = serializedObject.FindProperty("foodAmount");
        _housingAmount = serializedObject.FindProperty("housingAmount");
        _attackAmount = serializedObject.FindProperty("attackAmount");
        _collectTime = serializedObject.FindProperty("collectTime");
        _outputValue = serializedObject.FindProperty("outputValue");
        _recipes = serializedObject.FindProperty("recipes");
        _ageBelongsTo = serializedObject.FindProperty("ageBelongsTo");
        _tier = serializedObject.FindProperty("tier");

        _basePricePerProduct = serializedObject.FindProperty("basePricePerProduct");
        _pricePerProduct = serializedObject.FindProperty("pricePerProduct");
        _pricePerProductText = serializedObject.FindProperty("pricePerProductText");
        _pricePerProductExpText = serializedObject.FindProperty("pricePerProductExpText");
        _incomePerSecond = serializedObject.FindProperty("incomePerSecond");
        _incomePerSecondText = serializedObject.FindProperty("incomePerSecondText");

        _unlockLevel = serializedObject.FindProperty("unlockLevel");
        xpAmount = serializedObject.FindProperty("xpAmount");
        pricePerProductionUntilLevel50 = serializedObject.FindProperty("pricePerProductUntilLevel50");

        _toolImage = serializedObject.FindProperty("toolImage");
        _sourceImage = serializedObject.FindProperty("sourceImage");
        _iconImage = serializedObject.FindProperty("icon");
    }

    public override void OnInspectorGUI()
    {
        //DrawDefaultInspector();

        // Load the real class values into the serialized copy
        serializedObject.Update();
        EditorGUILayout.PropertyField(_description);
        EditorGUILayout.PropertyField(_translatedName, new GUIContent("Translated Name:"));

        EditorGUILayout.PropertyField(_product);
        EditorGUILayout.PropertyField(_itemTypes);

        List<ItemType> itemList = new List<ItemType>();
        for (int i = 0; i < _itemTypes.arraySize; i++)
        {
            var el = _itemTypes.GetArrayElementAtIndex(i);
            itemList.Add((ItemType)el.enumValueIndex);
        }

        if (itemList.Contains(ItemType.food))
            EditorGUILayout.PropertyField(_foodAmount);
        if (itemList.Contains(ItemType.warItem))
            EditorGUILayout.PropertyField(_attackAmount);
        if (itemList.Contains(ItemType.housing))
            EditorGUILayout.PropertyField(_housingAmount);

        EditorGUILayout.PropertyField(_collectTime, new GUIContent("Collect Time", (Texture)Resources.Load("clock")));
        EditorGUILayout.PropertyField(_outputValue, new GUIContent("Output Value", (Texture)Resources.Load("Dice")));
        EditorGUILayout.PropertyField(_recipes);

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider); // Seperator


        var foldOutStyle = new GUIStyle() { fontStyle = FontStyle.Bold, richText = true, wordWrap = true };
        var richTextStyle = new GUIStyle() { richText = true, wordWrap = true };

        foldoutForMoney = EditorGUILayout.Foldout(foldoutForMoney, "<color=red>MONEY</color>", foldOutStyle);
        if (foldoutForMoney)
        {
            EditorGUILayout.PropertyField(_basePricePerProduct, true);
            //EditorGUILayout.PropertyField(_pricePerProduct, true);
            EditorGUILayout.PropertyField(_pricePerProductExpText);
            EditorGUILayout.PropertyField(_pricePerProductText);
            //EditorGUILayout.PropertyField(_incomePerSecond, true);
            EditorGUILayout.PropertyField(_incomePerSecondText, true);
        }
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider); // Seperator

        EditorGUILayout.PropertyField(_unlockLevel);
        EditorGUILayout.PropertyField(xpAmount);
        EditorGUILayout.PropertyField(pricePerProductionUntilLevel50);

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider); // Seperator


        foldoutForImages = EditorGUILayout.Foldout(foldoutForImages, "<color=red>IMAGES</color>", foldOutStyle);
        if (foldoutForImages)
        {
            EditorGUILayout.PropertyField(_toolImage);
            EditorGUILayout.PropertyField(_sourceImage);
            EditorGUILayout.PropertyField(_iconImage);
        }
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider); // Seperator


        serializedObject.ApplyModifiedProperties();
    }

    private void OnValidate()
    {

    }
}
