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

    private BaseResources[] inputResources;
    private int[] inputAmounts;
    long attackAmount;
    [SerializeField] private List<BaseResources> tempResourceList;
    private float remainedBuildTime;

    #region Properties
    public List<BaseResources> RemainedResources
    {
        get { return tempResourceList; }
        set { tempResourceList = value; }
    }

    public float RemainedBuildTime
    {
        get { return remainedBuildTime; }
        set { remainedBuildTime = value; }
    }

    public BaseResources[] InputResources { get => inputResources; set => inputResources = value; }
    public long AttackAmount { get => attackAmount; set => attackAmount = value; }
    public int[] InputAmounts { get => inputAmounts; set => inputAmounts = value; }
    #endregion

    Image icon;
    Transform subResourceIcons;

    internal override void Start()
    {
        // TODO ONLY FOR DEBUG REMOVE IT
        IsAutomated = true;

        // Events
        ResourceManager.Instance.OnPricePerProductChanged += OnPricePerProductChanged;
        UpgradeSystem.Instance.OnProductionEfficiencyChanged += OnProductionEfficiencyChanged;
        UpgradeSystem.Instance.OnProductionSpeedChanged += OnProductionSpeedChanged;
        UpgradeSystem.Instance.OnProductionYieldChanged += OnProductionYieldChanged;

        inputResources = scriptableCompound.inputResources;
        tempResourceList = scriptableCompound.inputResources.ToList();
        itemTypes = scriptableCompound.itemTypes;
        attackAmount = scriptableCompound.attackAmount;
        inputAmounts = scriptableCompound.inputAmounts;

        fillBar = transform.Find("FillBar").transform.Find("Fill").GetComponent<Image>();
        nameText = transform.Find("Main_Panel").transform.Find("Mine_Name").GetComponent<TextMeshProUGUI>();
        icon = transform.Find("Main_Panel").Find("Icon").GetComponent<Image>();
        upgradeBtn = transform.Find("Upgrade_Btn").GetComponent<Button>();
        upgradeBtn.onClick.AddListener(() => ShowUpgradePanel());
        upgradeAmountText = upgradeBtn.GetComponentInChildren<TextMeshProUGUI>();
        icon.sprite = scriptableCompound.icon;
        subResourceIcons = transform.Find("Main_Panel").Find("Sub_Resource_Icons");
        if (subResourceIcons != null)
        {
            
            for (int i = 0; i < inputResources.Length; i++)
            {
                var icon = Instantiate(ResourceManager.Instance.iconPrefab, subResourceIcons);
                icon.transform.Find("Image").GetComponent<Image>().sprite = ResourceManager.Instance.GetSpriteFromResource(inputResources[i]);
                icon.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = inputAmounts[i].ToString();
            }
        }

        nameText.text = _name;
        workModeBtn = transform.Find("WorkingModeBtn").GetComponent<Button>();
        workModeText = workModeBtn.GetComponentInChildren<TextMeshProUGUI>();
        workModeBtn.onClick.AddListener(() => ChangeWorkingMode());

        SetWorkModeColor();

        base.Start();

        upgradeAmountText.text = ResourceManager.Instance.CurrencyToString(upgradeCost);
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
            if (remainedBuildTime > 0)
            {
                remainedBuildTime -= Time.deltaTime * UpgradeSystem.Instance.ProductionSpeedMultiplier;
                fillBar.fillAmount = ((collectTime - remainedBuildTime) / collectTime);
            }
            else
            {
                isCharging = false;

                if (workingMode == WorkingMode.production)
                {
                    ResourceManager.Instance.AddResource(producedResource, (outputValue * UpgradeSystem.Instance.ProductionYieldMultiplier));
                    StatSystem.Instance.PopupText(transform, outputValue, _name);
                }
                else if (workingMode == WorkingMode.sell)
                {
                    ResourceManager.Instance.AddResource(producedResource, (outputValue * UpgradeSystem.Instance.ProductionYieldMultiplier));
                    SellResource();
                    StatSystem.Instance.PopupText(transform, PricePerProduct, "Gold");
                }

                tempResourceList = inputResources.ToList();
                GameManager.Instance.AddXP(scriptableCompound.xpAmount);
                remainedBuildTime = 0;
                fillBar.fillAmount = 0;
            }
        }
    }

    void Produce()
    {
        if (!isCharging && workingMode != WorkingMode.stopProduction && transform.Find("Level_Lock(Clone)") == null)
        {
            var inputs = inputResources.Zip(inputAmounts, (resource, amount) => (Resource: resource, Amount: amount));
            foreach (var input in inputs)
            {
                if (ResourceManager.Instance.GetResourceAmount(input.Resource) >= input.Amount / UpgradeSystem.Instance.ProductionEfficiencyMultiplier && tempResourceList.Contains(input.Resource))
                {
                    //Debug.Log(string.Format("{0} added to {1} recipe", input.Resource, producedResource));
                    //Debug.Log(input.Resource + " added to recipe");
                    tempResourceList.Remove(input.Resource);
                    ResourceManager.Instance.ConsumeResource(input.Resource, (long)(input.Amount / UpgradeSystem.Instance.ProductionEfficiencyMultiplier));

                    if (subResourceIcons != null)
                        subResourceIcons.GetChild(Array.IndexOf(inputResources, input.Resource)).GetChild(0).GetChild(0).gameObject.SetActive(true);
                }
                else if (ResourceManager.Instance.GetResourceAmount(input.Resource) < input.Amount)
                {
                    //Debug.Log("Not enough " + input.Resource);
                }
            }

            if (tempResourceList.Count == 0)
            {
                Debug.Log(_name + " recipe completed");
                isCharging = true;
                remainedBuildTime = collectTime;

                foreach (var input in inputs)
                {
                    subResourceIcons.GetChild(Array.IndexOf(inputResources, input.Resource)).GetChild(0).GetChild(0).gameObject.SetActive(false);
                }
            }
        }
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        Produce();
        if (isCharging) RemainedBuildTime -= .35f;
    }
}