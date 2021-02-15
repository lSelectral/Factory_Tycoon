﻿using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/* 
 * TODO add alternative way to create some products.
 * Ex: Logs => 3 stick
*/
/// <summary>
/// Base class for all production units
/// </summary>
public abstract class ProductionBase : MonoBehaviour, IPointerClickHandler
{
    #region Class Variables
    public ScriptableProductionBase scriptableProductionBase;
    protected Dictionary<ContractBase, bool> contractStatueCheckDictionary;
    protected ContractBase[] contracts;
    [SerializeField] protected bool isAutomated;

    GameObject mainProductionPanel;
    GameObject agePanel;

    protected bool isUpgradePanelActive;

    protected Recipe[] recipes;
    protected Recipe currentRecipe;
    protected List<BaseResources> tempResourceList;

    // Scriptable object class variables
    [SerializeField] protected float collectTime;
    protected string _name;
    protected string resourceName;
    protected BaseResources producedResource;
    protected ItemType[] itemTypes;
    protected long foodAmount;
    protected long attackAmount;
    protected float remainedCollectTime;
    protected bool isCharging;
    protected long outputValue;
    protected BigDouble pricePerProduct;
    protected int unlockLevel;
    protected float xpAmount;
    protected bool isLockedByContract;
    protected float outputPerSecond;
    protected int level;
    protected BigDouble upgradeCost;
    protected BigDouble incomePerSecond;
    protected WorkingMode workingMode;
    protected LTDescr toolAnimation;

    // Attached gameobject transforms
    protected Transform subResourceIcons;
    protected RectTransform tool;
    protected Image fillBar;
    protected Sprite backgroundImage;
    protected TextMeshProUGUI nameText;
    protected Transform mainBtn;
    protected Button upgradeBtn;
    protected Button workModeBtn;
    protected TextMeshProUGUI upgradeAmountText;
    protected TextMeshProUGUI levelText;
    protected TextMeshProUGUI workModeText;
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

