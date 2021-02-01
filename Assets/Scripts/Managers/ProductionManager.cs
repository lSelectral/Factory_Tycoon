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
    public List<ScriptableMine> mineList;
    public List<ScriptableCompound> compoundList;
    public List<ScriptableProductionBase> productionUnitList;

    public GameObject mainPanel;
    // Hide objects on screen but still work on background
    public Transform temporaryMovementPanel;
    public Transform contentHolderPrefab;

    public GameObject collapsablePanelPrefab;

    public GameObject minePrefab;
    public GameObject compoundPrefab;

    public List<GameObject> instantiatedMines;
    public List<GameObject> instantiatedCompounds;

    UnityEngine.Object[] assets;

    public UnityEngine.Object[] Assets
    {
        get { return assets; }
    }

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
            instantiatedCompounds.Add(_unit);
        }
        else if (unit as ScriptableMine != null)
        {
            _unit = Instantiate(minePrefab, tierSeperatedContainer);
            _unit.GetComponent<Mine_Btn>().scriptableMine = unit as ScriptableMine;
            instantiatedMines.Add(_unit);
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
        assets = Resources.LoadAll("AGES");
        mineList = new List<ScriptableMine>();
        compoundList = new List<ScriptableCompound>();
        productionUnitList = new List<ScriptableProductionBase>();
        InitializeMineAndCompoundList(out mineList, out compoundList);

        for (int i = 0; i < assets.Length; i++)
        {
            if (assets[i] as ScriptableProductionBase != null)
            {
                productionUnitList.Add(assets[i] as ScriptableProductionBase);
                InstantiateProductionUnit(assets[i] as ScriptableProductionBase);
            }
        }

        // UNITY UI don't refresh layout when add element sometimes. So we are forcing it to refresh.
        for (int i = 0; i < tierSeperatedContainers.Count; i++)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(tierSeperatedContainers[i].GetComponent<RectTransform>());
        }

    }
    public float GET_OPTIMAL_PRICE_PER_PRODUCT(Compounds compound)
    {
        float pricePerProduct = 0f;
        foreach (BaseResources res in compound.InputResources)
        {
            var _mine = GetMineFromResource(res);
            if (_mine != null)
            {
                pricePerProduct += _mine.PricePerProduct * compound.InputAmounts[Array.IndexOf(compound.InputResources, res)] * compound.CollectTime / _mine.CollectTime;
            }

            var _compound = GetCompoundFromResource(res);
            if (_compound != null)
                pricePerProduct += _compound.PricePerProduct * compound.InputAmounts[Array.IndexOf(compound.InputResources, res)] * compound.CollectTime / _compound.CollectTime;
        }
        return pricePerProduct * UpgradeSystem.Instance.COMPOUND_PRICE_MULTIPLIER;
    }

    public float GET_OPTIMAL_PRICE_PER_PRODUCT_EDITOR(ScriptableCompound compound)
    {
        float pricePerProduct = 0f;
        foreach (BaseResources res in compound.inputResources)
        {
            ScriptableMine _mine = GetScriptableProductionUnitFromResource(res) as ScriptableMine;
            if (_mine != null)
            {
                pricePerProduct += _mine.pricePerProduct * compound.inputAmounts[Array.IndexOf(compound.inputResources, res)] * compound.collectTime / _mine.collectTime;
            }

            ScriptableCompound _compound = GetScriptableProductionUnitFromResource(res) as ScriptableCompound;
            if (_compound != null)
                pricePerProduct += _compound.pricePerProduct * compound.inputAmounts[Array.IndexOf(compound.inputResources, res)] * compound.collectTime / _compound.collectTime;
        }
        return pricePerProduct * UpgradeSystem.Instance.COMPOUND_PRICE_MULTIPLIER;
    }

    public float GetIncomePerSecondForEDITOR(BaseResources[] resources, int[] inputAmounts)
    {
        float incomePerSecond = 0f;
        foreach (BaseResources res in resources)
        {
            incomePerSecond += GetScriptableProductionUnitFromResource(res).incomePerSecond;
            //var _mine = GetScriptableProductionUnitFromResource(res);
            //if (_mine != null)
            //    incomePerSecond += _mine.incomePerSecond * inputAmounts[Array.IndexOf(resources, res)];

            //var _compound = GetScriptableCompoundFromResource(res);
            //if (_compound != null)
            //    incomePerSecond += _compound.incomePerSecond * inputAmounts[Array.IndexOf(resources, res)];
        }
        return incomePerSecond * UpgradeSystem.Instance.INCOME_PRICE_MULTIPLIER;
    }

    #region Helper Functions
    public void InitializeMineAndCompoundList(out List<ScriptableMine> _mineList, out List<ScriptableCompound> _compoundList)
    {
        assets = Resources.LoadAll("AGES");
        List<ScriptableCompound> compoundList = new List<ScriptableCompound>();
        List<ScriptableMine> mineList = new List<ScriptableMine>();

        ScriptableProductionBase[] units = assets.Where(a => (a as ScriptableProductionBase) != null).Cast<ScriptableProductionBase>().ToArray();

        for (int i = 0; i < assets.Length; i++)
        {
            var asset = assets[i];

            if (asset as ScriptableObject != null)
            {
                var sc = asset as ScriptableObject;
                if (sc.GetType() == typeof(ScriptableMine) && (sc as ScriptableMine).ageBelongsTo == Age._0_StoneAge)
                    mineList.Add(sc as ScriptableMine);
                else if (sc.GetType() == typeof(ScriptableCompound) && (sc as ScriptableCompound).ageBelongsTo == Age._0_StoneAge)
                    compoundList.Add(sc as ScriptableCompound);
            }
        }
        _mineList = mineList;
        _compoundList = compoundList;
    }

    ScriptableProductionBase[] GetProductionUnits()
    {
        var assets = Resources.LoadAll("AGES");
        return assets.Where(a => (a as ScriptableProductionBase) != null).Cast<ScriptableProductionBase>().ToArray();
    }


    public Mine_Btn GetMineFromResource(BaseResources res)
    {
        var q = instantiatedMines.Where(m => m.GetComponent<Mine_Btn>() != null && m.GetComponent<Mine_Btn>().ProducedResource == res).FirstOrDefault();
        return q != null ? q.GetComponent<Mine_Btn>() : null;
    }

    public ScriptableProductionBase GetScriptableProductionUnitFromResource(BaseResources res)
    {
        var q = GetProductionUnits().Where(u => u != null && u.product == res).FirstOrDefault();
        return q != null ? q : null;
    }

    public Compounds GetCompoundFromResource(BaseResources res)
    {
        var q = instantiatedCompounds.Where(c => c.GetComponent<Compounds>() != null && c.GetComponent<Compounds>().ProducedResource == res).FirstOrDefault();
        return q != null ? q.GetComponent<Compounds>() : null;
    }
    #endregion
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