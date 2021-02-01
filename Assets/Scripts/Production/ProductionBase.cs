using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;
using System.Collections.Generic;

/// <summary>
/// Base class for all production units
/// </summary>
public abstract class ProductionBase : MonoBehaviour, IPointerClickHandler
{
    #region Class Variables
    public ScriptableProductionBase scriptableProductionBase;

    internal Dictionary<ContractBase, bool> contractStatueCheckDictionary;
    internal ContractBase[] contracts;
    [SerializeField] internal bool isAutomated;

    internal bool isUpgradePanelActive;

    // Scriptable object class variables
    [SerializeField] internal float collectTime;
    internal string _name;
    internal string resourceName;
    internal BaseResources producedResource;
    internal ItemType[] itemTypes;
    internal long foodAmount;
    internal float remainedCollectTime;
    internal bool isCharging;
    internal long outputValue;
    internal float pricePerProduct;
    internal Sprite backgroundImage;
    internal int unlockLevel;
    internal float xpAmount;
    internal bool isLockedByContract;

    // Attached gameobject transforms
    internal Image fillBar;
    internal TextMeshProUGUI nameText;
    internal Transform mainBtn;
    internal Button upgradeBtn;
    internal Button workModeBtn;
    internal TextMeshProUGUI upgradeAmountText;
    internal TextMeshProUGUI levelText;
    internal TextMeshProUGUI workModeText;
    internal float outputPerSecond;

    internal int level;
    internal BNum upgradeCost;
    internal float incomePerSecond;
    internal WorkingMode workingMode;
    internal LTDescr toolAnimation;

    #endregion

    #region Properties
    public float RemainedCollectTime
    {
        get { return remainedCollectTime; }
        set { remainedCollectTime = value; }
    }
    public float CollectTime
    {
        get { return collectTime; }
        set { collectTime = value; OutputPerSecond = OutputPerSecond; }
    }

    public int Level
    {
        get { return level; }
        set { level = value; levelText.text = "LEVEL " + level.ToString(); }
    }

    public BNum UpgradeCost
    {
        get { return upgradeCost; }
        set { upgradeCost = value; upgradeAmountText.text = "$" + ResourceManager.Instance.CurrencyToString(upgradeCost); }
    }

    public bool IsAutomated
    {
        get { return isAutomated; }
        set { isAutomated = value; }
    }

    public float OutputPerSecond
    {
        get { return outputPerSecond; }
        set
        {
            outputPerSecond = value;
        }
    }

    public float XPAmount
    {
        get { return xpAmount; }
        set { xpAmount = value; }
    }

    public float PricePerProduct
    {
        get { return pricePerProduct; }
        set { pricePerProduct = value; }
    }

    public bool IsLockedByContract
    {
        get { return isLockedByContract; }
        set { isLockedByContract = value; }
    }

    public WorkingMode WorkingMode
    {
        get { return workingMode; }
        set { workingMode = value; workModeText.text = ResourceManager.Instance.GetValidName(workingMode.ToString()); }
    }

    public long OutputValue { get => outputValue; set { outputValue = value; OutputPerSecond = OutputPerSecond; } }

    public bool IsUpgradePanelActive
    {
        get { return isUpgradePanelActive; }
        set { isUpgradePanelActive = value; }
    }

    public BaseResources ProducedResource
    {
        get { return producedResource; }
    }

    public float IncomePerSecond
    {
        get { return incomePerSecond; }
        set
        {
            var oldValue = incomePerSecond;
            incomePerSecond = value * UpgradeSystem.Instance.EarnedCoinMultiplier;
            StatSystem.Instance.CurrencyPerSecond += incomePerSecond - oldValue;
        }
    }

    public ItemType[] ItemTypes { get => itemTypes; }
    public long FoodAmount { get => foodAmount; set => foodAmount = value; }

    public Dictionary<ContractBase, bool> ContractStatueCheckDictionary { get => contractStatueCheckDictionary; set => contractStatueCheckDictionary = value; }
    internal ContractBase[] Contracts { get => contracts; set => contracts = value; }

    #endregion

    GameObject mainProductionPanel;
    GameObject agePanel;

