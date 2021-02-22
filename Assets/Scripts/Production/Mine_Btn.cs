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
        workingMode = WorkingMode.sell;
        workModeText.text = ResourceManager.Instance.GetValidName(workingMode.ToString());

        IncomePerSecond = (outputPerSecond * pricePerProduct);
        StatSystem.Instance.CurrencyPerSecond += incomePerSecond;
    }

    #region Event Methods

    #endregion

    protected override void Update()
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
}