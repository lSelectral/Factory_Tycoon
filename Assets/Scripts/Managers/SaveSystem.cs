using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

/*
 * HOW QUESTS SHOULD BE SAVED.
 * 
 * 
 * 
 * 
 */

/// --------------------------------------------------------
///        ---THINGS SHOULD BE SAVED---
///             -BaseResources ( DONE as List )
///             -Mine and Compound State ( DONE as Class )
///             -Total earned and spended values (Currency and Resource)
///             -State of contracts
///             -State of quests
///             -State of story progression
///             -Some bool values for isFirst time doing that
///             -Current level and xp state ( DONE )
///             -Exit and Enter times
///             -User Prefs (Voice, animation, notification)

/// <summary>
/// Save the state of game for loading next time
/// </summary>
public class SaveSystem : Singleton<SaveSystem>
{
    public BigDouble totalEarnedCurrency;
    public BigDouble totalEarnedPremiumCurrency;
    public BigDouble totalProducedResource;
    public BigDouble totalSpendedCurrency;
    public BigDouble totalSpendedPremiumCurrency;
    public BigDouble totalConsumedResource;

    // For checking if values are loading so some events and popups don't show up.
    internal bool isLoading;

    int totalExitTime;

    [SerializeField] List<ProductionUnitSave> savedProductionUnits;

    SaveObject saveObject;

    [SerializeField] private bool isLoadAtStart;

