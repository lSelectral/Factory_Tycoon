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
    public Transform upgradePanel;
    private Dictionary<BaseResources, float> speedUpDictionary;
    private BigDouble population;
    [SerializeField] TextMeshProUGUI populationText;
    public Dictionary<WorkerType, BigDouble> totalWorkertypeDictionary;
    public Dictionary<WorkerType, BigDouble> availableWorkerTypeDictionary;
    public List<ProductionBase> fillTypeProductionUnits; // Get list of fill type units for updating

    // Set production speed for specific unit
    public KeyValuePair<BaseResources,float> SpeedUpDictionaryValue
    {
        set
        {
            speedUpDictionary[value.Key] = value.Value;
            ProductionManager.Instance.GetProductionUnitFromResource(value.Key).CollectTime /= value.Value;
        }
    }

    public BigDouble Population
    {
        get { return population; }
        set
        {
            population = value;
            populationText.text = population.ToString();
            OnPopulationChangedEvent?.Invoke(this, new OnPopulationChangedEventArgs() { population = this.population });
        }
    }

    ProductionBase upgradePanelOwner;

    // Upgrade panel components
    TextMeshProUGUI headerText;
    //TextMeshProUGUI levelUpText;
    TextMeshProUGUI levelText;
    TextMeshProUGUI outputAmountText;
    TextMeshProUGUI collectTimeText;
    TextMeshProUGUI pricePerProductText;
    TextMeshProUGUI maxWorkerText;
    Button buyBtn;
    TextMeshProUGUI buyBtnText;
    Button closeBtn;

    Toggle multiplier1;
    Toggle multiplier5;
    Toggle multiplier20;
    Toggle multiplierMax;

    public int upgradeMultiplier;

    #region Events

    public class OnPopulationChangedEventArgs : EventArgs
    {
        public BigDouble population;
    }

    public event EventHandler<OnPopulationChangedEventArgs> OnPopulationChangedEvent;

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

    [SerializeField] float workerSpeedMultiplier = 1;

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

    public float WorkerSpeedMultiplier { get => workerSpeedMultiplier; set => workerSpeedMultiplier = value; }

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

    public BigDouble GetNewMaxWorkerCount(int levelUpgradeMultiplier, BigDouble oldMaxWorker, int currentLevel)
    {
        BigDouble maxWorker = oldMaxWorker;
        int newLevel = currentLevel;
        for (int i = 0; i < levelUpgradeMultiplier; i++)
        {
            maxWorker += newLevel;
            newLevel++;
        }
        return maxWorker;
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

    private void Awake()
    {
        #region Variable Initialization and Finding
        fillTypeProductionUnits = new List<ProductionBase>();
        speedUpDictionary = new Dictionary<BaseResources, float>();
        foreach (BaseResources res in Enum.GetValues(typeof(BaseResources)))
        {
            // Default value 1 indicates no change in default speed
            speedUpDictionary.Add(res, 1);
        }
        totalWorkertypeDictionary = new Dictionary<WorkerType, BigDouble>();
        availableWorkerTypeDictionary = new Dictionary<WorkerType, BigDouble>();
        var workerTypes = Enum.GetValues(typeof(WorkerType)).Cast<WorkerType>();
        foreach (WorkerType worker in workerTypes)
        {
            if (worker == WorkerType.None || worker == WorkerType.Fill) continue;
            totalWorkertypeDictionary.Add(worker, 0);
            availableWorkerTypeDictionary.Add(worker, 0);
        }

        headerText = upgradePanel.Find("Header").GetComponent<TextMeshProUGUI>();
        //levelUpText = upgradePanel.Find("LevelUp_Text").GetComponent<TextMeshProUGUI>();
        var propertiesPanel = upgradePanel.Find("Properties");
        levelText = propertiesPanel.Find("Level").GetChild(1).GetComponent<TextMeshProUGUI>();
        outputAmountText = propertiesPanel.Find("Output_Amount").GetChild(1).GetComponent<TextMeshProUGUI>();
        collectTimeText = propertiesPanel.Find("CollectTime").GetChild(1).GetComponent<TextMeshProUGUI>();
        pricePerProductText = propertiesPanel.Find("PricePerProduct").GetChild(1).GetComponent<TextMeshProUGUI>();
        maxWorkerText = propertiesPanel.Find("MaxWorker").GetChild(1).GetComponent<TextMeshProUGUI>();
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
        workerCountText = workerPanel.Find("Background").GetChild(0).GetComponent<TextMeshProUGUI>();
        workerInputField = workerPanel.Find("WorkerInputField").GetComponent<TMP_InputField>();
        availableWorkerCountText = workerPanel.Find("AvailableWorkerCount").GetComponent<TextMeshProUGUI>();
        collectTimeBonusText = workerPanel.Find("CollectTimeChange").GetComponent<TextMeshProUGUI>();
        outputAmountBonusText = workerPanel.Find("OutputAmountChange").GetComponent<TextMeshProUGUI>();
        maxWorkerAmountText = workerPanel.Find("MaxWorker").GetComponent<TextMeshProUGUI>();
        workerTypeDropdown = workerPanel.Find("WorkerTypeDropdown").GetComponent<TMP_Dropdown>();
        #endregion

        #region Worker Numerical Buttons
        workerSubstractBtn.onClick.AddListener(() =>
        {
            workerInputField.text = (long.Parse(workerInputField.text) - 1).ToString();
            CheckConditionsForButtons();
        });

        workerAddBtn.onClick.AddListener(() => {
            workerInputField.text = (long.Parse(workerInputField.text) + 1).ToString();
            CheckConditionsForButtons();
        });

        workerMaxBtn.onClick.AddListener(() =>
        {
            if (availableWorkerTypeDictionary.ContainsKey(upgradePanelOwner.CurrentWorkerType) &&
            upgradePanelOwner.MaxWorkerCount <= availableWorkerTypeDictionary[upgradePanelOwner.CurrentWorkerType])
                workerInputField.text = upgradePanelOwner.MaxWorkerCount.ToString();
            else
                workerInputField.text = availableWorkerTypeDictionary[upgradePanelOwner.CurrentWorkerType].ToString();
            CheckConditionsForButtons();
        });
        #endregion

        workerInputField.onValueChanged.AddListener((string value) => 
        {
            if (value == "")
                value = "0";
            //if (availableWorkerTypeDictionary.ContainsKey(upgradePanelOwner.CurrentWorkerType) &&
            //long.Parse(value) > availableWorkerTypeDictionary[upgradePanelOwner.CurrentWorkerType])
            //    workerInputField.text = availableWorkerTypeDictionary[upgradePanelOwner.CurrentWorkerType].ToString();
            CheckConditionsForButtons();
        });

        workerInputField.onSubmit.AddListener((string value) =>
        {
            SetWorkerBonus(long.Parse(value), workerTypeDropdown.value);
        });

        // Add worker type options to the dropdown
        workerTypeDropdown.ClearOptions();
        foreach (var worker in Enum.GetNames(typeof(WorkerType)))
        {
            workerTypeDropdown.AddOptions(new List<TMP_Dropdown.OptionData>() { new TMP_Dropdown.OptionData(worker) });
        }

        workerTypeDropdown.onValueChanged.AddListener((int value) => 
        { 
            SetWorkerBonus(long.Parse(workerInputField.text), value); 
        });
    }

    void AddWorker(WorkerType workerType, BigDouble amount)
    {
        availableWorkerTypeDictionary[workerType] += amount;
        totalWorkertypeDictionary[workerType] += amount;
        Population += amount;
    }

    void UpgradeWorker(WorkerType upgradeTo, BigDouble amount)
    {

    }

    void SetWorker(WorkerType workerType, BigDouble amount)
    {
        availableWorkerTypeDictionary[workerType] -= amount;
    }

    void FreeWorker(WorkerType workerType, BigDouble amount)
    {
        availableWorkerTypeDictionary[workerType] += amount;
    }

    public void OnInputFieldDeSelect() { if (workerInputField.text == "") workerInputField.text = "0"; }
    public void OnInputFieldEndEditing() { if (workerInputField.text == "") workerInputField.text = "0"; }

    public void CheckConditionsForButtons()
    {
        workerSubstractBtn.interactable = true;
        workerAddBtn.interactable = true;
        workerMaxBtn.interactable = true;
        workerInputField.interactable = true;

        if (upgradePanelOwner.CurrentWorkerType == WorkerType.Fill || upgradePanelOwner.CurrentWorkerType == WorkerType.None || population == 0)
        {
            workerSubstractBtn.interactable = false;
            workerAddBtn.interactable = false;
            workerMaxBtn.interactable = false;
            workerInputField.interactable = false;
            return;
        }

        if (workerInputField.text == "0")
            workerSubstractBtn.interactable = false;
        if (workerInputField.text == availableWorkerTypeDictionary[upgradePanelOwner.CurrentWorkerType].ToString())
            workerAddBtn.interactable = false;
    }

    // TODO --- Add different kind of worker class (Gatherer, hunter, metal worker, warrior etc. Every one of them has extra power for their respected works
    Button workerSubstractBtn;
    Button workerAddBtn;
    Button workerMaxBtn;
    TMP_InputField workerInputField;
    TMP_Dropdown workerTypeDropdown;
    TextMeshProUGUI availableWorkerCountText;
    TextMeshProUGUI workerCountText;
    TextMeshProUGUI collectTimeBonusText; // Collect Time\t <color=green>+765</color>
    TextMeshProUGUI outputAmountBonusText; // Output Amount\t <color=green>+4856</color>
    TextMeshProUGUI maxWorkerAmountText; // Input Amount\t <color=red>-537%</color>

    public (int newLevel, BigDouble newOutputValue, float newCollectTime, BigDouble newPricePerProduct, BigDouble newUpgradeCost, BigDouble newMaxWorkerCount) 
        SetUpgradePanel(int levelUpgradeMultiplier, BigDouble outputValue, int oldLevel, float collectTime, 
        BigDouble pricePerProduct, BigDouble upgradeCost, BigDouble workerCount, BigDouble maxWorkerCount, string name)
    {
        BigDouble newMaxWorkerCount = GetNewMaxWorkerCount(levelUpgradeMultiplier, maxWorkerCount, oldLevel);
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
            availableWorkerCountText.text = availableWorkerTypeDictionary[(WorkerType)workerTypeDropdown.value].ToString();
        CheckConditionsForButtons();
        workerCountText.text = workerCount.ToString();

        levelText.text = string.Format("LEVEL\n{0} <color=green>--> {1}</color>", oldLevel, newLevel);
        outputAmountText.text = string.Format("Output\n{0} <color=green>--> {1}</color>", outputValue.ToString(), 
            newOutputValue.ToString());
        collectTimeText.text = string.Format("Collect Time\n{0} <color=green>--> {1}</color>", ResourceManager.Instance.CurrencyToString(collectTime), 
            ResourceManager.Instance.CurrencyToString(newCollectTime));
        pricePerProductText.text = string.Format("Price Per Product\n{0} <color=green>--> {1}</color>", (pricePerProduct), (newPricePerProduct));
        maxWorkerText.text = string.Format("Max Worker\n{0} <color=green>--> {1}</color>", maxWorkerCount, newMaxWorkerCount);

        return (newLevel, newOutputValue, newCollectTime, newPricePerProduct, newUpgradeCost, newMaxWorkerCount);
    }

    /*
     * Production units can have only 1 type of worker at a time.
     * None remove all workers and set workplace to passive. Disable change workmode button
     * Fill, if available fill unit with preffered workertype, else standard.
     * 
     * Unit worker mode indicates occupied worker type. (WorkerType Enum but only worker types. Can't be Fill)
     * Unit working mode indicates worker employing mode. (WorkerType Enum include everyting.)
     * 
     */


    /*
     * When setting worker type
     * if previous worker type is different than new one,
     * add previous worker count to dictionary and substract the new one.
     * If previous and new one is the same, just substract the difference from available worker dictionary.
     * 
     * 
     */

    void SetWorkerCount(ProductionBase unit, WorkerType workerType, BigDouble? amount = null)
    {
        FreeWorker(unit.CurrentWorkerType, unit.CurrentWorkerCount);
        var maxAvailable = availableWorkerTypeDictionary[workerType];
        Debug.Log("Max available: " + maxAvailable);
        unit.CurrentWorkerType = workerType;
        if (amount != null)
        {
            if (amount.Value > unit.MaxWorkerCount) 
                amount = unit.MaxWorkerCount;

            if (amount.Value <= maxAvailable)
                unit.CurrentWorkerCount = amount.Value;
            else
                unit.CurrentWorkerCount = maxAvailable;
        }
        else if (unit.MaxWorkerCount <= maxAvailable)
            unit.CurrentWorkerCount = unit.MaxWorkerCount;
        else
            unit.CurrentWorkerCount = maxAvailable;
        Debug.Log("Worker count is: " + unit.CurrentWorkerCount);
        SetWorker(workerType, unit.CurrentWorkerCount);
        workerCountText.text = unit.CurrentWorkerCount.ToString();
    }

    public void SetWorkerBonus(BigDouble amount, int enumIndex, ProductionBase productionBase = null)
    {
        if (productionBase != null)
            upgradePanelOwner = productionBase;
        var unit = upgradePanelOwner;

        WorkerType workingMode = (WorkerType)enumIndex; // Get worker type from dropdown
        //unit.IsRunning = true; // Set unit to working state

        var oldWorkerType = unit.CurrentWorkerType;
        var oldWorkerCount = unit.CurrentWorkerCount;

        switch (workingMode)
        {
            case WorkerType.None:
                unit.CurrentWorkerMode = WorkerType.None;
                unit.CurrentWorkerType = WorkerType.Standard;
                //unit.IsRunning = false;
                unit.CurrentWorkerCount = 0;
                workerInputField.text = "0";
                //workerTypeDropdown.value = 0;
                break;

            case WorkerType.Fill:
                unit.CurrentWorkerMode = WorkerType.Fill;
                // Biggest worker count available across all worker types
                var biggestWorkerType = availableWorkerTypeDictionary
                 .First(w => w.Value == availableWorkerTypeDictionary.Values.Max());

                if (availableWorkerTypeDictionary[unit.PrefferedWorkerType] >= unit.MaxWorkerCount)
                {
                    Debug.Log("Preffered worker type selected");
                    unit.CurrentWorkerType = unit.PrefferedWorkerType;
                }

                else if (availableWorkerTypeDictionary[WorkerType.Standard] >= unit.MaxWorkerCount)
                {
                    Debug.Log("Standard worker type selected");
                    unit.CurrentWorkerType = WorkerType.Standard;
                }

                else if (biggestWorkerType.Value >= unit.MaxWorkerCount)
                {
                    Debug.Log("Max worker type selected");
                    unit.CurrentWorkerType = biggestWorkerType.Key;
                }
                else if (availableWorkerTypeDictionary[unit.PrefferedWorkerType] > 0)
                {
                    Debug.Log("Preffered worker type has more than 0 worker");
                    unit.CurrentWorkerType = unit.PrefferedWorkerType;
                }
                else
                {
                    Debug.Log("Fill set to standard and have 0 worker");
                    unit.CurrentWorkerType = WorkerType.Standard;
                }
                Debug.Log("Fill selection result is :" + unit.CurrentWorkerType);
                SetWorkerCount(unit, unit.CurrentWorkerType);
                break;

            case WorkerType.Standard:
                SetWorkerCount(unit, WorkerType.Standard);
                unit.OutputValue += STANDARD_BONUS * unit.CurrentWorkerCount;
                break;

            case WorkerType.Gatherer:
                unit.CurrentWorkerType = WorkerType.Gatherer;

                break;
            case WorkerType.Cook:
                SetWorkerCount(unit, WorkerType.Cook);
                unit.FoodAmount += COOK_BONUS * unit.CurrentWorkerCount;
                break;
            case WorkerType.Warrior:
                unit.CurrentWorkerType = WorkerType.Warrior;
                SetWorkerCount(unit, WorkerType.Warrior);
                break;
            case WorkerType.Miner:
                unit.CurrentWorkerType = WorkerType.Miner;
                SetWorkerCount(unit, WorkerType.Miner);
                unit.OutputValue += MINER_BONUS * unit.CurrentWorkerCount;
                break;
            case WorkerType.Blacksmith:
                unit.CurrentWorkerType = WorkerType.Blacksmith;
                SetWorkerCount(unit, WorkerType.Blacksmith);
                if (unit.AttackAmount > 0)
                    unit.AttackAmount += BLACKSMITH_BONUS * unit.CurrentWorkerCount;
                else if (unit.DefenseAmount > 0)
                    unit.DefenseAmount += BLACKSMITH_BONUS * unit.CurrentWorkerCount;
                break;
            case WorkerType.Artist:
                unit.CurrentWorkerType = WorkerType.Artist;
                SetWorkerCount(unit, unit.CurrentWorkerType, amount);
                unit.PricePerProduct *= (1 + ARTIST_BONUS);
                break;
            case WorkerType.Engineer:
                unit.CurrentWorkerType = WorkerType.Engineer;
                SetWorkerCount(unit, unit.CurrentWorkerType, amount);
                break;
            case WorkerType.Chemist:
                unit.CurrentWorkerType = WorkerType.Chemist;
                SetWorkerCount(unit, unit.CurrentWorkerType, amount);
                break;
            case WorkerType.Scientist:
                unit.CurrentWorkerType = WorkerType.Scientist;
                SetWorkerCount(unit, unit.CurrentWorkerType, amount);
                break;
        }
        if (unit.CurrentWorkerMode != WorkerType.Fill && unit.CurrentWorkerMode != WorkerType.None)
            unit.CurrentWorkerMode = unit.CurrentWorkerType;

        Debug.Log("Current worker count is: " + unit.CurrentWorkerCount);
        workerInputField.text = unit.CurrentWorkerCount.ToString(); // Set ui texts
        //workerTypeDropdown.value = (int)unit.CurrentWorkerMode;
        availableWorkerCountText.text = availableWorkerTypeDictionary[unit.CurrentWorkerType].ToString();
        CheckConditionsForButtons();
        Debug.Log("Current worker count is: " + unit.CurrentWorkerCount);
        if (unit.CurrentWorkerMode == WorkerType.Fill && !fillTypeProductionUnits.Contains(unit)) 
            fillTypeProductionUnits.Add(unit);
        else if (fillTypeProductionUnits.Contains(unit)) 
            fillTypeProductionUnits.Remove(unit);
    }

    /*
     * Worker Count and Production Rate relationship function
     * \ln x^{33}+\frac{x}{54}+3
     * <seealso => https://www.desmos.com/calculator?lang=tr>
     */

    public void UpgradeWorker(WorkerType upgradeType, long amount)
    {
        availableWorkerTypeDictionary[WorkerType.Standard] -= amount;
        availableWorkerTypeDictionary[upgradeType] += amount;
    }

    //readonly Dictionary<WorkerType, (int, int)> workerUpgradeRequirementDict = new Dictionary<WorkerType, (int, int)>()
    //{
    //};

    // isUpgradePanelActive bool is used for updating upgrade panel when currency increased
    public void ShowUpgradePanel(ProductionBase owner, UnityAction<int> SetUpgradePanel, UnityAction Upgrade, bool isUpgradePanelActive, BigDouble upgradeCost, int level)
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
            Upgrade();
            tempAction(upgradeMultiplier);
        });
    }

    public void GrantReward(RewardType reward, BigDouble amount, ContractBase contract = null, BaseResources? resourceType = null)
    {
        switch (reward)
        {
            case RewardType.Currency:
                ResourceManager.Instance.Currency += amount;
                break;
            case RewardType.PremiumCurrency:
                ResourceManager.Instance.PremiumCurrency += amount;
                break;
            case RewardType.miningSpeedUp:
                MiningSpeedMultiplier += (float)amount.ToDouble();
                break;
            case RewardType.productionSpeedUp:
                ProductionSpeedMultiplier += (float)amount.ToDouble();
                break;
            case RewardType.miningYieldUpgrade:
                MiningYieldMultiplier += (float)amount.ToDouble();
                break;
            case RewardType.productionYieldUpgrade:
                ProductionYieldMultiplier += (float)amount.ToDouble();
                break;
            case RewardType.productionEfficiencyUpgrade:
                ProductionEfficiencyMultiplier += (float)amount.ToDouble();
                break;
            case RewardType.incresedCoinEarning:
                EarnedCoinMultiplier += (float)amount.ToDouble();
                break;
            case RewardType.unlockProductionUnit:
                for (int j = 0; j < ProductionManager.Instance.instantiatedProductionUnits.Length; j++)
                {
                    var unit = ProductionManager.Instance.instantiatedProductionUnits[j].GetComponent<ProductionBase>();
                    if (contract.productsToUnlock != null && contract.productsToUnlock.Contains(unit.scriptableProductionBase))
                    {
                        unit.ContractStatueCheckDictionary[contract] = true;
                        if (!unit.ContractStatueCheckDictionary.ContainsValue(false))
                        {
                            if (unit.transform.Find("Level_Lock(Clone)") != null)
                            {
                                Destroy(unit.transform.Find("Level_Lock(Clone)").gameObject);
                                unit.IsLockedByContract = false;
                            }
                        }
                    }
                }
                break;
            case RewardType.unitSpeedUp:
                SpeedUpDictionaryValue = new KeyValuePair<BaseResources, float>(contract.resourceToRewarded, contract.contractReward);
                break;
            case RewardType.workerSpeed:
                WorkerSpeedMultiplier += (float)amount.ToDouble();
                break;
            case RewardType.Resource:
                ResourceManager.Instance.AddResource(resourceType.Value, amount);
                break;
        }
    }
}

public enum RewardType
{
    Currency,
    PremiumCurrency,
    miningSpeedUp,
    productionSpeedUp,
    miningYieldUpgrade,
    productionYieldUpgrade,
    productionEfficiencyUpgrade,
    incresedCoinEarning,
    unlockProductionUnit, // Only for contracts
    unitSpeedUp,
    workerSpeed,
    Resource,
}