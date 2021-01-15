using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Mine_Btn : ProductionBase
{
    public ScriptableMine scriptableMine;
    RectTransform tool;

    internal override void Start()
    {
        base.Start();

        // Set gameobject hierarchy
        fillBar = transform.Find("GameObject").transform.Find("Outline").transform.Find("Fill").GetComponent<Image>();
        nameText = transform.Find("GameObject").transform.Find("Mine_Name").GetComponent<TextMeshProUGUI>();
        nameText.text = _name;
        mainBtn = transform.Find("Button");
        upgradeBtn = transform.Find("Upgrade_Btn").GetComponent<Button>();
        upgradeAmountText = transform.Find("Upgrade_Btn").Find("upgradeText").GetComponent<TextMeshProUGUI>();
        levelText = transform.Find("LevelText").GetComponent<TextMeshProUGUI>();
        workModeBtn = transform.Find("Sell_Btn").GetComponent<Button>();
        workModeBtn.onClick.AddListener(() => ChangeWorkingMode(true));
        workModeText = workModeBtn.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        transform.Find("Button").Find("Background").GetComponent<Image>().sprite = backgroundImage;
        if (scriptableMine.sourceImage != null)
            transform.Find("Button").Find("SourceImage").GetComponent<Image>().sprite = scriptableMine.sourceImage;
        if (scriptableMine.toolImage != null)
            transform.Find("Button").Find("Tool").GetComponent<Image>().sprite = scriptableMine.toolImage;
        tool = transform.Find("Button").Find("Tool").GetComponent<RectTransform>();
        transform.Find("GameObject").Find("Icon").GetComponent<Image>().sprite = scriptableMine.icon;


        upgradeAmountText.text = "$" + ResourceManager.Instance.CurrencyToString(upgradeCost);

        //if (remainedCollectTime > 0)
        //{
        //    fillBar.fillAmount = ((collectTime - remainedCollectTime) / collectTime);
        //}
        //else
        //{
        //    fillBar.fillAmount = 0;
        //}

        upgradeBtn.onClick.AddListener(() => ShowUpgradePanel());
        workingMode = WorkingMode.sell;
        levelText.text = "Level " + level.ToString();
        workModeText.text = ResourceManager.Instance.GetValidName(workingMode.ToString());

        SetWorkModeColor();

        IncomePerSecond = (outputPerSecond * pricePerProduct);
        StatSystem.Instance.CurrencyPerSecond += incomePerSecond;
    }

    #region Event Methods

    private void Instance_OnMiningYieldChanged(object sender, UpgradeSystem.OnMiningYieldChangedEventArgs e)
    {
        OutputValue *= e.miningYield;
    }

    private void Instance_OnMiningSpeedChanged(object sender, UpgradeSystem.OnMiningSpeedChangedEventArgs e)
    {
        CollectTime /= e.miningSpeed;
    }

    #endregion

    internal override void Update()
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

                    if (workingMode == WorkingMode.production)
                        StatSystem.Instance.PopupText(transform, OutputValue, resourceName);
                    else if (workingMode == WorkingMode.sell)
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
            case WorkingMode.production:
                /*resourceAmount = */ResourceManager.Instance.AddResource(producedResource, (long)(outputValue * UpgradeSystem.Instance.MiningYieldMultiplier));
                break;
            case WorkingMode.sell:
                ResourceManager.Instance.AddResource(producedResource, (long)(outputValue * UpgradeSystem.Instance.MiningYieldMultiplier));
                SellResource();
                break;
        }
        //resourceAmount = ResourceManager.Instance.AddResource(mine.baseResource, (long)(mine.outputValue * UpgradeSystem.Instance.MiningYieldMultiplier));
        //currency = incomeAmount;
        //ResourceManager.Instance.Currency += incomeAmount;
        GameManager.Instance.AddXP(XPAmount);
    }

    public double IdleEarn(int idleTime)
    {
        return incomePerSecond * idleTime * UpgradeSystem.Instance.EarnedCoinMultiplier;
    }

    public void CollectMine()
    {
        if (!isCharging && transform.Find("Level_Lock(Clone)") == null)
        {
            toolAnimation = TweenAnimation.Instance.MoveTool(tool.gameObject);
            isCharging = true;
            if (remainedCollectTime == 0)
                remainedCollectTime = collectTime;
        }
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        CollectMine();
        if (isCharging) remainedCollectTime -= .35f;
    }

    
}