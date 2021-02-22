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
    BigDouble attackPower;
    BigDouble defensePower;
    BigDouble foodAmount;
    BigDouble moneyAmount;
    int combatLives;
    bool isLeaderOfCountries;
    [SerializeField] bool isPlayerOwned;

    public int pixelSize;

    /// <summary>
    /// Connected Map Parts will 
    /// </summary>
    [SerializeField] Map_Part[] connectedMapParts;

    Dictionary<BaseResources, BigDouble> resourceValueDict;

    [Range(0f, 1f)] public float alphaSlider;
    Image image;

    [SerializeField] bool isCoroutineStarted;

    public ScriptableMap scriptableMap;

    public string CountryName { get => countryName; set => countryName = value; }
    public int CountryLevel { get => countryLevel; set { countryLevel = value; MapManager.Instance.SetupInfoPanel(MapManager.Instance.clickedMapPart); } }
    public Age CurrentAgeOfNation { get => currentAgeOfNation; set => currentAgeOfNation = value; }
    public BigDouble AttackPower { get => attackPower; set { attackPower = value * UpgradeSystem.Instance.CombatPowerMultiplier; MapManager.Instance.SetupInfoPanel(MapManager.Instance.clickedMapPart); } }
    public BigDouble FoodAmount { get => foodAmount; set => foodAmount = value; }
    public BigDouble MoneyAmount { get => moneyAmount; set => moneyAmount = value; }
    public BigDouble DefensePower { get => defensePower; set { defensePower = value * UpgradeSystem.Instance.DefensePowerMultiplier; MapManager.Instance.SetupInfoPanel(MapManager.Instance.clickedMapPart); }}
    public int CombatLives { get => combatLives; set { combatLives = Mathf.Clamp(value,0,3); } }
    public Dictionary<BaseResources, BigDouble> ResourceValueDict { get => resourceValueDict; set => resourceValueDict = value; }
    public Map_Part[] ConnectedMapParts { get => connectedMapParts; set => connectedMapParts = value; }
    public bool IsPlayerOwned { get => isPlayerOwned; set => isPlayerOwned = value; }
    /// <summary>
    /// This bool indicates if this country is the leader of all connected countries.
    /// </summary>
    public bool IsLeaderOfCountries { get => isLeaderOfCountries; set => isLeaderOfCountries = value; }
    #endregion

    private void Awake()
    {
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
            resourceValueDict = new Dictionary<BaseResources, BigDouble>();

            for (int i = 0; i < ProductionManager.Instance.scriptableProductionUnitList.Length; i++)
            {
                var scriptableUnit = ProductionManager.Instance.scriptableProductionUnitList[i];
                if (!resourceValueDict.ContainsKey(scriptableUnit.product))
                    resourceValueDict.Add(scriptableUnit.product, new BigDouble(1, 2));
            }

            // Add extra resource for every tribe if has one. Resource amount is related to area size of country
            resourceValueDict[scriptableMap.specificResource] += (pixelSize / 2); 
        }
        else
        {
            countryName = "SELECTRA";
            countryLevel = 133;
            currentAgeOfNation = Age._5_ModernAge;
            attackPower = new BigDouble(countryLevel * 15 * Random.Range(4, 24),0);
            defensePower = new BigDouble(countryLevel * 10 * Random.Range(4, 14),0);
            foodAmount = new BigDouble(countryLevel * 9 * Random.Range(8, 45),0);
            moneyAmount = new BigDouble(Mathf.Pow(countryLevel, Random.Range(1, 9)),0);
            combatLives = 3;
            connectedMapParts = new Map_Part[] { this };
            resourceValueDict = new Dictionary<BaseResources, BigDouble>();

            for (int i = 0; i < ProductionManager.Instance.scriptableProductionUnitList.Length; i++)
            {
                var scriptableUnit = ProductionManager.Instance.scriptableProductionUnitList[i];
                if (!resourceValueDict.ContainsKey(scriptableUnit.product))
                    resourceValueDict.Add(scriptableUnit.product, new BigDouble(1, 2));
            }

            image = GetComponent<Image>();
            image.alphaHitTestMinimumThreshold = alphaSlider;
        }

        UpgradeSystem.Instance.OnCombatPowerMultiplierChanged += OnCombatPowerMultiplierChanged;
        UpgradeSystem.Instance.OnDefensePowerMultiplierChanged += OnDefensePowerMultiplierChanged;
    }

    private void Start()
    {
        MapManager.Instance.allMaps.Add(this);
        if (MapManager.Instance.playerCurrentMapPart.ToList().Contains(this))
            isPlayerOwned = true;
    }

    private void OnCombatPowerMultiplierChanged(object sender, EventArgs e)
    {
        if (isPlayerOwned)
            AttackPower = attackPower;
    }

    private void OnDefensePowerMultiplierChanged(object sender, EventArgs e)
    {
        if (isPlayerOwned)
            DefensePower = defensePower;
    }


    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Clicked to the" + name);
        MapManager.Instance.OnMapClick(this);
    }

    void CountryProduction()
    {
        for (int i = 0; i < resourceValueDict.Keys.Count; i++)
        {
            resourceValueDict[resourceValueDict.Keys.ElementAt(i)] += 1;
        }

        moneyAmount += 500;
        MapManager.Instance.SetupInfoPanel(MapManager.Instance.clickedMapPart);
        MapManager.Instance.SetupResourcePanel(MapManager.Instance.clickedMapPart);
    }
}