using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Mine_Btn : ProductionBase
{
    public ScriptableMine scriptableMine;

    protected override void Start()
    {
        base.Start();

        workModeBtn.onClick.AddListener(() => ChangeWorkingMode(true));

        Transform itemTypePanel = transform.Find("UnitTypePanel");

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

        upgradeAmountText.text = "$" + upgradeCost.ToString();

        upgradeBtn.onClick.AddListener(() => ShowUpgradePanel());
        workingMode = WorkingMode.sell;
        workModeText.text = ResourceManager.Instance.GetValidName(workingMode.ToString());

        SetWorkModeColor();

        IncomePerSecond = (outputPerSecond * pricePerProduct);
        StatSystem.Instance.CurrencyPerSecond += incomePerSecond;
    }

    #region Event Methods

    private void Instance_OnMiningYieldChanged(object sender, UpgradeSystem.OnMiningYieldChangedEventArgs e)
    {
        OutputValue = (long)(outputValue * e.miningYield);
    }

    private void Instance_OnMiningSpeedChanged(object sender, UpgradeSystem.OnMiningSpeedChangedEventArgs e)
    {
        CollectTime /= e.miningSpeed;
    }

    #endregion

    protected override void Update()
    {
        if (unlockLevel <= GameManager.Instance.CurrentLevel && !isLockedByContract)
        {
            if (isAutomated) Produce();

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

                    if (CheckIfPanelActive())
                    {
                        if (workingMode == WorkingMode.production)
                            StatSystem.Instance.PopupText(transform, OutputValue, resourceName);
                        else if (workingMode == WorkingMode.sell)
                            StatSystem.Instance.PopupText(transform, pricePerProduct, "Gold");
                    }

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
                /*resourceAmount = */ResourceManager.Instance.AddResource(producedResource, new BigDouble(outputValue * UpgradeSystem.Instance.MiningYieldMultiplier,0));
                break;
            case WorkingMode.sell:
                ResourceManager.Instance.AddResource(producedResource, new BigDouble(outputValue * UpgradeSystem.Instance.MiningYieldMultiplier,0));
                SellResource();
                break;
        }
        //resourceAmount = ResourceManager.Instance.AddResource(mine.baseResource, (long)(mine.outputValue * UpgradeSystem.Instance.MiningYieldMultiplier));
        //currency = incomeAmount;
        //ResourceManager.Instance.Currency += incomeAmount;
        GameManager.Instance.AddXP(XPAmount);
    }

    public BigDouble IdleEarn(int idleTime)
    {
        return (incomePerSecond * idleTime * UpgradeSystem.Instance.EarnedCoinMultiplier);
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        Produce();
        if (isCharging) remainedCollectTime -= .35f;
    }

    
}