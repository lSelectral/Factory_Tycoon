using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// Save the state of game for loading next time
/// </summary>
/// <remarks>
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
///             -User Prefs
/// --------------------------------------------------------
/// </remarks>
public class SaveSystem : Singleton<SaveSystem>
{
    public double totalEarnedCurrency;
    public double totalEarnedPremiumCurrency;
    public double totalProducedResource;
    public double totalSpendedCurrency;
    public double totalSpendedPremiumCurrency;
    public double totalConsumedResource;

    int totalExitTime;

    public List<MineSave> mines;
    public List<CompoundSave> compounds;

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

        mines = new List<MineSave>();
        compounds = new List<CompoundSave>();
    }

    private void Start()
    {
        
    }

    public void Save()
    {
        #region Mine and Compound Save
        foreach (var _mine in ProductionManager.Instance.instantiatedMines)
        {
            var mine = _mine.GetComponent<Mine_Btn>();
            MineSave mineSave = new MineSave()
            {
                chargeTime = mine.CollectTime,
                isAutomated = mine.IsAutomated,
                mineLevel = mine.MineLevel,
                remainedChargeTime = mine.RemainedCollectTime,
                upgradeAmount = mine.UpgradeCost,
                workingMode = mine.WorkingMode,
                outputAmount = mine.OutputValue,
                isLockedByContract = mine.IsLockedByContract,
            };
            mines.Add(mineSave);
        }
        foreach (var _compound in ProductionManager.Instance.instantiatedCompounds)
        {
            var c = _compound.GetComponent<Compounds>();
            CompoundSave compoundSave = new CompoundSave()
            {
                compoundLevel = c.CompoundLevel,
                isAutomated = c.IsAutomated,
                outputAmount = c.OutputValue,
                remainedChargeTime = c.RemainedBuildTime,
                requiredResources = c.RemainedResources,
                upgradeAmount = c.UpgradeCost,
                workingMode = c.WorkingMode,
                isLockedByContract = c.IsLockedByContract,
            };
            compounds.Add(compoundSave);
        }
        #endregion

        // Save resource values
        List<long> resourceList = new List<long>();
        foreach (var res in ResourceManager.Instance.resourceValueDict)
        {
            resourceList.Add(res.Value);
        }

        List<MapSave> savedMaps = new List<MapSave>();
        foreach (var map in MapManager.Instance.allMaps)
        {
            var mapSave = new MapSave()
            {
                attackPower = map.AttackPower,
                countryLevel = map.CountryLevel,
                currentAgeOfNation = map.CurrentAgeOfNation,
                currentLives = map.CombatLives,
                defensePower = map.DefensePower,
                foodAmount = map.FoodAmount,
                moneyAmount = map.MoneyAmount,
                resourceAmounts = map.ResourceValueDict.Values.ToList()
            };
            savedMaps.Add(mapSave);
        }

        // Save all game values as one single object for later json conversion
        SaveObject saveObject = new SaveObject()
        {
            instantiatedCompounds = compounds,
            instantiatedMines = mines,
            resourceList = resourceList,
            mapSaves = savedMaps,

            // Save Contract Manager Values
            contractManagerSave = new ContractManagerSave()
            {
                activatedContracts = ContractManager.Instance.activatedContracts,
                completedContracts = ContractManager.Instance.completedContracts,
                contractResources = ContractManager.Instance.tempResources,
                contracts = ContractManager.Instance.contracts,
            },

            questManagerSave = new QuestManagerSave()
            {
                completedQuestList = QuestManager.Instance.completedQuests,
                quests = QuestManager.Instance.questBases,
            },

            currentLevel = GameManager.Instance.CurrentLevel,
            currentXP = GameManager.Instance.CurrentXP,
            requiredXPforNextLevel = GameManager.Instance.RequiredXPforNextLevel,

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
            string saveText = File.ReadAllText(Application.persistentDataPath + "/SAVES/save.txt");

            saveObject = JsonUtility.FromJson<SaveObject>(saveText);

            totalExitTime = (DateTime.Now - saveObject.lastExitTime).Seconds;
            //Debug.Log(string.Format("You were out of game for: {0} minutes and {1:00} seconds",(int)totalExitTime/60,(int)totalExitTime%60));

            double idleEarnedCurrency = 0;
            double idleEarnedResource = 0;

            GameManager.Instance.CurrentXP = saveObject.currentXP;
            GameManager.Instance.RequiredXPforNextLevel = saveObject.requiredXPforNextLevel;
            GameManager.Instance.CurrentLevel = saveObject.currentLevel;
            ResourceManager.Instance.isLoadingFromSaveFile = true;
            //ResourceManager.Instance.Currency = saveObject.currency;
            ResourceManager.Instance.isLoadingFromSaveFile = true;
            //ResourceManager.Instance.PremiumCurrency = saveObject.premiumCurrency;

            //this.totalEarnedCurrency = saveObject.totalEarnedCurrency;
            //this.totalConsumedResource = saveObject.totalConsumedResource;
            //this.totalEarnedPremiumCurrency = saveObject.totalEarnedPremiumCurrency;
            //this.totalProducedResource = saveObject.totalProducedResource;
            //this.totalSpendedCurrency = saveObject.totalSpendedCurrency;
            //this.totalSpendedPremiumCurrency = saveObject.totalSpendedPremiumCurrency;

            //ResourceManager.Instance.TotalResource = saveObject.totalResource;

            /*      ---------------------------  BASE_RESOURCE enum  and resource dictionary should be in same order ---------------------------     */
            var allResourceAndAmounts = Enum.GetValues(typeof(BaseResources)).Cast<BaseResources>().Zip(saveObject.resourceList, (res, amount) => (Res: res, Amount: amount));

            foreach (var resource in allResourceAndAmounts)
            {
                ResourceManager.Instance.AddResource(resource.Res,resource.Amount, true);
            }

            var savedMineInfos = ProductionManager.Instance.instantiatedMines.Zip(saveObject.instantiatedMines, (mines, infos) => (Mine: mines, Info: infos));
            var savedCompoundInfos = ProductionManager.Instance.instantiatedCompounds.Zip(saveObject.instantiatedCompounds, (compound, info) => (Compound: compound, Info: info));

            foreach (var m in savedMineInfos)
            {
                var mine = m.Mine.GetComponent<Mine_Btn>();
                mine.MineLevel = m.Info.mineLevel;
                mine.UpgradeCost = m.Info.upgradeAmount;
                mine.CollectTime = m.Info.chargeTime;
                mine.WorkingMode = m.Info.workingMode;
                mine.RemainedCollectTime = m.Info.remainedChargeTime;
                mine.IsLockedByContract = m.Info.isLockedByContract;

                if (m.Info.remainedChargeTime > 0)
                {
                    m.Mine.GetComponent<Mine_Btn>().CollectMine();
                }
                mine.IsAutomated = m.Info.isAutomated;
                mine.CollectMine();
            }

            //foreach (var m in savedMineInfos)
            //{
            //    var mine = m.Mine.GetComponent<Mine_Btn>();
            //    mine.MineLevel = m.Info.mineLevel;
            //    mine.UpgradeCost = m.Info.upgradeAmount;
            //    mine.CollectTime = m.Info.chargeTime;
            //    mine.WorkingMode = m.Info.workingMode;

            //    // Implement idle earning
            //    //for (int i = 0; i < (int)((totalExitTime + m.Info.remainedChargeTime) / (mine.CollectTime)); i++)
            //    //{

            //    //    //float currency, resource;
            //    //    //mine.AddResourceAndMoney(out currency, out resource);
            //    //    //idleEarnedCurrency += currency;
            //    //    //idleEarnedResource += resource;
            //    //}

            //    mine.RemainedCollectTime = ((totalExitTime + m.Info.remainedChargeTime) % mine.CollectTime);

            //    if (m.Info.remainedChargeTime > 0)
            //    {
            //        m.Mine.GetComponent<Mine_Btn>().CollectMine();
            //    }
            //    mine.IsAutomated = m.Info.isAutomated;
            //    mine.CollectMine();
            //}
            //Debug.Log("Idle earned currency is: " + idleEarnedCurrency);

            //foreach (var c in savedCompoundInfos)
            //{
            //    c.Compound.GetComponent<Compounds>().RemainedCollectTime = c.Info.remainedChargeTime;
            //    c.Compound.GetComponent<Compounds>().CompoundLevel = c.Info.compoundLevel;
            //    c.Compound.GetComponent<Compounds>().IsAutomated = c.Info.isAutomated;
            //    c.Compound.GetComponent<Compounds>().UpgradeAmount = c.Info.upgradeAmount;
            //    c.Compound.GetComponent<Compounds>().IncomeAmount = c.Info.incomeAmount;
            //    c.Compound.GetComponent<Compounds>().RemainedResources = c.Info.requiredResources;
            //}
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
        public List<long> resourceList = new List<long>();

        public DateTime firstEnterTime;
        public DateTime lastExitTime;

        public List<MineSave> instantiatedMines;
        public List<CompoundSave> instantiatedCompounds;

        public List<MapSave> mapSaves;

        public List<ContractSave> savedContracts;

        public ContractManagerSave contractManagerSave;
        public QuestManagerSave questManagerSave;

        public float currentXP;
        public long requiredXPforNextLevel;
        public int currentLevel;

        //public double currency;
        //public double premiumCurrency;
        //public double totalResource;

        public double totalEarnedCurrency;
        public double totalEarnedPremiumCurrency;
        public double totalProducedResource;
        public double totalSpendedCurrency;
        public double totalSpendedPremiumCurrency;
        public double totalConsumedResource;
    }
}

