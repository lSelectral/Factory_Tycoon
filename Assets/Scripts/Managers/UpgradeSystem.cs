using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UpgradeSystem : Singleton<UpgradeSystem>
{
    //[Range(1, 10)] public float timeSlider = 1f;
    public Transform upgradePanel;
    private Dictionary<BaseResources, float> speedUpDictionary;

    // Set production speed for specific unit
    public KeyValuePair<BaseResources,float> SpeedUpDictionaryValue
    {
        set
        {
            speedUpDictionary[value.Key] = value.Value;
            ProductionManager.Instance.GetProductionUnitFromResource(value.Key).CollectTime /= value.Value;
        }
    }

    // Upgrade panel components
    TextMeshProUGUI headerText;
    TextMeshProUGUI levelUpText;
    Image fillBar;
    TextMeshProUGUI levelText;
    TextMeshProUGUI outputAmountText;
    TextMeshProUGUI collectTimeText;
    TextMeshProUGUI pricePerProductText;
    Button buyBtn;
    TextMeshProUGUI buyBtnText;
    Button closeBtn; 

    Toggle multiplier1;
    Toggle multiplier5;
    Toggle multiplier20;
    Toggle multiplierMax;

    //private void Update()
    //{
    //    Time.timeScale = timeSlider;
    //}

    public int upgradeMultiplier;

    #region Events

    public class OnMiningSpeedChangedEventArgs : EventArgs
    {
        public float miningSpeed;
    }

    public event EventHandler<OnMiningSpeedChangedEventArgs> OnMiningSpeedChanged;

    public class OnMiningYieldChangedEventArgs : EventArgs
    {
        public float miningYield;
    }

    public event EventHandler<OnMiningYieldChangedEventArgs> OnMiningYieldChanged;

    public class OnProductionSpeedChangedEventArgs : EventArgs
    {
        public float productionSpeed;
    }

    public event EventHandler<OnProductionSpeedChangedEventArgs> OnProductionSpeedChanged;

    public class OnProductionYieldChangedEventArgs : EventArgs
    {
        public float productionYield;
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

    public class OnCombatPowerMultiplierChangedEventArgs : EventArgs { public float changeAmount; }

    public event EventHandler<OnCombatPowerMultiplierChangedEventArgs> OnCombatPowerMultiplierChanged;

    public event EventHandler OnDefensePowerMultiplierChanged;

    [SerializeField] private float miningSpeedMultiplier = 1;
    [SerializeField] private float miningYieldMultiplier = 1;
    [SerializeField] private float productionSpeedMultiplier = 1;
    [SerializeField] private float productionYieldMultiplier = 1;
    [SerializeField] private float productionEfficiencyMultiplier = 1;
    [SerializeField] private float earnedCoinMultiplier = 1;
    [SerializeField] private float earnedXPMultiplier = 1;

    [SerializeField] float combatPowerMultiplier = 1;
    [SerializeField] float defensePowerMultiplier = 1;

    // Set events
    public float MiningSpeedMultiplier { get => miningSpeedMultiplier;
        set { miningSpeedMultiplier = value;
            OnMiningSpeedChanged?.Invoke(this, new OnMiningSpeedChangedEventArgs() { miningSpeed = miningSpeedMultiplier }); } }
    public float MiningYieldMultiplier { get => miningYieldMultiplier;
        set { miningYieldMultiplier = value;
            OnMiningYieldChanged?.Invoke(this, new OnMiningYieldChangedEventArgs() { miningYield = miningYieldMultiplier }); } }
    public float ProductionSpeedMultiplier { get => productionSpeedMultiplier;
        set { productionSpeedMultiplier = value;
            OnProductionSpeedChanged?.Invoke(this, new OnProductionSpeedChangedEventArgs() { productionSpeed = productionSpeedMultiplier }); } }
    public float ProductionYieldMultiplier { get => productionYieldMultiplier;
        set { productionYieldMultiplier = value;
            OnProductionYieldChanged?.Invoke(this, new OnProductionYieldChangedEventArgs() { productionYield = productionYieldMultiplier }); } }
    public float ProductionEfficiencyMultiplier { get => productionEfficiencyMultiplier;
        set { productionEfficiencyMultiplier = value;
            OnProductionEfficiencyChanged?.Invoke(this, new OnProductionEfficiencyChangedEventArgs() { productionEfficiency = productionEfficiencyMultiplier }); } }
    public float EarnedCoinMultiplier { get => earnedCoinMultiplier;
        set { earnedCoinMultiplier = value;
            OnEarnedCoinMultiplierChanged?.Invoke(this, new OnEarnedCoinMultiplierChangedEventArgs() { earnedCoinMultiplier = earnedCoinMultiplier }); } }
    public float EarnedXPMultiplier { get => earnedXPMultiplier;
        set { earnedXPMultiplier = value;
            OnEarnedXpMultiplierChanged?.Invoke(this, new OnEarnedXpMultiplierChangedEventArgs() { earnedXpMultiplier = earnedXPMultiplier }); }
    }

    public float CombatPowerMultiplier
    {
        get => combatPowerMultiplier;
        set
        { //BUG Fix value relation between ResourceManager attack amount
            OnCombatPowerMultiplierChanged?.Invoke(this, new OnCombatPowerMultiplierChangedEventArgs() { changeAmount = value - combatPowerMultiplier });
            combatPowerMultiplier = value;
        }
    }

    public float DefensePowerMultiplier { get => defensePowerMultiplier;
        set { defensePowerMultiplier = value;
            OnDefensePowerMultiplierChanged?.Invoke(this, EventArgs.Empty); } }

    public float contractRewardMultiplier;
    public float contractXPMultiplier;

    public float questRewardMultiplier;
    public float questXPMultiplier;

    #endregion

    #region UPGRADE FORMULAS AND METHODS

    #region CONSTANTS

    const float PRICE_PER_PRODUCT_MULTIPLER_LOW_50 = 1.57f;

        const float UPGRADE_POWER_MULTIPLIER_LOW_50 = 1.007f;

        const float PRICE_PER_PRODUCT_MULTIPLIR_HIGH_50 = 3.7f;

        const float UPGRADE_POWER_MULTIPLIER_HIGH_50 = 1f;

        const float UPGRADE_BASE_MULTIPLIER = 1.522f;

        /// <summary>
        /// Constant used when calculating compounds price for per product.
        /// </summary>
        public float COMPOUND_PRICE_MULTIPLIER = 1.4f;

        public float INCOME_PRICE_MULTIPLIER = 1f;

        public const float STARTING_UPGRADE_COST_MULTIPLIER = 25f;

        public const float COMPOUND_STARTING_UPGRADE_COST_MULTIPLIER = 25f;

    #endregion

    public BigDouble GetNewUpgradeCost(BigDouble oldUpgradeCost, int currentLevel)
    {
        if (currentLevel <= 50)
            return oldUpgradeCost * UPGRADE_BASE_MULTIPLIER * Mathf.Pow(UPGRADE_POWER_MULTIPLIER_LOW_50, currentLevel);
        else
            return oldUpgradeCost * UPGRADE_BASE_MULTIPLIER * Mathf.Pow(UPGRADE_POWER_MULTIPLIER_HIGH_50, currentLevel);
    }

    BigDouble GetNewPricePerProduct(BigDouble oldPricePerProduct, int currentLevel)
    {
        if (currentLevel <= 50)
            return oldPricePerProduct * PRICE_PER_PRODUCT_MULTIPLER_LOW_50;
        else
            return oldPricePerProduct * PRICE_PER_PRODUCT_MULTIPLIR_HIGH_50;
    }

    public BigDouble GetNewPricePerProduct(int upgradeCount, BigDouble oldPricePerProduct, int currentLevel)
    {
        var newPricePerProduct = oldPricePerProduct;
        var newLevel = currentLevel++;
        for (int i = 0; i < upgradeCount; i++)
        {
            newLevel++;

            if (newLevel <= 50)
            {
                newPricePerProduct = GetNewPricePerProduct(newPricePerProduct, newLevel);
            }
            else if (newLevel % 3 == 0)
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
    public int GetMaximumUpgradeAmount(BigDouble oldUpgradeCost, int mineLevel)
    {
        if (oldUpgradeCost > ResourceManager.Instance.Currency) return 0;

        var currency = ResourceManager.Instance.Currency - oldUpgradeCost;
        var newUpgradeCost = oldUpgradeCost;
        BigDouble totalUpgradeCost = 0;
        var level = mineLevel++;
        int counter = 1;

        while (currency > 0)
        {
            totalUpgradeCost = GetNewUpgradeCost(newUpgradeCost, level);
            newUpgradeCost = totalUpgradeCost;
            level++;
            if (currency >= totalUpgradeCost)
            {
                currency -= totalUpgradeCost;
                counter++;
            }
            //else if (currency < totalUpgradeCost)
            //{

            //}
            else
                break;
        }
        upgradeMultiplier = counter;
        return counter;
    }

    public BigDouble GetNewUpgradeCost(int maximumUpgradeAmount, BigDouble oldUpgradeCost, int mineLevel)
    {
        BigDouble totalUpgradeCost = new BigDouble();
        totalUpgradeCost = oldUpgradeCost;
        var newUpgradeCost = oldUpgradeCost;
        var newMineLevel = mineLevel;
        if (maximumUpgradeAmount > 1)
        {
            for (int i = 0; i < maximumUpgradeAmount-1; i++)
            {
                newMineLevel++;
                Debug.Log("New mine level is: " + newMineLevel);
                newUpgradeCost = GetNewUpgradeCost(newUpgradeCost, newMineLevel);
                totalUpgradeCost += newUpgradeCost;
                Debug.Log("New upgrade cost for: " + newMineLevel.ToString() + " level is " + newUpgradeCost);
            }
        }
        return totalUpgradeCost;
    }

    #endregion

    private void Awake()
    {
        speedUpDictionary = new Dictionary<BaseResources, float>();
        foreach (BaseResources res in Enum.GetValues(typeof(BaseResources)))
        {
            // Default value 1 indicates no change in default speed
            speedUpDictionary.Add(res, 1);
        }

        headerText = upgradePanel.Find("Header").GetComponent<TextMeshProUGUI>();
        levelUpText = upgradePanel.Find("LevelUp_Text").GetComponent<TextMeshProUGUI>();
        fillBar = upgradePanel.Find("Outline").GetChild(0).GetComponent<Image>();
        var propertiesPanel = upgradePanel.Find("Properties");
        levelText = propertiesPanel.Find("Level").GetChild(1).GetComponent<TextMeshProUGUI>();
        outputAmountText = propertiesPanel.Find("Output_Amount").GetChild(1).GetComponent<TextMeshProUGUI>();
        collectTimeText = propertiesPanel.Find("CollectTime").GetChild(1).GetComponent<TextMeshProUGUI>();
        pricePerProductText = propertiesPanel.Find("PricePerProduct").GetChild(1).GetComponent<TextMeshProUGUI>();
        buyBtn = upgradePanel.Find("BuyBtn").GetComponent<Button>();
        buyBtnText = buyBtn.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        closeBtn = upgradePanel.Find("CloseBtn").GetComponent<Button>();

        multiplier1 = upgradePanel.Find("Multiplier_Panel").GetChild(0).GetComponent<Toggle>();
        multiplier5 = upgradePanel.Find("Multiplier_Panel").GetChild(1).GetComponent<Toggle>();
        multiplier20 = upgradePanel.Find("Multiplier_Panel").GetChild(2).GetComponent<Toggle>();
        multiplierMax = upgradePanel.Find("Multiplier_Panel").GetChild(3).GetComponent<Toggle>();
    }

    public (int newLevel, long newOutputValue, float newCollectTime, BigDouble newPricePerProduct, BigDouble newUpgradeCost) 
        SetUpgradePanel(int levelUpgradeMultiplier, long outputValue, int oldLevel, float collectTime, 
        BigDouble pricePerProduct, BigDouble upgradeCost, string name)
    {
        int newLevel = oldLevel + levelUpgradeMultiplier;
        long newOutputValue = GetNewOutputAmount(levelUpgradeMultiplier, outputValue, oldLevel);
        float newCollectTime = GetNewCollectTime(levelUpgradeMultiplier, collectTime);
        BigDouble newPricePerProduct = GetNewPricePerProduct(levelUpgradeMultiplier, pricePerProduct, oldLevel);
        BigDouble newUpgradeCost = GetNewUpgradeCost(levelUpgradeMultiplier, upgradeCost, oldLevel);

        headerText.text = string.Format("<color=red>{0}</color> Level {1}", name, oldLevel);
        buyBtnText.text = string.Format("BUY ${0}", newUpgradeCost.ToString());

        buyBtn.interactable = (newUpgradeCost <= ResourceManager.Instance.Currency);

        if (newLevel % 5 == 0)
            levelUpText.text = "New contract will be offered";
        else if (newLevel % 10 == 0)
            levelUpText.text = "Output amount will increase";
        else if (newLevel % 3 == 0)
            levelUpText.text = "Price per product will increase";

        levelText.text = string.Format("LEVEL\n{0} <color=green>--> {1}</color>", oldLevel, newLevel);
        outputAmountText.text = string.Format("Output\n{0} <color=green>--> {1}</color>", ResourceManager.Instance.CurrencyToString(outputValue), 
            ResourceManager.Instance.CurrencyToString(newOutputValue));
        collectTimeText.text = string.Format("Collect Time\n{0} <color=green>--> {1}</color>", ResourceManager.Instance.CurrencyToString(collectTime), 
            ResourceManager.Instance.CurrencyToString(newCollectTime));
        pricePerProductText.text = string.Format("Price Per Product\n{0} <color=green>--> {1}</color>", (pricePerProduct), (newPricePerProduct));

        return (newLevel, newOutputValue, newCollectTime, newPricePerProduct, newUpgradeCost);
    }


    // isUpgradePanelActive bool is used for updating upgrade panel when currency increased
    public void ShowUpgradePanel(UnityAction<int> SetUpgradePanel, UnityAction UpgradeMine, bool isUpgradePanelActive, BigDouble upgradeCost, int level)
    {
        UnityAction<int> tempAction = SetUpgradePanel;
        
        tempAction(upgradeMultiplier);

        isUpgradePanelActive = true;
        closeBtn.onClick.RemoveAllListeners();
        closeBtn.onClick.AddListener(() => isUpgradePanelActive = false);
        upgradePanel.gameObject.SetActive(true);

        multiplier1.onValueChanged.RemoveAllListeners();
        multiplier5.onValueChanged.RemoveAllListeners();
        multiplier20.onValueChanged.RemoveAllListeners();
        multiplierMax.onValueChanged.RemoveAllListeners();

        multiplier1.onValueChanged.AddListener((bool value) => { if (value) { tempAction(1); upgradeMultiplier = 1; } });
        multiplier5.onValueChanged.AddListener((bool value) => { if (value) { tempAction(5); upgradeMultiplier = 1; } });
        multiplier20.onValueChanged.AddListener((bool value) => { if (value) { tempAction(20); upgradeMultiplier = 1; } });
        multiplierMax.onValueChanged.AddListener((bool value) => { if (value) { tempAction(GetMaximumUpgradeAmount(upgradeCost, level)); } });

        buyBtn.onClick.RemoveAllListeners();
        buyBtn.onClick.AddListener(() =>
        {
            UpgradeMine();
            tempAction(upgradeMultiplier);
        });
    }
}