using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;
[CanEditMultipleObjects]
[CustomEditor(typeof(ContractBase))]
public class ScriptableContractCreatorWindow : Editor
{
    SerializedObject so;

    SerializedProperty contractName; 
    SerializedProperty description;
    SerializedProperty contractType; 

    SerializedProperty contractReward;
    SerializedProperty contractRewardType;
    SerializedProperty resourceToRewarded;

    SerializedProperty dependentContracts;
    SerializedProperty productsToUnlock;
    SerializedProperty requiredResources;
    SerializedProperty requiredResourceAmounts;

    SerializedProperty unlockLevel;
    SerializedProperty icon;

    SerializedProperty rewardPanelHeader;
    SerializedProperty rewardPanelDescription;

    SerializedProperty mainPageToGo;
    SerializedProperty pageNameToGo;
    
    SerializedProperty xpReward;

    SerializedProperty ageBelongsTo;
    SerializedProperty history;

    private void OnEnable()
    {
        so = serializedObject;

        contractName = so.FindProperty("contractName");
        description = so.FindProperty("description");
        contractType = so.FindProperty("contractType");
         
        contractReward = so.FindProperty("contractReward");
        contractRewardType = so.FindProperty("contractRewardType");
        resourceToRewarded = so.FindProperty("resourceToRewarded");

        dependentContracts = so.FindProperty("dependentContracts");
        productsToUnlock = so.FindProperty("productsToUnlock");
        requiredResources = so.FindProperty("requiredResources");
        requiredResourceAmounts = so.FindProperty("requiredResourceAmounts");

        unlockLevel = so.FindProperty("unlockLevel");
        icon = so.FindProperty("icon");

        rewardPanelHeader = so.FindProperty("rewardPanelHeader");
        rewardPanelDescription = so.FindProperty("rewardPanelDescription");

        mainPageToGo = so.FindProperty("mainPageToGo");
        pageNameToGo = so.FindProperty("pageNameToGo");
        xpReward = so.FindProperty("xpReward");

        ageBelongsTo = so.FindProperty("ageBelongsTo");
        history = so.FindProperty("history");
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.PropertyField(contractName);
        EditorGUILayout.PropertyField(description);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider); // Seperator
        EditorGUILayout.PropertyField(contractType);
        var contractTypeValue = (ContractType)contractType.enumValueIndex;

        EditorGUILayout.PropertyField(contractReward);
        EditorGUILayout.PropertyField(contractRewardType);
        var contractRewardTypeValue = (ContractRewardType)contractRewardType.enumValueIndex;

        if (contractRewardTypeValue == ContractRewardType.unitSpeedUp)
            EditorGUILayout.PropertyField(resourceToRewarded);

        EditorGUILayout.PropertyField(dependentContracts, true);

        if (contractRewardTypeValue == ContractRewardType.unlockProductionUnit)
            EditorGUILayout.PropertyField(productsToUnlock, true);

        EditorGUILayout.PropertyField(requiredResources, true);
        EditorGUILayout.PropertyField(requiredResourceAmounts, true);

        if (requiredResources.arraySize == 0 || requiredResourceAmounts.arraySize == 0)
            EditorGUILayout.HelpBox("Required resource type or size can't be 0", MessageType.Error, true);
        if (requiredResources.arraySize != requiredResourceAmounts.arraySize)
            EditorGUILayout.HelpBox("Resource type and amount array size can't be different", MessageType.Error, true);

        EditorGUILayout.PropertyField(unlockLevel);
        EditorGUILayout.PropertyField(icon);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider); // Seperator

        EditorGUILayout.PropertyField(rewardPanelHeader);
        EditorGUILayout.LabelField("PREVIEW");

        EditorGUILayout.PropertyField(rewardPanelDescription);
        EditorGUILayout.LabelField("PREVIEW");

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider); // Seperator
        EditorGUILayout.PropertyField(mainPageToGo);
        EditorGUILayout.PropertyField(pageNameToGo);

        bool _isPageNameToGoValid = false;
        for (int i = 0; i < ProductionManager.Instance.mainPanel.transform.childCount; i++)
        {
            var c = ProductionManager.Instance.mainPanel.transform.GetChild(i);
            if (c.name == pageNameToGo.stringValue)
            {
                _isPageNameToGoValid = true;
                break;
            }
            else
                _isPageNameToGoValid = false;
        }

        if (!_isPageNameToGoValid)
            EditorGUILayout.HelpBox("Page name is not exist.", MessageType.Error, true);

        EditorGUILayout.PropertyField(xpReward);

        EditorGUILayout.PropertyField(ageBelongsTo);
        EditorGUILayout.PropertyField(history);


        so.ApplyModifiedProperties();
    }
}
