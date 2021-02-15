using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Mines and factories prefab instantiater class
/// </summary>
public class ProductionManager : Singleton<ProductionManager>
{
    public ScriptableProductionBase[] scriptableProductionUnitList;
    public List<GameObject> instantiatedProductionUnits;

    public GameObject mainPanel;
    // Hide objects on screen but still work on background
    [SerializeField] Transform temporaryMovementPanel;
    [SerializeField] Transform contentHolderPrefab;

    [SerializeField] GameObject collapsablePanelPrefab;

    [SerializeField] GameObject minePrefab;
    [SerializeField] GameObject compoundPrefab;

    // Add tier containers to this list for force layout refreshing.
    List<GameObject> tierSeperatedContainers = new List<GameObject>();

    void InstantiateProductionUnit(ScriptableProductionBase unit)
    {
        // TODO Remove this if statement on release. This is just for quick testing
        //if (unit.ageBelongsTo != Age._0_StoneAge) return;

        GameObject collapsablePanel = null;

        var _contentHolder = mainPanel.transform.Find(unit.ageBelongsTo.ToString()).GetChild(0);
        Transform tierSeperatedContainer = null;

        if (_contentHolder.Find(unit.tier.ToString()) == null)
        {
            collapsablePanel = Instantiate(collapsablePanelPrefab, _contentHolder);
            collapsablePanel.GetComponentInChildren<TextMeshProUGUI>().text = "Tier " + unit.tier.ToString().Substring(unit.tier.ToString().Length - 1);
            collapsablePanel.name = unit.tier.ToString();

            tierSeperatedContainer = Instantiate(contentHolderPrefab, _contentHolder).transform;
            tierSeperatedContainer.gameObject.name = "container_" + unit.tier;
        }
        else
        {
            collapsablePanel = _contentHolder.Find(unit.tier.ToString()).gameObject;
            tierSeperatedContainer = _contentHolder.transform.GetChild(_contentHolder.Find(unit.tier.ToString()).GetSiblingIndex() + 1);
        }

        GameObject _unit = null;
        if (unit as ScriptableCompound != null)
        {
            _unit = Instantiate(compoundPrefab, tierSeperatedContainer);
            _unit.GetComponent<Compounds>().scriptableCompound = unit as ScriptableCompound;
            instantiatedProductionUnits.Add(_unit);
        }
        else if (unit as ScriptableMine != null)
        {
            _unit = Instantiate(minePrefab, tierSeperatedContainer);
            _unit.GetComponent<Mine_Btn>().scriptableMine = unit as ScriptableMine;
            instantiatedProductionUnits.Add(_unit);
        }
        _unit.GetComponent<ProductionBase>().scriptableProductionBase = unit;

        if (!tierSeperatedContainers.Contains(tierSeperatedContainer.gameObject))
            tierSeperatedContainers.Add(tierSeperatedContainer.gameObject);

        var expandBtn = collapsablePanel.transform.Find("ExpandBtn").GetComponent<Button>();
        var collapseBtn = collapsablePanel.transform.Find("CollapseBtn").GetComponent<Button>();

        expandBtn.onClick.RemoveAllListeners();
        expandBtn.onClick.AddListener(() =>
        {
            temporaryMovementPanel.Find("container_" + unit.tier).SetParent(_contentHolder);
            tierSeperatedContainer.SetSiblingIndex(_contentHolder.Find(unit.tier.ToString()).GetSiblingIndex() + 1);
        });

        collapseBtn.onClick.RemoveAllListeners();
        collapseBtn.onClick.AddListener(() =>
        {
            _contentHolder.transform.Find("container_" + unit.tier).SetParent(temporaryMovementPanel);
        });
    }