    public BigDouble UpgradeCost
    {
        get { return upgradeCost; }
        set { upgradeCost = value; upgradeAmountText.text = "$" + upgradeCost.ToString(); }
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

    public BigDouble PricePerProduct
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

    public BigDouble IncomePerSecond
    {
        get { return incomePerSecond; }
        set
        {
            var oldValue = incomePerSecond;
            incomePerSecond = value * UpgradeSystem.Instance.EarnedCoinMultiplier;
            StatSystem.Instance.CurrencyPerSecond += incomePerSecond - oldValue;
        }
    }

    public Recipe[] Recipes { get => recipes; set => recipes = value; }
    public Recipe CurrentRecipe { get => currentRecipe; set => currentRecipe = value; }

    public List<BaseResources> RemainedResources
    {
        get { return tempResourceList; }
        set { tempResourceList = value; }
    }

    public ItemType[] ItemTypes { get => itemTypes; }
    public long FoodAmount { get => foodAmount; set => foodAmount = value; }
    public long AttackAmount { get => attackAmount; set => attackAmount = value; }


    public Dictionary<ContractBase, bool> ContractStatueCheckDictionary { get => contractStatueCheckDictionary; set => contractStatueCheckDictionary = value; }
    internal ContractBase[] Contracts { get => contracts; set => contracts = value; }

    #endregion

    protected virtual void Start()
    {
        IsAutomated = false;
        mainProductionPanel = transform.parent.parent.parent.parent.parent.gameObject;
        agePanel = transform.parent.parent.parent.gameObject;
        #region DONE
        // Custom Events
        GameManager.Instance.OnLevelUp += OnLevelUp;
        ResourceManager.Instance.OnCurrencyChanged += OnCurrencyChanged;
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

        recipes = scriptableProductionBase.recipes;
        if (recipes.Length > 0)
            currentRecipe = recipes[0];
        else
            currentRecipe = new Recipe();

        collectTime = scriptableProductionBase.collectTime;
        _name = scriptableProductionBase.TranslatedName;
        resourceName = ResourceManager.Instance.GetValidNameForResource(scriptableProductionBase.product);
        producedResource = scriptableProductionBase.product;
        foodAmount = scriptableProductionBase.foodAmount;
        attackAmount = scriptableProductionBase.attackAmount;
        itemTypes = scriptableProductionBase.itemTypes;
        outputValue = scriptableProductionBase.outputValue;
        pricePerProduct = scriptableProductionBase.pricePerProduct;
        backgroundImage = scriptableProductionBase.backgroundImage;
        unlockLevel = scriptableProductionBase.unlockLevel;
        level = 1;
        xpAmount = scriptableProductionBase.xpAmount * 1f;
        outputPerSecond = outputValue * 1f / collectTime;

        if (upgradeCost == null || upgradeCost == new BigDouble())
            upgradeCost = (outputValue * pricePerProduct * 
                UpgradeSystem.STARTING_UPGRADE_COST_MULTIPLIER);

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

    protected void Produce()
    {
        if (!isCharging && transform.Find("Level_Lock(Clone)") == null)
        {
            // If true it needs resource to start produce else it produce without input
            if (currentRecipe.inputResources.Length > 0 && currentRecipe.inputAmounts[0] > 0 
                && workingMode != WorkingMode.stopProduction)
            {
                for (int i = 0; i < currentRecipe.inputAmounts.Length; i++)
                {
                    int inputAmount = currentRecipe.inputAmounts[i];
                    BaseResources inputResource = currentRecipe.inputResources[i];
                    Debug.Log(inputResource);
                    var q = ResourceManager.Instance.GetResourceAmount(inputResource);
                    if (ResourceManager.Instance.GetResourceAmount(inputResource) 
                        >= (inputAmount / UpgradeSystem.Instance.ProductionEfficiencyMultiplier) 
                        && tempResourceList.Contains(inputResource))
                    {
                        //Debug.Log(string.Format("{0} added to {1} recipe", input.Resource, producedResource));
                        tempResourceList.Remove(inputResource);
                        ResourceManager.Instance.ConsumeResource(inputResource, 
                            (long)(inputAmount / UpgradeSystem.Instance.ProductionEfficiencyMultiplier));

                        if (subResourceIcons != null)
                            subResourceIcons.GetChild(Array.IndexOf(currentRecipe.inputResources, inputResource))
                                .GetChild(0).GetChild(0).GetChild(0).gameObject.SetActive(true);
                    }
                    else if (ResourceManager.Instance.GetResourceAmount(inputResource) < inputAmount)
                    {
                        //Debug.Log("Not enough " + input.Resource);
                    }
                }

                if (tempResourceList.Count == 0)
                {
                    //Debug.Log(_name + " recipe completed");
                    isCharging = true;
                    remainedCollectTime = collectTime;

                    for (int i = 0; i < currentRecipe.inputAmounts.Length; i++)
                    {
                        subResourceIcons.GetChild(i).GetChild(0).GetChild(0).GetChild(0).gameObject.SetActive(false);
                    }
                }
            }
            else
            {
                if (CheckIfPanelActive())
                    toolAnimation = TweenAnimation.Instance.MoveTool(tool.gameObject);
                isCharging = true;
                if (remainedCollectTime == 0)
                    remainedCollectTime = collectTime;
            }
        }
    }

    internal virtual void SellResource()
    {
        ResourceManager.Instance.Currency += (outputValue * 1f * (pricePerProduct * 1f));
        ResourceManager.Instance.ConsumeResource(producedResource, outputValue);
    }

    internal void ChangeWorkingMode(bool isMine = false)
    {
        if (GameManager.Instance.CurrentLevel >= 0)
        {
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
        BigDouble newPricePerProduct = UpgradeSystem.Instance.GetNewPricePerProduct(upgradeMultiplier, pricePerProduct, level);

        BigDouble newUpgradeCost = new BigDouble();

        if (upgradeMultiplier > 1)
            newUpgradeCost = UpgradeSystem.Instance.GetNewUpgradeCost(upgradeMultiplier, UpgradeCost, level);
        else
            newUpgradeCost = UpgradeCost;

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


    /// <summary>
    /// Check if currently this production panel active
    /// If not stop all animation on non visible elements
    /// </summary>
    /// <returns>True if parent panel is active</returns>
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