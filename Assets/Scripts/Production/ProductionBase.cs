using System;
using System.Linq;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

/* 
 * TODO add alternative way to create some products.
 * Ex: Logs => 3 stick
*/
/// <summary>
/// Base class for all production units
/// </summary>
public class ProductionBase : MonoBehaviour, IPointerClickHandler
{
    GameObject _icon;
    LTDescr _iconMoveAnimation;
    LTDescr _iconScaleAnimation;

    #region Class Variables
    public ScriptableProductionBase scriptableProductionBase;
    protected Dictionary<ContractBase, bool> contractStatueCheckDictionary;
    [SerializeField] protected ContractBase[] contracts;
    [SerializeField] protected bool isAutomated;

    GameObject agePanel;

    protected bool isUpgradePanelActive;
    bool isCompound;

    protected Recipe[] recipes;
    protected Recipe currentRecipe;
    protected List<BaseResources> tempResourceList;
    [SerializeField] protected List<GameObject> resourceIconListForCompounds; // Just quick access to icon for compounds
    private LTDescr[] compoundAnimations;
    private GameObject[] compoundAnimationIcons;

    // Scriptable object class variables
    [SerializeField] protected float collectTime;
    private string _name;
    private bool isLockedByContract;
    private bool isLockedByLevel;
    private bool isLockedByAge;
    private bool isUnlocked = true;
    private string resourceName;
    private BaseResources producedResource;
    private ItemType[] itemTypes;

    protected bool isRunning; // If there is no worker set it to false
    [SerializeField] protected WorkerType currentWorkerType = WorkerType.Standard;
    [SerializeField] protected WorkerType currentWorkerMode;
    private WorkerType prefferedWorkerType;
    private BigDouble currentWorkerCount;
    private BigDouble maxWorkerCount;
    private sbyte minWorkerCount;

    private BigDouble foodAmount;
    private BigDouble attackAmount;
    private BigDouble defenseAmount;
    private float housingAmount;
    [SerializeField] private float remainedCollectTime;
    private bool isCharging;
    private BigDouble outputValue;
    private BigDouble outputValueWhenStart; // Set this value just when unit start to produce so upgrades take effect in next loop
    private BigDouble pricePerProduct;
    private int unlockLevel;
    private float xpAmount;
    private BigDouble outputPerSecond;
    private int level;
    private BigDouble upgradeCost;
    private BigDouble incomePerSecond;
    private WorkingMode workingMode;
    protected LTDescr toolAnimation;

    // Attached gameobject transforms
    protected Transform resourceBoard; // Only for compound
    protected Transform statPanel;
    protected Transform itemTypePanel;
    protected Image sourceImage;
    protected Image icon;

    protected Image fillBar;

    protected TextMeshProUGUI outputTextOnIcon;
    protected TextMeshProUGUI nameLevelText; // Name - Level {Level}

    protected Button upgradeBtn;
    protected TextMeshProUGUI upgradeAmountText;
    protected Button workModeBtn;
    protected TextMeshProUGUI workModeText;

    protected RectTransform tool;
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
        set { level = value; SetNameLevelText(Level); }
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

