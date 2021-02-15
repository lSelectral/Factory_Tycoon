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


        itemTypes = scriptableCompound.itemTypes;
        fillBar = transform.Find("FillBar").transform.Find("Fill").GetComponent<Image>();
        levelText = transform.Find("Main_Panel").Find("levelText").GetComponent<TextMeshProUGUI>();
        nameText = transform.Find("Main_Panel").transform.Find("Mine_Name").GetComponent<TextMeshProUGUI>();
        icon = transform.Find("Main_Panel").Find("Icon").GetComponent<Image>();
        upgradeBtn = transform.Find("Upgrade_Btn").GetComponent<Button>();
        upgradeBtn.onClick.AddListener(() => ShowUpgradePanel());
        upgradeAmountText = upgradeBtn.GetComponentInChildren<TextMeshProUGUI>();
        icon.sprite = scriptableCompound.icon;

        if (scriptableCompound.sourceImage != null)
            transform.Find("Source").GetComponent<Image>().sprite = scriptableCompound.sourceImage;
        //if (scriptableCompound.toolImage != null)
        //    transform.Find("").GetComponent<Image>().sprite = scriptableCompound.toolImage;

        

        workModeBtn = transform.Find("WorkingModeBtn").GetComponent<Button>();
        workModeText = workModeBtn.GetComponentInChildren<TextMeshProUGUI>();
        workModeBtn.onClick.AddListener(() => ChangeWorkingMode());

        SetWorkModeColor();

        base.Start();
        tempResourceList = currentRecipe.inputResources.ToList();
        nameText.text = _name;

        upgradeAmountText.text = upgradeCost.ToString();

        subResourceIcons = transform.Find("Main_Panel").Find("Sub_Resource_Icons");
        if (subResourceIcons != null)
        {
            for (int i = 0; i < currentRecipe.inputResources.Length; i++)
            {
                var icon = Instantiate(ResourceManager.Instance.iconPrefab, subResourceIcons);
                icon.transform.Find("Frame").Find("Icon").GetComponent<Image>().sprite = ResourceManager.Instance.GetSpriteFromResource(currentRecipe.inputResources[i]);
                icon.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = currentRecipe.inputAmounts[i].ToString();
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

    internal override void Update()
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