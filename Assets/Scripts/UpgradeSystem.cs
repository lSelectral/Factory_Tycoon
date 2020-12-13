using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeSystem : Singleton<UpgradeSystem>
{
    public GameObject upgradePanel;

    public int upgradeMultiplier;

    #region Events

    public class OnMiningSpeedChangedEventArgs: EventArgs
    {
        public float miningSpeed;
    }

    public event EventHandler<OnMiningSpeedChangedEventArgs> OnMiningSpeedChanged;

    public class OnMiningYieldChangedEventArgs : EventArgs
    {
        public long miningYield;
    }

    public event EventHandler<OnMiningYieldChangedEventArgs> OnMiningYieldChanged;

    public class OnProductionSpeedChangedEventArgs : EventArgs
    {
        public float productionSpeed;
    }

    public event EventHandler<OnProductionSpeedChangedEventArgs> OnProductionSpeedChanged;

    public class OnProductionYieldChangedEventArgs : EventArgs
    {
        public long productionYield;
    }

    public event EventHandler<OnProductionYieldChangedEventArgs> OnProductionYieldChanged;

    public class OnProductionEfficiencyChangedEventArgs : EventArgs
    {
        public float productionEfficiency;
    }

    public event EventHandler<OnProductionEfficiencyChangedEventArgs> OnProductionEfficiencyChanged;

    public class OnEarnedCoinMultiplierChangedEventArgs : EventArgs
    {
        public float earnedCoinMultiplier;
    }

    public event EventHandler<OnEarnedCoinMultiplierChangedEventArgs> OnEarnedCoinMultiplierChanged;

    public class OnEarnedXpMultiplierChangedEventArgs : EventArgs
    {
        public float earnedXpMultiplier;
    }

    public event EventHandler<OnEarnedXpMultiplierChangedEventArgs> OnEarnedXpMultiplierChanged;

    

    [SerializeField] private float miningSpeedMultiplier = 1;
    [SerializeField] private long miningYieldMultiplier = 1;
    [SerializeField] private float productionSpeedMultiplier = 1;
    [SerializeField] private long productionYieldMultiplier = 1;
    [SerializeField] private float productionEfficiencyMultiplier = 1;
    [SerializeField] private float earnedCoinMultiplier = 1;
    [SerializeField] private float earnedXPMultiplier = 1;

    // Set events
    public float MiningSpeedMultiplier { get => miningSpeedMultiplier; 
        set { miningSpeedMultiplier = value; 
            OnMiningSpeedChanged(this, new OnMiningSpeedChangedEventArgs() { miningSpeed = miningSpeedMultiplier }); } }
    public long MiningYieldMultiplier { get => miningYieldMultiplier; 
        set { miningYieldMultiplier = value; 
            OnMiningYieldChanged(this, new OnMiningYieldChangedEventArgs() { miningYield = miningYieldMultiplier }); } }
    public float ProductionSpeedMultiplier { get => productionSpeedMultiplier; 
        set { productionSpeedMultiplier = value; 
            OnProductionSpeedChanged(this, new OnProductionSpeedChangedEventArgs() { productionSpeed = productionSpeedMultiplier }); } }
    public long ProductionYieldMultiplier { get => productionYieldMultiplier; 
        set { productionYieldMultiplier = value; 
            OnProductionYieldChanged(this, new OnProductionYieldChangedEventArgs() { productionYield = productionYieldMultiplier }); } }
    public float ProductionEfficiencyMultiplier { get => productionEfficiencyMultiplier; 
        set { productionEfficiencyMultiplier = value; 
            OnProductionEfficiencyChanged(this, new OnProductionEfficiencyChangedEventArgs() { productionEfficiency = productionEfficiencyMultiplier }); } }
    public float EarnedCoinMultiplier { get => earnedCoinMultiplier; 
        set { earnedCoinMultiplier = value; 
            OnEarnedCoinMultiplierChanged(this, new OnEarnedCoinMultiplierChangedEventArgs() { earnedCoinMultiplier = earnedCoinMultiplier }); } }
    public float EarnedXPMultiplier { get => earnedXPMultiplier; 
        set { earnedXPMultiplier = value; 
            OnEarnedXpMultiplierChanged(this, new OnEarnedXpMultiplierChangedEventArgs() { earnedXpMultiplier = earnedXPMultiplier }); }
    }
    #endregion


    #region UPGRADE FORMULAS AND METHODS

        #region CONSTANTS

        const float PRICE_PER_PRODUCT_MULTIPLER_LOW_50 = 3.18f;

        const float UPGRADE_POWER_MULTIPLIER_LOW_50 = 1.003f;

        const float PRICE_PER_PRODUCT_MULTIPLIR_HIGH_50 = 3.7f;

        const float UPGRADE_POWER_MULTIPLIER_HIGH_50 = 1f;

        const float UPGRADE_BASE_MULTIPLIER = 1.63f;

        #endregion

    double GetNewUpgradeCost(double oldUpgradeCost, int currentLevel)
    {
        if (currentLevel <= 50)
            return oldUpgradeCost * UPGRADE_BASE_MULTIPLIER * Mathf.Pow(UPGRADE_POWER_MULTIPLIER_LOW_50, currentLevel);
        else
            return oldUpgradeCost * UPGRADE_BASE_MULTIPLIER * Mathf.Pow(UPGRADE_POWER_MULTIPLIER_HIGH_50, currentLevel);
    }

    float GetNewPricePerProduct(float oldPricePerProduct, int currentLevel)
    {
        if (currentLevel <= 50)
            return oldPricePerProduct * PRICE_PER_PRODUCT_MULTIPLER_LOW_50;
        else
            return oldPricePerProduct * PRICE_PER_PRODUCT_MULTIPLIR_HIGH_50;
    }

    public float GetNewPricePerProduct(int upgradeCount, float oldPricePerProduct, int currentLevel)
    {
        var newPricePerProduct = oldPricePerProduct;
        var newLevel = currentLevel++;
        for (int i = 0; i < upgradeCount; i++)
        {
            newLevel++;
            if (newLevel % 3 == 0)
                newPricePerProduct = GetNewPricePerProduct(newPricePerProduct, newLevel);
        }
        return newPricePerProduct;
    }

    public long GetNewOutputAmount(int upgradeCount, long oldOutputAmount, int currentLevel)
    {
        var newOutputAmount = oldOutputAmount;
        var newLevel = currentLevel++;

        for (int i = 0; i < upgradeCount; i++)
        {
            newLevel++;
            if (newLevel >=10 && newLevel % 10 == 0)
                newOutputAmount++;
        }
        return newOutputAmount;
    }

    public float GetNewCollectTime(int upgradeCount, float oldCollectTime)
    {
        var newCollectTime = oldCollectTime;

        for (int i = 0; i < upgradeCount; i++)
        {
            newCollectTime -= newCollectTime/100;
        }
        return newCollectTime;
    }

    // Calculate maximum upgrade for maximum buying capacity
    public int GetMaximumUpgradeAmount(double oldUpgradeCost, int mineLevel)
    {
        var currency = ResourceManager.Instance.Currency - oldUpgradeCost;
        var newUpgradeCost = oldUpgradeCost;
        var level = mineLevel++;
        int counter = 1;

        while (currency > 0)
        {
            newUpgradeCost = GetNewUpgradeCost(newUpgradeCost, mineLevel);
            if (currency >= newUpgradeCost)
            {
                currency -= newUpgradeCost;
                counter++;
            }
            else
                break;
        }
        upgradeMultiplier = counter;
        return counter;
    }

    public double GetNewUpgradeCost(int maximumUpgradeAmount, double oldUpgradeCost, int mineLevel)
    {
        double totalUpgradeCost = 0;
        totalUpgradeCost = oldUpgradeCost;
        var newUpgradeCost = oldUpgradeCost;
        var newMineLevel = mineLevel;
        if (maximumUpgradeAmount > 1)
        {
            for (int i = 0; i < maximumUpgradeAmount-1; i++)
            {
                newMineLevel++;
                newUpgradeCost = GetNewUpgradeCost(newUpgradeCost, mineLevel);
                totalUpgradeCost += newUpgradeCost;
                //Debug.Log("New upgrade cost for: " + newMineLevel.ToString() + " level is " + newUpgradeCost);
            }
        }
        return totalUpgradeCost;
    }

    #endregion

    [SerializeField] List<Button> upgradeAmountButtons;

    void Start()
    {
        var multiplierPanel = upgradePanel.transform.Find("Multiplier_Panel");

        var btn1 = multiplierPanel.GetChild(0).GetComponent<Button>();
        var btn2 = multiplierPanel.GetChild(1).GetComponent<Button>();
        var btn3 = multiplierPanel.GetChild(2).GetComponent<Button>();
        var btn4 = multiplierPanel.GetChild(3).GetComponent<Button>();

        btn1.onClick.AddListener(() => { upgradeMultiplier = 1; ChangeButtonColor(btn1); });
        btn2.onClick.AddListener(() => { upgradeMultiplier = 5; ChangeButtonColor(btn2); });
        btn3.onClick.AddListener(() => { upgradeMultiplier = 20; ChangeButtonColor(btn3); });
        // 0 is specify the maximum
        btn4.onClick.AddListener(() => { upgradeMultiplier = 0; ChangeButtonColor(btn4); });
    }

    void ChangeButtonColor(Button btn)
    {
        //Debug.Log(btn.name + "'s colour changed");
        //for (int i = 0; i < upgradeAmountButtons.Count; i++)
        //{
        //    if (btn.name == upgradeAmountButtons[i].name)
        //        btn.GetComponent<Image>().color = Color.green;
        //    else
        //        btn.GetComponent<Image>().color = Color.white;
        //}
    }
}