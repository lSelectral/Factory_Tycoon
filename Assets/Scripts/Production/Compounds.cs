using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine.EventSystems;

public class Compounds : ProductionBase
{
    public ScriptableCompound scriptableCompound;

    Image icon;
    protected override void Start()
    {
        // Events
        ResourceManager.Instance.OnPricePerProductChanged += OnPricePerProductChanged;
        UpgradeSystem.Instance.OnProductionEfficiencyChanged += OnProductionEfficiencyChanged;
        UpgradeSystem.Instance.OnProductionSpeedChanged += OnProductionSpeedChanged;
        UpgradeSystem.Instance.OnProductionYieldChanged += OnProductionYieldChanged;

        base.Start();

        itemTypes = scriptableCompound.itemTypes;
        upgradeBtn.onClick.AddListener(() => ShowUpgradePanel());

        workModeBtn.onClick.AddListener(() => ChangeWorkingMode());

        SetWorkModeColor();

        tempResourceList = currentRecipe.inputResources.ToList();
        upgradeAmountText.text = upgradeCost.ToString();

        if (resourceBoard != null)
        {
            for (int i = 0; i < currentRecipe.inputResources.Length; i++)
            {
                resourceBoard.GetChild(i).GetComponent<Image>().sprite = ResourceManager.Instance.GetSpriteFromResource(currentRecipe.inputResources[i]);
                resourceBoard.GetChild(i).GetComponentInChildren<TextMeshProUGUI>().text = currentRecipe.inputAmounts[i].ToString();
                resourceBoard.GetChild(i).gameObject.SetActive(true);
            }
        }
    }

    #region Event Methods
    private void OnPricePerProductChanged(object sender, ResourceManager.OnPricePerProductChangedEventArgs e)
    {
        PricePerProduct = ProductionManager.Instance.GET_OPTIMAL_PRICE_PER_PRODUCT(this) + scriptableCompound.basePricePerProduct;
    }

    private void OnProductionYieldChanged(object sender, UpgradeSystem.OnProductionYieldChangedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void OnProductionSpeedChanged(object sender, UpgradeSystem.OnProductionSpeedChangedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void OnProductionEfficiencyChanged(object sender, UpgradeSystem.OnProductionEfficiencyChangedEventArgs e)
    {
        throw new NotImplementedException();
    }

    #endregion

    protected override void Update()
    {
        if (isAutomated)
        {
            Produce();
        }

        if (isCharging)
        {
            if (remainedCollectTime > 0)
            {
                remainedCollectTime -= Time.deltaTime * UpgradeSystem.Instance.ProductionSpeedMultiplier;
                fillBar.fillAmount = ((collectTime - remainedCollectTime) / collectTime);
            }
            else
            {
                isCharging = false;

                if (workingMode == WorkingMode.production)
                {
                    ResourceManager.Instance.AddResource(producedResource, new BigDouble(outputValue * UpgradeSystem.Instance.ProductionYieldMultiplier, 0));
                    if (CheckIfPanelActive())
                        StatSystem.Instance.PopupText(transform, outputValue, _name);
                }
                else if (workingMode == WorkingMode.sell)
                {
                    ResourceManager.Instance.AddResource(producedResource, new BigDouble(outputValue * UpgradeSystem.Instance.ProductionYieldMultiplier, 0));
                    SellResource();
                    if (CheckIfPanelActive())
                        StatSystem.Instance.PopupText(transform, PricePerProduct, "Gold");
                }

                tempResourceList = currentRecipe.inputResources.ToList();
                GameManager.Instance.AddXP(scriptableCompound.xpAmount);
                remainedCollectTime = 0;
                fillBar.fillAmount = 0;
            }
        }
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        Produce();
        if (isCharging) RemainedCollectTime -= .35f;
    }
}