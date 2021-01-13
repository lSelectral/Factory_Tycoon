﻿using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class Mine_Btn : MonoBehaviour, IPointerClickHandler
{
    #region Class Variables

    public ScriptableMine scriptableMine;

    float miningSpeedUpgrade;
    float miningEfficiencyUpgrade;

    [SerializeField] private bool isAutomated;

    bool isUpgradePanelActive;

    // Scriptable object class variables
    [SerializeField] float collectTime;
    string mineName;
    string resourceName;
    BaseResources producedResource;
    ItemType[] itemTypes;
    long foodAmount;
    float remainedCollectTime;
    bool isCharging;
    bool isReadyToCollect;
    long outputValue;
    float pricePerProduct;
    Sprite backgroundImage;
    int unlockLevel;
    float xpAmount;


    bool isLockedByContract;
    ContractBase[] contracts;
    Dictionary<ContractBase, bool> contractStatueCheckDictionary;

    // Attached gameobject transforms
    Image fillBar;
    TextMeshProUGUI mineNameText;
    Transform mainBtn;
    Button upgradeBtn;
    Button workModeBtn;
    TextMeshProUGUI upgradeAmountText;
    TextMeshProUGUI mineLevelText;
    TextMeshProUGUI workModeText;
    RectTransform tool;
    float outputPerSecond;

    private int mineLevel;
    double upgradeCost;
    float incomePerSecond;
    MineWorkingMode workingMode;

    LTDescr miningAnimation;

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

    public int MineLevel
    {
        get { return mineLevel; }
        set { mineLevel = value; mineLevelText.text = "LEVEL " + mineLevel.ToString(); }
    }

    public double UpgradeCost
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

    public MineWorkingMode WorkingMode
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
            StatSystem.Instance.CurrencyPerSecond += incomePerSecond-oldValue;
        }
    }

    public ItemType[] ItemTypes { get => itemTypes;}
    public long FoodAmount { get => foodAmount; set => foodAmount = value; }
    public Dictionary<ContractBase, bool> ContractStatueCheckDictionary { get => contractStatueCheckDictionary; set => contractStatueCheckDictionary = value; }

    #endregion

    void Start()
    {
        // TODO ONLY FOR DEBUG REMOVE IT
        IsAutomated = true;

        // Custom Events
        GameManager.Instance.OnLevelUp += OnLevelUp;
        ResourceManager.Instance.OnResourceAmountChanged += OnResourceAmountChanged;
        ResourceManager.Instance.OnCurrencyChanged += OnCurrencyChanged;
        UpgradeSystem.Instance.OnMiningSpeedChanged += Instance_OnMiningSpeedChanged;
        UpgradeSystem.Instance.OnMiningYieldChanged += Instance_OnMiningYieldChanged;
        UpgradeSystem.Instance.OnEarnedCoinMultiplierChanged += Instance_OnEarnedCoinMultiplierChanged;
        UpgradeSystem.Instance.OnEarnedXpMultiplierChanged += Instance_OnEarnedXpMultiplierChanged;

        collectTime = scriptableMine.collectTime;
        mineName = scriptableMine.TranslatedName;
        resourceName = scriptableMine.resourceName;
        producedResource = scriptableMine.product;
        foodAmount = scriptableMine.foodAmount;
        itemTypes = scriptableMine.itemTypes;
        outputValue = scriptableMine.outputValue;
        pricePerProduct = scriptableMine.pricePerProduct;
        backgroundImage = scriptableMine.backgroundImage;
        unlockLevel = scriptableMine.unlockLevel;
        mineLevel = 1;
        xpAmount = scriptableMine.xpAmount* 1f;
        outputPerSecond = outputValue * 1f / collectTime;

        workingMode = MineWorkingMode.production;

        contracts = scriptableMine.lockedByContracts;
        contractStatueCheckDictionary = new Dictionary<ContractBase, bool>();
        if (contracts != null)
        {
            for (int i = 0; i < contracts.Length; i++)
            {
                contractStatueCheckDictionary.Add(contracts[i], false);
            }
        }

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

        // Set gameobject hierarchy
        fillBar = transform.Find("GameObject").transform.Find("Outline").transform.Find("Fill").GetComponent<Image>();
        mineNameText = transform.Find("GameObject").transform.Find("Mine_Name").GetComponent<TextMeshProUGUI>();
        mineNameText.text = mineName;
        mainBtn = transform.Find("Button");
        upgradeBtn = transform.Find("Upgrade_Btn").GetComponent<Button>();
        upgradeAmountText = transform.Find("Upgrade_Btn").Find("upgradeText").GetComponent<TextMeshProUGUI>();
        mineLevelText = transform.Find("LevelText").GetComponent<TextMeshProUGUI>();
        workModeBtn = transform.Find("Sell_Btn").GetComponent<Button>();
        workModeBtn.onClick.AddListener(() => ChangeWorkingMode());
        workModeText = workModeBtn.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        transform.Find("Button").Find("Background").GetComponent<Image>().sprite = backgroundImage;
        if (scriptableMine.sourceImage != null)
            transform.Find("Button").Find("SourceImage").GetComponent<Image>().sprite = scriptableMine.sourceImage;
        if (scriptableMine.toolImage != null)
            transform.Find("Button").Find("Tool").GetComponent<Image>().sprite = scriptableMine.toolImage;
        tool = transform.Find("Button").Find("Tool").GetComponent<RectTransform>();
        transform.Find("GameObject").Find("Icon").GetComponent<Image>().sprite = scriptableMine.icon;
        if (remainedCollectTime >0)
        {
            fillBar.fillAmount = ((collectTime - remainedCollectTime) / collectTime);
        }
        else
        {
            fillBar.fillAmount = 0;
        }
        upgradeBtn.onClick.AddListener(() => ShowUpgradePanel());
        workingMode = MineWorkingMode.sell;
        mineLevelText.text = "Level " + mineLevel.ToString();
        workModeText.text = ResourceManager.Instance.GetValidName(workingMode.ToString());

        SetWorkModeColor();

        IncomePerSecond = (outputPerSecond*pricePerProduct);
        StatSystem.Instance.CurrencyPerSecond += incomePerSecond;

        if (upgradeCost == 0)
            upgradeCost = outputValue*pricePerProduct* UpgradeSystem.MINE_STARTING_UPGRADE_COST_MULTIPLIER;
        upgradeAmountText.text = "$" + ResourceManager.Instance.CurrencyToString(upgradeCost);
    }

    private void OnCurrencyChanged(object sender, ResourceManager.OnCurrencyChangedEventArgs e)
    {
        if (IsUpgradePanelActive)
        {
            var q = UpgradeSystem.Instance.upgradeMultiplier;
            if (q != 1 && q != 5 && q != 20)
                UpgradeSystem.Instance.GetMaximumUpgradeAmount(UpgradeCost, MineLevel);
            SetUpgradePanel(UpgradeSystem.Instance.upgradeMultiplier);
        }
    }

    #region Event Methods

    private void Instance_OnEarnedXpMultiplierChanged(object sender, UpgradeSystem.OnEarnedXpMultiplierChangedEventArgs e)
    {
        XPAmount *= e.earnedXpMultiplier;
    }

    private void Instance_OnEarnedCoinMultiplierChanged(object sender, UpgradeSystem.OnEarnedCoinMultiplierChangedEventArgs e)
    {
        PricePerProduct *= e.earnedCoinMultiplier;
    }

    private void Instance_OnMiningYieldChanged(object sender, UpgradeSystem.OnMiningYieldChangedEventArgs e)
    {
        OutputValue *= e.miningYield;
    }

    private void Instance_OnMiningSpeedChanged(object sender, UpgradeSystem.OnMiningSpeedChangedEventArgs e)
    {
        CollectTime /= e.miningSpeed;
    }

    private void OnResourceAmountChanged(object sender, ResourceManager.OnResourceAmountChangedEventArgs e)
    {
    }

    private void OnLevelUp(object sender, GameManager.OnLevelUpEventArgs e)
    {
        if (transform.Find("Level_Lock(Clone)") != null && unlockLevel == e.currentLevel)
        {
            Destroy(transform.Find("Level_Lock(Clone)").gameObject);
            // Check if there is contract for that compound hasn't completed yet
            if (contractStatueCheckDictionary.ContainsValue(false))
            {
                for (int i = 0; i < contracts.Length; i++)
                {
                    if (contractStatueCheckDictionary[contracts[i]] == false)
                    {
                        var lockText = Instantiate(GameManager.Instance.levelLock, transform);
                        lockText.GetComponentInChildren<TextMeshProUGUI>().text = "UNLOCKED AT COMPLETION OF " + contracts[i].contractName + " Contract";
                    }
                }

            }
            else
            {
                // If mine is accesible to player create automation contract
                var contracts = ContractManager.Instance.contracts;
                for (int i = 0; i < contracts.Length; i++)
                {
                    if (contracts[i].contractRewardType == ContractRewardType.automate && contracts[i].minesToUnlock[0] == scriptableMine)
                    {
                        ContractManager.Instance.CreateContract(contracts[i]);
                    }
                }
            }
        }
    }

    #endregion

    void Update()
    {
        if (unlockLevel <= GameManager.Instance.CurrentLevel && !isLockedByContract)
        {
            if (isAutomated)
            {
                CollectMine();
            }

            if (isCharging)
            {
                if (remainedCollectTime > 0)
                {
                    remainedCollectTime -= Time.deltaTime * UpgradeSystem.Instance.MiningSpeedMultiplier;
                    fillBar.fillAmount = ((collectTime - remainedCollectTime) / collectTime);
                }
                else
                {
                    //float currency;
                    //float resource;
                    AddResourceAndMoney(/*out currency, out resource*/);

                    if (workingMode == MineWorkingMode.production)
                        StatSystem.Instance.PopupText(transform, OutputValue, resourceName);
                    else if (workingMode == MineWorkingMode.sell)
                        StatSystem.Instance.PopupText(transform, pricePerProduct, "Gold");

                    isCharging = false;
                    remainedCollectTime = 0;
                    fillBar.fillAmount = 0;
                    LeanTween.cancel(tool.gameObject);
                }
            }
        }
    }

    void AddResourceAndMoney(/*out float currency, out float resourceAmount*/)
    {
        switch (workingMode)
        {
            case MineWorkingMode.production:
                /*resourceAmount = */ResourceManager.Instance.AddResource(producedResource, (long)(outputValue * UpgradeSystem.Instance.MiningYieldMultiplier));
                break;
            case MineWorkingMode.sell:
                ResourceManager.Instance.AddResource(producedResource, (long)(outputValue * UpgradeSystem.Instance.MiningYieldMultiplier));
                SellResource();
                break;
        }
        //resourceAmount = ResourceManager.Instance.AddResource(mine.baseResource, (long)(mine.outputValue * UpgradeSystem.Instance.MiningYieldMultiplier));
        //currency = incomeAmount;
        //ResourceManager.Instance.Currency += incomeAmount;
        GameManager.Instance.AddXP(XPAmount);
    }

    public float IdleEarn(int idleTime)
    {
        return incomePerSecond * idleTime * UpgradeSystem.Instance.EarnedCoinMultiplier;
    }

    public void CollectMine()
    {
        if (!isCharging && transform.Find("Level_Lock(Clone)") == null)
        {
            miningAnimation = TweenAnimation.Instance.MoveTool(tool.gameObject);
            isCharging = true;
            if (remainedCollectTime == 0)
                remainedCollectTime = collectTime;
        }
    }

    void SellResource()
    {
        ResourceManager.Instance.Currency += outputValue * 1f * (pricePerProduct *1f);
        ResourceManager.Instance.ConsumeResource(producedResource, outputValue);
    }

    void ChangeWorkingMode()
    {
        if (GameManager.Instance.CurrentLevel >= 0)
        {
            Array a = Enum.GetValues(typeof(MineWorkingMode));
            int j = 0;
            for (int i = 0; i < a.Length; i++)
            {
                j = i + 1;
                if ((MineWorkingMode)(a.GetValue(i)) == workingMode)
                    break;
            }
            if (j < a.Length)
                workingMode = (MineWorkingMode)a.GetValue(j);
            else
                workingMode = (MineWorkingMode)a.GetValue(j - a.Length);
            workModeText.text = ResourceManager.Instance.GetValidName(workingMode.ToString());
            SetWorkModeColor();
        }
    }

    /// <summary>
    /// When player change work mode
    /// button color change according to that
    /// </summary>
    void SetWorkModeColor()
    {
        if (workingMode == MineWorkingMode.production)
            workModeBtn.GetComponent<Image>().color = Color.green;
        else if (workingMode == MineWorkingMode.sell)
            workModeBtn.GetComponent<Image>().color = Color.red;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        CollectMine();
        if (isCharging) remainedCollectTime -= .35f;
    }

    void ShowUpgradePanel()
    {
        UpgradeSystem.Instance.ShowUpgradePanel(SetUpgradePanel, UpgradeMine, IsUpgradePanelActive, UpgradeCost, MineLevel);
    }

    void SetUpgradePanel(int levelUpgradeMultiplier)
    {
        UpgradeSystem.Instance.SetUpgradePanel(levelUpgradeMultiplier, OutputValue, MineLevel, CollectTime, PricePerProduct, UpgradeCost, mineName);
    }

    void UpgradeMine()
    {
        var upgradeMultiplier = UpgradeSystem.Instance.upgradeMultiplier;
        var newLevel = mineLevel + upgradeMultiplier;
        var newOutputValue = UpgradeSystem.Instance.GetNewOutputAmount(upgradeMultiplier, OutputValue, mineLevel);
        var newCollectTime = UpgradeSystem.Instance.GetNewCollectTime(upgradeMultiplier, collectTime);
        var newPricePerProduct = UpgradeSystem.Instance.GetNewPricePerProduct(upgradeMultiplier, pricePerProduct, mineLevel);

        double newUpgradeCost = 0;

        if (upgradeMultiplier > 1)
            newUpgradeCost = UpgradeSystem.Instance.GetNewUpgradeCost(upgradeMultiplier, UpgradeCost, mineLevel);
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

            MineLevel = newLevel;
            OutputValue = newOutputValue;
            CollectTime = newCollectTime;
            PricePerProduct = newPricePerProduct;
            ResourceManager.Instance.SetNewPricePerProduct(producedResource, pricePerProduct);
            UpgradeCost = UpgradeSystem.Instance.GetNewUpgradeCost(newUpgradeCost, MineLevel);
            SetUpgradePanel(upgradeMultiplier);
        }
    }
}