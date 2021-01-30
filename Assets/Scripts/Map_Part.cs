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
    BNum attackPower;
    BNum defensePower;
    BNum foodAmount;
    BNum moneyAmount;
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
    public int CountryLevel { get => countryLevel; set { countryLevel = value; MapManager.Instance.SetupInfoPanel(MapManager.Instance.clickedMapPart); } }
    public Age CurrentAgeOfNation { get => currentAgeOfNation; set => currentAgeOfNation = value; }
    public BNum AttackPower { get => attackPower; set { attackPower = value; MapManager.Instance.SetupInfoPanel(MapManager.Instance.clickedMapPart); } }
    public BNum FoodAmount { get => foodAmount; set => foodAmount = value; }
    public BNum MoneyAmount { get => moneyAmount; set => moneyAmount = value; }
    public BNum DefensePower { get => defensePower; set { defensePower = value; MapManager.Instance.SetupInfoPanel(MapManager.Instance.clickedMapPart); }}
    public int CombatLives { get => combatLives; set { combatLives = Mathf.Clamp(value,0,3); } }
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
                    if (!resourceValueDict.ContainsKey(scriptableMines[i].product))
                        resourceValueDict.Add(scriptableMines[i].product, 100);
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
            currentAgeOfNation = Age._5_ModernAge;
            attackPower = new BNum(countryLevel * 15 * Random.Range(4, 24),0);
            defensePower = new BNum(countryLevel * 10 * Random.Range(4, 14),0);
            foodAmount = new BNum(countryLevel * 9 * Random.Range(8, 45),0);
            moneyAmount = new BNum(Mathf.Pow(countryLevel, Random.Range(1, 9)),0);
            combatLives = 3;
            connectedMapParts = new Map_Part[] { this };
            //resourceValueDict = ResourceManager.Instance.resourceValueDict;
            resourceValueDict = new Dictionary<BaseResources, long>();
            var scriptableMines = ProductionManager.Instance.mineList;
            var scriptableCompounds = ProductionManager.Instance.compoundList;
            for (int i = 0; i < scriptableMines.Count; i++)
            {
                if (scriptableMines[i].ageBelongsTo == currentAgeOfNation)
                {
                    if (!resourceValueDict.ContainsKey(scriptableMines[i].product))
                        resourceValueDict.Add(scriptableMines[i].product, 100);
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
            image = GetComponent<Image>();
            image.alphaHitTestMinimumThreshold = alphaSlider;
        }

        UpgradeSystem.Instance.OnCombatPowerMultiplierChanged += OnCombatPowerMultiplierChanged;
        UpgradeSystem.Instance.OnDefensePowerMultiplierChanged += OnDefensePowerMultiplierChanged;
    }

    private void OnDefensePowerMultiplierChanged(object sender, UpgradeSystem.OnDefensePowerMultiplierChangedEventArgs e)
    {
        if (isPlayerOwned)
        {
            DefensePower = (defensePower * e.defensePowerMultiplier);
        }
    }

    private void OnCombatPowerMultiplierChanged(object sender, UpgradeSystem.OnCombatPowerMultiplierChangedEventArgs e)
    {
        if (isPlayerOwned)
        {
            AttackPower = (attackPower * e.combatPowerMultiplier);
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
            var res = scriptableMine.product;
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
                var earnedResource = Mathf.CeilToInt(scriptableCompound.outputValue * 1f/ scriptableCompound.collectTime);
                resourceValueDict[res] += earnedResource;
            }
        }
        moneyAmount += 500;
        MapManager.Instance.SetupInfoPanel(MapManager.Instance.clickedMapPart);
        MapManager.Instance.SetupResourcePanel(MapManager.Instance.clickedMapPart);
    }
}