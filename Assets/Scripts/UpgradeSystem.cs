using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
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

        /// <summary>
        /// Constant used when calculating compounds price for per product.
        /// </summary>
        public const float COMPOUND_PRICE_MULTIPLIER = 1.49f;

        #endregion

    public double GetNewUpgradeCost(double oldUpgradeCost, int currentLevel)
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
                Debug.Log("New mine level is: " + newMineLevel);
                newUpgradeCost = GetNewUpgradeCost(newUpgradeCost, mineLevel);
                totalUpgradeCost += newUpgradeCost;
                Debug.Log("New upgrade cost for: " + newMineLevel.ToString() + " level is " + newUpgradeCost);
            }
        }
        return totalUpgradeCost;
    }

    #endregion

    void Start()
    {
        var multiplierPanel = upgradePanel.transform.Find("Multiplier_Panel");

        var btn1 = multiplierPanel.GetChild(0).GetComponent<Button>();
        var btn2 = multiplierPanel.GetChild(1).GetComponent<Button>();
        var btn3 = multiplierPanel.GetChild(2).GetComponent<Button>();
        var btn4 = multiplierPanel.GetChild(3).GetComponent<Button>();

        // I had to write this cursed code for fucking bug of unity.
        // May luck be with you
        btn1.onClick.AddListener(() => { btn1.GetComponent<Image>().color = Color.green; ChangeButtonColor(btn2); ChangeButtonColor(btn3); ChangeButtonColor(btn4); });
        btn2.onClick.AddListener(() => { btn2.GetComponent<Image>().color = Color.green; ChangeButtonColor(btn1); ChangeButtonColor(btn3); ChangeButtonColor(btn4); });
        btn3.onClick.AddListener(() => { btn3.GetComponent<Image>().color = Color.green; ChangeButtonColor(btn1); ChangeButtonColor(btn2); ChangeButtonColor(btn4); });
        btn4.onClick.AddListener(() => { btn4.GetComponent<Image>().color = Color.green; ChangeButtonColor(btn1); ChangeButtonColor(btn2); ChangeButtonColor(btn3); });
    }

    void ChangeButtonColor(Button btn)
    {
        btn.GetComponent<Image>().color = Color.white;
    }

    /// <summary>
    /// Setup the upgrade panel infos according to upgrade multiplier
    /// </summary>
    /// <param name="levelUpgradeMultiplier">How many level will be upgraded. If 0 max upgrade will be applied</param>
    public void SetUpgradePanel(int levelUpgradeMultiplier, long outputValue, int mineLevel, float collectTime, float pricePerProduct, double upgradeCost, string name, bool hideBottomPanel = true)
    {
        var newLevel = mineLevel + levelUpgradeMultiplier;
        var newOutputValue = UpgradeSystem.Instance.GetNewOutputAmount(levelUpgradeMultiplier, outputValue, mineLevel);
        var newCollectTime = UpgradeSystem.Instance.GetNewCollectTime(levelUpgradeMultiplier, collectTime);
        var newPricePerProduct = UpgradeSystem.Instance.GetNewPricePerProduct(levelUpgradeMultiplier, pricePerProduct, mineLevel);
        var newUpgradeCost = UpgradeSystem.Instance.GetNewUpgradeCost(levelUpgradeMultiplier, upgradeCost, mineLevel);

        var panel = UpgradeSystem.Instance.upgradePanel.transform;
        panel.Find("Header").GetComponent<TextMeshProUGUI>().text = string.Format("<color=red>{0}</color> Level {1}", name, mineLevel);
        var levelUpText = panel.Find("LevelUp_Text").GetComponent<TextMeshProUGUI>();

        panel.transform.Find("BuyBtn").GetComponentInChildren<TextMeshProUGUI>().text = string.Format("BUY ${0}", ResourceManager.Instance.CurrencyToString(newUpgradeCost));

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
            string.Format("Output\n{0} <color=green>--> {1}</color>", ResourceManager.Instance.CurrencyToString(outputValue), ResourceManager.Instance.CurrencyToString(newOutputValue));

        propertiesPanel.Find("Mid").Find("CollectTime").GetComponentInChildren<TextMeshProUGUI>().text =
            string.Format("Collect Time\n{0} <color=green>--> {1}</color>", ResourceManager.Instance.CurrencyToString(collectTime), ResourceManager.Instance.CurrencyToString(newCollectTime));

        propertiesPanel.Find("Mid").Find("PricePerProduct").GetComponentInChildren<TextMeshProUGUI>().text =
            string.Format("Price Per Product\n{0} <color=green>--> {1}</color>", ResourceManager.Instance.CurrencyToString(pricePerProduct), ResourceManager.Instance.CurrencyToString(newPricePerProduct));

        if (!hideBottomPanel)
        {
            propertiesPanel.Find("Bottom").Find("Input Amount").GetComponentInChildren<TextMeshProUGUI>().text =
                string.Format("");
            //propertiesPanel.Find("Bottom").Find("IncomePerSecond").GetComponentInChildren<TextMeshProUGUI>().text =
            //    string.Format("");
        }
    }

    public void ShowUpgradePanel(UnityAction<int> SetUpgradePanel, UnityAction UpgradeMine, bool isUpgradePanelActive, double upgradeCost, int level)
    {
        UnityAction<int> tempAction = SetUpgradePanel;

        isUpgradePanelActive = true;
        var panel = UpgradeSystem.Instance.upgradePanel;
        panel.transform.Find("CloseBtn").GetComponent<Button>().onClick.AddListener(() => isUpgradePanelActive = false);
        panel.SetActive(true);
        UpgradeSystem.Instance.upgradeMultiplier = 1;
        tempAction(1);

        var multiplierPanel = panel.transform.Find("Multiplier_Panel");

        var btn1 = multiplierPanel.GetChild(0).GetComponent<Button>();
        var btn2 = multiplierPanel.GetChild(1).GetComponent<Button>();
        var btn3 = multiplierPanel.GetChild(2).GetComponent<Button>();
        var btn4 = multiplierPanel.GetChild(3).GetComponent<Button>();

        btn1.onClick.AddListener(() => { tempAction(1); });
        btn2.onClick.AddListener(() => { tempAction(5); });
        btn3.onClick.AddListener(() => { tempAction(20); });
        // 0 is specify the maximum
        btn4.onClick.AddListener(() => { tempAction(GetMaximumUpgradeAmount(upgradeCost, level)); });

        panel.transform.Find("BuyBtn").GetComponent<Button>().onClick.AddListener(() =>
        {
            UpgradeMine();
            tempAction(UpgradeSystem.Instance.upgradeMultiplier);
        });
    }
}