using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Runtime.InteropServices.ComTypes;

public class Mine_Btn : MonoBehaviour
{
    public ScriptableMine scriptableMine;

    const float incomeConstant = 1.4f;
    const float upgradeCostConstant = 1.55f;

    [SerializeField] private bool isAutomated;

    // Scriptable object class variables
    [SerializeField] float collectTime;
    string mineName;
    string resourceName;
    BaseResources producedResource;
    float remainedCollectTime;
    bool isCharging;
    bool isReadyToCollect;
    int outputValue;
    int pricePerProduct;
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
    Button mainBtn;
    Button upgradeBtn;
    Button workModeBtn;
    TextMeshProUGUI upgradeAmountText;
    TextMeshProUGUI mineLevelText;
    TextMeshProUGUI workModeText;
    TextMeshProUGUI outputPerSecondText;
    RectTransform tool;
    float outputPerSecond;

    private int mineLevel;
    double upgradeAmount;
    float incomePerSecond;
    WorkingMode workingMode;

    LTDescr miningAnimation;

    #region Properties
    public float RemainedCollectTime
    {
        get { return remainedCollectTime; }
        set { remainedCollectTime = value; }
    }
    public float CollectTime
    {
        get { return collectTime; }
        set { collectTime = value; }
    }

    public int MineLevel
    {
        get { return mineLevel; }
        set { mineLevel = value; mineLevelText.text = "LEVEL " + mineLevel.ToString(); }
    }

    public double UpgradeAmount
    {
        get { return upgradeAmount; }
        set { upgradeAmount = value; upgradeAmountText.text = "$" + ResourceManager.Instance.CurrencyToString(upgradeAmount); }
    }

    public bool IsAutomated
    {
        get { return isAutomated; }
        set { isAutomated = value; }
    }

    public float OutputPerSecond
    {
        get { return outputPerSecond; }
        set { outputPerSecond = value; outputPerSecondText.text = string.Format("+{0} {1}", (outputValue / collectTime) * UpgradeSystem.Instance.MiningYieldMultiplier, resourceName) + "/Sec"; }
    }

    public float XPAmount
    {
        get { return xpAmount; }
        set { xpAmount = value; }
    }

    public int PricePerProduct
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
        //ContractManager.Instance.OnContractComplete += OnContractComplete;

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

        workingMode = WorkingMode.production;

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
        mainBtn = transform.Find("Button").GetComponent<Button>();
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
        transform.Find("GameObject").Find("Icon").GetComponent<Image>().sprite = ResourceManager.Instance.GetSpriteFromResource(producedResource);
        infoPanel = transform.Find("InfoPanel");
        infoBtn = transform.Find("Info_Btn").GetComponent<Button>();
        infoBtn.onClick.AddListener(() => { TweenAnimation.Instance.ShowHideElement(infoPanel.gameObject); infoBtn.transform.SetAsLastSibling(); });
        if (remainedCollectTime >0)
        {
            fillBar.fillAmount = ((collectTime - remainedCollectTime) / collectTime);
        }
        else
        {
            fillBar.fillAmount = 0;
        }
        mainBtn.onClick.AddListener(() => CollectMine());
        mainBtn.onClick.AddListener(() => { if (isCharging) remainedCollectTime -= .35f; });
        upgradeBtn.onClick.AddListener(() => UpgradeMine());
        upgradeAmount = 15f;
        upgradeAmountText.text = "$0";
        mineLevelText.text = "Level 0";
        workModeText.text = ResourceManager.Instance.GetValidName(workingMode.ToString());

        incomePerSecond = (outputPerSecond*pricePerProduct) * UpgradeSystem.Instance.EarnedCoinMultiplier;
        StatSystem.Instance.CurrencyPerSecond += incomePerSecond;

        //IsAutomated = true;
    }

    private void Instance_OnEarnedXpMultiplierChanged(object sender, UpgradeSystem.OnEarnedXpMultiplierChangedEventArgs e)
    {
        xpAmount *= e.earnedXpMultiplier;
    }

    private void Instance_OnEarnedCoinMultiplierChanged(object sender, UpgradeSystem.OnEarnedCoinMultiplierChangedEventArgs e)
    {
    }

    private void Instance_OnMiningYieldChanged(object sender, UpgradeSystem.OnMiningYieldChangedEventArgs e)
    {
        OutputPerSecond *= e.miningYield;
    }

    private void Instance_OnMiningSpeedChanged(object sender, UpgradeSystem.OnMiningSpeedChangedEventArgs e)
    {
        OutputPerSecond *= e.miningSpeed;
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
            case WorkingMode.production:
                /*resourceAmount = */ResourceManager.Instance.AddResource(producedResource, (long)(outputValue * UpgradeSystem.Instance.MiningYieldMultiplier));
                break;
            case WorkingMode.sell:
                ResourceManager.Instance.AddResource(producedResource, (long)(outputValue * UpgradeSystem.Instance.MiningYieldMultiplier));
                Sell();
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
        StatSystem.Instance.CurrencyPerSecond-= incomePerSecond;
        MineLevel += 1;
        upgradeAmount *= (Mathf.Pow(upgradeCostConstant,mineLevel+1));
        if (mineLevel % 10 == 0)
            collectTime -= collectTime / 10;
        upgradeAmountText.text = "$" + ResourceManager.Instance.CurrencyToString(upgradeAmount);
        incomePerSecond = OutputPerSecond*pricePerProduct;
        StatSystem.Instance.CurrencyPerSecond+= incomePerSecond;
        outputPerSecondText.text = string.Format("+{0:0.#} {1}", (outputValue / collectTime) * UpgradeSystem.Instance.MiningYieldMultiplier, resourceName) + "/Sec";
    }

    void Sell()
    {
        ResourceManager.Instance.Currency += outputValue * 1f * (pricePerProduct *1f);
        ResourceManager.Instance.ConsumeResource(producedResource, outputValue);
    }

    void ChangeWorkingMode()
    {
        Array a = Enum.GetValues(typeof(WorkingMode));
        int j = 0;
        for (int i = 0; i < a.Length; i++)
        {
            j = i + 1;
            if ((WorkingMode)(a.GetValue(i)) == workingMode)
                break;
        }
        if (j < a.Length)
            workingMode = (WorkingMode)a.GetValue(j);
        else
            workingMode = (WorkingMode)a.GetValue(j - a.Length);
        workModeText.text = ResourceManager.Instance.GetValidName(workingMode.ToString());
    }
}