    internal virtual void Start()
    {
        // TODO ONLY FOR DEBUG REMOVE IT
        IsAutomated = false;
        mainProductionPanel = transform.parent.parent.parent.parent.parent.gameObject;
        agePanel = transform.parent.parent.parent.gameObject;
        #region DONE
        // Custom Events
        GameManager.Instance.OnLevelUp += OnLevelUp;
        ResourceManager.Instance.OnResourceAmountChanged += OnResourceAmountChanged;
        ResourceManager.Instance.OnCurrencyChanged += OnCurrencyChanged;
        //UpgradeSystem.Instance.OnMiningSpeedChanged += Instance_OnMiningSpeedChanged;
        //UpgradeSystem.Instance.OnMiningYieldChanged += Instance_OnMiningYieldChanged;
        UpgradeSystem.Instance.OnEarnedCoinMultiplierChanged += Instance_OnEarnedCoinMultiplierChanged;
        UpgradeSystem.Instance.OnEarnedXpMultiplierChanged += Instance_OnEarnedXpMultiplierChanged;

        if (scriptableProductionBase.lockedByContracts != null)
            contracts = scriptableProductionBase.lockedByContracts;
        contractStatueCheckDictionary = new Dictionary<ContractBase, bool>();
        if (contracts != null)
        {
            for (int i = 0; i < contracts.Length; i++)
            {
                contractStatueCheckDictionary.Add(contracts[i], false);
            }
        }

        collectTime = scriptableProductionBase.collectTime;
        _name = scriptableProductionBase.TranslatedName;
        resourceName = scriptableProductionBase.resourceName;
        producedResource = scriptableProductionBase.product;
        foodAmount = scriptableProductionBase.foodAmount;
        itemTypes = scriptableProductionBase.itemTypes;
        outputValue = scriptableProductionBase.outputValue;
        pricePerProduct = scriptableProductionBase.pricePerProduct;
        backgroundImage = scriptableProductionBase.backgroundImage;
        unlockLevel = scriptableProductionBase.unlockLevel;
        level = 1;
        xpAmount = scriptableProductionBase.xpAmount * 1f;
        outputPerSecond = outputValue * 1f / collectTime;

        if (upgradeCost == null || upgradeCost == new BNum())
            upgradeCost = new BNum(outputValue * pricePerProduct * UpgradeSystem.STARTING_UPGRADE_COST_MULTIPLIER,0);

        // Set lock text
        if (unlockLevel > GameManager.Instance.CurrentLevel)
        {
            var lockText = Instantiate(GameManager.Instance.levelLock, transform);
            lockText.GetComponentInChildren<TextMeshProUGUI>().text = "UNLOCKED AT LEVEL " + unlockLevel.ToString();
        }
        else if (contracts != null && contracts.Length > 0)
        {
            var lockText = Instantiate(GameManager.Instance.levelLock, transform);
            lockText.GetComponentInChildren<TextMeshProUGUI>().text = "UNLOCKED AT COMPLETION OF " + contracts[0].contractName + " Contract";
        }
        #endregion

    }

    internal void OnCurrencyChanged(object sender, ResourceManager.OnCurrencyChangedEventArgs e)
    {
        
    }

    #region Event Methods

    internal void Instance_OnEarnedXpMultiplierChanged(object sender, UpgradeSystem.OnEarnedXpMultiplierChangedEventArgs e)
    {
        XPAmount *= e.earnedXpMultiplier;
    }

    internal void Instance_OnEarnedCoinMultiplierChanged(object sender, UpgradeSystem.OnEarnedCoinMultiplierChangedEventArgs e)
    {
        PricePerProduct *= e.earnedCoinMultiplier;
    }

    //internal void Instance_OnMiningYieldChanged(object sender, UpgradeSystem.OnMiningYieldChangedEventArgs e)
    //{
    //    OutputValue *= e.miningYield;
    //}

    //internal void Instance_OnMiningSpeedChanged(object sender, UpgradeSystem.OnMiningSpeedChangedEventArgs e)
    //{
    //    CollectTime /= e.miningSpeed;
    //}

    internal void OnResourceAmountChanged(object sender, ResourceManager.OnResourceAmountChangedEventArgs e)
    {
    }

    internal void OnLevelUp(object sender, GameManager.OnLevelUpEventArgs e)
    {
        if (transform.Find("Level_Lock(Clone)") != null && unlockLevel == e.currentLevel)
        {
            Destroy(transform.Find("Level_Lock(Clone)").gameObject);

            // Check if there is contract for that unit hasn't completed yet
            if (contracts != null && contractStatueCheckDictionary.ContainsValue(false))
            {
                for (int i = 0; i < contracts.Length; i++)
                {
                    if (contractStatueCheckDictionary[contracts[i]] == false)
                    {
                        var lockText = Instantiate(GameManager.Instance.levelLock, transform);
                        lockText.GetComponentInChildren<TextMeshProUGUI>().text = "UNLOCKED AT COMPLETION OF " + contracts[i].contractName + " Contract";
                        return;
                    }
                }

            }
            // Needed code for creating automation contracts when unit unlocked
            //else
            //{
            //    var contracts = ContractManager.Instance.contracts;
            //    for (int i = 0; i < contracts.Length; i++)
            //    {
            //        if (contracts[i].contractRewardType == ContractRewardType.automate && contracts[i].minesToUnlock[0] == scriptableProductionBase)
            //        {
            //            ContractManager.Instance.CreateContract(contracts[i]);
            //        }
            //    }
            //}
        }
    }

