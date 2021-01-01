﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using Random = UnityEngine.Random;
using System.Linq;

/// <summary>
/// 
/// Incoming resource related to___
/// - Size of region
/// - Place (Some place has bonus production for specific resource, not every resource)
/// - Level of region
/// 
/// </summary>

public class Map_Part : MonoBehaviour, IPointerClickHandler
{
    #region Variable and Properties
    string countryName;
    int countryLevel;
    Age currentAgeOfNation;
    long attackPower;
    long defensePower;
    long foodAmount;
    double moneyAmount;
    int combatLives;
    [SerializeField] bool isPlayerOwned;

    public int pixelSize;

    [SerializeField] Map_Part[] connectedMapParts;

    Dictionary<BaseResources, long> resourceValueDict;

    [Range(0f, 1f)] public float alphaSlider;
    Image image;

    [SerializeField] bool isCoroutineStarted;

    public ScriptableMap scriptableMap;

    public string CountryName { get => countryName; set => countryName = value; }
    public int CountryLevel { get => countryLevel; set => countryLevel = value; }
    public Age CurrentAgeOfNation { get => currentAgeOfNation; set => currentAgeOfNation = value; }
    public long AttackPower { get => attackPower; set => attackPower = value; }
    public long FoodAmount { get => foodAmount; set => foodAmount = value; }
    public double MoneyAmount { get => moneyAmount; set => moneyAmount = value; }
    public long DefensePower { get => defensePower; set => defensePower = value; }
    public int CombatLives { get => combatLives; set => combatLives = value; }
    public Dictionary<BaseResources, long> ResourceValueDict { get => resourceValueDict; set => resourceValueDict = value; }
    public Map_Part[] ConnectedMapParts { get => connectedMapParts; set => connectedMapParts = value; }
    public bool IsPlayerOwned { get => isPlayerOwned; set => isPlayerOwned = value; }
    #endregion

    Dictionary<ScriptableMine, int> mineLevelDict;
    Dictionary<ScriptableCompound, int> compoundLevelDict;

    private void Awake()
    {
        MapManager.Instance.allMaps.Add(this);
        mineLevelDict = new Dictionary<ScriptableMine, int>();
        compoundLevelDict = new Dictionary<ScriptableCompound, int>();

        foreach (var mine in ProductionManager.Instance.mineList)
        {
            mineLevelDict.Add(mine, 1);
        }
        foreach (var compound in ProductionManager.Instance.compoundList)
        {
            compoundLevelDict.Add(compound, 1);
        }

        if (MapManager.Instance.playerCurrentMapPart.ToList().Contains(this))
            isPlayerOwned = true;

        if (scriptableMap != null)
        {
            countryName = scriptableMap.countryName;
            countryLevel = 1;
            currentAgeOfNation = scriptableMap.startAgeOfNation;
            attackPower = scriptableMap.startAttackPower;
            defensePower = scriptableMap.startDefensePower;
            foodAmount = scriptableMap.startFoodAmount;
            moneyAmount = scriptableMap.startMoneyAmount;
            countryLevel = scriptableMap.startCountryLevel;
            combatLives = 3;
            connectedMapParts = new Map_Part[] { this };
            image = GetComponent<Image>();
            image.alphaHitTestMinimumThreshold = alphaSlider;

            // Create resource dictionary with all resource in it for this age
            resourceValueDict = new Dictionary<BaseResources, long>();
            var scriptableMines = ProductionManager.Instance.mineList;
            var scriptableCompounds = ProductionManager.Instance.compoundList;
            for (int i = 0; i < scriptableMines.Count; i++)
            {
                if (scriptableMines[i].ageBelongsTo == currentAgeOfNation)
                {
                    if (!resourceValueDict.ContainsKey(scriptableMines[i].baseResource))
                        resourceValueDict.Add(scriptableMines[i].baseResource, 100);
                }
            }
            for (int i = 0; i < scriptableCompounds.Count; i++)
            {
                if (scriptableCompounds[i].ageBelongsTo == currentAgeOfNation)
                {
                    if (!resourceValueDict.ContainsKey(scriptableCompounds[i].product))
                        resourceValueDict.Add(scriptableCompounds[i].product, 100);
                }
            }
            // Add extra resource for every tribe if has one. Resource amount is related to area size of country
            resourceValueDict[scriptableMap.specificResource] += pixelSize; 
        }
        else
        {
            countryName = "SELECTRA";
            countryLevel = 133;
            currentAgeOfNation = Age._3_warAge;
            attackPower = countryLevel * 15 * Random.Range(4, 24);
            defensePower = countryLevel * 10 * Random.Range(4, 14);
            foodAmount = countryLevel * 9 * Random.Range(8, 45);
            moneyAmount = Mathf.Pow(countryLevel, Random.Range(1, 9));
            combatLives = 3;
            connectedMapParts = new Map_Part[] { this };
            resourceValueDict = ResourceManager.Instance.resourceValueDict;
            image = GetComponent<Image>();
            image.alphaHitTestMinimumThreshold = alphaSlider;
        }
    }

    private void Start()
    {
        InvokeRepeating("countryProduction", 2f, 5f);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Mouse on the" + name);
        MapManager.Instance.OnMapClick(this);
    }

    void countryProduction()
    {
        foreach (var scriptableMine in ProductionManager.Instance.mineList)
        {
            var res = scriptableMine.baseResource;
            if (resourceValueDict.ContainsKey(res))
            {
                var earnedResource = Mathf.CeilToInt(scriptableMine.outputValue * 1f / scriptableMine.collectTime) ;
                resourceValueDict[res] += earnedResource;
            }
        }

        foreach (var scriptableCompound in ProductionManager.Instance.compoundList)
        {
            var res = scriptableCompound.product;
            if (resourceValueDict.ContainsKey(res))
            {
                var earnedResource = Mathf.CeilToInt(scriptableCompound.outputValue * 1f/ scriptableCompound.buildTime);
                resourceValueDict[res] += earnedResource;
            }
        }
        moneyAmount += 500;
        MapManager.Instance.SetupInfoPanel(MapManager.Instance.clickedMapPart);
        MapManager.Instance.SetupResourcePanel(MapManager.Instance.clickedMapPart);
    }
}