    public BigDouble OutputPerSecond
    {
        get { return outputPerSecond; }
        set
        {
            outputPerSecond = OutputValue / collectTime;
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

    public WorkingMode WorkingMode
    {
        get { return workingMode; }
        set { workingMode = value; workModeText.text = ResourceManager.Instance.GetValidName(workingMode.ToString()); }
    }

    public BigDouble OutputValue 
    { 
        get => outputValue; 
        set 
        { 
            outputValue = value; 
            OutputPerSecond = OutputPerSecond; 
            outputTextOnIcon.text = outputValue.ToString();
            if (isUnlocked)
                ResourceManager.Instance.UpdateOutputStats(scriptableProductionBase.ageBelongsTo, outputPerSecond);
        } 
    }

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
    public BigDouble FoodAmount { get => foodAmount; set => foodAmount = value; }
    public BigDouble AttackAmount { get => attackAmount; set => attackAmount = value; }
    public BigDouble DefenseAmount { get => defenseAmount; set => defenseAmount = value; }
    public Dictionary<ContractBase, bool> ContractStatueCheckDictionary { get => contractStatueCheckDictionary; set => contractStatueCheckDictionary = value; }
    internal ContractBase[] Contracts { get => contracts; set => contracts = value; }
    public bool IsUnlocked 
    { 
        get => isUnlocked; 
        set 
        { 
            isUnlocked = value; 
            if (value == true) 
                PrestigeSystem.Instance.CheckProductionUnitUnlockingUI(this); 
        } 
    }
    public bool IsLockedByContract { get => isLockedByContract; set { isLockedByContract = value; CheckIfIsUnlocked(); } }
    public bool IsLockedByLevel { get => isLockedByLevel; set { isLockedByLevel = value; CheckIfIsUnlocked(); } }
    public bool IsLockedByAge { get => isLockedByAge; set { isLockedByAge = value; CheckIfIsUnlocked(); } }

    public BigDouble CurrentWorkerCount { get => currentWorkerCount; set => currentWorkerCount = value; }
    public WorkerType PrefferedWorkerType { get => prefferedWorkerType; set => prefferedWorkerType = value; }
    public bool IsRunning { get => isRunning; set => isRunning = value; }
    public BigDouble MaxWorkerCount { get => maxWorkerCount; set => maxWorkerCount = value; }
    public WorkerType CurrentWorkerType { get => currentWorkerType; set => currentWorkerType = value; }
    public float HousingAmount { get => housingAmount; set => housingAmount = value; }
    public sbyte MinWorkerCount { get => minWorkerCount; }
    public WorkerType CurrentWorkerMode { get => currentWorkerMode; set => currentWorkerMode = value; }
    public bool IsCompound { get => isCompound; set => isCompound = value; }

    #endregion

    protected virtual void Start()
    {
        if (scriptableProductionBase as ScriptableMine == null)
            isCompound = true;

        IsAutomated = true;
        agePanel = transform.parent.parent.parent.gameObject;

        #region DONE
        // Custom Events
        GameManager.Instance.OnLevelUp += OnLevelUp;
        GameManager.Instance.OnSubPanelChanged += OnSubPanelChanged;

        contracts = ContractManager.Instance.contracts.Where(c => c.contractRewardType == RewardType.unlockProductionUnit 
        && c.productsToUnlock.Contains(scriptableProductionBase)).ToArray();

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
        prefferedWorkerType = WorkerType.Gatherer; // TODO Change to scriptable object when setting
        minWorkerCount = scriptableProductionBase.minimumWorkerCount;
        maxWorkerCount = 10;
        currentWorkerCount = scriptableProductionBase.minimumWorkerCount;
        foodAmount = scriptableProductionBase.foodAmount;
        housingAmount = scriptableProductionBase.housingAmount;
        attackAmount = scriptableProductionBase.attackAmount;
        defenseAmount = scriptableProductionBase.defenseAmount;
        itemTypes = scriptableProductionBase.itemTypes;
        outputValue = scriptableProductionBase.outputValue;
        pricePerProduct = scriptableProductionBase.pricePerProduct;
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
            IsLockedByLevel = true;
        }
        if (contracts != null && contracts.Length > 0)
        {
            IsLockedByContract = true;
            if (IsLockedByLevel == false)
            {
                var lockText = Instantiate(GameManager.Instance.levelLock, transform);
                lockText.GetComponentInChildren<TextMeshProUGUI>().text = "UNLOCKED AT COMPLETION OF " + contracts[0].contractName + " Contract";
            }
        }
        else if ((int)scriptableProductionBase.ageBelongsTo > (int)PrestigeSystem.Instance.CurrentAge)
            IsLockedByAge = true;
        #endregion

        if (transform.Find("ResourceBoard") != null)
        {
            resourceBoard = transform.Find("ResourceBoard");
            resourceIconListForCompounds = new List<GameObject>();
            compoundAnimations = new LTDescr[currentRecipe.inputResources.Length];
            compoundAnimationIcons = new GameObject[currentRecipe.inputResources.Length];
        }

        statPanel = transform.Find("StatPanel");

        sourceImage = transform.Find("SourceImage").GetComponent<Image>();
        sourceImage.sprite = scriptableProductionBase.sourceImage;
        if (transform.Find("Tool") != null)
        {
            transform.Find("Tool").GetComponent<Image>().sprite = scriptableProductionBase.toolImage;
            tool = transform.Find("Tool").GetComponent<RectTransform>();
        }
        itemTypePanel = transform.Find("UnitTypePanel");

        // Add item type icons to panel
        for (int i = 0; i < itemTypes.Length; i++)
        {
            var res = Instantiate(ResourceManager.Instance.iconPrefab, itemTypePanel);
            Destroy(res.transform.GetChild(1).gameObject);

            // Set recttransform values
            var rect = res.transform.GetChild(0).GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            res.transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = ResourceManager.Instance.GetSpriteFromItemType(itemTypes[i]);
        }

        icon = transform.Find("Icon").GetComponent<Image>();
        icon.sprite = scriptableProductionBase.icon;
        fillBar = transform.Find("Icon").GetChild(0).GetComponent<Image>();
        fillBar.fillAmount = 0;
        fillBar.sprite = scriptableProductionBase.icon;


        outputTextOnIcon = fillBar.GetComponentInChildren<TextMeshProUGUI>();
        outputTextOnIcon.text = outputValue.ToString();
        nameLevelText = transform.Find("Background").GetComponentInChildren<TextMeshProUGUI>();
        SetNameLevelText(level);

        upgradeBtn = transform.Find("Upgrade_Btn").GetComponent<Button>();
        upgradeAmountText = upgradeBtn.GetComponentInChildren<TextMeshProUGUI>();
        upgradeAmountText.text = "$" + upgradeCost.ToString();
        upgradeBtn.onClick.AddListener(() => ShowUpgradePanel());
        workModeBtn = transform.Find("WorkingModeBtn").GetComponent<Button>();
        workModeText = workModeBtn.GetComponentInChildren<TextMeshProUGUI>();

        SetWorkModeColor();
        while (workingMode != WorkingMode.sell)
        {
            ChangeWorkingMode();
        }

        // Add minimum worker and assign it to this building.
        UpgradeSystem.Instance.SetWorkerBonus(minWorkerCount, (int)WorkerType.Standard, this, true);
    }

    // This starts animation when player changes pages, if animation wasn't active yet.
    private void OnSubPanelChanged(object sender, GameManager.OnSubPanelChangedEventArgs e)
    {
        if (isCharging && CheckIfPanelActive() && toolAnimation != null && toolAnimation.ratioPassed < .1f)
        {
            toolAnimation.setPassed(collectTime - remainedCollectTime);
            toolAnimation = TweenAnimation.Instance.MoveTool(tool.gameObject, remainedCollectTime);
        }
        //else if ( isCharging && !CheckIfPanelActive() )
        //    LeanTween.cancel(tool.gameObject);
    }

    void CheckIfIsUnlocked()
    {
        if (!isLockedByAge && !isLockedByContract && !isLockedByLevel)
            IsUnlocked = true;
        else
            isUnlocked = false;
    }

    void SetNameLevelText(long level)
    {
        nameLevelText.text = string.Format("{0} - LEVEL {1}", _name, level);
    }

    #region Event Methods

    internal void OnLevelUp(object sender, GameManager.OnLevelUpEventArgs e)
    {
        if (isLockedByLevel && unlockLevel == e.currentLevel)
        {
            Destroy(transform.Find("Level_Lock(Clone)").gameObject);
            IsLockedByLevel = false;
            // Check if there is contract for that unit hasn't completed yet
            if (IsLockedByContract)
            {
                for (int i = 0; i < contracts.Length; i++)
                {
                    if (contractStatueCheckDictionary[contracts[i]] == false)
                    {
                        var lockText = Instantiate(GameManager.Instance.levelLock, transform);
                        lockText.GetComponentInChildren<TextMeshProUGUI>().text = 
                            "UNLOCKED AT COMPLETION OF " + contracts[i].contractName + " Contract";
                        return;
                    }
                }

            }
            else
                IsLockedByContract = false;
        }
    }

    #endregion

    void CreateIconObj()
    {
        GameObject temp = new GameObject(resourceName);
        _icon = temp;
        temp.transform.SetParent(sourceImage.transform);
        var rect = temp.AddComponent<RectTransform>();
        temp.transform.localPosition = Vector3.zero;
        rect.sizeDelta = new Vector2(130, 130);
        rect.localScale = Vector3.one;
        temp.AddComponent<Image>().sprite = ResourceManager.Instance.GetSpriteFromResource(producedResource);

        _iconScaleAnimation = LeanTween.size(_icon.GetComponent<RectTransform>(), new Vector2(290, 290), .7f);
        _iconMoveAnimation = LeanTween.move(_icon, icon.transform.position, .7f)
            .setOnUpdateObject((float value, object obj) =>
            {
                _icon.transform.position = new Vector3(_icon.transform.position.x,
                    Mathf.Clamp(_icon.transform.position.y + 30, sourceImage.transform.position.y, icon.transform.position.y), 0);
            })
            .setOnComplete(() => { Destroy(_icon); _iconMoveAnimation = null; _iconScaleAnimation = null; });
    }

    /*
     * As example let there be 2 unit required for compound
     * Start time is remainedTime <= 2.5f
     * Every product will enter to tool image in .3f
     * After that there should be .15f * 2(Unit count) process time
     * Finished product will move in .3f<
     */

    void CreateIconForCompound(int index)
    {
        GameObject ico = compoundAnimationIcons[index];
        Transform parent = resourceBoard;
        Transform target = icon.transform;
        Debug.Log("Icon count is: " + resourceIconListForCompounds.Count);
        ico = Instantiate(resourceIconListForCompounds[index].transform.parent.gameObject, parent);
        ico.transform.localScale = Vector3.one;

        compoundAnimations[index] = LeanTween.move(ico, target.position, 1.6f)
            .setOnUpdateObject((float value, object obj) =>
            {
                ico.transform.position = new Vector3(ico.transform.position.x,
                    Mathf.Clamp(ico.transform.position.y, parent.position.y, target.position.y), 0);
            })
            .setOnComplete(() =>
            {
                Destroy(ico);
                compoundAnimations[index] = null;
            });
    }

    public void CustomUpdate()
    {
        if (isAutomated) Produce();

        if (isCharging && isRunning)
        {
            if (remainedCollectTime > 0)
            {
                remainedCollectTime -= Time.deltaTime * UpgradeSystem.Instance.ProductionSpeedMultiplier * 2;

                fillBar.fillAmount = ((collectTime - remainedCollectTime) / collectTime);

                if (!isCompound && remainedCollectTime <= 0.6f && _icon == null)
                {
                    CreateIconObj();
                }
            }
            else if (isCompound)
            {
                isCharging = false;
                ResourceManager.Instance.AddResource(producedResource,
                        outputValueWhenStart * UpgradeSystem.Instance.ProductionYieldMultiplier);
                if (workingMode == WorkingMode.production)
                {
                    if (CheckIfPanelActive())
                        StatSystem.Instance.PopupText(transform, outputValueWhenStart, _name);
                }
                else if (workingMode == WorkingMode.sell)
                {
                    SellResource();
                    if (CheckIfPanelActive())
                        StatSystem.Instance.PopupText(transform, PricePerProduct, "Gold");
                }

                tempResourceList = currentRecipe.inputResources.ToList();
                GameManager.Instance.AddXP(scriptableProductionBase.xpAmount);
                remainedCollectTime = 0;
                fillBar.fillAmount = 0;
            }
            else
            {
                //float currency;
                //float resource;
                AddResourceAndMoney(/*out currency, out resource*/);

                if (CheckIfPanelActive())
                {
                    if (workingMode == WorkingMode.production)
                        StatSystem.Instance.PopupText(transform, outputValueWhenStart, resourceName);
                    else if (workingMode == WorkingMode.sell)
                        StatSystem.Instance.PopupText(transform, (pricePerProduct * outputValueWhenStart), "Gold");
                }

                isCharging = false;
                remainedCollectTime = 0;
                fillBar.fillAmount = 0;
                LeanTween.cancel(tool.gameObject);
            }
        }
    }

    void AddResourceAndMoney(/*out float currency, out float resourceAmount*/)
    {
        switch (workingMode)
        {
            case WorkingMode.production:
                /*resourceAmount = */
                ResourceManager.Instance.AddResource(producedResource, outputValueWhenStart * UpgradeSystem.Instance.MiningYieldMultiplier);
                break;
            case WorkingMode.sell:
                ResourceManager.Instance.AddResource(producedResource, (outputValueWhenStart * UpgradeSystem.Instance.MiningYieldMultiplier));
                SellResource();
                break;
        }
        //resourceAmount = ResourceManager.Instance.AddResource(mine.baseResource,
        //(long)(mine.outputValueWhenStart * UpgradeSystem.Instance.MiningYieldMultiplier));
        //currency = incomeAmount;
        //ResourceManager.Instance.Currency += incomeAmount;
        GameManager.Instance.AddXP(XPAmount);
    }

    private void Produce()
    {
        if (isUnlocked && !isCharging && workingMode != WorkingMode.stopProduction)
        {
            if (isCompound)
            {
                for (int i = 0; i < currentRecipe.inputAmounts.Length; i++)
                {
                    int inputAmount = currentRecipe.inputAmounts[i];
                    BaseResources inputResource = currentRecipe.inputResources[i];
                    if (ResourceManager.Instance.GetResourceAmount(inputResource) 
                        >= (inputAmount / UpgradeSystem.Instance.ProductionEfficiencyMultiplier) 
                        && tempResourceList.Contains(inputResource))
                    {
                        //Debug.Log(string.Format("{0} added to {1} recipe", input.Resource, producedResource));
                        tempResourceList.Remove(inputResource);
                        ResourceManager.Instance.ConsumeResource(inputResource, 
                            (long)(inputAmount / UpgradeSystem.Instance.ProductionEfficiencyMultiplier));
                        CreateIconForCompound(i);
                        resourceIconListForCompounds[i].SetActive(true);
                    }
                    else if (ResourceManager.Instance.GetResourceAmount(inputResource) < inputAmount)
                    {
                        //Debug.Log("Not enough " + input.Resource);
                    }
                }

                if (tempResourceList.Count == 0)
                {
                    //Debug.Log(_name + " recipe completed");
                    outputValueWhenStart = outputValue;
                    isCharging = true;
                    remainedCollectTime = collectTime;

                    resourceIconListForCompounds.ForEach(r => r.SetActive(false));
                }
            }
            else
            {
                outputValueWhenStart = outputValue;
                if (CheckIfPanelActive())
                    toolAnimation = TweenAnimation.Instance.MoveTool(tool.gameObject, collectTime);
                isCharging = true;
                if (remainedCollectTime == 0)
                    remainedCollectTime = collectTime;
            }
        }
    }

    protected void SellResource()
    {
        ResourceManager.Instance.Currency += (outputValueWhenStart * pricePerProduct);
        ResourceManager.Instance.ConsumeResource(producedResource, outputValueWhenStart);
    }

    public BigDouble IdleEarn(int idleTime, bool isSelling, float multiplier = 1) // Multiplier for lowering rate in compounds
    {
        if (isSelling) // Selling Resource
        {
            BigDouble value = (outputValue * pricePerProduct * idleTime * multiplier);
            return new BigDouble(value.Mantissa, value.Exponent);
        }
        else // Producing Resource
        {
            BigDouble value = (outputValue * idleTime / collectTime * multiplier);
            return new BigDouble(Mathf.CeilToInt((float)value.Mantissa), value.Exponent);
        }
    }

    protected void ChangeWorkingMode(bool isMine = false)
    {
        if (GameManager.Instance.CurrentLevel >= 0)
        {
            workingMode = workingMode.Next();
            // Mines don't have stop production work mode
            if (isMine && workingMode == WorkingMode.stopProduction)
                workingMode = WorkingMode.Next();

            workModeText.text = ResourceManager.Instance.GetValidName(workingMode.ToString());
            SetWorkModeColor();
        }
    }

    /// <summary>
    /// When player change work mode
    /// button color change according to that
    /// </summary>
    protected void SetWorkModeColor()
    {
        if (workingMode == WorkingMode.production)
            workModeBtn.GetComponent<Image>().color = Color.green;
        else if (workingMode == WorkingMode.sell)
            workModeBtn.GetComponent<Image>().color = Color.white;
        else if (workingMode == WorkingMode.stopProduction)
            workModeBtn.GetComponent<Image>().color = Color.red;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isCharging)
        {
            remainedCollectTime -= .35f;
            if (isCompound)
            {
                for (int i = 0; i < compoundAnimations.Length; i++)
                {
                    if (compoundAnimations[i] != null)
                        compoundAnimations[i].time -= .3f;
                }
            }
            else
            {
                if (_iconMoveAnimation != null)
                    _iconMoveAnimation.time -= .3f;
                if (_iconScaleAnimation != null)
                    _iconScaleAnimation.time -= .3f;
            }
        }
        else
            Produce();
    }