    private void Awake()
    {
        if (!Directory.Exists(Application.persistentDataPath + "/SAVES"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/SAVES");
        }
        if (isLoadAtStart)
            Load();

        savedProductionUnits = new List<ProductionUnitSave>();
    }

    //private void OnApplicationFocus(bool focus)
    //{
    //    if (!focus)
    //    {
    //        Time.timeScale = 0;
    //        Save();
    //    }
    //    else
    //    {
    //        Time.timeScale = 1;
    //        Load();
    //    }
    //}

    public void Save()
    {
        // Save all production buildings info
        foreach (var _unit in ProductionManager.Instance.instantiatedProductionUnits)
        {
            var unit = _unit.GetComponent<ProductionBase>();
            ProductionUnitSave unitSave = new ProductionUnitSave()
            {
                chargeTime = unit.CollectTime,
                remainedChargeTime = unit.RemainedCollectTime,
                currentLevel = unit.Level,
                isAutomated = unit.IsAutomated,
                isUnlocked = unit.IsUnlocked,
                outputAmount = unit.OutputValue,
                upgradeAmount = unit.UpgradeCost,
                workingMode = unit.WorkingMode
            };
            savedProductionUnits.Add(unitSave);
        }

        // Save resource values to a list
        List<BigDouble> resourceList = new List<BigDouble>();
        foreach (var res in ResourceManager.Instance.resourceValueDict)
            resourceList.Add(res.Value);

        List<MapSave> savedMaps = new List<MapSave>();
        //foreach (var map in MapManager.Instance.allMaps)
        //{
        //    var mapSave = new MapSave()
        //    {
        //        attackPower = map.AttackPower,
        //        countryLevel = map.CountryLevel,
        //        currentAgeOfNation = map.CurrentAgeOfNation,
        //        currentLives = map.CombatLives,
        //        defensePower = map.DefensePower,
        //        foodAmount = map.FoodAmount,
        //        moneyAmount = map.MoneyAmount,
        //        resourceAmounts = map.ResourceValueDict.Values.ToList()
        //    };
        //    savedMaps.Add(mapSave);
        //}

        // Save all game values as one single object for later json conversion
        SaveObject saveObject = new SaveObject()
        {
            resourceList = resourceList,
            instantiatedProductionUnits = savedProductionUnits,

            activatedContracts = ContractManager.Instance.activatedContracts,
            completedContracts = ContractManager.Instance.completedContracts,
            completedQuestList = QuestManager.Instance.completedQuests,
            instantiatedQuestList = QuestManager.Instance.instantiatedQuests,

            mapSaves = savedMaps,

            currency = ResourceManager.Instance.Currency,
            premiumCurrency = ResourceManager.Instance.PremiumCurrency,
            
            currentLevel = GameManager.Instance.CurrentLevel,
            currentXP = GameManager.Instance.CurrentXP,
            requiredXPforNextLevel = GameManager.Instance.RequiredXPforNextLevel,

            totalEarnedCurrency = totalEarnedCurrency,
            totalEarnedPremiumCurrency = totalEarnedPremiumCurrency,
            totalProducedResource = totalProducedResource,
            totalSpendedCurrency = totalSpendedCurrency,
            totalSpendedPremiumCurrency = totalSpendedPremiumCurrency,
            totalConsumedResource = totalConsumedResource,
            
            lastExitTime = DateTime.Now,
    };
        
        string saveText = JsonUtility.ToJson(saveObject);
        File.WriteAllText(Application.persistentDataPath + "/SAVES/save.txt", saveText);
        Debug.Log(String.Format("Game data saved to {0}", Application.persistentDataPath + "/SAVES" + "/save.txt"));
    }

    public void Load()
    {
        if (File.Exists(Application.persistentDataPath + "/SAVES/save.txt"))
        {
            isLoading = true;
            string saveText = File.ReadAllText(Application.persistentDataPath + "/SAVES/save.txt");

            saveObject = JsonUtility.FromJson<SaveObject>(saveText);

            totalExitTime = (DateTime.Now - saveObject.lastExitTime).Seconds;
            Debug.Log(string.Format("You were out of game for: {0} minutes and {1:00} seconds", (int)totalExitTime / 60, (int)totalExitTime % 60));

            BigDouble idleEarnedCurrency = new BigDouble();
            Dictionary<BaseResources, BigDouble> idleEarnedResourceDict = new Dictionary<BaseResources, BigDouble>();

            // Load all variables
            GameManager.Instance.CurrentLevel = saveObject.currentLevel;
            GameManager.Instance.CurrentXP = saveObject.currentXP;
            GameManager.Instance.RequiredXPforNextLevel = saveObject.requiredXPforNextLevel;

            ResourceManager.Instance.Currency = saveObject.currency;
            ResourceManager.Instance.PremiumCurrency = saveObject.premiumCurrency;
            ResourceManager.Instance.TotalResource = saveObject.totalResource;

            var allResourceAndAmounts = Enum.GetValues(typeof(BaseResources)).Cast<BaseResources>().Zip(saveObject.resourceList, (res, amount) => (Res: res, Amount: amount));
            foreach (var (Res, Amount) in allResourceAndAmounts)
            {
                ResourceManager.Instance.AddResource(Res, Amount);
            }

            ContractManager.Instance.activatedContracts = saveObject.activatedContracts;
            ContractManager.Instance.completedContracts = saveObject.completedContracts;

            QuestManager.Instance.completedQuests = saveObject.completedQuestList;
            QuestManager.Instance.instantiatedQuests = saveObject.instantiatedQuestList;

            ResourceManager.Instance.Currency = saveObject.currency;
            ResourceManager.Instance.PremiumCurrency = saveObject.premiumCurrency;

            GameManager.Instance.CurrentXP = saveObject.currentXP;
            GameManager.Instance.RequiredXPforNextLevel = saveObject.requiredXPforNextLevel;
            GameManager.Instance.CurrentLevel = saveObject.currentLevel;

            var savedProductionUnitInfos = ProductionManager.Instance.instantiatedProductionUnits.Zip(
                saveObject.instantiatedProductionUnits, (instance, save) => (Instance: instance, Save: save));

            foreach (var (Instance, Save) in savedProductionUnitInfos)
            {
                var unit = Instance.GetComponent<ProductionBase>();
                unit.Level = Save.currentLevel;
                unit.UpgradeCost = Save.upgradeAmount;
                unit.CollectTime = Save.chargeTime;
                unit.RemainedCollectTime = Save.remainedChargeTime;
                unit.WorkingMode = Save.workingMode;
                unit.IsUnlocked = Save.isUnlocked;
                unit.OutputValue = Save.outputAmount;
                unit.IsAutomated = Save.isAutomated;
            }

            for (int i = 0; i < ProductionManager.Instance.instantiatedProductionUnits.Count; i++)
            {
                var unit = ProductionManager.Instance.instantiatedProductionUnits[i].GetComponent<ProductionBase>();
                if (unit.GetComponent<Mine_Btn>() != null)
                {
                    if (unit.WorkingMode == WorkingMode.sell)
                        idleEarnedCurrency += unit.GetComponent<Mine_Btn>().IdleEarn(totalExitTime, true);
                    else
                        idleEarnedResourceDict[unit.ProducedResource] += unit.GetComponent<Mine_Btn>().IdleEarn(totalExitTime, false);
                }
                else   // Currently compounds, Idle production won't use any resource. But production rate is x10 slower
                {
                    if (unit.WorkingMode == WorkingMode.sell)
                        idleEarnedCurrency += unit.GetComponent<Mine_Btn>().IdleEarn(totalExitTime, true, .1f);
                    else
                        idleEarnedResourceDict[unit.ProducedResource] += unit.GetComponent<Mine_Btn>().IdleEarn(totalExitTime, false, .1f);
                }
            }

            this.totalEarnedCurrency = saveObject.totalEarnedCurrency;
            this.totalConsumedResource = saveObject.totalConsumedResource;
            this.totalEarnedPremiumCurrency = saveObject.totalEarnedPremiumCurrency;
            this.totalProducedResource = saveObject.totalProducedResource;
            this.totalSpendedCurrency = saveObject.totalSpendedCurrency;
            this.totalSpendedPremiumCurrency = saveObject.totalSpendedPremiumCurrency;

            ResourceManager.Instance.Currency += idleEarnedCurrency;

            isLoading = false;
        }
    }

    private void OnApplicationQuit()
    {
        if (isLoadAtStart)
            Save();
    }

    [Serializable]
    public class SaveObj
    {
        public Dictionary<BaseResources, long> resourceDict;
    }

    [Serializable]
    public class SaveObject
    {
        public List<BigDouble> resourceList = new List<BigDouble>();

        public DateTime firstEnterTime;
        public DateTime lastExitTime;

        public List<ProductionUnitSave> instantiatedProductionUnits;

        public List<ContractHolder> activatedContracts;
        public List<ContractHolder> completedContracts;

        public List<QuestHolder> instantiatedQuestList;
        public List<QuestBase> completedQuestList;

        public List<MapSave> mapSaves;

        public float currentXP;
        public long requiredXPforNextLevel;
        public int currentLevel;

        public BigDouble currency;
        public BigDouble premiumCurrency;
        public BigDouble totalResource;

        public BigDouble totalEarnedCurrency;
        public BigDouble totalEarnedPremiumCurrency;
        public BigDouble totalProducedResource;
        public BigDouble totalSpendedCurrency;
        public BigDouble totalSpendedPremiumCurrency;
        public BigDouble totalConsumedResource;
    }
}

[Serializable]
public class ProductionUnitSave
{
    public float chargeTime;
    public float remainedChargeTime;
    public BigDouble upgradeAmount;
    public int currentLevel;
    public bool isAutomated;
    public BigDouble outputAmount;
    public WorkingMode workingMode;
    public bool isUnlocked;
}

[Serializable]
public class MapSave
{
    public int countryLevel;
    public Age currentAgeOfNation;
    public BigDouble attackPower;
    public BigDouble defensePower;
    public BigDouble foodAmount;
    public BigDouble moneyAmount;
    public int currentLives;
    public List<BigDouble> resourceAmounts;
}