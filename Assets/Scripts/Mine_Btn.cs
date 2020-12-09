using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;

public class Mine_Btn : MonoBehaviour, IPointerClickHandler
{
    #region Class Variables

    public ScriptableMine scriptableMine;

    float miningSpeedUpgrade;
    float miningEfficiencyUpgrade;

    [SerializeField] private bool isAutomated;

    #region CONSTANTS

    const float PRICE_PER_PRODUCT_MULTIPLER_LOW_50 = 3.18f;

    const float UPGRADE_POWER_MULTIPLIER_LOW_50 = 1.003f;

    const float PRICE_PER_PRODUCT_MULTIPLIR_HIGH_50 = 3.7f;

    const float UPGRADE_POWER_MULTIPLIER_HIGH_50 = 1f;

    const float UPGRADE_BASE_MULTIPLIER = 1.63f;

    #endregion

    // Scriptable object class variables
    [SerializeField] float collectTime;
    string mineName;
    string resourceName;
    BaseResources producedResource;
    float remainedCollectTime;
    bool isCharging;
    bool isReadyToCollect;
    long outputValue;
    float pricePerProduct;
    Sprite backgroundImage;
    int unlockLevel;
    float xpAmount;
    bool isLockedByContract;
    ContractBase lockedByContract;

    // Attached gameobject transforms
    Transform infoPanel;
    Button infoBtn;
    Image fillBar;
    TextMeshProUGUI mineNameText;
    Transform mainBtn;
    Button upgradeBtn;
    Button workModeBtn;
    TextMeshProUGUI upgradeAmountText;
    TextMeshProUGUI mineLevelText;
    TextMeshProUGUI workModeText;
    TextMeshProUGUI outputPerSecondText;
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
        set { collectTime = value; UpdateInfoPanel(); OutputPerSecond = OutputPerSecond; UpdateInfoPanel(); }
    }

    public int MineLevel
    {
        get { return mineLevel; }
        set { mineLevel = value; mineLevelText.text = "LEVEL " + mineLevel.ToString(); UpdateInfoPanel(); }
    }

    public double UpgradeCost
    {
        get { return upgradeCost; }
        set { upgradeCost = value; upgradeAmountText.text = "$" + ResourceManager.Instance.CurrencyToString(upgradeCost); UpdateInfoPanel(); }
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
            outputPerSecondText.text = string.Format("+{0} {1}", (outputValue / collectTime) * UpgradeSystem.Instance.MiningYieldMultiplier, resourceName) + "/Sec";
            UpdateInfoPanel();
        }
    }

    public float XPAmount
    {
        get { return xpAmount; }
        set { xpAmount = value; UpdateInfoPanel(); }
    }

    public float PricePerProduct
    {
        get { return pricePerProduct; }
        set { pricePerProduct = value; UpdateInfoPanel(); }
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

    public long OutputValue { get => outputValue; set { outputValue = value; OutputPerSecond = OutputPerSecond; UpdateInfoPanel(); } }
    #endregion

    void Start()
    {
        // Custom Events
        GameManager.Instance.OnLevelUp += OnLevelUp;
        ResourceManager.Instance.OnResourceAmountChanged += OnResourceAmountChanged;
        UpgradeSystem.Instance.OnMiningSpeedChanged += Instance_OnMiningSpeedChanged;
        UpgradeSystem.Instance.OnMiningYieldChanged += Instance_OnMiningYieldChanged;
        UpgradeSystem.Instance.OnEarnedCoinMultiplierChanged += Instance_OnEarnedCoinMultiplierChanged;
        UpgradeSystem.Instance.OnEarnedXpMultiplierChanged += Instance_OnEarnedXpMultiplierChanged;

        collectTime = scriptableMine.collectTime;
        mineName = scriptableMine.Name;
        resourceName = scriptableMine.resourceName;
        producedResource = scriptableMine.baseResource;
        outputValue = scriptableMine.outputValue;
        pricePerProduct = scriptableMine.pricePerProduct;
        backgroundImage = scriptableMine.backgroundImage;
        unlockLevel = scriptableMine.unlockLevel;
        xpAmount = scriptableMine.xpAmount* 1f;
        isLockedByContract = scriptableMine.isLockedByContract;

        outputPerSecond = outputValue * 1f / collectTime;

        workingMode = MineWorkingMode.production;

        // Set lock text
        if (unlockLevel > GameManager.Instance.CurrentLevel)
        {
            var lockText = Instantiate(GameManager.Instance.levelLock, transform);
            lockText.GetComponentInChildren<TextMeshProUGUI>().text = "UNLOCKED AT LEVEL " + unlockLevel.ToString();
        }
        else if (isLockedByContract)
        {
            var lockText = Instantiate(GameManager.Instance.levelLock, transform);
            lockText.GetComponentInChildren<TextMeshProUGUI>().text = "UNLOCKED AT COMPLETION OF " + lockedByContract.contractName + " Contract";
        }

        // Set gameobject hierarchy
        fillBar = transform.Find("GameObject").transform.Find("Outline").transform.Find("Fill").GetComponent<Image>();
        mineNameText = transform.Find("GameObject").transform.Find("Mine_Name").GetComponent<TextMeshProUGUI>();
        mineNameText.text = mineName;
        mainBtn = transform.Find("Button");
        outputPerSecondText = mainBtn.GetComponentInChildren<TextMeshProUGUI>();
        outputPerSecondText.text = string.Format("+{0} {1}", (outputValue/collectTime)* UpgradeSystem.Instance.MiningYieldMultiplier, resourceName) + "/Sec";
        upgradeBtn = transform.Find("Upgrade_Btn").GetComponent<Button>();
        upgradeAmountText = transform.Find("Upgrade_Btn").Find("upgradeText").GetComponent<TextMeshProUGUI>();
        mineLevelText = transform.Find("LevelText").GetComponent<TextMeshProUGUI>();
        workModeBtn = transform.Find("Sell_Btn").GetComponent<Button>();
        workModeBtn.onClick.AddListener(() => ChangeWorkingMode());
        workModeText = workModeBtn.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        transform.Find("Button").Find("Image").GetComponent<Image>().sprite = backgroundImage;
        tool = transform.Find("Button").Find("Tool").GetComponent<RectTransform>();
        transform.Find("GameObject").Find("Icon").GetComponent<Image>().sprite = scriptableMine.icon;
        //transform.Find("GameObject").Find("Icon").GetComponent<Image>().sprite = ResourceManager.Instance.GetSpriteFromResource(producedResource);
        infoPanel = transform.Find("InfoPanel");
        infoBtn = transform.Find("Info_Btn").GetComponent<Button>();
        infoBtn.onClick.AddListener(() => { TweenAnimation.Instance.ShowHideElement(infoPanel.gameObject); infoBtn.transform.SetAsLastSibling(); });
        UpdateInfoPanel();
        if (remainedCollectTime >0)
        {
            fillBar.fillAmount = ((collectTime - remainedCollectTime) / collectTime);
        }
        else
        {
            fillBar.fillAmount = 0;
        }
        //mainBtn.onClick.AddListener(() => CollectMine());
        //mainBtn.onClick.AddListener(() => { if (isCharging) remainedCollectTime -= .35f; });
        upgradeBtn.onClick.AddListener(() => UpgradeMine());
        workingMode = MineWorkingMode.sell;
        mineLevelText.text = "Level 0";
        workModeText.text = ResourceManager.Instance.GetValidName(workingMode.ToString());

        SetWorkModeColor();

        incomePerSecond = (outputPerSecond*pricePerProduct) * UpgradeSystem.Instance.EarnedCoinMultiplier;
        StatSystem.Instance.CurrencyPerSecond += incomePerSecond;

        //IsAutomated = true;

        if (upgradeCost == 0)
            upgradeCost = 50;
        upgradeAmountText.text = "$" + ResourceManager.Instance.CurrencyToString(upgradeCost);
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
        UpdateInfoPanel();
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
            if (isLockedByContract)
            {
                var lockText = Instantiate(GameManager.Instance.levelLock, transform);
                lockText.GetComponentInChildren<TextMeshProUGUI>().text = "UNLOCKED AT COMPLETION OF " + lockedByContract.contractName + " Contract";
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
                    
                    //var text = new TextMeshProUGUI();
                    
                    ////text.transform.parent.SetParent(mainBtn.transform);
                    //text.text = string.Format("+{0} {1}", outputValue, ResourceManager.Instance.GetValidName(resourceName));
                    //text.raycastTarget = false;
                    //text.fontSize = 20;
                    ////text.enableAutoSizing = true;
                    //text.alignment = TextAlignmentOptions.Midline;
                    //text.autoSizeTextContainer = true;

                    isCharging = false;
                    remainedCollectTime = 0;
                    fillBar.fillAmount = 0;
                    LeanTween.cancel(tool.gameObject);
                    UpdateInfoPanel();
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
        if (!isCharging)
        {
            miningAnimation = TweenAnimation.Instance.MoveTool(tool.gameObject);
            isCharging = true;
            if (remainedCollectTime == 0)
                remainedCollectTime = collectTime;
        }
    }

    void UpgradeMine()
    {
        if (ResourceManager.Instance.Currency >= upgradeCost)
        {
            //StatSystem.Instance.CurrencyPerSecond -= incomePerSecond;

            //outputValue += 2;
            //OutputPerSecond = (outputValue / collectTime) * UpgradeSystem.Instance.MiningYieldMultiplier;
            //
            //incomePerSecond = OutputPerSecond * pricePerProduct;
            //StatSystem.Instance.CurrencyPerSecond += incomePerSecond;
            //outputPerSecondText.text = string.Format("+{0:0.#} {1}", (outputValue / collectTime) * UpgradeSystem.Instance.MiningYieldMultiplier, resourceName) + "/Sec";

            MineLevel += 1;
            ResourceManager.Instance.Currency -= upgradeCost;

            collectTime -= collectTime / 100;
            if (mineLevel % 2 == 0)
                outputValue++;
            if (mineLevel % 3 == 0)
                if (mineLevel <= 50)
                    pricePerProduct *= PRICE_PER_PRODUCT_MULTIPLER_LOW_50;
                else
                    pricePerProduct *= PRICE_PER_PRODUCT_MULTIPLIR_HIGH_50;

            
            UpgradeCost = upgradeCost * 2 * Mathf.Pow(1.007f, mineLevel);
            if (mineLevel <= 50)
                upgradeCost = upgradeCost * UPGRADE_BASE_MULTIPLIER * Mathf.Pow(UPGRADE_POWER_MULTIPLIER_LOW_50, mineLevel);
            else
                upgradeCost = upgradeCost * UPGRADE_BASE_MULTIPLIER * Mathf.Pow(UPGRADE_POWER_MULTIPLIER_HIGH_50, mineLevel);

            upgradeAmountText.text = "$" + ResourceManager.Instance.CurrencyToString(upgradeCost);
            UpdateInfoPanel();
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

    void UpdateInfoPanel()
    {
        infoPanel.Find("Top").Find("IronAmount").GetComponentInChildren<TextMeshProUGUI>().text = ResourceManager.Instance.CurrencyToString(ResourceManager.Instance.GetResourceAmount(scriptableMine.baseResource));
        infoPanel.Find("Top").Find("PerProductPrice").GetComponentInChildren<TextMeshProUGUI>().text = "$"+pricePerProduct.ToString("F2");
        infoPanel.Find("Bottom").Find("ProductionAmountPerSec").GetComponentInChildren<TextMeshProUGUI>().text = ResourceManager.Instance.CurrencyToString(OutputPerSecond)+"/Sec";
        infoPanel.Find("Bottom").Find("MineLevel").GetComponentInChildren<TextMeshProUGUI>().text = "Level " + mineLevel.ToString();
    }
}   