    #endregion

    internal virtual void Update()
    {
        
    }

    internal virtual void SellResource()
    {
        ResourceManager.Instance.Currency += outputValue * 1f * (pricePerProduct * 1f);
        ResourceManager.Instance.ConsumeResource(producedResource, outputValue);
    }

    internal void ChangeWorkingMode(bool isMine = false)
    {
        if (GameManager.Instance.CurrentLevel >= 0)
        {
            //Array a = Enum.GetValues(typeof(WorkingMode));
            //int j = 0;
            //for (int i = 0; i < a.Length; i++)
            //{
            //    j = i + 1;
            //    if ((WorkingMode)(a.GetValue(i)) == workingMode)
            //        break;
            //}
            //if (j < a.Length)
            //    workingMode = (WorkingMode)a.GetValue(j);
            //else
            //    workingMode = (WorkingMode)a.GetValue(j - a.Length);
            //if (workingMode == WorkingMode.stopProduction && isMine)
            //    workingMode = WorkingMode.production;

            workingMode = workingMode.Next();
            // Mines don't have stop production work mode
            if (workingMode == WorkingMode.stopProduction && isMine)
                workingMode = WorkingMode.Next();

            workModeText.text = ResourceManager.Instance.GetValidName(workingMode.ToString());
            SetWorkModeColor();
        }
    }

    /// <summary>
    /// When player change work mode
    /// button color change according to that
    /// </summary>
    internal void SetWorkModeColor()
    {
        if (workingMode == WorkingMode.production)
            workModeBtn.GetComponent<Image>().color = Color.green;
        else if (workingMode == WorkingMode.sell)
            workModeBtn.GetComponent<Image>().color = Color.white;
        else if (workingMode == WorkingMode.stopProduction)
            workModeBtn.GetComponent<Image>().color = Color.red;
    }

    public virtual void OnPointerClick(PointerEventData eventData)
    {
    }

    internal virtual void ShowUpgradePanel()
    {
        UpgradeSystem.Instance.ShowUpgradePanel(SetUpgradePanel, Upgrade, IsUpgradePanelActive, UpgradeCost, Level);
    }

    internal virtual void SetUpgradePanel(int levelUpgradeMultiplier)
    {
        UpgradeSystem.Instance.SetUpgradePanel(levelUpgradeMultiplier, OutputValue, Level, CollectTime, PricePerProduct, UpgradeCost, _name);
    }

    internal virtual void Upgrade()
    {
        var upgradeMultiplier = UpgradeSystem.Instance.upgradeMultiplier;
        var newLevel = level + upgradeMultiplier;
        var newOutputValue = UpgradeSystem.Instance.GetNewOutputAmount(upgradeMultiplier, OutputValue, level);
        var newCollectTime = UpgradeSystem.Instance.GetNewCollectTime(upgradeMultiplier, collectTime);
        var newPricePerProduct = UpgradeSystem.Instance.GetNewPricePerProduct(upgradeMultiplier, pricePerProduct, level);

        BNum newUpgradeCost = new BNum();

        if (upgradeMultiplier > 1)
            newUpgradeCost = UpgradeSystem.Instance.GetNewUpgradeCost(upgradeMultiplier, UpgradeCost, level);
        else
            newUpgradeCost = UpgradeCost;


        //Debug.Log("Upgrade Multiplier: " + upgradeMultiplier);
        //Debug.Log("New Level: " + newLevel);
        //Debug.Log("Output Value: " + newOutputValue);
        //Debug.Log("Collect Time: " + newCollectTime);
        //Debug.Log("Price Per Product: " + newPricePerProduct);
        //Debug.Log("Upgrade Cost: " + newUpgradeCost);

        if (ResourceManager.Instance.Currency >= newUpgradeCost)
        {
            ResourceManager.Instance.Currency -= newUpgradeCost;

            Level = newLevel;
            OutputValue = newOutputValue;
            CollectTime = newCollectTime;
            PricePerProduct = newPricePerProduct;
            ResourceManager.Instance.SetNewPricePerProduct(producedResource, pricePerProduct);
            UpgradeCost = UpgradeSystem.Instance.GetNewUpgradeCost(newUpgradeCost, Level);
            SetUpgradePanel(upgradeMultiplier);
        }
    }

    // Check if currently this production panel active
    // If not stop all animation on non visible elements
    internal virtual bool CheckIfPanelActive()
    {
        if ( GameManager.Instance.VisiblePanelForPlayer == mainProductionPanel &&
                        GameManager.Instance.VisibleSubPanelForPlayer == agePanel)
        {
            return true;
        }
        else
            return false;
    }
}