[Serializable]
public class MineSave
{
    public float chargeTime;
    public float remainedChargeTime;
    public double upgradeAmount;
    public int mineLevel;
    public bool isAutomated;
    public long outputAmount;
    public MineWorkingMode workingMode;
    public bool isLockedByContract;
}

[Serializable]
public class CompoundSave
{
    public float remainedChargeTime;
    public double upgradeAmount;
    public int compoundLevel;
    public bool isAutomated;
    public long outputAmount;
    public List<BaseResources> requiredResources;
    public CompoundWorkingMode workingMode;
    public bool isLockedByContract;
}

[Serializable]
public class MapSave
{
    public int countryLevel;
    public Age currentAgeOfNation;
    public long attackPower;
    public long defensePower;
    public long foodAmount;
    public double moneyAmount;
    public int currentLives;
    public List<long> resourceAmounts;
}

[Serializable]
public class ContractManagerSave
{
    public ContractBase[] contracts;
    public List<ContractBase> activatedContracts;
    public List<ContractBase> completedContracts;
    public List<List<BaseResources>> contractResources;
}

[Serializable]
public class ContractSave
{
    public ContractBase contractBase;
    public bool isContractCompleted;
    public BaseResources[] remainedRequiredResources;
}

[Serializable]
public class QuestManagerSave
{
    public QuestBase[] quests;
    public List<QuestBase> completedQuestList;
}

[Serializable]
public class QuestSave
{
    public QuestBase questBase;
}