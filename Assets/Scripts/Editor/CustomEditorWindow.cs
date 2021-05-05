﻿using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class CustomEditorWindow : EditorWindow
{
    [MenuItem("Window/DEBUGGER")]
    public static void ShowWindow()
    {
        GetWindow<CustomEditorWindow>("DebugWindow");
    }

    [SearchableEnum] BaseResources resource = BaseResources._0_stone;
    [SearchableEnum] WorkerType workerType = WorkerType.Standard;

    string resourceAmountString = "Add Resource Amount";

    string xpString = "Enter XP value";
    string currencyString = "Enter Currency Amount";

    float c1 = 0f;
    float c2 = 0f;

    private void OnGUI()
    {
        GUILayout.BeginVertical();
        resourceAmountString = EditorGUILayout.TextField("Resource Amout", resourceAmountString);
        GUILayout.Label("Add Resource", EditorStyles.boldLabel);

        resource = (BaseResources)EditorGUI.EnumPopup(new Rect(5, 80, 380, 20), "Select Resource", resource);

        if (GUILayout.Button("+" + resourceAmountString + " " + ResourceManager.Instance.GetValidName(resource.ToString()) ))
        {
            ResourceManager.Instance.AddResource(resource, new BigDouble(long.Parse(resourceAmountString),0));
        }
        GUILayout.EndVertical();

        xpString = EditorGUILayout.TextField("XP Amount", xpString);

        if (GUILayout.Button("Add XP"))
        {
            GameManager.Instance.AddXP(float.Parse(xpString));
        }

        GUILayout.Space(10);

        currencyString = EditorGUILayout.TextField("Currency Amount", currencyString);
        if (GUILayout.Button("Add Currency"))
        {
            ResourceManager.Instance.Currency += BigDouble.Parse(currencyString);
        }

        // SAVE and LOAD
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

        if (GUILayout.Button("Add Units To Production Manager"))
        {
            var assets = Resources.LoadAll("AGES");
            var tempList = new List<ScriptableProductionBase>();

            for (int i = 0; i < assets.Length; i++)
            {
                if (assets[i] as ScriptableProductionBase != null)
                {
                    tempList.Add(assets[i] as ScriptableProductionBase);
                }
            }
            ProductionManager.Instance.scriptableProductionUnitList = tempList.ToArray();
        }

        if (GUILayout.Button("Add Contracts To Contract Manager"))
        {
            var assets = Resources.LoadAll("Contracts");
            var tempList = new List<ContractBase>();

            for (int i = 0; i < assets.Length; i++)
            {
                tempList.Add(assets[i] as ContractBase);
            }
            ContractManager.Instance.contracts = tempList.ToArray();
        }

        if (GUILayout.Button("Add Quests To Quest Manager"))
        {
            var assets = Resources.LoadAll("Quests");
            var tempList = new List<QuestBase>();
            for (int i = 0; i < assets.Length; i++)
            {
                tempList.Add(assets[i] as QuestBase);
            }
            QuestManager.Instance.questBases = tempList.ToArray();
        }

        // Automatically create automation contracts
        //if (GUILayout.Button("Create Automation Contracts"))
        //{
        //foreach (ContractBase contract in ContractManager.Instance.CreateAutomationContracts())
        //{
        //    AssetDatabase.CreateAsset(contract, "Assets/Resources/Contracts/Automation_Contracts/" + contract.contractName + "_automation" + ".asset");
        //    AssetDatabase.SaveAssets();
        //}
        //}

        //c1 = EditorGUILayout.Slider("COMPOUND_PRICE_MULTIPLIER", c1, 1, 7, GUILayout.MinHeight(10f));
        //c2 = EditorGUILayout.Slider("INCOME_PRICE_MULTIPLIER", c2, 1, 7, GUILayout.MinHeight(10f));

        //UpgradeSystem.Instance.COMPOUND_PRICE_MULTIPLIER = c1;
        //UpgradeSystem.Instance.INCOME_PRICE_MULTIPLIER = c2;

        //if (GUILayout.Button("Set Pixel Size of Maps"))
        //    SetPixelSizeAmount();

        //if (GUILayout.Button("Increase Combat Power"))
        //    UpgradeSystem.Instance.CombatPowerMultiplier *= 2;

        //if (GUILayout.Button("Add Assets to Helper Class"))
        //{
        //    var assets = Resources.LoadAll("AGES");

        //    List<ScriptableProductionBase> tempList = new List<ScriptableProductionBase>();
        //    for (int i = 0; i < assets.Length; i++)
        //    {
        //        tempList.Add(assets[i] as ScriptableProductionBase);
        //    }
        //    HelperMethods.scriptableProductionBases = tempList.ToArray();
        //}
        EditorGUILayout.BeginVertical();
        workerType = (WorkerType)EditorGUI.EnumPopup(new Rect(5, 330, 380, 20), "Select Worker", workerType);
        if (GUILayout.Button("ADD WORKER"))
        {
            UpgradeSystem.Instance.AddWorker(workerType, 10);
        }

        if (GUILayout.Button("Add Parent Node"))
        {
            ResearchManager.Instance.CreateNode(ResearchManager.Instance.testPosition);
        }

        EditorGUILayout.EndVertical();
    }

    void CreateUiImage()
    {
        var assets = Resources.LoadAll("MAP_SLICES");

        for (int i = 0; i < assets.Length; i++)
        {
            var asset = assets[i] as Sprite;
            if (asset != null && asset.name != "MAP_MAIN")
            {
                var obj = new GameObject("Part_" + i);
                obj.transform.SetParent(MapManager.Instance.mapTransform);
                obj.transform.localPosition = new Vector2(.5f, .5f);
                obj.AddComponent<Image>();
                obj.GetComponent<Image>().sprite = asset;
                var rect = obj.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(.5f,.5f);
                rect.anchorMax = new Vector2(.5f, .5f);
                obj.transform.localScale = Vector3.one;
                obj.GetComponent<Image>().SetNativeSize();
            }
        }
    }

    void SetAnchor()
    {
        //for (int i = 0; i < MapManager.Instance.mapTransform.childCount; i++)
        //{
            var obj = MapManager.Instance.mapTransform.GetChild(1);
            var rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(rect.rect.xMin, rect.rect.yMin);
            rect.anchorMax = new Vector2(rect.rect.xMax, rect.rect.yMax);
        //}
    }


    void SetPixelSizeAmount()
    {
        int pixelSize = 0;
        for (int i = 0; i < MapManager.Instance.mapTransform.childCount; i++)
        {
            Transform m = MapManager.Instance.mapTransform.GetChild(i);

            var pixels = m.GetComponent<Image>().sprite.texture.GetPixels32();
            foreach (var pixel in pixels)
            {
                if (pixel.a > 1)
                    pixelSize++;
            }
            m.GetComponent<Map_Part>().pixelSize = pixelSize;
            pixelSize = 0;
        }
    }
}