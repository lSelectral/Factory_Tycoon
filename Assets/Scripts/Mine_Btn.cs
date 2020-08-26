using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;
using System.Runtime.InteropServices;

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
    float incomeAmount;

    Sprite backgroundImage;

    Image fillBar;
    TextMeshProUGUI mineNameText;
    Button btn;
    Button upgradeBtn;
    TextMeshProUGUI upgradeAmountText;
    TextMeshProUGUI mineLevelText;

    private int mineLevel;
    float upgradeAmount;

    float incomePerSecond;

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

    void Start()
    {
        collectTime = mine.collectTime;

        GameManager.Instance.OnLevelUp += OnLevelUp;
        if (mine.unlockLevel > GameManager.Instance.CurrentLevel)
        {
            var lockText = Instantiate(GameManager.Instance.levelLock, transform);
            lockText.GetComponentInChildren<TextMeshProUGUI>().text = "UNLOCKED AT LEVEL " + mine.unlockLevel.ToString();
        }

        incomeAmount = mine.incomeAmount;
        backgroundImage = mine.backgroundImage;

        fillBar = transform.Find("GameObject").transform.Find("Outline").transform.Find("Fill").GetComponent<Image>();
        mineNameText = transform.Find("GameObject").transform.Find("Mine_Name").GetComponent<TextMeshProUGUI>();
        btn = transform.Find("Button").GetComponent<Button>();
        btn.GetComponentInChildren<TextMeshProUGUI>().text = string.Format("+{0} {1}", mine.outputValue, mine.resourceName);
        upgradeBtn = transform.Find("Upgrade_Btn").GetComponent<Button>();
        upgradeAmountText = transform.Find("Upgrade_Btn").Find("upgradeText").GetComponent<TextMeshProUGUI>();
        mineLevelText = transform.Find("LevelText").GetComponent<TextMeshProUGUI>();

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
        upgradeAmountText.text = "$0";
        mineLevelText.text = "Level 0";

        incomePerSecond = IncomeAmount / collectTime;
        StatSystem.Instance.CurrencyPerSecond += incomePerSecond;
    }

    private void OnLevelUp(object sender, GameManager.OnLevelUpEventArgs e)
    {
        if (transform.Find("Level_Lock(Clone)") != null && mine.unlockLevel == e.currentLevel)
        {
            Debug.Log("Unlocked " + mine.Name);
            Destroy(transform.Find("Level_Lock(Clone)").gameObject);
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
                remainedCollectTime -= Time.deltaTime * UpgradeSystem.Instance.miningSpeedMultiplier;
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
        resourceAmount = ResourceManager.Instance.AddResource(mine.baseResource, (long)(mine.outputValue * UpgradeSystem.Instance.miningYieldMultiplier));
        currency = incomeAmount;
        ResourceManager.Instance.Currency += incomeAmount;
        GameManager.Instance.AddXP(mine.xpAmount);
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
    }

    public float IdleEarn(int idleTime)
    {
        return incomePerSecond * idleTime;
    }
}
