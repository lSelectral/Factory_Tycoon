using System;
using System.Linq;
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
    [SerializeField] protected ContractBase[] contracts;
    [SerializeField] protected bool isAutomated;

    GameObject mainProductionPanel;
    GameObject agePanel;

    protected bool isUpgradePanelActive;

    protected Recipe[] recipes;
    protected Recipe currentRecipe;
    protected List<BaseResources> tempResourceList;
    protected List<GameObject> resourceIconListForCompounds; // Just quick access to icon for compounds

    // Scriptable object class variables
    [SerializeField] protected float collectTime;
    protected string _name;
    protected bool isUnlocked = false;
    protected string resourceName;
    protected BaseResources producedResource;
    protected ItemType[] itemTypes;
    protected long foodAmount;
    protected long attackAmount;
    [SerializeField] protected float remainedCollectTime;
    protected bool isCharging;
    protected long outputValue;
    protected BigDouble pricePerProduct;
    protected int unlockLevel;
    protected float xpAmount;
    protected float outputPerSecond;
    protected int level;
    protected BigDouble upgradeCost;
    protected BigDouble incomePerSecond;
    protected WorkingMode workingMode;
    protected LTDescr toolAnimation;

    // Attached gameobject transforms
    protected Transform resourceBoard; // Only for compound
    protected Transform statPanel;
    protected Transform itemTypePanel;

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

    public WorkingMode WorkingMode
    {
        get { return workingMode; }
        set { workingMode = value; workModeText.text = ResourceManager.Instance.GetValidName(workingMode.ToString()); }
    }

    public long OutputValue { get => outputValue; set { outputValue = value; OutputPerSecond = OutputPerSecond; outputTextOnIcon.text = outputValue.ToString(); } }

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
    public bool IsUnlocked { get => isUnlocked; set => isUnlocked = value; }

    #endregion

    protected virtual void Start()
    {
        IsAutomated = true;
        mainProductionPanel = transform.parent.parent.parent.parent.parent.gameObject;
        agePanel = transform.parent.parent.parent.gameObject;

        #region DONE
        // Custom Events
        GameManager.Instance.OnLevelUp += OnLevelUp;
        ResourceManager.Instance.OnCurrencyChanged += OnCurrencyChanged;
        UpgradeSystem.Instance.OnEarnedCoinMultiplierChanged += Instance_OnEarnedCoinMultiplierChanged;
        UpgradeSystem.Instance.OnEarnedXpMultiplierChanged += Instance_OnEarnedXpMultiplierChanged;

        contracts = ContractManager.Instance.contracts.Where(c => c.contractRewardType == ContractRewardType.unlockProductionUnit 
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
        foodAmount = scriptableProductionBase.foodAmount;
        attackAmount = scriptableProductionBase.attackAmount;
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
        }
        else if (contracts != null && contracts.Length > 0)
        {
            var lockText = Instantiate(GameManager.Instance.levelLock, transform);
            lockText.GetComponentInChildren<TextMeshProUGUI>().text = "UNLOCKED AT COMPLETION OF " + contracts[0].contractName + " Contract";
        }
        else 
            isUnlocked = true;
        #endregion

        if (transform.Find("ResourceBoard") != null)
            resourceBoard = transform.Find("ResourceBoard");
        statPanel = transform.Find("StatPanel");

        transform.Find("SourceImage").GetComponent<Image>().sprite = scriptableProductionBase.sourceImage;
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

        transform.Find("Icon").GetComponent<Image>().sprite = scriptableProductionBase.icon;
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
    }

    void SetNameLevelText(long level)
    {
        nameLevelText.text = string.Format("{0} - LEVEL {1}", _name, level);
    }

    protected void OnCurrencyChanged(object sender, ResourceManager.OnCurrencyChangedEventArgs e)
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
            else
                isUnlocked = true;
        }
    }

    #endregion

    protected virtual void Update()
    {
        
    }

    protected void Produce()
    {
        if (!isCharging && isUnlocked && workingMode != WorkingMode.stopProduction)
        {
            // If true it needs resource to start produce else it produce without input
            if (currentRecipe.inputResources.Length > 0 && currentRecipe.inputAmounts[0] > 0 )
            {
                for (int i = 0; i < currentRecipe.inputAmounts.Length; i++)
                {
                    int inputAmount = currentRecipe.inputAmounts[i];
                    BaseResources inputResource = currentRecipe.inputResources[i];
                    var q = ResourceManager.Instance.GetResourceAmount(inputResource);
                    if (ResourceManager.Instance.GetResourceAmount(inputResource) 
                        >= (inputAmount / UpgradeSystem.Instance.ProductionEfficiencyMultiplier) 
                        && tempResourceList.Contains(inputResource))
                    {
                        //Debug.Log(string.Format("{0} added to {1} recipe", input.Resource, producedResource));
                        tempResourceList.Remove(inputResource);
                        ResourceManager.Instance.ConsumeResource(inputResource, 
                            (long)(inputAmount / UpgradeSystem.Instance.ProductionEfficiencyMultiplier));

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
                    isCharging = true;
                    remainedCollectTime = collectTime;

                    resourceIconListForCompounds.ForEach(r => r.SetActive(false));
                }
            }
            else
            {
                if (CheckIfPanelActive())
                    toolAnimation = TweenAnimation.Instance.MoveTool(tool.gameObject, collectTime);
                isCharging = true;
                if (remainedCollectTime == 0)
                    remainedCollectTime = collectTime;
            }
        }
    }

    protected virtual void SellResource()
    {
        ResourceManager.Instance.Currency += (outputValue * 1f * (pricePerProduct * 1f));
        ResourceManager.Instance.ConsumeResource(producedResource, outputValue);
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
    protected void SetWorkModeColor()
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
        if (isCharging) remainedCollectTime -= .35f;
        Produce();
    }

    protected virtual void ShowUpgradePanel()
    {
        UpgradeSystem.Instance.ShowUpgradePanel(SetUpgradePanel, Upgrade, IsUpgradePanelActive, UpgradeCost, Level);
    }

    protected virtual void SetUpgradePanel(int levelUpgradeMultiplier)
    {
        UpgradeSystem.Instance.SetUpgradePanel(levelUpgradeMultiplier, OutputValue, Level, CollectTime, PricePerProduct, UpgradeCost, _name);
    }

    protected virtual void Upgrade()
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