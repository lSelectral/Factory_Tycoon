using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class SaveSystem : Singleton<SaveSystem>
{
    public float totalEarnedCurrency;
    public float totalEarnedPremiumCurrency;
    public float totalProducedResource;
    public float totalSpendedCurrency;
    public float totalSpendedPremiumCurrency;
    public float totalConsumedResource;

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

    public void Save()
    {
        foreach (var mine in ProductionManager.Instance.instantiatedMines)
        {
            var mineSave = new MineSave
            {
                isAutomated = mine.GetComponent<Mine_Btn>().IsAutomated,
                mineLevel = mine.GetComponent<Mine_Btn>().MineLevel,
                remainedChargeTime = mine.GetComponent<Mine_Btn>().RemainedCollectTime,
                upgradeAmount = mine.GetComponent<Mine_Btn>().UpgradeAmount
            };
            mines.Add(mineSave);
        }

        foreach (var compound in ProductionManager.Instance.instantiatedCompounds)
        {
            var compoundSave = new CompoundSave
            {
                compoundLevel = compound.GetComponent<Compounds>().CompoundLevel,
                isAutomated = compound.GetComponent<Compounds>().IsAutomated,
                remainedChargeTime = compound.GetComponent<Compounds>().RemainedCollectTime,
                upgradeAmount = compound.GetComponent<Compounds>().UpgradeAmount,
                requiredResources = compound.GetComponent<Compounds>().RemainedResources
            };
            compounds.Add(compoundSave);
        }
        Debug.Log(DateTime.Now);
        SaveObject saveObject = new SaveObject()
        {
            currentLevel = GameManager.Instance.CurrentLevel,
            currentXP = GameManager.Instance.CurrentXP,
            requiredXPforNextLevel = GameManager.Instance.RequiredXPforNextLevel,

            instantiatedCompounds = compounds,
            instantiatedMines = mines,
            
            aiChip = ResourceManager.Instance.AiChip,
            circuitBoard = ResourceManager.Instance.CircuitBoard,
            coal = ResourceManager.Instance.Coal,
            copperIngot = ResourceManager.Instance.CopperIngot,
            copperOre = ResourceManager.Instance.CopperOre,
            currency = ResourceManager.Instance.Currency,
            fiberOpticCable = ResourceManager.Instance.FiberOpticCable,
            hardenedPlate = ResourceManager.Instance.HardenedPlate,
            integrationChip = ResourceManager.Instance.IntegrationChip,
            ironIngot = ResourceManager.Instance.IronIngot,
            ironOre = ResourceManager.Instance.IronOre,
            ironRod = ResourceManager.Instance.IronRod,
            ironScrew = ResourceManager.Instance.IronScrew,
            lastExitTime = DateTime.Now,
            motor = ResourceManager.Instance.Motor,
            oil = ResourceManager.Instance.Oil,
            premiumCurrency = ResourceManager.Instance.PremiumCurrency,
            rotor = ResourceManager.Instance.Rotor,
            rubber = ResourceManager.Instance.Rubber,
            siliconOre = ResourceManager.Instance.SiliconOre,
            siliconWafer = ResourceManager.Instance.SiliconWafer,
            stator = ResourceManager.Instance.Stator,
            steelBeam = ResourceManager.Instance.SteelBeam,
            steelIngot = ResourceManager.Instance.SteelIngot,
            steelPlate = ResourceManager.Instance.SteelPlate,
            steelRod = ResourceManager.Instance.SteelTube,
            steelScrew = ResourceManager.Instance.SteelScrew,
            wire = ResourceManager.Instance.Wire,
            totalResource = ResourceManager.Instance.TotalResource,

            totalEarnedCurrency = this.totalEarnedCurrency,
            totalConsumedResource = this.totalConsumedResource,
            totalEarnedPremiumCurrency = this.totalEarnedPremiumCurrency,
            totalProducedResource = this.totalProducedResource,
            totalSpendedCurrency = this.totalSpendedCurrency,
            totalSpendedPremiumCurrency = this.totalSpendedPremiumCurrency,
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
            Debug.Log(string.Format("You were out of game for: {0} minutes and {1:00} seconds",(int)totalExitTime/60,(int)totalExitTime%60));

            float idleEarnedCurrency = 0;
            float idleEarnedResource = 0;

            var savedMineInfos = ProductionManager.Instance.instantiatedMines.Zip(saveObject.instantiatedMines, (mines, infos) => (Mine: mines, Info: infos));
            var savedCompoundInfos = ProductionManager.Instance.instantiatedCompounds.Zip(saveObject.instantiatedCompounds, (compound, info) => ( Compound: compound, Info: info ));

            foreach (var m in savedMineInfos)
            {
                var mine = m.Mine.GetComponent<Mine_Btn>();
                mine.MineLevel = m.Info.mineLevel;
                mine.IsAutomated = m.Info.isAutomated;
                mine.UpgradeAmount = m.Info.upgradeAmount;
                //mine.CalculateValues();

                // Implement idle earning
                for (int i = 0; i < (int)((totalExitTime + m.Info.remainedChargeTime) / (mine.CollectTime)); i++)
                {
                    float currency, resource;
                    mine.AddResourceAndMoney(out currency, out resource);
                    idleEarnedCurrency += currency;
                    idleEarnedResource += resource;
                }

                mine.RemainedCollectTime = ((totalExitTime+m.Info.remainedChargeTime)%mine.CollectTime);

                if (m.Info.remainedChargeTime > 0)
                {
                    Debug.Log("Mine collecting");
                    m.Mine.GetComponent<Mine_Btn>().CollectMine();
                }
            }
            Debug.Log("Idle earned currency is: " +idleEarnedCurrency);
            //foreach (var c in savedCompoundInfos)
            //{
            //    c.Compound.GetComponent<Compounds>().RemainedCollectTime = c.Info.remainedChargeTime;
            //    c.Compound.GetComponent<Compounds>().CompoundLevel = c.Info.compoundLevel;
            //    c.Compound.GetComponent<Compounds>().IsAutomated = c.Info.isAutomated;
            //    c.Compound.GetComponent<Compounds>().UpgradeAmount = c.Info.upgradeAmount;
            //    c.Compound.GetComponent<Compounds>().IncomeAmount = c.Info.incomeAmount;
            //    c.Compound.GetComponent<Compounds>().RemainedResources = c.Info.requiredResources;
            //}


            GameManager.Instance.CurrentXP = saveObject.currentXP;
            GameManager.Instance.RequiredXPforNextLevel = saveObject.requiredXPforNextLevel;
            GameManager.Instance.CurrentLevel = saveObject.currentLevel;
            ResourceManager.Instance.isLoadingFromSaveFile = true;
            ResourceManager.Instance.Currency = saveObject.currency;
            ResourceManager.Instance.isLoadingFromSaveFile = true;
            ResourceManager.Instance.PremiumCurrency = saveObject.premiumCurrency;
            //ResourceManager.Instance.TotalResource = saveObject.totalResource;

            this.totalEarnedCurrency = saveObject.totalEarnedCurrency;
            this.totalConsumedResource = saveObject.totalConsumedResource;
            this.totalEarnedPremiumCurrency = saveObject.totalEarnedPremiumCurrency;
            this.totalProducedResource = saveObject.totalProducedResource;
            this.totalSpendedCurrency = saveObject.totalSpendedCurrency;
            this.totalSpendedPremiumCurrency = saveObject.totalSpendedPremiumCurrency;

            ResourceManager.Instance.IronOre = saveObject.ironOre;
            ResourceManager.Instance.CopperOre = saveObject.copperOre;
            ResourceManager.Instance.SiliconOre = saveObject.siliconOre;

        }
    }

    private void OnApplicationQuit()
    {
        if (isLoadAtStart)
            Save();
    }

    [Serializable]
    public class SaveObject
    {
        public DateTime lastExitTime;

        public List<MineSave> instantiatedMines;
        public List<CompoundSave> instantiatedCompounds;

        public float currentXP;
        public long requiredXPforNextLevel;
        public int currentLevel;

        public float currency;
        public float premiumCurrency;
        public float totalResource;

        public float totalEarnedCurrency;
        public float totalEarnedPremiumCurrency;
        public float totalProducedResource;

        public float totalSpendedCurrency;
        public float totalSpendedPremiumCurrency;
        public float totalConsumedResource;

        #region Resources
        public long ironOre;
        public long copperOre;
        public long siliconOre;
        public long coal;
        public long oil;

        public long ironIngot;
        public long copperIngot;
        public long siliconWafer;
        public long ironRod;
        public long ironScrew;

        public long wire;
        public long hardenedPlate;
        public long rotor;
        public long steelIngot;
        public long steelPlate;
        public long steelRod;
        public long steelScrew;
        public long steelBeam;
        public long rubber;
        public long stator;
        public long motor;
        public long fiberOpticCable;
        public long circuitBoard;
        public long integrationChip;
        public long aiChip;
        #endregion
    }
}

[Serializable]
public class MineSave
{
    public float chargeTime;
    public float remainedChargeTime;
    public float upgradeAmount;
    public int mineLevel;
    public bool isAutomated;
}

[Serializable]
public class CompoundSave
{
    public float remainedChargeTime;
    public float upgradeAmount;
    public int compoundLevel;
    public bool isAutomated;
    public List<BaseResources> requiredResources;
}