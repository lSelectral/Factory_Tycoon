using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UpgradeSystem : Singleton<UpgradeSystem>
{
    [Range(1, 10)] public float timeSlider = 1f;
    public GameObject upgradePanel;
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

    private void Update()
    {
        Time.timeScale = timeSlider;
    }

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

    public event EventHandler OnCombatPowerMultiplierChanged;

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
        {
            combatPowerMultiplier = value;
            OnCombatPowerMultiplierChanged?.Invoke(this, EventArgs.Empty);
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
        //var level = mineLevel++;
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
                //Debug.Log("New mine level is: " + newMineLevel);
                newUpgradeCost = GetNewUpgradeCost(newUpgradeCost, newMineLevel);
                totalUpgradeCost += newUpgradeCost;
                //Debug.Log("New upgrade cost for: " + newMineLevel.ToString() + " level is " + newUpgradeCost);
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
    }

    void Start()
    {
        var multiplierPanel = upgradePanel.transform.Find("Multiplier_Panel");

        var btn1 = multiplierPanel.GetChild(0).GetComponent<Button>();
        var btn2 = multiplierPanel.GetChild(1).GetComponent<Button>();
        var btn3 = multiplierPanel.GetChild(2).GetComponent<Button>();
        var btn4 = multiplierPanel.GetChild(3).GetComponent<Button>();

        // I had to write this cursed code for fucking bug of unity.
        btn1.onClick.AddListener(() => { btn1.GetComponent<Image>().color = Color.green; 
            ChangeButtonColor(btn2); ChangeButtonColor(btn3); ChangeButtonColor(btn4); });
        btn2.onClick.AddListener(() => { btn2.GetComponent<Image>().color = Color.green; 
            ChangeButtonColor(btn1); ChangeButtonColor(btn3); ChangeButtonColor(btn4); });
        btn3.onClick.AddListener(() => { btn3.GetComponent<Image>().color = Color.green; 
            ChangeButtonColor(btn1); ChangeButtonColor(btn2); ChangeButtonColor(btn4); });
        btn4.onClick.AddListener(() => { btn4.GetComponent<Image>().color = Color.green; 
            ChangeButtonColor(btn1); ChangeButtonColor(btn2); ChangeButtonColor(btn3); });
    }

    void ChangeButtonColor(Button btn)
    {
        btn.GetComponent<Image>().color = Color.white;
    }

    /// <summary>
    /// Setup the upgrade panel infos according to upgrade multiplier
    /// </summary>
    /// <param name="levelUpgradeMultiplier">How many level will be upgraded. If 0 max upgrade will be applied</param>
    public void SetUpgradePanel(int levelUpgradeMultiplier, long outputValue, int mineLevel, float collectTime, 
        BigDouble pricePerProduct, BigDouble upgradeCost, string name, bool hideBottomPanel = true)
    {
        var newLevel = mineLevel + levelUpgradeMultiplier;
        var newOutputValue = GetNewOutputAmount(levelUpgradeMultiplier, outputValue, mineLevel);
        var newCollectTime = GetNewCollectTime(levelUpgradeMultiplier, collectTime);
        var newPricePerProduct = GetNewPricePerProduct(levelUpgradeMultiplier, pricePerProduct, mineLevel);
        var newUpgradeCost = GetNewUpgradeCost(levelUpgradeMultiplier, upgradeCost, mineLevel);

        var panel = upgradePanel.transform;
        panel.Find("Header").GetComponent<TextMeshProUGUI>().text = string.Format("<color=red>{0}</color> Level {1}", name, mineLevel);
        var levelUpText = panel.Find("LevelUp_Text").GetComponent<TextMeshProUGUI>();

        panel.transform.Find("BuyBtn").GetComponentInChildren<TextMeshProUGUI>().text = string.Format("BUY ${0}", newUpgradeCost.ToString());

        if (newUpgradeCost > ResourceManager.Instance.Currency)
            panel.transform.Find("BuyBtn").GetComponent<Button>().interactable = false;
        else
            panel.transform.Find("BuyBtn").GetComponent<Button>().interactable = true;
        
        if (newLevel % 5 == 0)
            levelUpText.text = "New contract will be offered";
        else if (newLevel % 10 == 0)
            levelUpText.text = "Output amount will increase";
        else if (newLevel % 3 == 0)
            levelUpText.text = "Price per product will increase";

        var propertiesPanel = panel.Find("Properties");
        // We don't need bottom panel for mine objects
        if (hideBottomPanel)
            (propertiesPanel.Find("Bottom")).gameObject.SetActive(false);
        else
            propertiesPanel.Find("Bottom").gameObject.SetActive(true);

        propertiesPanel.Find("Top").Find("Level").GetComponentInChildren<TextMeshProUGUI>().text =
            string.Format("LEVEL\n{0} <color=green>--> {1}</color>", mineLevel, newLevel);
         
        propertiesPanel.Find("Top").Find("Output_Amount").GetComponentInChildren<TextMeshProUGUI>().text =
            string.Format("Output\n{0} <color=green>--> {1}</color>", ResourceManager.Instance.CurrencyToString(outputValue), 
            ResourceManager.Instance.CurrencyToString(newOutputValue));

        propertiesPanel.Find("Mid").Find("CollectTime").GetComponentInChildren<TextMeshProUGUI>().text =
            string.Format("Collect Time\n{0} <color=green>--> {1}</color>", ResourceManager.Instance.CurrencyToString(collectTime), 
            ResourceManager.Instance.CurrencyToString(newCollectTime));

        propertiesPanel.Find("Mid").Find("PricePerProduct").GetComponentInChildren<TextMeshProUGUI>().text =
            string.Format("Price Per Product\n{0} <color=green>--> {1}</color>", (pricePerProduct), (newPricePerProduct));

        if (!hideBottomPanel)
        {
            propertiesPanel.Find("Bottom").Find("Input Amount").GetComponentInChildren<TextMeshProUGUI>().text =
                string.Format("");
            //propertiesPanel.Find("Bottom").Find("IncomePerSecond").GetComponentInChildren<TextMeshProUGUI>().text =
            //    string.Format("");
        }
    }

    public void ShowUpgradePanel(UnityAction<int> SetUpgradePanel, UnityAction UpgradeMine, bool isUpgradePanelActive, BigDouble upgradeCost, int level)
    {
        UnityAction<int> tempAction = SetUpgradePanel;

        isUpgradePanelActive = true;
        var panel = upgradePanel;
        panel.transform.Find("CloseBtn").GetComponent<Button>().onClick.RemoveAllListeners();
        panel.transform.Find("CloseBtn").GetComponent<Button>().onClick.AddListener(() => isUpgradePanelActive = false);
        panel.SetActive(true);
        upgradeMultiplier = 1;
        tempAction(1);

        var multiplierPanel = panel.transform.Find("Multiplier_Panel");

        var btn1 = multiplierPanel.GetChild(0).GetComponent<Button>(); // 1x Buy
        var btn2 = multiplierPanel.GetChild(1).GetComponent<Button>(); // 5x Buy
        var btn3 = multiplierPanel.GetChild(2).GetComponent<Button>(); // 20x Buy
        var btn4 = multiplierPanel.GetChild(3).GetComponent<Button>(); // Maximum Buy

        btn1.onClick.RemoveAllListeners();
        btn2.onClick.RemoveAllListeners();
        btn3.onClick.RemoveAllListeners();
        btn4.onClick.RemoveAllListeners();

        btn1.onClick.AddListener(() => { tempAction(1); });
        btn2.onClick.AddListener(() => { tempAction(5); });
        btn3.onClick.AddListener(() => { tempAction(20); });
        btn4.onClick.AddListener(() => { tempAction(GetMaximumUpgradeAmount(upgradeCost, level)); });

        panel.transform.Find("BuyBtn").GetComponent<Button>().onClick.RemoveAllListeners();
        panel.transform.Find("BuyBtn").GetComponent<Button>().onClick.AddListener(() =>
        {
            UpgradeMine();
            tempAction(UpgradeSystem.Instance.upgradeMultiplier);
        });
    }
}