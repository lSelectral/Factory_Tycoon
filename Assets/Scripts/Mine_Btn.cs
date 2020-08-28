using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Mine_Btn : MonoBehaviour
{
    public ScriptableMine mine;

    const float incomeConstant = 1.4f;
    const float upgradeCostConstant = 1.55f;

    [SerializeField] private bool isAutomated;

    [SerializeField] float collectTime;
    float remainedCollectTime;
    bool isCharging;
    bool isReadyToCollect;

    Sprite backgroundImage;

    Image fillBar;
    TextMeshProUGUI mineNameText;
    Button btn;
    Button upgradeBtn;
    Button sellBtn;
    TextMeshProUGUI upgradeAmountText;
    TextMeshProUGUI mineLevelText;
    TextMeshProUGUI sellAmountText;
    TextMeshProUGUI availableResourceAmountText;
    TextMeshProUGUI outputPerSecondText;
    float outputPerSecond;

    float incomeAmount;
    private int mineLevel;
    float upgradeAmount;
    float xpAmount;

    float incomePerSecond;

    WorkingMode workingMode;

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

    public float UpgradeAmount
    {
        get { return upgradeAmount; }
        set { upgradeAmount = value; }
    }

    public float IncomeAmount
    {
        get { return incomeAmount; }
        set { incomeAmount = value; }
    }

    public bool IsAutomated
    {
        get { return isAutomated; }
        set { isAutomated = value; }
    }

    public float OutputPerSecond
    {
        get { return outputPerSecond; }
        set { outputPerSecond = value; outputPerSecondText.text = string.Format("+{0} {1}", (mine.outputValue / collectTime)* UpgradeSystem.Instance.MiningYieldMultiplier, mine.resourceName) + "/Sec"; }
    }

    public float XPAmount
    {
        get { return xpAmount; }
        set { xpAmount = value; }
    }

    void Start()
    {
        // Custom Events
        GameManager.Instance.OnLevelUp += OnLevelUp;
        ResourceManager.Instance.OnResourceAmountChanged += OnResourceAmountChanged;
        UpgradeSystem.Instance.OnMiningSpeedChanged += Instance_OnMiningSpeedChanged;
        UpgradeSystem.Instance.OnMiningYieldChanged += Instance_OnMiningYieldChanged;
        UpgradeSystem.Instance.OnEarnedCoinMultiplierChanged += Instance_OnEarnedCoinMultiplierChanged;
        UpgradeSystem.Instance.OnEarnedXpMultiplierChanged += Instance_OnEarnedXpMultiplierChanged;
        ContractManager.Instance.OnContractComplete += OnContractComplete;

        // Set lock text
        if (mine.unlockLevel > GameManager.Instance.CurrentLevel)
        {
            var lockText = Instantiate(GameManager.Instance.levelLock, transform);
            lockText.GetComponentInChildren<TextMeshProUGUI>().text = "UNLOCKED AT LEVEL " + mine.unlockLevel.ToString();
        }
        else if (mine.isLockedByContract)
        {
            var lockText = Instantiate(GameManager.Instance.levelLock, transform);
            lockText.GetComponentInChildren<TextMeshProUGUI>().text = "UNLOCKED AT COMPLETION OF " + mine.lockedByContract.contractName + " Contract";
        }

        collectTime = mine.collectTime;
        incomeAmount = mine.incomeAmount;
        backgroundImage = mine.backgroundImage;

        workingMode = WorkingMode.production;

        fillBar = transform.Find("GameObject").transform.Find("Outline").transform.Find("Fill").GetComponent<Image>();
        mineNameText = transform.Find("GameObject").transform.Find("Mine_Name").GetComponent<TextMeshProUGUI>();
        btn = transform.Find("Button").GetComponent<Button>();
        outputPerSecondText = btn.GetComponentInChildren<TextMeshProUGUI>();
        outputPerSecondText.text = string.Format("+{0} {1}", (mine.outputValue/collectTime)* UpgradeSystem.Instance.MiningYieldMultiplier, mine.resourceName) + "/Sec";
        upgradeBtn = transform.Find("Upgrade_Btn").GetComponent<Button>();
        upgradeAmountText = transform.Find("Upgrade_Btn").Find("upgradeText").GetComponent<TextMeshProUGUI>();
        mineLevelText = transform.Find("LevelText").GetComponent<TextMeshProUGUI>();
        sellBtn = transform.Find("Sell_Btn").GetComponent<Button>();
        sellBtn.onClick.AddListener(() => SellButton());
        sellAmountText = sellBtn.transform.GetChild(0).GetComponent<TextMeshProUGUI>();

        if (backgroundImage != null)
            transform.Find("Button").GetComponent<Image>().sprite = backgroundImage;

        transform.Find("GameObject").Find("Icon").GetComponent<Image>().sprite = ResourceManager.Instance.GetSpriteFromResource(mine.baseResource);

        if (remainedCollectTime >0)
        {
            fillBar.fillAmount = ((collectTime - remainedCollectTime) / collectTime);
        }
        else
        {
            fillBar.fillAmount = 0;
        }
        mineNameText.text = mine.Name;
        btn.onClick.AddListener(() => CollectMine());
        btn.onClick.AddListener(() => { if (isCharging) remainedCollectTime -= .35f; });
        upgradeBtn.onClick.AddListener(() => UpgradeMine());
        upgradeAmount = 15f;
        xpAmount = mine.xpAmount;
        upgradeAmountText.text = "$0";
        mineLevelText.text = "Level 0";
        sellAmountText.text = "SELL => 0";

        incomePerSecond = (IncomeAmount / collectTime) * UpgradeSystem.Instance.EarnedCoinMultiplier;
        StatSystem.Instance.CurrencyPerSecond += incomePerSecond;
    }

    private void OnContractComplete(object sender, ContractManager.OnContractCompleteEventArgs e)
    {
        if (mine.lockedByContract == e.contract)
            mine.isLockedByContract = false;
    }

    private void Instance_OnEarnedXpMultiplierChanged(object sender, UpgradeSystem.OnEarnedXpMultiplierChangedEventArgs e)
    {
        xpAmount *= e.earnedXpMultiplier;
    }

    private void Instance_OnEarnedCoinMultiplierChanged(object sender, UpgradeSystem.OnEarnedCoinMultiplierChangedEventArgs e)
    {
        sellAmountText.text = "SELL => " + (mine.pricePerProduct * (ResourceManager.Instance.GetResourceAmount(mine.baseResource) * UpgradeSystem.Instance.EarnedCoinMultiplier)).ToString();
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
        sellAmountText.text = "SELL => " + (mine.pricePerProduct * (ResourceManager.Instance.GetResourceAmount(mine.baseResource) * UpgradeSystem.Instance.EarnedCoinMultiplier)).ToString();
    }

    private void OnLevelUp(object sender, GameManager.OnLevelUpEventArgs e)
    {
        if (transform.Find("Level_Lock(Clone)") != null && mine.unlockLevel == e.currentLevel)
        {
            Destroy(transform.Find("Level_Lock(Clone)").gameObject);
            if (mine.isLockedByContract)
            {
                var lockText = Instantiate(GameManager.Instance.levelLock, transform);
                lockText.GetComponentInChildren<TextMeshProUGUI>().text = "UNLOCKED AT COMPLETION OF " + mine.lockedByContract.contractName + " Contract";
            }
        }
        else if (mine.isLockedByContract && mine.unlockLevel > e.currentLevel)
        {
            var lockText = Instantiate(GameManager.Instance.levelLock, transform);
            lockText.GetComponentInChildren<TextMeshProUGUI>().text = "UNLOCKED AT COMPLETION OF " + mine.lockedByContract.contractName + " Contract";
            mine.isLockedByContract = false;
        }
    }

    void Update()
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
                float currency;
                float resource;
                AddResourceAndMoney(out currency, out resource) ;
                isCharging = false;
                remainedCollectTime = 0;
                fillBar.fillAmount = 0;
            }
        }
    }

    public void AddResourceAndMoney(out float currency, out float resourceAmount)
    {
        switch (workingMode)
        {
            case WorkingMode.production:
                resourceAmount = ResourceManager.Instance.AddResource(mine.baseResource, (long)(mine.outputValue * UpgradeSystem.Instance.MiningYieldMultiplier));
                break;
            case WorkingMode.sell:

                break;
        }
        resourceAmount = ResourceManager.Instance.AddResource(mine.baseResource, (long)(mine.outputValue * UpgradeSystem.Instance.MiningYieldMultiplier));
        currency = incomeAmount;
        ResourceManager.Instance.Currency += incomeAmount;
        GameManager.Instance.AddXP(XPAmount* UpgradeSystem.Instance.EarnedXPMultiplier);
    }

    public void CollectMine()
    {
        if (!isCharging)
        {
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
        incomePerSecond = IncomeAmount / collectTime;
        StatSystem.Instance.CurrencyPerSecond+= incomePerSecond;
        outputPerSecondText.text = string.Format("+{0:0.#} {1}", (mine.outputValue / collectTime) * UpgradeSystem.Instance.MiningYieldMultiplier, mine.resourceName) + "/Sec";
    }

    public float IdleEarn(int idleTime)
    {
        return incomePerSecond * idleTime;
    }

    void SellButton()
    {
        ResourceManager.Instance.Currency += (ResourceManager.Instance.GetResourceAmount(mine.baseResource) *1f * (mine.pricePerProduct *1f));
        ResourceManager.Instance.ConsumeResource(mine.baseResource, ResourceManager.Instance.GetResourceAmount(mine.baseResource));
    }

    void ChangeWorkingMode()
    {
        workingMode.Next();
    }
}
