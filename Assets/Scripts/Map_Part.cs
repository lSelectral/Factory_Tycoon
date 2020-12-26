using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using Random = UnityEngine.Random;

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

    [SerializeField] int pixelSize;

    [SerializeField] Map_Part[] connectedMapParts;

    Dictionary<BaseResources, long> resourceValueDict;

    [Range(0f, 1f)] public float alphaSlider;
    Image image;

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
    #endregion

    Dictionary<ScriptableMine, int> mineLevelDict;
    Dictionary<ScriptableCompound, int> compoundLevelDict;

    private void Awake()
    {
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

        countryName = "SELECTRA";
        countryLevel = 133;
        currentAgeOfNation = Age._3_warAge;
        attackPower = countryLevel * 15 * UnityEngine.Random.Range(4, 24);
        defensePower = countryLevel * 10 * UnityEngine.Random.Range(4, 14);
        foodAmount = countryLevel * 9 * UnityEngine.Random.Range(8, 45);
        moneyAmount = Mathf.Pow(countryLevel, UnityEngine.Random.Range(1, 9));
        combatLives = 3;
        connectedMapParts = new Map_Part[] { this };
        resourceValueDict = ResourceManager.Instance.resourceValueDict;
        image = GetComponent<Image>();
        image.alphaHitTestMinimumThreshold = alphaSlider;

        var pixels = image.sprite.texture.GetPixels32();
        foreach (var pixel in pixels)
        {
            if (pixel.a > 1)
                pixelSize++;
        }

        while (true)
        {
            StartCoroutine(CollectAndUpgradeResource(5f));
        }
    }

    private void Update()
    {
        
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Mouse on the" + name);
        MapManager.Instance.OnMapClick(this);
    }

    IEnumerator CollectAndUpgradeResource(float refreshTime)
    {
        // Add money to country at random rate based on player earning
        MoneyAmount += StatSystem.Instance.CurrencyPerSecond * (Random.Range(.7f,1.3f)) * refreshTime;

        List<long> playerResourceAmounts = new List<long>(ResourceManager.Instance.resourceValueDict.Values);
        List<BaseResources> playerResourceTypes = new List<BaseResources>(ResourceManager.Instance.resourceValueDict.Keys);

        resourceValueDict.Clear();

        for (int i = 0; i < playerResourceTypes.Count; i++)
        {
            resourceValueDict.Add(playerResourceTypes[i], (long)(playerResourceAmounts[i] * Random.Range(.7f, 1.57f)) );
        }

        

        yield return new WaitForSeconds(refreshTime);
    }
}