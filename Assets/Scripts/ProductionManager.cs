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

    public GameObject mainPanel;
    // Hide objects on screen but still work on background
    public Transform temporaryMovementPanel;
    public Transform contentHolderPrefab;

    public GameObject collapsablePanelPrefab;

    public GameObject minePrefab;
    public GameObject compoundPrefab;

    public List<GameObject> instantiatedMines;
    public List<GameObject> instantiatedCompounds;

    public Sprite smelterSprite;
    public Sprite assemblerSprite;
    public Sprite manufacturerSprite;

    UnityEngine.Object[] assets;

    public UnityEngine.Object[] Assets
    {
        get { return assets; }
    }

    /// <summary>
    /// Instantiate mine prefab and create expand and collapse button for tier groups
    /// </summary>
    /// <param name="mine"></param>
    void InstantiateMine(ScriptableMine mine)
    {
        var _contentHolder = mainPanel.transform.Find(mine.ageBelongsTo.ToString()).GetChild(0);

        Transform tierSeperatedContainer = null;


        if (_contentHolder.Find(mine.tier.ToString()) == null)
        {
            var collapsablePanel = Instantiate(collapsablePanelPrefab, _contentHolder);
            collapsablePanel.GetComponentInChildren<TextMeshProUGUI>().text = "Tier " + mine.tier.ToString().Substring(mine.tier.ToString().Length - 1);
            collapsablePanel.name = mine.tier.ToString();

            tierSeperatedContainer = Instantiate(contentHolderPrefab, _contentHolder).transform;
            tierSeperatedContainer.gameObject.name = "container_" + mine.tier;
        }
        else
            tierSeperatedContainer = _contentHolder.transform.GetChild(_contentHolder.Find(mine.tier.ToString()).GetSiblingIndex() + 1);

        var _mine = Instantiate(minePrefab, tierSeperatedContainer);
        _mine.GetComponent<Mine_Btn>().scriptableMine = mine;
        instantiatedMines.Add(_mine);

        var expandBtn = _contentHolder.Find(mine.tier.ToString()).Find("Image").Find("ExpandBtn").GetComponent<Button>();
        var collapseBtn = _contentHolder.Find(mine.tier.ToString()).Find("Image").Find("CollapseBtn").GetComponent<Button>();

        expandBtn.onClick.RemoveAllListeners();
        expandBtn.onClick.AddListener(() => 
        { 
            temporaryMovementPanel.Find("container_"+mine.tier).SetParent(_contentHolder);
            tierSeperatedContainer.SetSiblingIndex(_contentHolder.Find(mine.tier.ToString()).GetSiblingIndex()+1);
        });

        collapseBtn.onClick.RemoveAllListeners();
        collapseBtn.onClick.AddListener(() => 
        {
            _contentHolder.transform.Find("container_" + mine.tier).SetParent(temporaryMovementPanel);
        });
    }

    void InstantiateCompound(ScriptableCompound compound)
    {
        var _contentHolder = mainPanel.transform.Find(compound.ageBelongsTo.ToString()).GetChild(0);

        Transform tierSeperatedContainer = null;


        if (_contentHolder.Find(compound.tier.ToString()) == null)
        {
            var collapsablePanel = Instantiate(collapsablePanelPrefab, _contentHolder);
            collapsablePanel.GetComponentInChildren<TextMeshProUGUI>().text = "Tier " + compound.tier.ToString().Substring(compound.tier.ToString().Length - 1);
            collapsablePanel.name = compound.tier.ToString();

            tierSeperatedContainer = Instantiate(contentHolderPrefab, _contentHolder).transform;
            tierSeperatedContainer.gameObject.name = "container_" + compound.tier;
        }
        else
            tierSeperatedContainer = _contentHolder.transform.GetChild(_contentHolder.Find(compound.tier.ToString()).GetSiblingIndex() + 1);

        var _compound = Instantiate(compoundPrefab, tierSeperatedContainer);
        _compound.GetComponent<Compounds>().scriptableCompound = compound;
        instantiatedCompounds.Add(_compound);

        var expandBtn = _contentHolder.Find(compound.tier.ToString()).Find("Image").Find("ExpandBtn").GetComponent<Button>();
        var collapseBtn = _contentHolder.Find(compound.tier.ToString()).Find("Image").Find("CollapseBtn").GetComponent<Button>();

        expandBtn.onClick.RemoveAllListeners();
        expandBtn.onClick.AddListener(() =>
        {
            temporaryMovementPanel.Find("container_" + compound.tier).SetParent(_contentHolder);
            tierSeperatedContainer.SetSiblingIndex(_contentHolder.Find(compound.tier.ToString()).GetSiblingIndex() + 1);
        });

        collapseBtn.onClick.RemoveAllListeners();
        collapseBtn.onClick.AddListener(() =>
        {
            _contentHolder.transform.Find("container_" + compound.tier).SetParent(temporaryMovementPanel);
        });
    }

    private void Awake()
    {
        assets = Resources.LoadAll("AGES");
        mineList = new List<ScriptableMine>();
        compoundList = new List<ScriptableCompound>();
        InitializeMineAndCompoundList(out mineList, out compoundList);

        for (int i = 0; i < mineList.Count; i++)
        {
            InstantiateMine(mineList[i]);
        }

        for (int j = 0; j < compoundList.Count; j++)
        {
            InstantiateCompound(compoundList[j]);
        }
    }

    /// <summary>
    /// // Formula is => ( (A1*O+A2*O+...+An*C) * COMPOUND_PRICE_MULTIPLIER ) / C    ***  IN COMPOUND CLASS, there is base price per product too. This formula + base price  ***
    /// Where A is every sub product delta price and O is output amount and C is production time of compound
    /// Delta price is net income per second => Price Per Product * Output Amount / Collect Time
    /// </summary>
    /// <param name="resources"></param>
    /// <param name="inputAmounts"></param>
    /// <param name="collectTime"></param>
    /// <returns></returns>
    /// <see cref="Compounds"/>
    //public float GetPricePerProductForCompound(BaseResources[] resources, int[] inputAmounts, float collectTime)
    //{
    //    float pricePerProduct = 0f;

    //    foreach (BaseResources res in resources)
    //    {
    //        var _mine = GetMineFromResource(res);
    //        if (_mine != null)
    //            pricePerProduct += (inputAmounts[Array.IndexOf(resources, res)] * _mine.PricePerProduct * collectTime / _mine.CollectTime);

    //        var _compound = GetCompoundFromResource(res);
    //        if (_compound != null)
    //            pricePerProduct += (inputAmounts[Array.IndexOf(resources, res)] * _compound.PricePerProduct * collectTime / _compound.BuildTime);
    //    }
    //    return pricePerProduct * UpgradeSystem.Instance.COMPOUND_PRICE_MULTIPLIER / collectTime;
    //}

    public float GET_OPTIMAL_PRICE_PER_PRODUCT(Compounds compound)
    {
        float pricePerProduct = 0f;
        foreach (BaseResources res in compound.InputResources)
        {
            var _mine = GetMineFromResource(res);
            if (_mine != null)
            {
                pricePerProduct += _mine.PricePerProduct * compound.InputAmounts[Array.IndexOf(compound.InputResources, res)] * compound.BuildTime / _mine.CollectTime;
            }

            var _compound = GetCompoundFromResource(res);
            if (_compound != null)
                pricePerProduct += _compound.PricePerProduct * compound.InputAmounts[Array.IndexOf(compound.InputResources, res)] * compound.BuildTime / _compound.BuildTime;
        }
        return pricePerProduct * UpgradeSystem.Instance.COMPOUND_PRICE_MULTIPLIER;
    }

    public float GET_OPTIMAL_PRICE_PER_PRODUCT_EDITOR(ScriptableCompound compound)
    {
        float pricePerProduct = 0f;
        foreach (BaseResources res in compound.inputResources)
        {
            ScriptableMine _mine = GetScriptableMineFromResource(res);
            if (_mine != null)
            {
                pricePerProduct += _mine.pricePerProduct * compound.inputAmounts[Array.IndexOf(compound.inputResources, res)] * compound.collectTime / _mine.collectTime;
            }

            ScriptableCompound _compound = GetScriptableCompoundFromResource(res);
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
            var _mine = GetScriptableMineFromResource(res);
            if (_mine != null)
                incomePerSecond += _mine.incomePerSecond * inputAmounts[Array.IndexOf(resources, res)];

            var _compound = GetScriptableCompoundFromResource(res);
            if (_compound != null)
                incomePerSecond += _compound.incomePerSecond * inputAmounts[Array.IndexOf(resources, res)];
        }
        return incomePerSecond * UpgradeSystem.Instance.INCOME_PRICE_MULTIPLIER;
    }

    #region Helper Functions
    void InitializeMineAndCompoundList(out List<ScriptableMine> _mineList, out List<ScriptableCompound> _compoundList)
    {
        var assets = Resources.LoadAll("AGES");
        List<ScriptableCompound> compoundList = new List<ScriptableCompound>();
        List<ScriptableMine> mineList = new List<ScriptableMine>();

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

    public Mine_Btn GetMineFromResource(BaseResources res)
    {
        var q = instantiatedMines.Where(m => m.GetComponent<Mine_Btn>() != null && m.GetComponent<Mine_Btn>().ProducedResource == res).FirstOrDefault();
        return q != null ? q.GetComponent<Mine_Btn>() : null;
    }

    public ScriptableMine GetScriptableMineFromResource(BaseResources res)
    {
        List<ScriptableMine> mineList = new List<ScriptableMine>();
        List<ScriptableCompound> compoundList = new List<ScriptableCompound>();
        InitializeMineAndCompoundList(out mineList, out compoundList);

        var q = mineList.Where(m => m != null && m.product == res).FirstOrDefault();
        return q != null ? q : null;
    }

    public Compounds GetCompoundFromResource(BaseResources res)
    {
        var q = instantiatedCompounds.Where(c => c.GetComponent<Compounds>() != null && c.GetComponent<Compounds>().Product == res).FirstOrDefault();
        return q != null ? q.GetComponent<Compounds>() : null;
    }

    public ScriptableCompound GetScriptableCompoundFromResource(BaseResources res)
    {
        List<ScriptableMine> mineList = new List<ScriptableMine>();
        List<ScriptableCompound> compoundList = new List<ScriptableCompound>();
        InitializeMineAndCompoundList(out mineList, out compoundList);
        var q = compoundList.Where(c => c != null && c.product == res).FirstOrDefault();
        return q != null ? q : null;
    }
    #endregion
}

/// <summary>
/// Working Mode for Mines and Factories
/// </summary>
public enum MineWorkingMode
{
    /// <summary>
    /// Use produced materials for further and advanced production
    /// </summary>
    production = 1,
    /// <summary>
    /// Sell produced products
    /// </summary>
    sell = 2,
}

/// <summary>
/// Working Mode for Mines and Factories
/// </summary>
public enum CompoundWorkingMode
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