using System;
using System.Collections.Generic;
using System.Linq;
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

    ProductionBase upgradePanelOwner;

    // Upgrade panel components
    TextMeshProUGUI headerText;
    //TextMeshProUGUI levelUpText;
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

    // Worker constants
    public const float STANDARD_BONUS = .05f;
    public const float GATHERER_BONUS = .1f;
    public const float CONSTRUCTOR_BONUS = .2f;
    public const float COOK_BONUS = .15f;
    public const float WARRIOR_BONUS = .2f;
    public const float MINER_BONUS = .32f;
    public const float BLACKSMITH_BONUS = .16f;
    public const float ARTIST_BONUS = .24f;
    public const float ENGINEER_BONUS = .2f;
    public const float CHEMIST_BONUS = .14f;
    public const float SCIENTIST_BONUS = .2f;

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

    public BigDouble GetNewOutputAmount(int upgradeCount, BigDouble oldOutputAmount, int currentLevel)
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


    /*
     * SET FILL as default value
     * Set dropdown and 
     * 
     * 
     */

    private void Awake()
    {
        speedUpDictionary = new Dictionary<BaseResources, float>();
        foreach (BaseResources res in Enum.GetValues(typeof(BaseResources)))
        {
            // Default value 1 indicates no change in default speed
            speedUpDictionary.Add(res, 1);
        }

        headerText = upgradePanel.Find("Header").GetComponent<TextMeshProUGUI>();
        //levelUpText = upgradePanel.Find("LevelUp_Text").GetComponent<TextMeshProUGUI>();
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

        // Set worker panel transforms
        Transform workerPanel = upgradePanel.Find("Worker_Panel");
        workerSubstractBtn = workerPanel.Find("SubstractWorker").GetComponent<Button>();
        workerAddBtn = workerPanel.Find("AddWorker").GetComponent<Button>();
        workerMaxBtn = workerPanel.Find("MaxWorker").GetComponent<Button>();
        workerInputField = workerPanel.Find("WorkerInputField").GetComponent<TMP_InputField>();
        availableWorkerCountText = workerPanel.Find("AvailableWorkerCount").GetComponent<TextMeshProUGUI>();
        collectTimeBonusText = workerPanel.Find("CollectTimeChange").GetComponent<TextMeshProUGUI>();
        outputAmountBonusText = workerPanel.Find("OutputAmountChange").GetComponent<TextMeshProUGUI>();
        maxWorkerAmountText = workerPanel.Find("MaxWorker").GetComponent<TextMeshProUGUI>();
        workerTypeDropdown = workerPanel.Find("WorkerTypeDropdown").GetComponent<TMP_Dropdown>();

        workerTypeDropdown.value = (int)WorkerType.Standard;
        workerInputField.text = "0";

        workerSubstractBtn.onClick.AddListener(() =>
        {
            workerInputField.text = (long.Parse(workerInputField.text) - 1).ToString();
            CheckConditionsForButtons();
        });
        workerAddBtn.onClick.AddListener(() =>
        {
            workerInputField.text = (long.Parse(workerInputField.text) + 1).ToString();
            CheckConditionsForButtons();
        });
        workerMaxBtn.onClick.AddListener(() =>
        {
            if (ResourceManager.Instance.availableWrokerTypeDictionary.ContainsKey(upgradePanelOwner.CurrentWorkerType) && 
            upgradePanelOwner.MaxWorkerCount <= ResourceManager.Instance.availableWrokerTypeDictionary[upgradePanelOwner.CurrentWorkerType])
                workerInputField.text = upgradePanelOwner.MaxWorkerCount.ToString();
            else
                workerInputField.text = ResourceManager.Instance.availableWrokerTypeDictionary[upgradePanelOwner.CurrentWorkerType].ToString();
            CheckConditionsForButtons();
        });

        workerInputField.onValueChanged.AddListener((string value) => 
        {
            if (value == "")
                value = "0";
            if (ResourceManager.Instance.availableWrokerTypeDictionary.ContainsKey(upgradePanelOwner.CurrentWorkerType) &&
            long.Parse(value) > ResourceManager.Instance.availableWrokerTypeDictionary[upgradePanelOwner.CurrentWorkerType])
                workerInputField.text = ResourceManager.Instance.availableWrokerTypeDictionary[upgradePanelOwner.CurrentWorkerType].ToString();
            CheckConditionsForButtons();
        });

        workerInputField.onSubmit.AddListener((string value) =>
        {
            SetWorkerBonus(long.Parse(value), workerTypeDropdown.value);
        });

        workerTypeDropdown.ClearOptions();
        foreach (var worker in Enum.GetNames(typeof(WorkerType)))
        {
            workerTypeDropdown.AddOptions(new List<TMP_Dropdown.OptionData>() { new TMP_Dropdown.OptionData(worker) });
        }

        workerTypeDropdown.onValueChanged.AddListener((int value) => 
        { 
            OnWorkerTypeChanged(value); 
            CheckConditionsForButtons(); 
            SetWorkerBonus(long.Parse(workerInputField.text), value); 
        });
    }

    public void OnInputFieldDeSelect() { if (workerInputField.text == "") workerInputField.text = "0"; }
    public void OnInputFieldEndEditing() { if (workerInputField.text == "") workerInputField.text = "0"; }

    public void OnWorkerTypeChanged(int value)
    {
        Debug.Log("OnWorkerTypeChanged event called");
        upgradePanelOwner.CurrentWorkerType = (WorkerType)value;
        Debug.Log(upgradePanelOwner.ProducedResource + " worker type has been changed to " + (WorkerType)value);
    }

    public void CheckConditionsForButtons()
    {
        workerSubstractBtn.interactable = true;
        workerAddBtn.interactable = true;
        workerMaxBtn.interactable = true;
        workerInputField.interactable = true;

        if (upgradePanelOwner.CurrentWorkerType == WorkerType.Fill || upgradePanelOwner.CurrentWorkerType == WorkerType.None)
        {
            workerSubstractBtn.interactable = false;
            workerAddBtn.interactable = false;
            workerMaxBtn.interactable = false;
            workerInputField.interactable = false;
            return;
        }

        if (workerInputField.text == "0")
            workerSubstractBtn.interactable = false;
        if (workerInputField.text == ResourceManager.Instance.availableWrokerTypeDictionary[upgradePanelOwner.CurrentWorkerType].ToString())
            workerAddBtn.interactable = false;
    }

    // TODO --- Add different kind of worker class (Gatherer, hunter, metal worker, warrior etc. Every one of them has extra power for their respected works
    Button workerSubstractBtn;
    Button workerAddBtn;
    Button workerMaxBtn;
    TMP_InputField workerInputField;
    TMP_Dropdown workerTypeDropdown;
    TextMeshProUGUI availableWorkerCountText;
    TextMeshProUGUI collectTimeBonusText; // Collect Time\t <color=green>+765</color>
    TextMeshProUGUI outputAmountBonusText; // Output Amount\t <color=green>+4856</color>
    TextMeshProUGUI maxWorkerAmountText; // Input Amount\t <color=red>-537%</color>

    public (int newLevel, BigDouble newOutputValue, float newCollectTime, BigDouble newPricePerProduct, BigDouble newUpgradeCost) 
        SetUpgradePanel(int levelUpgradeMultiplier, BigDouble outputValue, int oldLevel, float collectTime, 
        BigDouble pricePerProduct, BigDouble upgradeCost, string name)
    {
        int newLevel = oldLevel + levelUpgradeMultiplier;
        BigDouble newOutputValue = GetNewOutputAmount(levelUpgradeMultiplier, outputValue, oldLevel);
        float newCollectTime = GetNewCollectTime(levelUpgradeMultiplier, collectTime);
        BigDouble newPricePerProduct = GetNewPricePerProduct(levelUpgradeMultiplier, pricePerProduct, oldLevel);
        BigDouble newUpgradeCost = GetNewUpgradeCost(levelUpgradeMultiplier, upgradeCost, oldLevel);

        headerText.text = string.Format("<color=red>{0}</color> Level {1}", name, oldLevel);
        buyBtnText.text = string.Format("BUY ${0}", newUpgradeCost.ToString());

        buyBtn.interactable = (newUpgradeCost <= ResourceManager.Instance.Currency);

        //if (newLevel % 5 == 0)
        //    levelUpText.text = "New contract will be offered";
        //else if (newLevel % 10 == 0)
        //    levelUpText.text = "Output amount will increase";
        //else if (newLevel % 3 == 0)
        //    levelUpText.text = "Price per product will increase";

        workerInputField.text = upgradePanelOwner.CurrentWorkerCount.ToString();
        if ((WorkerType)workerTypeDropdown.value == WorkerType.None)
            availableWorkerCountText.text = "0";
        else
            availableWorkerCountText.text = ResourceManager.Instance.availableWrokerTypeDictionary[(WorkerType)workerTypeDropdown.value].ToString();
        CheckConditionsForButtons();

        levelText.text = string.Format("LEVEL\n{0} <color=green>--> {1}</color>", oldLevel, newLevel);
        outputAmountText.text = string.Format("Output\n{0} <color=green>--> {1}</color>", outputValue.ToString(), 
            newOutputValue.ToString());
        collectTimeText.text = string.Format("Collect Time\n{0} <color=green>--> {1}</color>", ResourceManager.Instance.CurrencyToString(collectTime), 
            ResourceManager.Instance.CurrencyToString(newCollectTime));
        pricePerProductText.text = string.Format("Price Per Product\n{0} <color=green>--> {1}</color>", (pricePerProduct), (newPricePerProduct));

        return (newLevel, newOutputValue, newCollectTime, newPricePerProduct, newUpgradeCost);
    }

    /*
     * Production units can have only 1 type of worker at a time.
     * None remove all workers and set workplace to passive. Disable change workmode button
     * Fill, if available fill unit with preffered workertype, else standard.
     * 
     */

    public void SetWorkerBonus(BigDouble amount, int enumIndex)
    {
        var unit = upgradePanelOwner;
        WorkerType workerType = (WorkerType)enumIndex;
        unit.IsRunning = true;

        var oldWorkerType = unit.CurrentWorkerType;
        var oldWorkerCount = unit.CurrentWorkerCount;
        // Give back all workers in the units to available dictionary for true calculation
        unit.CurrentWorkerType = WorkerType.None;
        unit.CurrentWorkerCount = 0;
        if (oldWorkerType != WorkerType.Fill && oldWorkerType != WorkerType.None)
            ResourceManager.Instance.availableWrokerTypeDictionary[oldWorkerType] += oldWorkerCount;

        var biggestWorkerType = ResourceManager.Instance.availableWrokerTypeDictionary
            .First(w => w.Value == ResourceManager.Instance.availableWrokerTypeDictionary.Values.Max());

        switch (workerType)
        {
            case WorkerType.None:
                unit.IsRunning = false;
                unit.CurrentWorkerCount = 0;
                workerInputField.text = "0";
                workerTypeDropdown.value = 0;
                break;

            case WorkerType.Fill:
                if (ResourceManager.Instance.availableWrokerTypeDictionary[unit.PrefferedWorkerType] >= unit.MaxWorkerCount)
                {
                    unit.CurrentWorkerType = unit.PrefferedWorkerType;
                    unit.CurrentWorkerCount = unit.MaxWorkerCount;
                    SetWorkerBonus(unit.MaxWorkerCount, (int)unit.PrefferedWorkerType);
                }

                else if (ResourceManager.Instance.availableWrokerTypeDictionary[WorkerType.Standard] >= unit.MaxWorkerCount)
                {
                    unit.CurrentWorkerType = WorkerType.Standard;
                    unit.CurrentWorkerCount = unit.MaxWorkerCount;
                }

                else if (biggestWorkerType.Value >= unit.MaxWorkerCount)
                {
                    unit.CurrentWorkerType = biggestWorkerType.Key;
                    unit.CurrentWorkerCount = biggestWorkerType.Value;
                }
                else if (ResourceManager.Instance.availableWrokerTypeDictionary[unit.PrefferedWorkerType] > 0)
                {
                    unit.CurrentWorkerType = unit.PrefferedWorkerType;
                    unit.CurrentWorkerCount = ResourceManager.Instance.availableWrokerTypeDictionary[unit.PrefferedWorkerType];
                }
                else
                {
                    unit.CurrentWorkerType = WorkerType.Standard;
                    unit.CurrentWorkerCount = 0;
                }
                    
                break;

            case WorkerType.Standard:
                unit.CurrentWorkerType = WorkerType.Standard;
                unit.OutputValue += STANDARD_BONUS * amount;
                break;

            case WorkerType.Gatherer:
                unit.CurrentWorkerType = WorkerType.Gatherer;
                break;
            case WorkerType.Cook:
                unit.CurrentWorkerType = WorkerType.Cook;
                unit.FoodAmount += COOK_BONUS * amount;
                break;
            case WorkerType.Warrior:
                unit.CurrentWorkerType = WorkerType.Warrior;
                break;
            case WorkerType.Miner:
                unit.CurrentWorkerType = WorkerType.Miner;
                unit.OutputValue += MINER_BONUS * amount;
                break;
            case WorkerType.Blacksmith:
                unit.CurrentWorkerType = WorkerType.Blacksmith;
                if (unit.AttackAmount > 0)
                    unit.AttackAmount += BLACKSMITH_BONUS * amount;
                else if (unit.DefenseAmount > 0)
                    unit.DefenseAmount += BLACKSMITH_BONUS * amount;
                break;
            case WorkerType.Artist:
                unit.CurrentWorkerType = WorkerType.Artist;
                unit.PricePerProduct *= (1 + ARTIST_BONUS);
                break;
            case WorkerType.Engineer:
                unit.CurrentWorkerType = WorkerType.Engineer;
                break;
            case WorkerType.Chemist:
                unit.CurrentWorkerType = WorkerType.Chemist;
                break;
            case WorkerType.Scientist:
                unit.CurrentWorkerType = WorkerType.Scientist;
                break;
        }

        if (workerType != WorkerType.Fill && workerType != WorkerType.None)
            unit.CurrentWorkerCount = amount;

        if (unit.CurrentWorkerType != WorkerType.None)
            ResourceManager.Instance.availableWrokerTypeDictionary[unit.CurrentWorkerType] -= unit.CurrentWorkerCount; // Setup available workers

        if (unit.CurrentWorkerType != WorkerType.None)
            availableWorkerCountText.text = ResourceManager.Instance.availableWrokerTypeDictionary[unit.CurrentWorkerType].ToString();

        workerInputField.text = unit.CurrentWorkerCount.ToString(); // Set ui texts
        workerTypeDropdown.value = (int)unit.CurrentWorkerType;
    }

    /*
     * Worker Count and Production Rate relationship function
     * \ln x^{33}+\frac{x}{54}+3
     * <seealso => https://www.desmos.com/calculator?lang=tr>
     */

    // isUpgradePanelActive bool is used for updating upgrade panel when currency increased
    public void ShowUpgradePanel(ProductionBase owner, UnityAction<int> SetUpgradePanel, UnityAction UpgradeMine, bool isUpgradePanelActive, BigDouble upgradeCost, int level)
    {
        upgradePanelOwner = owner;
        UnityAction<int> tempAction = SetUpgradePanel;
        
        tempAction(upgradeMultiplier);

        isUpgradePanelActive = true;
        closeBtn.onClick.RemoveAllListeners();
        closeBtn.onClick.AddListener(() => { isUpgradePanelActive = false; upgradePanelOwner = null; });
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