    private void Awake()
    {
        for (int i = 0; i < scriptableProductionUnitList.Length; i++)
        {
            InstantiateProductionUnit(scriptableProductionUnitList[i]);
        }

        // UNITY UI don't refresh layout when add element sometimes. So we are forcing it to refresh.
        for (int i = 0; i < tierSeperatedContainers.Count; i++)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(tierSeperatedContainers[i].GetComponent<RectTransform>());
        }

    }

    #region Helper Functions
    public BigDouble GET_OPTIMAL_PRICE_PER_PRODUCT(ProductionBase productionUnit)
    {
        BigDouble pricePerProduct = 0f;
        var inputResources = productionUnit.CurrentRecipe.inputResources;
        var inputAmounts = productionUnit.CurrentRecipe.inputAmounts;
        for (int i = 0; i < inputResources.Length; i++)
        {
            var _productionUnit = GetProductionUnitFromResource(inputResources[i]);
            pricePerProduct += _productionUnit.PricePerProduct * inputAmounts[i] * _productionUnit.CollectTime / _productionUnit.CollectTime;
        }
        return pricePerProduct * UpgradeSystem.Instance.COMPOUND_PRICE_MULTIPLIER;
    }
    public BigDouble GET_OPTIMAL_PRICE_PER_PRODUCT_EDITOR(ScriptableProductionBase compound)
    {
        BigDouble pricePerProduct = 0f;
        var inputResources = compound.recipes[0].inputResources;
        var inputAmounts = compound.recipes[0].inputAmounts;

        for (int i = 0; i < inputResources.Length; i++)
        {
            var productionUnit = GetScriptableProductionUnitFromResource(inputResources[i]);
            pricePerProduct += productionUnit.pricePerProduct * inputAmounts[i] * compound.collectTime / productionUnit.collectTime;
        }
        return pricePerProduct * UpgradeSystem.Instance.COMPOUND_PRICE_MULTIPLIER;
    }

    public BigDouble GetIncomePerSecondForEDITOR(BaseResources[] resources, int[] inputAmounts)
    {
        BigDouble incomePerSecond = 0f;
        foreach (BaseResources res in resources)
        {
            incomePerSecond += GetScriptableProductionUnitFromResource(res).incomePerSecond;
        }
        return incomePerSecond * UpgradeSystem.Instance.INCOME_PRICE_MULTIPLIER;
    }

    public ProductionBase GetProductionUnitFromResource(BaseResources res)
    {
        var q = instantiatedProductionUnits.Where(u => u.GetComponent<ProductionBase>().ProducedResource == res).FirstOrDefault();
        return q.GetComponent<ProductionBase>();
    }

    public ScriptableProductionBase GetScriptableProductionUnitFromResource(BaseResources res)
    {
        return scriptableProductionUnitList.Where(u => u != null && u.product == res).FirstOrDefault();
    }
    #endregion
}

[Serializable]
public class Recipe
{
    [SearchableEnum]
    public BaseResources[] inputResources;
    public int[] inputAmounts;
    public float collectTime;
    public int outputAmount;

    public Recipe()
    {
        this.inputAmounts = new int[] { 0 };
        this.inputResources = new BaseResources[] { BaseResources._0_berry };
    }
}

public enum WorkingMode
{
    /// <summary>
    /// Use produced materials for further and advanced production
    /// </summary>
    production = 1,
    /// <summary>
    /// Sell produced products
    /// </summary>
    sell = 2,
    /// <summary>
    /// Stop the production for recipes that require another resource
    /// so this recipe will not consume any resource if player not wants it.
    /// </summary>
    stopProduction = 3,
}


public enum Age
{
    /// <summary>
    /// Start of humanity. That's the point where humanity begin to sharpen their survival insticts.
    /// </summary>
    _0_StoneAge = 0,
    _1_BronzeAge = 1,
    _2_IronAge = 2,
    _3_MiddleAge = 3,
    _4_IndustrialAge = 4,
    _5_ModernAge = 5,
    _6_SpaceAge = 6,
    _7_RoboticAge = 7,
    _8_IntergalacticAge = 8,
    _9_GalacticWarAge = 9,
}

public enum Tier
{
    Tier_1,
    Tier_2,
    Tier_3,
    Tier_4,
    Tier_5,
}