    // upgradeValues store values that returned from SetUpgradePanel. By storing values, don't need to recalculate same values.
    (int level, BigDouble outputValue, float collectTime, BigDouble pricePerProduct, 
        BigDouble upgradeCost, BigDouble newMaxWorkerCount) upgradeValues;

    protected void ShowUpgradePanel()
    {
        UpgradeSystem.Instance.ShowUpgradePanel(this, SetUpgradePanel, Upgrade, IsUpgradePanelActive, UpgradeCost, Level);
    }

    void SetUpgradePanel(int levelUpgradeMultiplier)
    {
        upgradeValues = UpgradeSystem.Instance.SetUpgradePanel(levelUpgradeMultiplier, OutputValue, 
            Level, CollectTime, PricePerProduct, UpgradeCost, currentWorkerCount ,maxWorkerCount, _name);
    }

    protected void Upgrade()
    {
        BigDouble newUpgradeCost = upgradeValues.upgradeCost;

        if (ResourceManager.Instance.Currency >= newUpgradeCost)
        {
            ResourceManager.Instance.Currency -= newUpgradeCost;

            Level = upgradeValues.level;
            OutputValue = upgradeValues.outputValue;
            CollectTime = upgradeValues.collectTime;
            PricePerProduct = upgradeValues.pricePerProduct;
            MaxWorkerCount = upgradeValues.newMaxWorkerCount;
            ResourceManager.Instance.SetNewPricePerProduct(producedResource, pricePerProduct);
            UpgradeCost = UpgradeSystem.Instance.GetNewUpgradeCost(newUpgradeCost, Level);
            SetUpgradePanel(UpgradeSystem.Instance.upgradeMultiplier);
            UpgradeSystem.Instance.workerRequiredUnitsList.Add(this);
        }
    }

    private void SetWorkerBonus()
    {

    }

    /// <summary>
    /// Check if currently this production panel active
    /// If not stop all animation on non visible elements
    /// </summary>
    /// <returns>True if parent panel is active</returns>
    private bool CheckIfPanelActive()
    {
        if ( GameManager.Instance.VisiblePanelForPlayer == transform.parent.parent.parent.parent.parent.gameObject &&
                        GameManager.Instance.VisibleSubPanelForPlayer == agePanel)
        {
            return true;
        }
        else
            return false;
    }
}